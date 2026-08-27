/*
    COLUMBUS_WEIGH_HUB - 통합 계량 데이터 허브 DB 초안 DDL
    ------------------------------------------------------------------
    - 대상 서버: 소나무 개발서버 (121.66.17.30:16433) 예정
    - A지점(Columbus_total/MES, sql16ssd-006.localnet.kr,1433 / columbusdb_pineit)과
      B/C지점(TSDB.mdb) 데이터를 하나의 구조로 흡수하기 위한 통합 스키마 초안입니다.
    - 지점별 원본 키/코드 체계가 서로 달라 그대로 매핑할 수 없으므로,
      모든 테이블에 BRANCH_CODE + SOURCE_CODE(원본 키/코드)를 남겨 추적 가능하게 했습니다.
    - 화면 로직에 필요한 IN_OUT_TYPE / IS_COMPLETED 등은 원본 값을 그대로 믿지 않고
      동기화 잡에서 통합 규칙으로 재계산한 값을 저장합니다. (자세한 배경은
      "지점별 데이터 차이점 정리" 문서 참고)

    실행 순서: 이 스크립트는 초안이므로, 실제 실행 전 담당자 검토를 거쳐주세요.
*/

IF DB_ID(N'COLUMBUS_WEIGH_HUB') IS NULL
BEGIN
    RAISERROR(N'COLUMBUS_WEIGH_HUB 데이터베이스를 먼저 생성한 뒤 그 안에서 이 스크립트를 실행하세요.', 16, 1);
    RETURN;
END
GO

USE COLUMBUS_WEIGH_HUB;
GO

------------------------------------------------------------
-- 1. 지점 마스터
------------------------------------------------------------
CREATE TABLE dbo.BRANCH
(
    BRANCH_CODE     CHAR(1)         NOT NULL PRIMARY KEY,   -- 'A' / 'B' / 'C'
    BRANCH_NAME     NVARCHAR(50)    NOT NULL,
    SOURCE_TYPE     NVARCHAR(20)    NOT NULL,               -- 'MES_SQL' / 'ACCESS_MDB'
    REMARK          NVARCHAR(200)   NULL
);
GO

INSERT INTO dbo.BRANCH (BRANCH_CODE, BRANCH_NAME, SOURCE_TYPE, REMARK) VALUES
    (N'A', N'A지점', N'MES_SQL',    N'Columbus_total(MES) - sql16ssd-006.localnet.kr,1433 / columbusdb_pineit'),
    (N'B', N'B지점', N'ACCESS_MDB', N'TSDB.mdb - 별도 PC, 10분 주기 동기화'),
    (N'C', N'C지점', N'ACCESS_MDB', N'TSDB.mdb - 별도 PC, 10분 주기 동기화');
GO

------------------------------------------------------------
-- 2. 거래처 마스터 (mdb TB_CUSTO / MES CVCOD·CVNAM 통합)
------------------------------------------------------------
CREATE TABLE dbo.CUSTOMER
(
    CUSTOMER_ID     BIGINT IDENTITY(1,1) PRIMARY KEY,
    BRANCH_CODE     CHAR(1)         NOT NULL REFERENCES dbo.BRANCH(BRANCH_CODE),
    SOURCE_CODE     NVARCHAR(30)    NOT NULL,   -- mdb: CUSTONO / MES: CVCOD
    CUSTOMER_NAME   NVARCHAR(100)   NOT NULL,   -- mdb: CUSTONM / MES: CVNAM
    CEO_NAME        NVARCHAR(50)    NULL,
    MANAGER_NAME    NVARCHAR(50)    NULL,       -- DAMDANG
    TEL             NVARCHAR(50)    NULL,
    FAX             NVARCHAR(50)    NULL,
    ADDRESS         NVARCHAR(200)   NULL,
    BIZ_NO          NVARCHAR(50)    NULL,       -- 사업자번호(BUSSNO)
    BIZ_TYPE        NVARCHAR(100)   NULL,       -- 업태(UPTAE)
    BIZ_ITEM        NVARCHAR(100)   NULL,       -- 종목(JONGMOK)
    REMARK          NVARCHAR(200)   NULL,
    SYNCED_AT       DATETIME2       NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT UQ_CUSTOMER_SOURCE UNIQUE (BRANCH_CODE, SOURCE_CODE)
);
GO

