using System;
using System.Data;
using ColumbusSync.BranchA.Source;

namespace ColumbusSync.BranchA.Hub
{
    /// <summary>
    /// 통합 허브 DB(COLUMBUS_WEIGH_HUB)에 데이터를 적재하는 writer.
    /// 지점 코드(A)와 원본 코드(SOURCE_CODE/SOURCE_KEY) 조합으로 있으면 갱신, 없으면 삽입한다.
    /// (실제 테이블 정의는 sql/COLUMBUS_WEIGH_HUB_schema.sql 참고)
    /// </summary>
    public class HubWriter
    {
        private const string BranchCode = "A";
        private readonly string _connectionString;

        public HubWriter(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void UpsertCustomer(RawCustomerRow c)
        {
            const string sql = @"
MERGE dbo.CUSTOMER AS target
USING (SELECT @BranchCode AS BRANCH_CODE, @SourceCode AS SOURCE_CODE) AS src
    ON target.BRANCH_CODE = src.BRANCH_CODE AND target.SOURCE_CODE = src.SOURCE_CODE
WHEN MATCHED THEN UPDATE SET
    CUSTOMER_NAME = @CustomerName, CEO_NAME = @CeoName, MANAGER_NAME = @ManagerName,
    TEL = @Tel, FAX = @Fax, ADDRESS = @Address, BIZ_NO = @BizNo, BIZ_TYPE = @BizType,
    BIZ_ITEM = @BizItem, REMARK = @Remark, SYNCED_AT = SYSDATETIME()
WHEN NOT MATCHED THEN INSERT
    (BRANCH_CODE, SOURCE_CODE, CUSTOMER_NAME, CEO_NAME, MANAGER_NAME, TEL, FAX, ADDRESS, BIZ_NO, BIZ_TYPE, BIZ_ITEM, REMARK)
    VALUES (@BranchCode, @SourceCode, @CustomerName, @CeoName, @ManagerName, @Tel, @Fax, @Address, @BizNo, @BizType, @BizItem, @Remark);";

            SqlHelper.ExecuteNonQuery(_connectionString, sql, new[]
            {
                new SqlParam("@BranchCode", BranchCode),
                new SqlParam("@SourceCode", c.CvCod),
                new SqlParam("@CustomerName", c.CvNam),
                new SqlParam("@CeoName", c.Ceo),
                new SqlParam("@ManagerName", c.Damdang),
                new SqlParam("@Tel", c.Tel),
                new SqlParam("@Fax", c.Fax),
                new SqlParam("@Address", c.Addr),
                new SqlParam("@BizNo", c.BussNo),
                new SqlParam("@BizType", c.Uptae),
                new SqlParam("@BizItem", c.Jongmok),
                new SqlParam("@Remark", c.Remark),
            });
        }

        public void UpsertVehicle(RawVehicleRow v)
        {
            const string sql = @"
MERGE dbo.VEHICLE AS target
USING (SELECT @BranchCode AS BRANCH_CODE, @SourceCode AS SOURCE_CODE) AS src
    ON target.BRANCH_CODE = src.BRANCH_CODE AND target.SOURCE_CODE = src.SOURCE_CODE
WHEN MATCHED THEN UPDATE SET
    VEHICLE_NO = @VehicleNo, CARRIER_NAME = @CarrierName, VEHICLE_TYPE = @VehicleType,
    DRIVER_NAME = @DriverName, TARE_WEIGHT = @TareWeight, REMARK = @Remark, SYNCED_AT = SYSDATETIME()
WHEN NOT MATCHED THEN INSERT
    (BRANCH_CODE, SOURCE_CODE, VEHICLE_NO, CARRIER_NAME, VEHICLE_TYPE, DRIVER_NAME, TARE_WEIGHT, REMARK)
    VALUES (@BranchCode, @SourceCode, @VehicleNo, @CarrierName, @VehicleType, @DriverName, @TareWeight, @Remark);";

            SqlHelper.ExecuteNonQuery(_connectionString, sql, new[]
            {
                new SqlParam("@BranchCode", BranchCode),
                new SqlParam("@SourceCode", v.CarNo),
                new SqlParam("@VehicleNo", v.CarNo),
                new SqlParam("@CarrierName", v.CarrierName),
                new SqlParam("@VehicleType", v.VehicleType),
                new SqlParam("@DriverName", v.DriverName),
                new SqlParam("@TareWeight", v.TareWeight, SqlDbType.Decimal),
                new SqlParam("@Remark", v.Remark),
            });
        }

        public void UpsertProduct(RawProductRow p)
        {
            const string sql = @"
MERGE dbo.PRODUCT AS target
USING (SELECT @BranchCode AS BRANCH_CODE, @SourceCode AS SOURCE_CODE) AS src
    ON target.BRANCH_CODE = src.BRANCH_CODE AND target.SOURCE_CODE = src.SOURCE_CODE
WHEN MATCHED THEN UPDATE SET
    PRODUCT_NAME = @ProductName, UNIT = @Unit, UNIT_PRICE = @UnitPrice, REMARK = @Remark, SYNCED_AT = SYSDATETIME()
WHEN NOT MATCHED THEN INSERT
    (BRANCH_CODE, SOURCE_CODE, PRODUCT_NAME, UNIT, UNIT_PRICE, REMARK)
    VALUES (@BranchCode, @SourceCode, @ProductName, @Unit, @UnitPrice, @Remark);";

            SqlHelper.ExecuteNonQuery(_connectionString, sql, new[]
            {
                new SqlParam("@BranchCode", BranchCode),
                new SqlParam("@SourceCode", p.ItCod),
                new SqlParam("@ProductName", p.ItNam),
                new SqlParam("@Unit", p.Unit),
                new SqlParam("@UnitPrice", p.UnitPrice, SqlDbType.Decimal),
                new SqlParam("@Remark", p.Remark),
            });
        }

        public void UpsertWeighRecord(WeighRecordForHub w)
        {
            const string sql = @"
MERGE dbo.WEIGH_RECORD AS target
USING (SELECT @BranchCode AS BRANCH_CODE, @SourceKey AS SOURCE_KEY) AS src
    ON target.BRANCH_CODE = src.BRANCH_CODE AND target.SOURCE_KEY = src.SOURCE_KEY
WHEN MATCHED THEN UPDATE SET
    WEIGH_DATE = @WeighDate, WEIGH_SEQ = @WeighSeq, VEHICLE_NO = @VehicleNo,
    CUSTOMER_SOURCE_CODE = @CustomerSourceCode, CUSTOMER_NAME = @CustomerName,
    PRODUCT_SOURCE_CODE = @ProductSourceCode, PRODUCT_NAME = @ProductName,
    FIRST_DATETIME = @FirstDateTime, FIRST_WEIGHT = @FirstWeight,
    SECOND_DATETIME = @SecondDateTime, SECOND_WEIGHT = @SecondWeight,
    NET_WEIGHT = @NetWeight, LOSS_WEIGHT = @LossWeight, UNIT_PRICE = @UnitPrice,
    IN_OUT_TYPE = @InOutType, IS_COMPLETED = @IsCompleted, WEIGHER_NAME = @WeigherName,
    REMARK = @Remark, SOURCE_RAW_STATUS = @SourceRawStatus, SYNCED_AT = SYSDATETIME()
WHEN NOT MATCHED THEN INSERT
    (BRANCH_CODE, SOURCE_KEY, WEIGH_DATE, WEIGH_SEQ, VEHICLE_NO, CUSTOMER_SOURCE_CODE, CUSTOMER_NAME,
     PRODUCT_SOURCE_CODE, PRODUCT_NAME, FIRST_DATETIME, FIRST_WEIGHT, SECOND_DATETIME, SECOND_WEIGHT,
     NET_WEIGHT, LOSS_WEIGHT, UNIT_PRICE, IN_OUT_TYPE, IS_COMPLETED, WEIGHER_NAME, REMARK, SOURCE_RAW_STATUS)
    VALUES
    (@BranchCode, @SourceKey, @WeighDate, @WeighSeq, @VehicleNo, @CustomerSourceCode, @CustomerName,
     @ProductSourceCode, @ProductName, @FirstDateTime, @FirstWeight, @SecondDateTime, @SecondWeight,
     @NetWeight, @LossWeight, @UnitPrice, @InOutType, @IsCompleted, @WeigherName, @Remark, @SourceRawStatus);";

            SqlHelper.ExecuteNonQuery(_connectionString, sql, new[]
            {
                new SqlParam("@BranchCode", BranchCode),
                new SqlParam("@SourceKey", w.SourceKey),
                new SqlParam("@WeighDate", w.WeighDate, SqlDbType.Date),
                new SqlParam("@WeighSeq", w.WeighSeq, SqlDbType.Int),
                new SqlParam("@VehicleNo", w.VehicleNo),
                new SqlParam("@CustomerSourceCode", w.CustomerSourceCode),
                new SqlParam("@CustomerName", w.CustomerName),
                new SqlParam("@ProductSourceCode", w.ProductSourceCode),
                new SqlParam("@ProductName", w.ProductName),
                new SqlParam("@FirstDateTime", w.FirstDateTime, SqlDbType.DateTime2),
                new SqlParam("@FirstWeight", w.FirstWeight, SqlDbType.Decimal),
                new SqlParam("@SecondDateTime", w.SecondDateTime, SqlDbType.DateTime2),
                new SqlParam("@SecondWeight", w.SecondWeight, SqlDbType.Decimal),
                new SqlParam("@NetWeight", w.NetWeight, SqlDbType.Decimal),
                new SqlParam("@LossWeight", w.LossWeight, SqlDbType.Decimal),
                new SqlParam("@UnitPrice", w.UnitPrice, SqlDbType.Decimal),
                new SqlParam("@InOutType", w.InOutType),
                new SqlParam("@IsCompleted", w.IsCompleted, SqlDbType.Bit),
                new SqlParam("@WeigherName", w.WeigherName),
                new SqlParam("@Remark", w.Remark),
                new SqlParam("@SourceRawStatus", w.SourceRawStatus),
            });
        }

        /// <summary>MES 로그인 계정(zUSRLST) 1건을 dbo.APP_USER에 적재한다. B/C지점과 마찬가지로
        /// LOGIN_ID만으로 매칭한다(로그인 ID는 지점과 무관하게 전체에서 유일해야 함 -
        /// UQ_APP_USER_LOGIN_ID). MES는 비밀번호를 HASHBYTES('SHA2_256', 평문)로만 저장하고
        /// 평문을 알 방법이 없으므로, PBKDF2로 재해시하지 않고 그 SHA-256 해시 바이트를 그대로
        /// 복사해서 저장한다. PASSWORD_ALGORITHM = 'SHA256'으로 표시해두면 로그인 시
        /// SqlAuthenticationService가 PBKDF2 대신 같은 방식(HASHBYTES SHA2_256)으로 검증한다.
        /// 이후 ColumbusWeighing 관리자가 이 계정 비밀번호를 직접 재설정하면, 그때부터는
        /// PBKDF2로 전환된다(SqlUserRepository 참고).</summary>
        public void UpsertUser(RawUserRow u)
        {
            const string sql = @"
MERGE dbo.APP_USER AS target
USING (SELECT @LoginId AS LOGIN_ID) AS src
    ON target.LOGIN_ID = src.LOGIN_ID
WHEN MATCHED THEN UPDATE SET
    BRANCH_CODE = @BranchCode, DISPLAY_NAME = @DisplayName, PHONE = @Phone, REMARK = @Remark,
    PASSWORD_HASH = @PasswordHash, PASSWORD_SALT = '', PASSWORD_ALGORITHM = 'SHA256',
    MODIFIED_BY = @ModifiedBy, MODIFIED_AT = SYSDATETIME()
WHEN NOT MATCHED THEN INSERT
    (BRANCH_CODE, LOGIN_ID, DISPLAY_NAME, PHONE, REMARK, CAN_PRINT, CAN_EDIT, CAN_DELETE, IS_ADMIN,
     PASSWORD_HASH, PASSWORD_SALT, PASSWORD_ALGORITHM, MODIFIED_BY)
    VALUES
    (@BranchCode, @LoginId, @DisplayName, @Phone, @Remark, 0, 0, 0, 0,
     @PasswordHash, '', 'SHA256', @ModifiedBy);";

            SqlHelper.ExecuteNonQuery(_connectionString, sql, new[]
            {
                new SqlParam("@BranchCode", BranchCode),
                new SqlParam("@LoginId", u.LoginId),
                new SqlParam("@DisplayName", u.DisplayName),
                new SqlParam("@Phone", u.Phone),
                new SqlParam("@Remark", u.Remark),
                new SqlParam("@PasswordHash", Convert.ToBase64String(u.PasswordHashSha256)),
                new SqlParam("@ModifiedBy", "sync:" + BranchCode),
            });
        }

        public void WriteSyncLog(DateTime startedAt, DateTime finishedAt, bool success, int inserted, int updated, string errorMessage)
        {
            const string sql = @"
INSERT INTO dbo.SYNC_LOG (BRANCH_CODE, STARTED_AT, FINISHED_AT, STATUS, INSERTED_COUNT, UPDATED_COUNT, ERROR_MESSAGE)
VALUES (@BranchCode, @StartedAt, @FinishedAt, @Status, @Inserted, @Updated, @ErrorMessage);";

            SqlHelper.ExecuteNonQuery(_connectionString, sql, new[]
            {
                new SqlParam("@BranchCode", BranchCode),
                new SqlParam("@StartedAt", startedAt, SqlDbType.DateTime2),
                new SqlParam("@FinishedAt", finishedAt, SqlDbType.DateTime2),
                new SqlParam("@Status", success ? "SUCCESS" : "FAILED"),
                new SqlParam("@Inserted", inserted, SqlDbType.Int),
                new SqlParam("@Updated", updated, SqlDbType.Int),
                new SqlParam("@ErrorMessage", (object)errorMessage ?? DBNull.Value),
            });
        }
    }

    /// <summary>통합 규칙(IN_OUT_TYPE/IS_COMPLETED 재계산 등)까지 적용을 마친, 적재 직전 상태의 계근 레코드.</summary>
    public class WeighRecordForHub
    {
        public string SourceKey { get; set; }
        public DateTime WeighDate { get; set; }
        public int? WeighSeq { get; set; }
        public string VehicleNo { get; set; }
        public string CustomerSourceCode { get; set; }
        public string CustomerName { get; set; }
        public string ProductSourceCode { get; set; }
        public string ProductName { get; set; }
        public DateTime? FirstDateTime { get; set; }
        public decimal? FirstWeight { get; set; }
        public DateTime? SecondDateTime { get; set; }
        public decimal? SecondWeight { get; set; }
        public decimal? NetWeight { get; set; }
        public decimal? LossWeight { get; set; }
        public decimal? UnitPrice { get; set; }
        public string InOutType { get; set; }
        public bool IsCompleted { get; set; }
        public string WeigherName { get; set; }
        public string Remark { get; set; }
        public string SourceRawStatus { get; set; }
    }
}
