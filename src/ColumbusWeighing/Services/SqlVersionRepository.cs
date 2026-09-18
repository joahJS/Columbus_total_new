using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using ColumbusWeighing.ComnLib;
using ColumbusWeighing.Models;

namespace ColumbusWeighing.Services
{
    /// <summary>
    /// 통합 허브 DB(COLUMBUS_WEIGH_HUB)의 dbo.PROGRAM_VERSION 테이블에 버전 이력을 저장하는
    /// 실제 구현체. 이전에 쓰던 InMemoryVersionRepository는 프로세스 메모리에만 값을 들고
    /// 있어서 프로그램을 재시작하면 등록한 버전이 그대로 사라지는 문제가 있었다 - 그 문제를
    /// 고치기 위한 구현체다. 업로드된 실행 파일 원본(FILE_DATA)도 그대로 DB에 저장한다.
    /// </summary>
    public sealed class SqlVersionRepository : IVersionRepository
    {
        private const string SelectSql = @"
SELECT PROGRAM_VERSION_ID, VERSION_NO, UPLOAD_DATE, FILE_NAME, FILE_SIZE, REMARK, UPLOADED_BY
FROM dbo.PROGRAM_VERSION
ORDER BY PROGRAM_VERSION_ID DESC";

        public BindingList<VersionRecord> Records { get; } = new BindingList<VersionRecord>();

        public SqlVersionRepository()
        {
            // VS 디자이너가 폼을 디자인 타임에 로드할 때도 이 생성자가 호출되는데, 그 시점에
            // 실제 DB 접속을 시도하면 디자이너가 멈추거나 오류가 날 수 있어 건너뛴다.
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            Refresh();
        }

        public void Refresh()
        {
            DataTable table;
            try
            {
                table = DBConn.GetDataTable(SelectSql, new List<Parameter>(), CommandType.Text);
            }
            catch (SqlException ex)
            {
                ComnFunc.gp_PrintMessage(
                    "통합 허브 DB 조회에 실패했습니다. 네트워크/DB 접속 정보를 확인해주세요.\r\n" + ex.Message,
                    "DB 조회 오류", MessageType.오류);
                return;
            }

            Records.RaiseListChangedEvents = false;
            Records.Clear();
            foreach (DataRow row in table.Rows)
            {
                Records.Add(ToRecord(row));
            }

            Records.RaiseListChangedEvents = true;
            Records.ResetBindings();
        }

        // 목록 조회에는 실행 파일 원본(FILE_DATA, VARBINARY(MAX))이 필요 없어 SelectSql에서부터
        // 빼뒀다 - 매번 전체를 읽으면 느려지기 때문이다. 그래서 여기서 만드는 레코드의
        // FileData는 항상 null이다(다운로드 기능이 생기면 그때 ID로 개별 조회하면 된다).
        private static VersionRecord ToRecord(DataRow row)
        {
            return new VersionRecord
            {
                Id = Convert.ToInt32(row["PROGRAM_VERSION_ID"]),
                VersionId = row["VERSION_NO"].ToString(),
                UploadDate = Convert.ToDateTime(row["UPLOAD_DATE"]),
                FileName = row["FILE_NAME"].ToString(),
                FileSize = Convert.ToInt64(row["FILE_SIZE"]),
                Remark = row["REMARK"] == DBNull.Value ? null : row["REMARK"].ToString(),
                UploadedBy = row["UPLOADED_BY"] == DBNull.Value ? null : row["UPLOADED_BY"].ToString(),
            };
        }

        public VersionRecord AddVersion(
            string versionId,
            DateTime uploadDate,
            string fileName,
            byte[] fileData,
            string remark,
            string uploadedBy)
        {
            const string sql = @"
INSERT INTO dbo.PROGRAM_VERSION
    (VERSION_NO, UPLOAD_DATE, FILE_NAME, FILE_SIZE, FILE_DATA, REMARK, UPLOADED_BY)
VALUES
    (@VersionNo, @UploadDate, @FileName, @FileSize, @FileData, @Remark, @UploadedBy)";

            DBConn.ExecuteNonQuery(sql, new List<Parameter>
            {
                new Parameter("VersionNo", versionId),
                new Parameter("UploadDate", uploadDate, SqlDbType.DateTime2),
                new Parameter("FileName", fileName),
                new Parameter("FileSize", fileData?.LongLength ?? 0, SqlDbType.BigInt),
                new Parameter("FileData", fileData, SqlDbType.VarBinary),
                new Parameter("Remark", remark),
                new Parameter("UploadedBy", uploadedBy),
            }, CommandType.Text);

            return new VersionRecord
            {
                VersionId = versionId,
                UploadDate = uploadDate,
                FileName = fileName,
                FileSize = fileData?.LongLength ?? 0,
                FileData = fileData,
                Remark = remark,
                UploadedBy = uploadedBy,
            };
        }
    }
}