------------------------------------------------------------
-- 3. 차량 마스터 (mdb TB_CAR / MES CM009 차량관리 통합)
------------------------------------------------------------
CREATE TABLE dbo.VEHICLE
(
    VEHICLE_ID      BIGINT IDENTITY(1,1) PRIMARY KEY,
    BRANCH_CODE     CHAR(1)         NOT NULL REFERENCES dbo.BRANCH(BRANCH_CODE),
    SOURCE_CODE     NVARCHAR(30)    NOT NULL,   -- CARNO (지점 내에서 유일)
    VEHICLE_NO      NVARCHAR(30)    NOT NULL,
    CARRIER_NAME    NVARCHAR(100)   NULL,       -- mdb: CARSONM(운송사) / MES: 연결 거래처명
    VEHICLE_TYPE    NVARCHAR(50)    NULL,       -- CARTYPE
    DRIVER_NAME     NVARCHAR(50)    NULL,
    TARE_WEIGHT     DECIMAL(15,3)   NULL,       -- 공차중량(EMPTWT), 1회계량에 사용
    REMARK          NVARCHAR(200)   NULL,
    SYNCED_AT       DATETIME2       NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT UQ_VEHICLE_SOURCE UNIQUE (BRANCH_CODE, SOURCE_CODE)
);
GO

------------------------------------------------------------
-- 4. 제품 마스터 (mdb TB_PUM / MES 품목관리 통합)
------------------------------------------------------------
CREATE TABLE dbo.PRODUCT
(
    PRODUCT_ID      BIGINT IDENTITY(1,1) PRIMARY KEY,
    BRANCH_CODE     CHAR(1)         NOT NULL REFERENCES dbo.BRANCH(BRANCH_CODE),
    SOURCE_CODE     NVARCHAR(30)    NOT NULL,   -- mdb: PUMNO / MES: ITCOD
    PRODUCT_NAME    NVARCHAR(100)   NOT NULL,   -- PUMNM / ITNAM
    UNIT            NVARCHAR(50)    NULL,       -- UNIT1
    UNIT_PRICE      DECIMAL(15,3)   NULL,       -- DANKA (품목 기본단가, 전표단가와 별개)
    LOSS_WEIGHT     DECIMAL(15,3)   NULL,       -- LossWt
    LOSS_RATE       DECIMAL(15,3)   NULL,       -- LossPro
    REMARK          NVARCHAR(200)   NULL,
    SYNCED_AT       DATETIME2       NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT UQ_PRODUCT_SOURCE UNIQUE (BRANCH_CODE, SOURCE_CODE)
);
GO

