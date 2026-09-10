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
        /// <summary>App.config의 connectionStrings에 등록된 "ColumbusWeighHub"(통합 허브 DB) 연결 문자열. DB 연동 전까지는 비어 있을 수 있다.</summary>
        public static string ConnectionString
        {
            get
            {
                var setting = ConfigurationManager.ConnectionStrings["ColumbusWeighHub"];
                return setting == null ? string.Empty : setting.ConnectionString;
            }
        }

        public static string TxtVersion = "Copyright Columbus @ 2026 Ver.";
        public static string TxtSaveOk = "저장 되었습니다.";
        public static string TxtSaveFail = "저장에 실패하였습니다. \r\n입력정보를 확인해주세요.";
        public static string TxtLoginOk = "로그인에 성공하였습니다.";
        public static string TxtLoginFail = "아이디 또는 비밀번호를 확인해 주세요.";

        /// <summary>"접속정보 기억하기" 저장에 사용하는 IniHelper 섹션/키.</summary>
        public static string IniSectionLogin = "LOGIN";
        public static string IniKeyLoginId = "ID";
        public static string IniKeyLoginPw = "PW";
        public static string IniKeyLoginRemember = "REMEMBER";

        /// <summary>
        /// 시스템 설정의 "자동 로그인 사용"이 쓰는 별도의 저장 키. "접속정보 기억하기"(위 세 키)는
        /// 로그인창에 아이디/비밀번호를 미리 채워주기만 하는 값이라 체크를 끄면 지워지는데,
        /// 자동 로그인은 그 체크와 무관하게 동작해야 하므로 완전히 분리해서 저장한다.
        /// </summary>
        public static string IniKeyAutoLoginId = "AUTO_ID";
        public static string IniKeyAutoLoginPw = "AUTO_PW";
    }
}
