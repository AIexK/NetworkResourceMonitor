using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ResourcesMonitor.Models;
using ResourcesMonitor.Utils;
using System.Web.Configuration;
using ResourcesMonitor.Hubs;
using System.Threading;

namespace ResourcesMonitor.Controllers
{
    public class HomeController : Controller
    {
        private static System.Threading.Timer _timer;
        private static ResourcesStatusModel _resourcesStatusModel;

        private bool _isFirstTime = true;
        
        private const int FIRST_TIME_DELAY_MS = 1000;

        [Authorize]
        public ActionResult Index()
        {
            OptionsHelper.ReloadOptions();
            if (_resourcesStatusModel == null)
            {
                _resourcesStatusModel = new ResourcesStatusModel();
            }
            else
            {
                _resourcesStatusModel.IsFirstIntervalChanging = true;
            }

            if (_timer == null)
            {
                _resourcesStatusModel.Initialize();
                CheckStatus(_resourcesStatusModel);
                this._isFirstTime = false;              
                _timer = new System.Threading.Timer(CheckStatus, _resourcesStatusModel, FIRST_TIME_DELAY_MS, Timeout.Infinite);
            }
            return View();
        }

        private Result InitBlocksFromOptions()
        {
            var result = new Result();

            var options = OptionsHelper.Options;

            return result;
        }

