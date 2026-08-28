using System;
using System.Threading;
using ColumbusSync.BranchBC.Config;
using ColumbusSync.BranchBC.Hub;
using ColumbusSync.BranchBC.Logging;
using ColumbusSync.BranchBC.Source;

namespace ColumbusSync.BranchBC
{
    /// <summary>
    /// B/C지점(TSDB.mdb) 데이터를 통합 허브 DB(COLUMBUS_WEIGH_HUB)로 옮기는 동기화 잡.
    ///
    /// 독립 실행 프로그램입니다: 이 폴더만 빌드/배포하면 됩니다. B지점 PC와 C지점 PC 양쪽에
    /// 코드는 완전히 동일하게 설치하고, App.config의 BranchCode("B" 또는 "C")와
    /// MdbFilePath(그 PC의 실제 mdb 경로)만 다르게 설정합니다.
    ///
    /// mdb 파일 읽기는 Microsoft Access Database Engine(ACE OLEDB 12.0)을 사용하므로,
    /// 그 드라이버가 설치되어 있어야 하고 이 exe의 빌드 비트수(x86/x64)와 일치해야 합니다.
    /// (자세한 내용은 README 참고)
    /// </summary>
    public static class Program
    {
        public static void Main()
        {
            FileLogger.Info("ColumbusSync.BranchBC 시작");

            while (true)
            {
                try
                {
                    var branchCode = SyncSettings.BranchCode;
                    FileLogger.Info(string.Format("담당 지점: {0}", branchCode));

                    var source = new MdbSourceReader(SyncSettings.MdbConnectionString);
                    var hub = new HubWriter(SyncSettings.HubConnectionString, branchCode);
                    var orchestrator = new SyncOrchestrator(source, hub);

                    orchestrator.RunOnce();
                }
                catch (Exception ex)
                {
                    // App.config 설정 오류 등 RunOnce 진입 전 단계에서 나는 예외까지 방어한다.
                    // 이 catch가 없으면 설정 실수 한 번으로 백그라운드 프로세스 자체가 죽는다.
                    FileLogger.Error("동기화 루프에서 처리되지 않은 예외 발생", ex);
                }

                var interval = SyncSettings.SyncInterval;
                FileLogger.Info(string.Format("다음 동기화까지 {0}분 대기", interval.TotalMinutes));
                Thread.Sleep(interval);
            }
        }
    }
}
