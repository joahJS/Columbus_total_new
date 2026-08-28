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

        /// <summary>
        /// 계근 데이터를 매 배치마다 다시 훑는 기간(일). MEASURE_RETR은 수정일시가 아니라
        /// 계량일자(TDATE)로만 조회 조건을 받기 때문에, 검수/수정이 늦게 들어온 건을 놓치지
        /// 않으려면 그 지연 가능성만큼 기간을 넉넉히 잡아야 한다. 실제 업무에서 검수가 며칠까지
        /// 늦어질 수 있는지에 맞춰 조정한다.
        /// </summary>
        public static int WeighSyncLookbackDays
        {
            get
            {
                int days;
                if (!int.TryParse(ConfigurationManager.AppSettings["WeighSyncLookbackDays"], out days) || days <= 0)
                {
                    days = 30;
                }

                return days;
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
