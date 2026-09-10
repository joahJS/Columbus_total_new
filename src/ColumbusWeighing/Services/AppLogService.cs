using System;
using System.IO;

namespace ColumbusWeighing.Services
{
    public enum LogLevel
    {
        INF,
        WRN,
        ERR
    }

    public class LogEventArgs : EventArgs
    {
        public LogEventArgs(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }

    /// <summary>
    /// 화면 상단 통신/이벤트 로그 패널에 표시되는 "[HH:mm:ss] : INF : 거래처 : 메시지"
    /// 형식의 로그를 생성/전달한다.
    /// </summary>
    public sealed class AppLogService
    {
        private static readonly string LogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ColumbusWeighing",
            "logs");

        /// <summary>시스템 설정의 "로그 데이터 저장" 체크 여부. true면 화면에 찍는 로그를 파일에도 남긴다.</summary>
        public bool SaveToFile { get; set; }

        public event EventHandler<LogEventArgs> LogAdded;

        public void Write(LogLevel level, string source, string message)
        {
            var line = string.Format(
                "[{0:HH:mm:ss}] : {1} : {2} : {3}",
                DateTime.Now,
                level,
                source,
                message);

            Raw(line);
        }

        public void Info(string source, string message)
        {
            Write(LogLevel.INF, source, message);
        }

        public void Raw(string text)
        {
            LogAdded?.Invoke(this, new LogEventArgs(text));
            AppendToFileIfEnabled(text);
        }

        private void AppendToFileIfEnabled(string text)
        {
            if (!SaveToFile)
            {
                return;
            }

            try
            {
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }

                var filePath = Path.Combine(LogDirectory, string.Format("{0:yyyyMMdd}.log", DateTime.Now));
                File.AppendAllText(filePath, text + Environment.NewLine);
            }
            catch (IOException)
            {
                // 로그 파일 저장은 부가 기능이므로, 실패해도 화면 로그 표시에는 영향을 주지 않는다.
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
