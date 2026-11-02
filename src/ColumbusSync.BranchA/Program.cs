using System;
using System.Threading;
using System.Threading.Tasks;
using ColumbusSync.BranchA.Config;
using ColumbusSync.BranchA.Hub;
using ColumbusSync.BranchA.Logging;
using ColumbusSync.BranchA.Source;

namespace ColumbusSync.BranchA
{
    /// <summary>
    /// A지점(Columbus_total/MES) 데이터를 통합 허브 DB(COLUMBUS_WEIGH_HUB)로 옮기는 동기화 잡.
    ///
    /// 독립 실행 프로그램입니다:
    ///   - 메인 계량 화면(ColumbusWeighing) 프로젝트를 참조하지 않으며, 이 폴더만 빌드/배포하면 됩니다.
    ///   - App.config의 연결 문자열 두 개(BranchA_Mes, IntegrationHub)만 채우면 어느 PC에서도 실행 가능합니다.
    ///   - 화면(UI) 없이 콘솔/백그라운드로 계속 실행되며, App.config의 SyncIntervalMinutes 주기로
    ///     동기화를 반복합니다. B/C지점의 mdb 10분 복사 프로그램과 동일한 방식으로, 그냥 실행해두면 됩니다.
    ///   - 운영 안정성을 높이려면 이후 Windows 서비스 등록이나 작업 스케줄러 등록으로 바꿀 수 있습니다
    ///     (지금은 콘솔 앱 + 내부 타이머 루프로 충분히 동작합니다).
    /// </summary>
    public static class Program
    {
        // RunOnce() 안에는 이미 자체 try/catch가 있어서 대부분의 실패는 "동기화 실패" 로그를
        // 남기고 정상적으로 다음 주기로 넘어간다. B/C지점(ColumbusSync.BranchBC) 쪽에서
        // "동기화 시작" 로그만 찍힌 채 며칠씩 완전히 멈춰버리는 사례가 실제로 있었다(각각
        // 원인을 찾아 타임아웃을 걸었지만, 앞으로 또 다른 새로운 원인으로 멈출 가능성을
        // 배제할 수 없다). A지점도 같은 방어 차원에서, RunOnce() 호출 자체를 통째로 시간제한
        // 안에서 돌려서 뭐가 됐든 이 시간 안에 안 끝나면 포기하고 다음 주기로 넘어가게
        // 해둔다. 멈춘 백그라운드 스레드 자체는 못 끊고 그대로 남지만(.NET에 스레드 강제
        // 종료 수단이 없다), 적어도 메인 루프는 계속 살아서 재시도한다.
        private static readonly TimeSpan RunOnceTimeout = TimeSpan.FromMinutes(20);

        public static void Main()
        {
            FileLogger.Info("ColumbusSync.BranchA 시작");

            while (true)
            {
                try
                {
                    var source = new MesSourceReader(SyncSettings.MesConnectionString);
                    var hub = new HubWriter(SyncSettings.HubConnectionString);
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
