using System.Configuration;

namespace ColumbusWeighing.ComnLib
{
    /// <summary>
    /// 프로그램 전역에서 반복 사용하는 문구/설정값.
    /// VisionIns 솔루션의 ComnString과 같은 역할이지만, DB 접속정보처럼 배포 환경마다 달라지고
    /// 보안에 민감한 값은 소스에 하드코딩하지 않고 App.config(connectionStrings)에서 읽어온다.
    /// </summary>
    public static class ComnString
    {
        /// <summary>App.config의 connectionStrings에 등록된 "Columbus" 연결 문자열. DB 연동 전까지는 비어 있을 수 있다.</summary>
        public static string ConnectionString
        {
            get
            {
                var setting = ConfigurationManager.ConnectionStrings["Columbus"];
                return setting == null ? string.Empty : setting.ConnectionString;
            }
        }

        public static string TxtVersion = "Copyright Columbus @ 2026 Ver.";
        public static string TxtSaveOk = "저장 되었습니다.";
        public static string TxtSaveFail = "저장에 실패하였습니다. \r\n입력정보를 확인해주세요.";
        public static string TxtLoginOk = "로그인에 성공하였습니다.";
        public static string TxtLoginFail = "아이디 또는 비밀번호를 확인해 주세요.";
    }
}
