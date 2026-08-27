using System;
using System.Configuration;

namespace ColumbusSync.BranchA.Config
{
    /// <summary>App.config에서 읽어오는 동기화 잡 설정값.</summary>
    public static class SyncSettings
    {
        public static string MesConnectionString
        {
            get { return GetConnectionString("BranchA_Mes"); }
        }

        public static string HubConnectionString
        {
            get { return GetConnectionString("IntegrationHub"); }
        }

        public static TimeSpan SyncInterval
        {
            get
            {
                int minutes;
                if (!int.TryParse(ConfigurationManager.AppSettings["SyncIntervalMinutes"], out minutes) || minutes <= 0)
                {
                    minutes = 10;
                }

                return TimeSpan.FromMinutes(minutes);
            }
        }

        public static string LogFilePath
        {
            get
            {
                var configured = ConfigurationManager.AppSettings["LogFilePath"];
                return string.IsNullOrWhiteSpace(configured) ? "logs\\ColumbusSync.BranchA.log" : configured;
            }
        }

        private static string GetConnectionString(string name)
        {
            var setting = ConfigurationManager.ConnectionStrings[name];
            if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
            {
                throw new InvalidOperationException(string.Format("App.config에 연결문자열 '{0}'이(가) 설정되어 있지 않습니다.", name));
            }

            return setting.ConnectionString;
        }
    }
}
