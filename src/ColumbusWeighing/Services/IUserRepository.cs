using System.ComponentModel;
using ColumbusWeighing.Models;

namespace ColumbusWeighing.Services
{
    /// <summary>사용자(로그인 계정) 조회 저장소. 이 프로그램은 조회 전용이라 조회만 제공한다.</summary>
    public interface IUserRepository
    {
        BindingList<UserAccount> Records { get; }

        /// <summary>ID/사용자명에 검색어가 포함된 건만 다시 조회한다(빈 문자열이면 전체).</summary>
        void Refresh(string searchText);
    }
}
