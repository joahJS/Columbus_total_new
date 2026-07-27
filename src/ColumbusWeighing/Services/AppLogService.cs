using System;

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
        public event EventHandler<LogEventArgs> LogAdded;

        public void Write(LogLevel level, string source, string message)
        {
            var line = string.Format(
                "[{0:HH:mm:ss}] : {1} : {2} : {3}",
                DateTime.Now,
                level,
                source,
                message);

            LogAdded?.Invoke(this, new LogEventArgs(line));
        }

        public void Info(string source, string message)
        {
            Write(LogLevel.INF, source, message);
        }

        public void Raw(string text)
        {
            LogAdded?.Invoke(this, new LogEventArgs(text));
        }
    }
}