        public void SendToTelegramIfNesessary(ResourcesStatusModel resourcesStatusModel)
        {           
            var messageToSendGeneral = string.Empty;

            var sitesErrorCount = 0;
            var diskErrorsCount = 0;
            var baseErrorsCount = 0;
            var serviceErrorsCount = 0;
            var commonErrorsCount = 0;

            // Errors with websites
            var sitesErrorMessage = string.Empty;
            foreach (var siteStatus in resourcesStatusModel.SitesStatus)
            {
                if (!siteStatus.IsAvailable) {
                    sitesErrorCount++;
                    sitesErrorMessage += $"{Environment.NewLine}{siteStatus.ErrorMessage}";
                }                
            }

            foreach (var serverStatus in resourcesStatusModel.ServersStatus)
            {
                var messageToSendByServer = string.Empty;
                var isCommonError = false;
                var isServiceError = false;
                var isBaseError = false;
                var isDiskError = false;

                // Errors with disks on current server
                var disksWithProblems = new List<string>();                
                foreach (var diskData in serverStatus.DisksData)
                {
                    if (diskData.IsOverflow)
                    {
                        disksWithProblems.Add($"\t{diskData.Name} Used space: {diskData.UsedSpaceInPercent}% (free space:{diskData.FreeSpaceToDisplay})");
                    }                
                }
                if (diskErrorsCount == 0)
                {
                    diskErrorsCount = disksWithProblems.Count;
                }
                if (disksWithProblems.Count > 0)
                {
                    messageToSendByServer += $"{Environment.NewLine}Disks with problems:{Environment.NewLine}{string.Join(Environment.NewLine, disksWithProblems) }";
                    isDiskError = true;
                    Logger.Log($"{serverStatus.HostName} DISK ERROR!{Environment.NewLine}{messageToSendByServer}");
                }

                // Check services' errors
                if (serverStatus.WindowsServicesStatus.Count > 0)
                {
                    var servicesWithProblems = new List<string>();
                    foreach (var serviceStatus in serverStatus.WindowsServicesStatus)
                    {
                        if (serviceStatus.State != Constants.ServiceState.Running)
                        {
                            servicesWithProblems.Add($"{serviceStatus.Name}: {serviceStatus.State.ToString()}");
                        }
                    }
                    if (serviceErrorsCount == 0)
                    {
                        serviceErrorsCount = servicesWithProblems.Count;
                    }
                    if (servicesWithProblems.Count > 0)
                    {
                        messageToSendByServer += $"{Environment.NewLine}Services with problems:{Environment.NewLine}{string.Join(Environment.NewLine, servicesWithProblems) }";
                        isServiceError = true;
                    }
                }

                // Checking for errors with DB
                if (serverStatus.IsNesseseryToCheckSqlBases)
                {
                    var dataBaseProblems = new List<string>();
                    var baseCheckResult = serverStatus.CheckingDatabaseResult;
                    if (!string.IsNullOrEmpty(baseCheckResult.ErrorMessage))
                    {
                        dataBaseProblems.Add($"Base error: '{baseCheckResult.ErrorMessage}'");
                    }
                    foreach (var basesStatus in baseCheckResult.DataBaseInfoList)
                    {                       
                        dataBaseProblems.Add($"{basesStatus.Name}: {basesStatus.StatusesOneLine}");
                    }
                    if (baseErrorsCount == 0)
                    {
                        baseErrorsCount = dataBaseProblems.Count;
                    }
                    if (baseErrorsCount > 0)
                    {                        
                        messageToSendByServer += $"{Environment.NewLine}Database problems:{Environment.NewLine}{string.Join(Environment.NewLine, dataBaseProblems) }";
                        isBaseError = true;
                    }
                }

                // Checking common errors
                if (serverStatus.ErrorMessages.Count > 0)
                {
                    var importantMessages = new List<string>();
                    foreach (var errorMessage in serverStatus.ErrorMessages)
                    {
                        if (!errorMessage.Contains("The RPC server is unavailable") &&
                            !errorMessage.Contains("RPC_E_CALL_CANCELED") &&
                            !errorMessage.Contains("The remote procedure call failed") &&
                            !string.IsNullOrEmpty(errorMessage))
                        {
                            importantMessages.Add(errorMessage);
                        }
                    }
                    if (importantMessages.Count > 0)
                    {
                        messageToSendByServer += $"{Environment.NewLine}Common errors:{Environment.NewLine}{string.Join(Environment.NewLine, importantMessages) }";
                        isCommonError = true;
                    }
                }

                if ((isCommonError || isServiceError || isBaseError || isDiskError)
                        && !string.IsNullOrEmpty(messageToSendByServer))
                {
                    messageToSendByServer = $"{Environment.NewLine}{serverStatus.HostName} ({serverStatus.Ip}){Environment.NewLine}{messageToSendByServer}";
                }
                messageToSendGeneral += messageToSendByServer;
            }

            if (sitesErrorCount > 0)
            {
                messageToSendGeneral += $"{Environment.NewLine}Site problems:{Environment.NewLine}{sitesErrorMessage}";
            }

            if (InformationMessagesPreviousState.ServiceErrorsCount == 0 && serviceErrorsCount > 0)
            {
                InformationMessagesPreviousState.IterationWithErrorsByService = 1;
            }
            else
            if (InformationMessagesPreviousState.ServiceErrorsCount > 0 && serviceErrorsCount > 0)
            {
                InformationMessagesPreviousState.IterationWithErrorsByService += 1;
            }

            const int MAX_ITERATION_COUNT_WITH_SERVICE_ERROR = 3;

            if (
                    (
                        sitesErrorCount != InformationMessagesPreviousState.SiteErrorsCount ||
                        diskErrorsCount != InformationMessagesPreviousState.DiskErrorsCount ||
                        baseErrorsCount != InformationMessagesPreviousState.BaseErrorsCount ||
                        (
                            serviceErrorsCount == InformationMessagesPreviousState.ServiceErrorsCount && 
                            InformationMessagesPreviousState.IterationWithErrorsByService == MAX_ITERATION_COUNT_WITH_SERVICE_ERROR
                        ) ||
                        commonErrorsCount != InformationMessagesPreviousState.CommonErrorsCount
                    ) &&
                    !string.IsNullOrEmpty(messageToSendGeneral) &&
                    (
                        sitesErrorCount    > 0 ||
                        diskErrorsCount    > 0 ||
                        baseErrorsCount    > 0 ||
                        serviceErrorsCount > 0 ||
                        commonErrorsCount  > 0
                    )
                )
            {
                Logger.LogError($"⚠️ {messageToSendGeneral}", sendToTelegram: true);

                if (serviceErrorsCount > 0)
                {
                    InformationMessagesPreviousState.IterationWithErrorsByService = 0;
                    InformationMessagesPreviousState.ThereWasServiceErrorButItWasReset = true;
                }
            }

            if (
                (
                    (InformationMessagesPreviousState.SiteErrorsCount > 0 && sitesErrorCount == 0) ||
                    (InformationMessagesPreviousState.DiskErrorsCount > 0 && diskErrorsCount == 0) ||
                    (InformationMessagesPreviousState.BaseErrorsCount > 0 && baseErrorsCount == 0) ||
                    (InformationMessagesPreviousState.ThereWasServiceErrorButItWasReset && serviceErrorsCount == 0) ||
                    (InformationMessagesPreviousState.CommonErrorsCount > 0 && commonErrorsCount == 0)
                ) &&
                sitesErrorCount == 0 &&
                diskErrorsCount == 0 &&
                baseErrorsCount == 0 &&
                serviceErrorsCount == 0 &&
                commonErrorsCount == 0
                )
            {
                Logger.LogError($"InformationMessagesPreviousState.SiteErrorsCount: {InformationMessagesPreviousState.SiteErrorsCount}");
                Logger.LogError($"sitesErrorCount: {sitesErrorCount}");
                Logger.LogError($"InformationMessagesPreviousState.CommonErrorsCount: {InformationMessagesPreviousState.CommonErrorsCount}");
                Logger.LogError($"commonErrorsCount: {commonErrorsCount}");
                Logger.LogError($"InformationMessagesPreviousState.DiskErrorsCount: {InformationMessagesPreviousState.DiskErrorsCount}");
                Logger.LogError($"diskErrorsCount: {diskErrorsCount}");
                Logger.LogError($"InformationMessagesPreviousState.BaseErrorsCount: {InformationMessagesPreviousState.BaseErrorsCount}");
                Logger.LogError($"baseErrorsCount: {baseErrorsCount}");
                Logger.LogError($"InformationMessagesPreviousState.ThereWasServiceErrorButItWasReset: {InformationMessagesPreviousState.ThereWasServiceErrorButItWasReset}");
                Logger.LogError($"serviceErrorsCount: {serviceErrorsCount}");               

                Logger.LogError("There are no more errors ✅", sendToTelegram: true);
                InformationMessagesPreviousState.ThereWasServiceErrorButItWasReset = false;
            }

            // Save info about previous state
            InformationMessagesPreviousState.SiteErrorsCount = sitesErrorCount;
            InformationMessagesPreviousState.DiskErrorsCount = diskErrorsCount;
            InformationMessagesPreviousState.BaseErrorsCount = baseErrorsCount;
            InformationMessagesPreviousState.ServiceErrorsCount = serviceErrorsCount;
            InformationMessagesPreviousState.CommonErrorsCount = commonErrorsCount;
        }

