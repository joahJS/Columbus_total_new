using System;
using System.Configuration;
using System.IO;

namespace ColumbusSync.BranchBC.Config
{
    /// <summary>App.config에서 읽어오는 동기화 잡 설정값.</summary>
    public static class SyncSettings
    {
        /// <summary>이 PC가 담당하는 지점 코드('B' 또는 'C'). B/C지점 PC 양쪽에 같은 프로그램을
        /// 설치하고, 이 값만 다르게 설정한다.</summary>
        public static string BranchCode
        {
            get
            {
                var value = ConfigurationManager.AppSettings["BranchCode"];
                if (string.IsNullOrWhiteSpace(value) || value == "__SET_ME__")
                {
                    throw new InvalidOperationException("App.config의 BranchCode를 'B' 또는 'C'로 설정하세요.");
                }

                return value.Trim().ToUpperInvariant();
            }
        }

        /// <summary>TS2020이 쓰는 mdb 파일 경로를 OLE DB 연결 문자열로 변환한다.
        /// Microsoft Access Database Engine(ACE OLEDB 12.0)이 설치되어 있어야 하며,
        /// 이 프로그램(ColumbusSync.BranchBC.exe)의 빌드 비트수(x86/x64)와 설치된
        /// ACE 드라이버의 비트수가 일치해야 한다.</summary>
        public static string MdbConnectionString
        {
            get
            {
                var path = ConfigurationManager.AppSettings["MdbFilePath"];
                if (string.IsNullOrWhiteSpace(path) || path == "__SET_ME__")
                {
                    throw new InvalidOperationException("App.config의 MdbFilePath를 실제 TSDB.mdb 경로로 설정하세요.");
                }

                if (!File.Exists(path))
                {
                    throw new InvalidOperationException(string.Format("mdb 파일을 찾을 수 없습니다: {0}", path));
                }

                return string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Persist Security Info=False;", path);
            }
        }

        public static string HubConnectionString
        {
            get
            {
                var setting = ConfigurationManager.ConnectionStrings["IntegrationHub"];
                if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
                {
                    throw new InvalidOperationException("App.config에 연결문자열 'IntegrationHub'이(가) 설정되어 있지 않습니다.");
                }

                return setting.ConnectionString;
            }
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
                return string.IsNullOrWhiteSpace(configured) ? "logs\\ColumbusSync.BranchBC.log" : configured;
            }
        }

        /// <summary>최초 동기화(워터마크가 없을 때)에 한 번 백필할 기간(일). 그 이후의 매
        /// 주기 동기화는 이 값과 무관하게 WORK_DATE 증분 조회로 바뀐다(SyncOrchestrator
        /// 참고) - 더 오래된 과거 데이터까지 조회하고 싶으면 이 값을 늘린 다음, 허브 DB의
        /// dbo.SYNC_STATE에서 이 지점의 WEIGH_WATERMARK 행을 지우고 다시 백필을 돌리면 된다.</summary>
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

        /// <summary>계근 사진(IMAGE\yyyyMMdd\*.jpg)이 들어있는 폴더의 루트 경로
        /// (예: "C:\TS2020\IMAGE"). 비어있으면 사진 동기화를 건너뛴다 - 이 기능을 아직 쓰지
        /// 않는 지점의 기존 배포가 깨지지 않도록 기본값을 "사용 안 함"으로 둔다.</summary>
        public static string ImagesFolderPath
        {
            get
            {
                var path = ConfigurationManager.AppSettings["ImagesFolderPath"];
                return string.IsNullOrWhiteSpace(path) || path == "__SET_ME__" ? null : path;
            }
        }
    }
}
