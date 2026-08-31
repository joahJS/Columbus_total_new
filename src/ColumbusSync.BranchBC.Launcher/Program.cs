using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace ColumbusSync.BranchBC.Launcher
{
    /// <summary>
    /// ColumbusSync.BranchBC(mdb 동기화 워커)는 Access 파일을 읽는 ACE OLEDB 12.0 드라이버를
    /// 쓰는데, 이 드라이버는 32비트/64비트 중 그 PC에 설치된 것과 정확히 같은 비트수의
    /// 프로세스에서만 로드된다. 어느 쪽이 설치되어 있는지 PC마다 다를 수 있어서, 이 런처가
    /// 레지스트리를 보고 설치된 드라이버 비트수를 판단한 뒤 맞는 쪽 워커 exe를 대신 실행한다.
    ///
    /// 이 런처 자신은 AnyCPU로 빌드되어 있어 어느 PC에서든 그냥 실행된다 — Access 파일을
    /// 직접 다루지 않으므로 비트수 제약이 없다.
    ///
    /// 배포 폴더 구성(README 참고):
    ///   ColumbusSync.BranchBC.Launcher.exe   (이 프로그램 — 실행할 것)
    ///   x86\ColumbusSync.BranchBC.exe        (32비트 빌드)
    ///   x64\ColumbusSync.BranchBC.exe        (64비트 빌드)
    /// </summary>
    public static class Program
    {
        private const string AceProgId = "Microsoft.ACE.OLEDB.12.0";

        public static int Main()
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;

                bool ace64 = IsAceRegistered(RegistryView.Registry64);
                bool ace32 = IsAceRegistered(RegistryView.Registry32);

                Log(string.Format("ACE OLEDB 드라이버 감지 결과 - 64비트: {0}, 32비트: {1}", ace64, ace32));

                string chosenPlatform;
                if (ace64)
                {
                    // 둘 다 설치되어 있으면 64비트를 우선한다.
                    chosenPlatform = "x64";
                }
                else if (ace32)
                {
                    chosenPlatform = "x86";
                }
                else
                {
                    Log("Microsoft Access Database Engine(ACE OLEDB 12.0)이 설치되어 있지 않습니다.");
                    Log("https://www.microsoft.com/download/details.aspx?id=54920 에서 설치한 뒤 다시 실행하세요.");
                    Pause();
                    return 1;
                }

                var workerPath = Path.Combine(baseDir, chosenPlatform, "ColumbusSync.BranchBC.exe");
                if (!File.Exists(workerPath))
                {
                    Log(string.Format("워커 실행파일을 찾을 수 없습니다: {0}", workerPath));
                    Log("배포 폴더 구성이 올바른지 확인하세요 (README의 '배포 폴더 구성' 참고).");
                    Pause();
                    return 1;
                }

                Log(string.Format("{0}용 워커를 실행합니다: {1}", chosenPlatform, workerPath));

                var startInfo = new ProcessStartInfo
                {
                    FileName = workerPath,
                    WorkingDirectory = Path.GetDirectoryName(workerPath),
                    UseShellExecute = true,
                };
                Process.Start(startInfo);

                return 0;
            }
            catch (Exception ex)
            {
                Log("런처 실행 중 예상치 못한 오류가 발생했습니다: " + ex);
                Pause();
                return 1;
            }
        }

        /// <summary>지정한 레지스트리 뷰(32비트/64비트)에 ACE OLEDB가 "실제로 로드 가능한 상태로"
        /// 등록되어 있는지 확인한다. RegistryView는 이 프로세스 자신의 비트수와 무관하게 원하는
        /// 쪽 레지스트리 하이브를 그대로 열어준다(.NET 4.0+의 WOW64 리다이렉션 지원).
        ///
        /// ProgID 키(Microsoft.ACE.OLEDB.12.0)만 있는지 확인하는 걸로는 부족하다 — Office와
        /// 반대 비트수의 ACE를 설치하려다 실패한 흔적 등으로 ProgID 키만 고아 상태로 남아있는
        /// 경우가 실제로 있다(이번에 발생한 문제가 정확히 이 케이스). ProgID -> CLSID ->
        /// InprocServer32(실제 dll 경로)까지 따라가서 그 dll 파일이 디스크에 실제로 존재하는지까지
        /// 확인해야 "설치되어 있다"고 신뢰할 수 있다.</summary>
        private static bool IsAceRegistered(RegistryView view)
        {
            try
            {
                using (var classesRoot = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, view))
                {
                    using (var progIdKey = classesRoot.OpenSubKey(AceProgId))
                    {
                        if (progIdKey == null) return false;
                    }

                    string clsid;
                    using (var clsidKey = classesRoot.OpenSubKey(AceProgId + @"\CLSID"))
                    {
                        clsid = clsidKey == null ? null : clsidKey.GetValue(null) as string;
                    }
                    if (string.IsNullOrEmpty(clsid))
                    {
                        // CLSID 서브키가 없으면 ProgID가 고아 상태 - 실제로는 못 쓴다.
                        return false;
                    }

                    using (var inprocKey = classesRoot.OpenSubKey(@"CLSID\" + clsid + @"\InprocServer32"))
                    {
                        var dllPath = inprocKey == null ? null : inprocKey.GetValue(null) as string;
                        if (string.IsNullOrEmpty(dllPath))
                        {
                            return false;
                        }
                        // 값이 dll 파일 전체 경로가 아니라(예: 환경변수 미확장 등) 그대로는
                        // File.Exists가 실패할 수 있으니 확장해서 한 번 더 확인한다.
                        var expanded = Environment.ExpandEnvironmentVariables(dllPath);
                        return File.Exists(expanded);
                    }
                }
            }
            catch
            {
                // 32비트 전용 OS에서 Registry64 뷰를 열 때 등, 뷰 자체를 못 여는 환경도 있다.
                // 이런 경우는 "그 비트수는 없다"로 취급하면 충분하다.
                return false;
            }
        }

        private static void Log(string message)
        {
            Console.WriteLine(string.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}", DateTime.Now, message));
        }

        private static void Pause()
        {
            Console.WriteLine();
            Console.WriteLine("아무 키나 누르면 창이 닫힙니다...");
            try { Console.ReadKey(); } catch { /* 콘솔이 리다이렉트된 환경 등에서는 무시 */ }
        }
    }
}
