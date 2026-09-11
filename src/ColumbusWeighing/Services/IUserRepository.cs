using System.ComponentModel;
using ColumbusWeighing.Models;

namespace ColumbusWeighing.Services
{
    /// <summary>사용자(로그인 계정) 저장소.</summary>
    public interface IUserRepository
    {
        BindingList<UserAccount> Records { get; }

        /// <summary>ID/사용자명에 검색어가 포함된 건만 다시 조회한다(빈 문자열이면 전체).</summary>
        void Refresh(string searchText);

        /// <summary>새 계정을 추가한다. 이미 있는 로그인 ID면 DuplicateLoginIdException을 던진다.</summary>
        void Add(UserAccount account, string password, string modifiedBy);

        /// <summary>기존 계정 정보를 수정한다. newPassword가 비어 있으면 비밀번호는 바꾸지 않는다.
        /// 마지막 관리자 계정의 관리자 권한을 해제하려 하면 InvalidOperationException을 던진다.</summary>
        void Update(UserAccount account, string newPassword, string modifiedBy);

        /// <summary>계정을 삭제한다. 마지막 관리자 계정을 삭제하려 하면 InvalidOperationException을 던진다.</summary>
        void Delete(int userId);
    }
}
