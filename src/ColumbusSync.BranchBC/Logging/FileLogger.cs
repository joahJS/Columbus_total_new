using System;
using System.IO;
using ColumbusSync.BranchBC.Config;

namespace ColumbusSync.BranchBC.Logging
{
    /// <summary>
    /// 이 프로그램은 화면 없이 백그라운드로 오래 돌아가는 것을 전제로 하므로,
    /// 콘솔 출력과 별개로 파일에도 항상 로그를 남긴다.
    /// </summary>
    public static class FileLogger
    {
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
                    var path = SyncSettings.LogFilePath;
                    var dir = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    File.AppendAllText(path, line + Environment.NewLine);
                }
                catch
                {
                    // 로그 기록 실패는 동기화 잡 자체를 중단시킬 이유가 아니므로 무시한다.
                }
            }
        }
    }
}
