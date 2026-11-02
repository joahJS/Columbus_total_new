using System;
using System.Threading;
using System.Threading.Tasks;
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
        // RunOnce() 안에는 이미 자체 try/catch가 있어서 대부분의 실패는 "동기화 실패" 로그를
        // 남기고 정상적으로 다음 주기로 넘어간다. 그런데도 실제로 "동기화 시작" 로그만 찍힌 채
        // 며칠씩 완전히 멈춰버리는 사례가 있었다(mdb 잠금, 로그 파일 쓰기 데드락 등 - 각각
        // 찾아서 타임아웃을 걸어뒀지만, 앞으로 또 다른 새로운 원인으로 멈출 가능성을 배제할
        // 수 없다). 그래서 원인을 하나하나 쫓는 대신, RunOnce() 호출 자체를 통째로 시간제한
        // 안에서 돌려서, 뭐가 됐든 이 시간 안에 안 끝나면 그냥 포기하고 다음 주기로 넘어가도록
        // 최후의 안전장치를 걸어둔다. 멈춘 백그라운드 스레드 자체는 못 끊고 그대로 남지만
        // (.NET에 스레드 강제 종료 수단이 없다), 적어도 메인 루프는 계속 살아서 재시도한다.
        private static readonly TimeSpan RunOnceTimeout = TimeSpan.FromMinutes(20);

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

                    var task = Task.Run(() => orchestrator.RunOnce());
                    if (!task.Wait(RunOnceTimeout))
                    {
                        FileLogger.Error(string.Format(
                            "동기화가 {0}분 안에 끝나지 않았습니다. 어딘가에서 멈춘 것으로 보여 포기하고 다음 주기로 넘어갑니다.",
                            RunOnceTimeout.TotalMinutes));
                    }
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
