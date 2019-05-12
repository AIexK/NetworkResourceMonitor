using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ResourcesMonitor.Models;
using System.IO;
using Newtonsoft.Json;

namespace ResourcesMonitor.Utils
{

    public static class OptionsHelper
    {
        private const string OPTIONS_FILE_NAME = "options.json";
        private static OptionsModel _options;

        public static bool IsNessesaryToCheckDatabase(string hostName)
        {
            var result = Options.ServerDatas.FirstOrDefault(x => x.HostName == hostName).IsNesseseryToCheckSqlBases;
            return result;
        }

        public static List<string> GetWindowsServiceNamesToControl(string hostName)
        {
            var result = Options.ServerDatas.FirstOrDefault(x => x.HostName == hostName).WindowsServicesNames;
            return result;
        }

        public static List<string> GetDatabasesNameToIgnor(string hostName)
        {
            var result = Options.ServerDatas.FirstOrDefault(x => x.HostName == hostName).DatabasesIgnoreList;
            return result;
        }

        public static DataBaseOptionsModel GetDataBaseOptions(string hostName)
        {
            var result = new DataBaseOptionsModel();
            var serverOptions = Options.ServerDatas.FirstOrDefault(x => x.HostName == hostName);
            result.Login = serverOptions?.SqlServerLogin;
            result.Password = serverOptions?.SqlServerPassword;
            result.IsNesseseryToCheckSqlBases = serverOptions?.IsNesseseryToCheckSqlBases ?? false;
            return result;
        }

        public static void ReloadOptions()
        {
            _options = LoadFromJson<OptionsModel>(OptionsFileName);
        }

        public static OptionsModel Options
        {
            get
            {
                if (_options == null)
                {
                    _options = LoadFromJson<OptionsModel>(OptionsFileName);
                }
                return _options;
            }
        }

        public static string OptionsFileName
        {
            get
            {
                var result = System.Web.Hosting.HostingEnvironment.MapPath(Path.Combine("~/bin/", OPTIONS_FILE_NAME));
                return result;
            }
        }

        private static T LoadFromJson<T>(string fileName)
        {
            T result = default(T);
            try
            {
                var json = File.ReadAllText(fileName);
                result = JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Loading options error: {ex.Message}");
                throw new Exception("Loading options error!", ex);
            }
            return result;
        }

        public static void SaveToJson<T>(T model, string fileName)
        {
            try
            {
                var stringifyModel = JsonConvert.SerializeObject(model, Formatting.Indented);
                File.WriteAllText(fileName, stringifyModel);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Saving options error: {ex.Message}");
                throw new Exception("Saving options error!", ex);
            }
        }

        public static void SaveOptions<T>(T options)
        {
            if (typeof(OptionsModel).FullName == typeof(T).FullName)
            {
                SaveToJson<T>(options, OptionsFileName);
            }
        }
    }
}