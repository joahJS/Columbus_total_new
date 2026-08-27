using System;
using System.Threading;
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
