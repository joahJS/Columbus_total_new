using System;

namespace ColumbusWeighing.Models
{
    /// <summary>
    /// 프로그램 배포 버전 1건의 정보. 버전관리 화면(그리드)에 표시되는 단위.
    /// VisionIns 솔루션의 SY000(버전관리) 테이블과 동일한 항목 구성이다.
    /// </summary>
    public class VersionRecord
    {
        public int Id { get; set; }

        /// <summary>버전 번호(예: 1.1.10).</summary>
        public string VersionId { get; set; }

        public DateTime UploadDate { get; set; }

        public string FileName { get; set; }

        /// <summary>업로드된 파일 크기(byte).</summary>
        public long FileSize { get; set; }

        public string Remark { get; set; }

        /// <summary>업로드된 실행 파일의 원본 바이트.</summary>
        public byte[] FileData { get; set; }

        public string UploadedBy { get; set; }
    }
}
