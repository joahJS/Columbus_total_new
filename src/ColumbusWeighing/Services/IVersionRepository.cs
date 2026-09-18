using System;
using System.ComponentModel;
using ColumbusWeighing.Models;

namespace ColumbusWeighing.Services
{
    /// <summary>
    /// 프로그램 버전 이력 저장소(dbo.PROGRAM_VERSION). 화면(GridControl)은 Records 컬렉션에
    /// 직접 바인딩되어 Refresh() 후 즉시 반영된다.
    /// </summary>
    public interface IVersionRepository
    {
        BindingList<VersionRecord> Records { get; }

        /// <summary>DB에서 버전 이력 목록을 다시 조회한다.</summary>
        void Refresh();

        /// <summary>새 버전 이력을 등록한다.</summary>
        VersionRecord AddVersion(
            string versionId,
            DateTime uploadDate,
            string fileName,
            byte[] fileData,
            string remark,
            string uploadedBy);
    }
}
