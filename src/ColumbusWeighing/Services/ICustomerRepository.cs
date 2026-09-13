using System.ComponentModel;
using ColumbusWeighing.Models;

namespace ColumbusWeighing.Services
{
    /// <summary>거래처 마스터(dbo.CUSTOMER) 조회 저장소. 이 프로그램은 조회 전용이라 조회만 제공한다.</summary>
    public interface ICustomerRepository
    {
        BindingList<CustomerRecord> Records { get; }

        /// <summary>거래처코드/거래처명에 검색어가 포함된 건만 다시 조회한다(빈 문자열이면 전체).</summary>
        void Refresh(string searchText);
    }
}
