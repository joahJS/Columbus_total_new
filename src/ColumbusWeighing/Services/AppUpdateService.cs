using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using ColumbusWeighing.ComnLib;
using ColumbusWeighing.Models;

namespace ColumbusWeighing.Services
{
    /// <summary>
    /// 프로그램 시작 시 통합 허브 DB(dbo.PROGRAM_VERSION)에 현재 실행 중인 버전보다 높은
    /// 버전이 등록되어 있으면 사용자에게 물어보고, 동의하면 그 실행 파일로 자기 자신을
    /// 교체한 뒤 재시작한다.
    /// 실행 중인 exe는 자기 자신을 직접 덮어쓸 수 없으므로, 이 프로세스가 완전히 종료된
    /// 뒤에 파일을 바꿔치기하고 재시작하는 배치 스크립트를 따로 띄운 다음 이 프로세스는
    /// 종료한다(Program.cs가 Application.Run을 호출하지 않고 그냥 리턴하는 방식으로).
    /// </summary>
    public static class AppUpdateService
    {
        /// <summary>새 버전이 있고 사용자가 업데이트에 동의해서 실제로 적용했으면 true.
        /// 이 경우 호출한 쪽(Program.cs)은 MainForm을 띄우지 말고 그냥 종료해야 한다
        /// (교체/재시작은 별도로 띄운 배치 스크립트가 대신 처리한다).</summary>
        public static bool CheckAndApply(IVersionRepository repository)
        {
            VersionRecord latest;
            try
            {
                latest = FindLatestNewerVersion(repository);
            }
            catch (Exception)
            {
                // 허브 DB 접속 실패 등으로 업데이트 확인 자체가 안 되는 상황이 프로그램 사용을
                // 막으면 안 되므로, 확인을 건너뛰고 평소대로 실행한다.
                return false;
            }

            if (latest == null)
            {
                return false;
            }

            var message = string.IsNullOrEmpty(latest.Remark)
                ? string.Format("새 버전({0})이 있습니다. 지금 업데이트하시겠습니까?", latest.VersionId)
                : string.Format("새 버전({0})이 있습니다. 지금 업데이트하시겠습니까?\r\n\r\n{1}", latest.VersionId, latest.Remark);

            if (!ComnFunc.gp_PrintQuestion(message, "업데이트 확인", MessageType.질문))
            {
                return false;
            }

            var full = repository.GetVersionWithFileData(latest.Id);
            if (full?.FileData == null || full.FileData.Length == 0)
            {
                ComnFunc.gp_PrintMessage("업데이트 파일을 받아오지 못했습니다.", "업데이트 오류", MessageType.오류);
                return false;
            }

            try
            {
                ApplyUpdate(full.FileData);
                return true;
            }
            catch (Exception ex)
            {
                ComnFunc.gp_PrintMessage("업데이트 적용 중 오류가 발생했습니다.\r\n" + ex.Message, "업데이트 오류", MessageType.오류);
                return false;
            }
        }

        /// <summary>등록된 버전 이력 중 현재 실행 중인 버전보다 높은 것 중 가장 높은 버전을
        /// 찾는다. VersionId가 System.Version 형식(예: 1.2.3.4)이 아닌 값은 비교할 수 없어
        /// 건너뛴다.</summary>
        private static VersionRecord FindLatestNewerVersion(IVersionRepository repository)
        {
            repository.Refresh();

            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

            VersionRecord best = null;
            Version bestVersion = null;

            foreach (var record in repository.Records)
            {
                if (!Version.TryParse(record.VersionId, out var parsed))
                {
                    continue;
                }

                if (parsed <= currentVersion)
                {
                    continue;
                }

                if (bestVersion == null || parsed > bestVersion)
                {
                    bestVersion = parsed;
                    best = record;
                }
            }

            return best;
        }

        /// <summary>새 실행 파일을 임시 폴더에 저장한 뒤, 이 프로그램이 완전히 종료되는 것을
        /// 기다렸다가 원래 exe 자리에 덮어쓰고 다시 실행하는 배치 스크립트를 만들어 띄운다.
        /// 이 메서드는 프로세스를 직접 종료하지 않는다 - 호출한 쪽(Program.cs)이 MainForm을
        /// 띄우지 않고 그냥 리턴해서 프로세스를 끝내야 배치 스크립트가 파일을 바꿀 수 있다.</summary>
        private static void ApplyUpdate(byte[] fileData)
        {
            var targetExePath = Application.ExecutablePath;
            var tempExePath = Path.Combine(Path.GetTempPath(), "ColumbusWeighing_update_" + Guid.NewGuid().ToString("N") + ".exe");
            File.WriteAllBytes(tempExePath, fileData);

            var batchPath = Path.Combine(Path.GetTempPath(), "ColumbusWeighing_update_" + Guid.NewGuid().ToString("N") + ".bat");
            var pid = Process.GetCurrentProcess().Id;

            // 현재 프로세스(pid)가 완전히 끝날 때까지 1초 간격으로 기다렸다가, exe를 새
            // 파일로 바꾸고 다시 실행한 뒤 자기 자신(임시 파일들)을 지운다.
            var script = string.Format(
                "@echo off\r\n" +
                ":wait\r\n" +
                "tasklist /FI \"PID eq {0}\" | find \"{0}\" >nul\r\n" +
                "if not errorlevel 1 (\r\n" +
                "    timeout /t 1 /nobreak >nul\r\n" +
                "    goto wait\r\n" +
                ")\r\n" +
                "copy /y \"{1}\" \"{2}\" >nul\r\n" +
                "start \"\" \"{2}\"\r\n" +
                "del \"{1}\"\r\n" +
                "del \"%~f0\"\r\n",
                pid, tempExePath, targetExePath);

            File.WriteAllText(batchPath, script, Encoding.Default);

            Process.Start(new ProcessStartInfo
            {
                FileName = batchPath,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = true,
            });
        }
    }
}