------------------------------------------------------------
-- 5. 계량(계근) 원장 - 통합 스키마의 핵심 테이블
--    mdb TB_WEIGH / MES MEASURE(SLINO) 통합
------------------------------------------------------------
CREATE TABLE dbo.WEIGH_RECORD
(
    WEIGH_ID                BIGINT IDENTITY(1,1) PRIMARY KEY,
    BRANCH_CODE             CHAR(1)         NOT NULL REFERENCES dbo.BRANCH(BRANCH_CODE),
    SOURCE_KEY              NVARCHAR(30)    NOT NULL,   -- mdb: SENO(정수) / MES: SLINO(전표번호) 원본 그대로 보존
    WEIGH_DATE              DATE            NOT NULL,   -- mdb: DATE1 / MES: TDATE
    WEIGH_SEQ               INT             NULL,       -- mdb: BUNHO / MES: SEQNO (당일 순번)

    VEHICLE_NO              NVARCHAR(30)    NULL,
    CUSTOMER_SOURCE_CODE    NVARCHAR(30)    NULL,       -- CUSTOMER.SOURCE_CODE 참조용(느슨한 연결, FK 미설정)
    CUSTOMER_NAME           NVARCHAR(100)   NULL,       -- 코드 매핑 전에도 화면에 표시 가능하도록 이름 스냅샷 보관
    PRODUCT_SOURCE_CODE     NVARCHAR(30)    NULL,
    PRODUCT_NAME            NVARCHAR(100)   NULL,

    FIRST_DATETIME          DATETIME2       NULL,       -- mdb: DATE1+TIME1 / MES: TDATE+FTIME
    FIRST_WEIGHT            DECIMAL(15,3)   NULL,       -- mdb: OVTOTWT / MES: FWEIT
    SECOND_DATETIME         DATETIME2       NULL,
    SECOND_WEIGHT           DECIMAL(15,3)   NULL,       -- mdb: OVNETWT / MES: SWEIT

    NET_WEIGHT              DECIMAL(15,3)   NULL,       -- 재계산: ABS(FIRST_WEIGHT - SECOND_WEIGHT)
    LOSS_WEIGHT             DECIMAL(15,3)   NULL,       -- mdb: LOSSWT / MES: LWEIT
    UNIT_PRICE              DECIMAL(15,3)   NULL,       -- mdb: DANKA / MES: UCOST

    IN_OUT_TYPE             CHAR(1)         NULL,       -- 'I'/'O' - 재계산값. 원본 INOUT/JOBGU를 그대로 믿지 않음
    IS_COMPLETED            BIT             NOT NULL DEFAULT 0,  -- 재계산: 2차중량 존재 여부

    WEIGHER_NAME            NVARCHAR(50)    NULL,       -- mdb: DAMDANG / MES: 로그인 사용자
    REMARK                  NVARCHAR(200)   NULL,
    SOURCE_RAW_STATUS       NVARCHAR(20)    NULL,       -- 원본 상태값 원문 보존 (mdb: WEIGH_STS, MES: CHKYN 등 참고용)

    SYNCED_AT               DATETIME2       NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT UQ_WEIGH_SOURCE UNIQUE (BRANCH_CODE, SOURCE_KEY)
);
GO

CREATE INDEX IX_WEIGH_RECORD_DATE ON dbo.WEIGH_RECORD (WEIGH_DATE, BRANCH_CODE);
GO

------------------------------------------------------------
-- 6. 지점 간 거래처/차량/제품 코드 매핑 (선택, 필요 시 사용)
--    동일 거래처가 지점마다 다른 코드로 등록된 경우, 통합 조회를 위해
--    사람이 한 번 확인 후 매핑을 등록해두는 보조 테이블입니다.
------------------------------------------------------------
CREATE TABLE dbo.CODE_MAP
(
    CODE_MAP_ID     BIGINT IDENTITY(1,1) PRIMARY KEY,
    ENTITY_TYPE     NVARCHAR(20)    NOT NULL,   -- 'CUSTOMER' / 'VEHICLE' / 'PRODUCT'
    BRANCH_CODE     CHAR(1)         NOT NULL REFERENCES dbo.BRANCH(BRANCH_CODE),
    SOURCE_CODE     NVARCHAR(30)    NOT NULL,
    UNIFIED_KEY     NVARCHAR(30)    NOT NULL,   -- 지점 무관 통합 식별자(수기 지정)
    CONSTRAINT UQ_CODE_MAP UNIQUE (ENTITY_TYPE, BRANCH_CODE, SOURCE_CODE)
);
GO

------------------------------------------------------------
-- 7. 동기화 실행 로그 - 지점별 동기화 잡이 매 배치 실행마다 기록
------------------------------------------------------------
CREATE TABLE dbo.SYNC_LOG
(
    SYNC_LOG_ID     BIGINT IDENTITY(1,1) PRIMARY KEY,
    BRANCH_CODE     CHAR(1)         NOT NULL,
    STARTED_AT      DATETIME2       NOT NULL,
    FINISHED_AT     DATETIME2       NULL,
    STATUS          NVARCHAR(20)    NOT NULL,   -- 'SUCCESS' / 'FAILED'
    INSERTED_COUNT  INT             NULL,
    UPDATED_COUNT   INT             NULL,
    ERROR_MESSAGE   NVARCHAR(MAX)   NULL
);
GO
