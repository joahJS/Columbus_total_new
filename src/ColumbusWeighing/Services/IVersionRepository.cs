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

        /// <summary>지정한 버전 이력을 실행 파일 원본(FileData)까지 포함해서 가져온다.
        /// 자동 업데이트에서 실제로 받아 적용할 때만 쓴다 - 목록 조회(Refresh)는 용량이 큰
        /// FileData를 아예 가져오지 않는다.</summary>
        VersionRecord GetVersionWithFileData(int id);
    }
}
