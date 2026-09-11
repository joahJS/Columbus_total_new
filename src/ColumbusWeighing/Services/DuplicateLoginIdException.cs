using System;

namespace ColumbusWeighing.Services
{
    /// <summary>이미 존재하는 로그인 ID로 계정을 추가/수정하려 할 때 던진다(dbo.APP_USER의
    /// UNIQUE 제약 위반을 사용자에게 보여줄 메시지로 변환한 것).</summary>
    public sealed class DuplicateLoginIdException : Exception
    {
        public DuplicateLoginIdException(string loginId)
            : base(string.Format("이미 사용 중인 아이디입니다: {0}", loginId))
        {
        }
    }
}
