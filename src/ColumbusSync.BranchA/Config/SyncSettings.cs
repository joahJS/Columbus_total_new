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

        /// <summary>완료된 계근건의 내용 해시를 저장해두는 로컬 캐시 파일 경로(WeighHashCache
        /// 참고) - 내용이 바뀌지 않은 건은 매 주기 허브 DB에 다시 업서트하지 않기 위한
        /// 것이다. 상대경로면 실행파일 폴더 기준.</summary>
        public static string WeighHashCachePath
        {
            get
            {
                var configured = ConfigurationManager.AppSettings["WeighHashCachePath"];
                return string.IsNullOrWhiteSpace(configured) ? "state\\weigh_hash_cache.tsv" : configured;
            }
        }

        /// <summary>
        /// 계근 데이터를 매 배치마다 다시 훑는 기간(일). MEASURE_RETR은 수정일시가 아니라
        /// 계량일자(TDATE)로만 조회 조건을 받기 때문에, 검수/수정이 늦게 들어온 건을 놓치지
        /// 않으려면 그 지연 가능성만큼 기간을 넉넉히 잡아야 한다. 기본값(1년)은 매 주기
        /// 그대로 유지된다 - B/C지점과 달리 MES에는 수정시각 컬럼이 없어서 워터마크 증분
        /// 조회로 바꿀 수 없기 때문이다. 대신 WeighHashCache가 내용이 안 바뀐 완료건의 허브
        /// 업서트는 걸러내므로, 매 주기 전체 기간을 "다시 받기"는 하지만 "다시 쓰기"는 하지
        /// 않는다.
        /// </summary>
        public static int WeighSyncLookbackDays
        {
            get
            {
                int days;
                if (!int.TryParse(ConfigurationManager.AppSettings["WeighSyncLookbackDays"], out days) || days <= 0)
                {
                    days = 365;
                }

                return days;
            }
        }

        /// <summary>계근 사진이 올라가 있는 FTP 서버 루트 경로(예: "ftp://121.66.17.30:16021/Columbus/").
        /// 끝에 "/"를 포함해야 한다(Columbus_total의 SA015F00.cs와 같은 규칙). 비어있으면
        /// 사진 동기화를 건너뛴다.</summary>
        public static string FtpPath
        {
            get
            {
                var value = ConfigurationManager.AppSettings["FtpPath"];
                return string.IsNullOrWhiteSpace(value) || value == "__SET_ME__" ? null : value;
            }
        }

        public static string FtpUserId
        {
            get { return ConfigurationManager.AppSettings["FtpUserId"]; }
        }

        public static string FtpPassword
        {
            get { return ConfigurationManager.AppSettings["FtpPassword"]; }
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
