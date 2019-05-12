using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using static ResourcesMonitor.Utils.ConvertUtils;
using ResourcesMonitor.Models;

namespace ResourcesMonitor.Utils
{
    public class DataBaseUtils
    {
        public static SqlServerCheckResult CheckBases(string hostName)
        {
            var result = new SqlServerCheckResult();

            var baseData = OptionsHelper.GetDataBaseOptions(hostName);
            var basesIgnorList = OptionsHelper.GetDatabasesNameToIgnor(hostName);
            if (!baseData.IsNesseseryToCheckSqlBases ||
                string.IsNullOrEmpty(baseData.Login) ||
                string.IsNullOrEmpty(baseData.Password))
            {
                result.ErrorMessage = $@"Checking of database on server '{hostName}' is not nesessary or login/password did not load";
                return result;
            }
            var connection = new SqlConnection(
                                       $"user id={baseData.Login};" +
                                       $"password={baseData.Password};server={hostName};" +
                                       $"database=master; " +
                                       $"connection timeout=30");
            try
            {
                connection.Open();
                SqlDataReader reader = null;
                SqlCommand myCommand = new SqlCommand("select ssdb.name, ssdb.status, sdb.state_desc from sys.sysdatabases ssdb " +
                                                      "join sys.databases sdb on ssdb.name = sdb.name", connection);
                reader = myCommand.ExecuteReader();
                while (reader.Read())
                {
                    var baseName = reader["name"].ToString();
                    if (basesIgnorList?.Contains(baseName) ?? false)
                    {
                        continue;
                    }
                    var baseStatus = ConvertSafe.ToInt32(reader["status"]);
                    var baseStatusSimple = reader["state_desc"].ToString();

                    var statuses = GetBaseStatusesByStatusCode(baseStatus);

                    if (
                            (
                                !(
                                    statuses.Contains(Constants.DatabaseStates.ONLINE) ||
                                    statuses.Contains(Constants.DatabaseStates.TRUNCATE_LOG_ON_CHKPT) ||
                                    statuses.Contains(Constants.DatabaseStates.TORN_PAGE_DETECTION)
                                  )
                            ) ||
                            baseStatusSimple.ToUpper() != Constants.DatabaseStates.SIMPLE_STATUS_ONLINE
                       )
                    {
                        statuses.Add(baseStatusSimple);

                        result.DataBaseInfoList.Add(new DataBaseInfoModel()
                        {
                            Name = baseName,
                            Statuses = statuses
                        });
                    }
                }
            }
            catch (Exception e)
            {
                result.ErrorMessage = $"DataBase on server '{hostName}' get databases list error: {e.Message}";
                Logger.LogError($"Database error. ^^^Direct write to log^^^ ERROR:{result.ErrorMessage}{Environment.NewLine}Full error text: {e.ToString()}");
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            return result;
        }

        private static List<string> GetBaseStatusesByStatusCode(int status)
        {
            var result = new List<string>();

            if ((1 & status) == 1)
            {
                result.Add(Constants.DatabaseStates.AUTOCLOSE);
            }
            if ((2 & status) == 2)
            {
                result.Add(Constants.DatabaseStates.UNKNOWN_CODE_2);
            }
            if ((4 & status) == 4)
            {
                result.Add(Constants.DatabaseStates.SELECT_INTO_OR_BULKCOPY);
            }
            if ((8 & status) == 8)
            {
                result.Add(Constants.DatabaseStates.TRUNCATE_LOG_ON_CHKPT);
            }
            if ((16 & status) == 16)
            {
                result.Add(Constants.DatabaseStates.TORN_PAGE_DETECTION);
            }
            if ((32 & status) == 32)
            {
                result.Add(Constants.DatabaseStates.LOADING);
            }
            if ((64 & status) == 64)
            {
                result.Add(Constants.DatabaseStates.PRE_RECOVERY);
            }
            if ((128 & status) == 128)
            {
                result.Add(Constants.DatabaseStates.RECOVERING);
            }
            if ((256 & status) == 256)
            {
                result.Add(Constants.DatabaseStates.NOT_RECOVERED);
            }
            if ((512 & status) == 512)
            {
                result.Add(Constants.DatabaseStates.OFFLINE);
            }
            if ((1024 & status) == 1024)
            {
                result.Add(Constants.DatabaseStates.READ_ONLY);
            }
            if ((2048 & status) == 2048)
            {
                result.Add(Constants.DatabaseStates.DBO_USE_ONLY);
            }
            if ((4096 & status) == 4096)
            {
                result.Add(Constants.DatabaseStates.SINGLE_USER);
            }
            if ((8192 & status) == 8192)
            {
                result.Add(Constants.DatabaseStates.UNKNOWN_CODE_8192);
            }
            if ((16384 & status) == 16384)
            {
                result.Add(Constants.DatabaseStates.UNKNOWN_CODE_16384);
            }
            if ((32768 & status) == 32768)
            {
                result.Add(Constants.DatabaseStates.EMERGENCY_MODE);
            }
            if ((65536 & status) == 65536)
            {
                result.Add(Constants.DatabaseStates.ONLINE);
            }
            if ((131072 & status) == 131072)
            {
                result.Add(Constants.DatabaseStates.UNKNOWN_CODE_131072);
            }
            if ((262144 & status) == 262144)
            {
                result.Add(Constants.DatabaseStates.UNKNOWN_CODE_262144);
            }
            if ((524288 & status) == 524288)
            {
                result.Add(Constants.DatabaseStates.UNKNOWN_CODE_524288);
            }
            if ((1048576 & status) == 1048576)
            {
                result.Add(Constants.DatabaseStates.UNKNOWN_CODE_1048576);
            }
            if ((2097152 & status) == 2097152)
            {
                result.Add(Constants.DatabaseStates.UNKNOWN_CODE_2097152);
            }
            if ((4194304 & status) == 4194304)
            {
                result.Add(Constants.DatabaseStates.AUTOSHRINK);
            }
            if ((1073741824 & status) == 1073741824)
            {
                result.Add(Constants.DatabaseStates.CLEANLY_SHUTDOWN);
            }

            return result;
        }


    }
}