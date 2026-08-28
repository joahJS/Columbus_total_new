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
    }
}
