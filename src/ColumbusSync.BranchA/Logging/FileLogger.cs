using System;
using System.IO;
using System.Threading.Tasks;
using ColumbusSync.BranchA.Config;

namespace ColumbusSync.BranchA.Logging
{
    /// <summary>
    /// 이 프로그램은 화면 없이(콘솔조차 안 보고) 백그라운드로 오래 돌아가는 것을 전제로 하므로,
    /// 콘솔 출력과 별개로 파일에도 항상 로그를 남긴다. 문제가 생겼을 때 담당자가 나중에
    /// 로그 파일만 열어봐도 원인을 추적할 수 있어야 한다.
    /// </summary>
    public static class FileLogger
    {
        // 로그 파일 쓰기(File.AppendAllText)가 어떤 이유로든(네트워크 경로 문제, 백업/바이러스
        // 검사 프로그램의 일시적 잠금 등) 멈춰버리면, 그 호출을 감싸는 lock을 영원히 들고
        // 있게 되어 이후의 모든 로그 호출(동기화 성공/실패 로그 포함)이 전부 막혀버린다 -
        // SqlHelper의 DB 호출 타임아웃과 똑같은 이유로, 여기도 타임아웃을 건다.
        private static readonly TimeSpan WriteTimeout = TimeSpan.FromSeconds(10);
        private static readonly object SyncRoot = new object();

        public static void Info(string message)
        {
            Write("INFO", message);
        }

        public static void Error(string message, Exception ex = null)
        {
            Write("ERROR", ex == null ? message : string.Format("{0} - {1}", message, ex));
        }

        private static void Write(string level, string message)
        {
            var line = string.Format("[{0:yyyy-MM-dd HH:mm:ss}] [{1}] {2}", DateTime.Now, level, message);

            Console.WriteLine(line);

            lock (SyncRoot)
            {
                try
                {
                    var task = Task.Run(() => WriteToFile(line));
                    // 시간 안에 못 끝나면 이번 한 줄만 못 쓰고 넘어간다(lock은 바로 풀려서
                    // 다음 로그 호출은 막히지 않는다) - 파일 쓰기 하나 실패했다고 동기화 잡
                    // 전체가 조용히 멈춰버리는 상황을 막기 위함이다.
                    task.Wait(WriteTimeout);
                }
                catch
                {
                    // 로그 기록 실패는 동기화 잡 자체를 중단시킬 이유가 아니므로 무시한다.
                }
            }
        }

        private static void WriteToFile(string line)
        {
            var path = SyncSettings.LogFilePath;
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.AppendAllText(path, line + Environment.NewLine);
        }
    }
}
