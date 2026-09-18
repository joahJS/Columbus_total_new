/*
    COLUMBUS_WEIGH_HUB - 프로그램 버전 이력(dbo.PROGRAM_VERSION) 테이블 추가
    ------------------------------------------------------------------
    - COLUMBUS_WEIGH_HUB_schema.sql로 이미 구축된 DB에 추가로 실행하는 증분 스크립트입니다.
    - 배경: 지금까지 "버전관리" 화면(VersionManagementForm)이 InMemoryVersionRepository를
      써서, 등록한 버전 이력(업로드한 실행 파일 포함)이 DB가 아니라 프로그램 메모리에만
      있었습니다. 그래서 프로그램을 껐다 켜면 방금 등록한 버전이 그대로 사라지는 문제가
      있었습니다("업로드한 내용이 계속 사라진다"). 이 테이블에 실제로 저장하도록 바꿉니다.
    - 실행 파일 원본(FILE_DATA)도 그대로 DB에 저장해서, 다른 PC에서 이 화면을 열어도 같은
      이력을 보고 받을 수 있게 합니다.
    - 실행 전 담당자 검토를 거쳐주세요.
*/

USE COLUMBUS_WEIGH_HUB;
GO

IF OBJECT_ID('dbo.PROGRAM_VERSION') IS NULL
BEGIN
    CREATE TABLE dbo.PROGRAM_VERSION
    (
        PROGRAM_VERSION_ID INT IDENTITY(1,1) PRIMARY KEY,
        VERSION_NO          NVARCHAR(50)    NOT NULL,   -- 예: 1.1.10
        UPLOAD_DATE         DATETIME2       NOT NULL,
        FILE_NAME           NVARCHAR(255)   NOT NULL,
        FILE_SIZE           BIGINT          NOT NULL,
        FILE_DATA           VARBINARY(MAX)  NOT NULL,
        REMARK              NVARCHAR(1000)  NULL,
        UPLOADED_BY         NVARCHAR(50)    NULL
    );
END
GO