        public void CheckStatus(Object resourcesStatusModel)
        {
            var localStatusData = (ResourcesStatusModel)resourcesStatusModel;
            var currentServerId = string.Empty;
            var serversLogicGroups = OptionsHelper.Options.ServerLogicGroups;
            try
            {
                var resourcesStatusDto = new ResourcesStatusDto();
                localStatusData.SitesStatus.Clear();
                // Fill servers groups for DTO
                foreach (var serversLogicGroup in serversLogicGroups)
                {
                    var websiteStatesDto = new List<WebsiteStateDto>();
                    if (serversLogicGroup.IsSpesialBlockForWebsites)
                    {
                        foreach (var site in serversLogicGroup.Sites)
                        {                            
                            var siteErrorMessage = string.Empty;
                            var isSiteAvailable = WebsitesUtils.CheckSite(site.Url, site.HtmlToControl, out siteErrorMessage);
                            websiteStatesDto.Add(new WebsiteStateDto()
                            {
                                Id = site.Id,
                                IsAvailable = isSiteAvailable,
                                Name = site.Name,
                                Url = site.Url
                            });
                            localStatusData.SitesStatus.Add(new SiteStatusModel()
                            {
                                SiteId = site.Id,
                                ErrorMessage = siteErrorMessage,
                                IsAvailable = isSiteAvailable,
                                Url = site.Url
                            });
                        }                      
                    }

                    resourcesStatusDto.ServersLogicGroupsDto.Add(new ServersLogicGroupDto()
                    {
                        Id = serversLogicGroup.Id,
                        Caption = serversLogicGroup.Caption,
                        WebsiteStatesDto = websiteStatesDto
                    });
                }
                
                // Filling servers data for DTO
                foreach (var serversStatus in localStatusData.ServersStatus)
                {
                    serversStatus.MakeChecking();
                    currentServerId = serversStatus.Id;

                    var disksSpaceInfo = new List<DiskSpaceDto>(); // Disks
                    var servicesStates = new List<WindowsServicesStatusDto>(); // Services
                    var sqlBasesWithProblems = new List<DatabaseInfoDto>(); // DB

                    try
                    {
                        // Disks
                        foreach (var diskData in serversStatus?.DisksData)
                        {
                            disksSpaceInfo.Add(new DiskSpaceDto()
                            {
                                Letter = diskData.Name,
                                FullnessPercent = 100 - diskData.FreeSpaceInPercent,
                                FreeSpaceToDisplay = diskData.FreeSpaceToDisplay,
                                IsOverflow = diskData.IsOverflow
                            });
                        }

                        // Serices
                        foreach (var windowsServicesStatus in serversStatus?.WindowsServicesStatus)
                        {
                            servicesStates.Add(new WindowsServicesStatusDto()
                            {
                                Name = windowsServicesStatus.Name,
                                StateName = windowsServicesStatus.State.ToString()
                            });
                        }

                        // DB                   
                        var basesStatus = serversStatus?.CheckingDatabaseResult;
                        if (basesStatus != null)
                        {
                            foreach (var baseStatus in basesStatus?.DataBaseInfoList)
                            {
                                sqlBasesWithProblems.Add(new DatabaseInfoDto()
                                {
                                    Name = baseStatus.Name,
                                    Status = baseStatus.StatusesOneLine
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        serversStatus.ErrorMessages.Add(ex.Message);
                    }
                    resourcesStatusDto.ServersStatusDto.Add(new ServerStatusDto()
                    {
                        Id = serversStatus.Id,
                        LogicGroupId = serversStatus.LogicGroupId,
                        HostName = serversStatus.HostName,
                        Ip = serversStatus.Ip,
                        Ping = serversStatus.PingMsec,
                        DiskSpace = disksSpaceInfo,
                        WindowsServicesStatuses = servicesStates,
                        IsSqlBaseUnderMonitoring = serversStatus?.IsNesseseryToCheckSqlBases ?? false, 
                        SqlBasesWithProblems = sqlBasesWithProblems,
                        ErrorMessage = serversStatus.ErrorMessagesOneLine
                    });
                }

                var errorCode = Constants.ResultCodes.RESULT_OK;
                if (resourcesStatusDto.ServersStatusDto.Any(x => !string.IsNullOrEmpty(x.ErrorMessage)))
                {
                    errorCode = Constants.ResultCodes.RESULT_COMMON_HANDLED_ERROR;
                }
                var jsonRequest = CommonUtils.MakeResultJson(errorCode, null, resourcesStatusDto, null);
                SendMessage(jsonRequest);

                this.SendToTelegramIfNesessary(localStatusData);
                
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                var jsonRequest = CommonUtils.MakeResultJson(Constants.ResultCodes.RESULT_COMMON_ERROR, $"ERORR: {ex.ToString()}", null, currentServerId);
                SendMessage(jsonRequest);               
            }
            finally
            {
                if (!this._isFirstTime)
                {
                    var delay = OptionsHelper.Options.CheckingIntervalMilliseconds;
                    if (localStatusData.IsFirstIntervalChanging)
                    {
                        delay = FIRST_TIME_DELAY_MS;
                        localStatusData.IsFirstIntervalChanging = false;
                    }

                    _timer.Change(delay, Timeout.Infinite);
                }               
            }           
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Login(Models.LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (Security.ValidateCredentials(model.Login, model.Password))
            {
                FormsAuthentication.SetAuthCookie(model.Login, model.RememberMe);
                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError("", "Invalid username or password");
            return View(model);
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logoff()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            // Clear authentication cookie
            HttpCookie cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            cookie1.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie1);

            SessionStateSection sessionStateSection = (SessionStateSection)WebConfigurationManager.GetSection("system.web/sessionState");
            HttpCookie cookie2 = new HttpCookie(sessionStateSection.CookieName, "");
            cookie2.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie2);

            return RedirectToAction("Index", "Home");
        }

        private void SendMessage(string message)
        {
            var context = Microsoft
                    .AspNet
                    .SignalR
                    .GlobalHost
                    .ConnectionManager
                    .GetHubContext<NotificationHub>();
            context.Clients.All.displayMessage(message);
        }
    }
}