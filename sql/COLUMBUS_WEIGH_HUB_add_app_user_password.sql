/*
    COLUMBUS_WEIGH_HUB - 계정당 비밀번호 여러 개 허용 (dbo.APP_USER_PASSWORD)
    ------------------------------------------------------------------
    - COLUMBUS_WEIGH_HUB_add_app_user.sql로 이미 만들어진 dbo.APP_USER에 추가로 실행하는
      증분 스크립트입니다.
    - 배경: 지점 mdb/MES에서 계정을 동기화해오다 보면, 이미 있는 아이디(예: 기존 admin)와
      지점 쪽 아이디가 우연히 같아서 서로 다른 비밀번호를 쓰던 두 계정이 하나로 합쳐지는
      경우가 있습니다. 이때 둘 중 하나의 비밀번호만 남기지 않고, 두 비밀번호 모두로 로그인이
      되게 해달라는 요청에 따라 이 테이블을 추가합니다.
    - dbo.APP_USER.PASSWORD_HASH/PASSWORD_SALT/PASSWORD_ALGORITHM은 지금까지처럼 "주 비밀번호"
      역할을 그대로 유지합니다 (관리자가 UserManagementForm에서 비밀번호를 바꾸거나, 동기화
      프로그램이 지점 계정을 갱신할 때 계속 이 컬럼만 다룹니다 - 기존 코드 변경 없음).
    - dbo.APP_USER_PASSWORD는 "그 외에도 유효한 과거/다른 비밀번호"를 추가로 보관하는
      테이블입니다. 로그인 시 주 비밀번호가 안 맞으면 이 테이블도 확인합니다
      (SqlAuthenticationService.TryLogin 참고).
    - 실행 전 담당자 검토를 거쳐주세요.
*/

USE COLUMBUS_WEIGH_HUB;
GO

IF OBJECT_ID('dbo.APP_USER_PASSWORD') IS NULL
BEGIN
    CREATE TABLE dbo.APP_USER_PASSWORD
    (
        PASSWORD_ID         INT IDENTITY(1,1) PRIMARY KEY,
        USER_ID             INT             NOT NULL REFERENCES dbo.APP_USER (USER_ID) ON DELETE CASCADE,
        PASSWORD_HASH       NVARCHAR(200)   NOT NULL,
        PASSWORD_SALT       NVARCHAR(100)   NOT NULL,
        PASSWORD_ALGORITHM  NVARCHAR(20)    NOT NULL DEFAULT 'PBKDF2',
        REMARK              NVARCHAR(200)   NULL,
        CREATED_BY          NVARCHAR(50)    NULL,
        CREATED_AT          DATETIME2       NOT NULL DEFAULT SYSDATETIME()
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = 'IX_APP_USER_PASSWORD_USER_ID' AND object_id = OBJECT_ID('dbo.APP_USER_PASSWORD')
)
BEGIN
    CREATE INDEX IX_APP_USER_PASSWORD_USER_ID ON dbo.APP_USER_PASSWORD (USER_ID);
END
GO
