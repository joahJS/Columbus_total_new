/*
    COLUMBUS_WEIGH_HUB - 사용자(로그인 계정) 테이블 추가
    ------------------------------------------------------------------
    - COLUMBUS_WEIGH_HUB_schema.sql로 이미 구축된 DB에 추가로 실행하는 증분 스크립트입니다
      (기존 스크립트를 다시 실행하면 이미 있는 테이블 때문에 오류가 나므로 이 파일만 따로 둡니다).
    - 지금까지 ColumbusWeighing은 admin/1234 고정 계정 하나로만 로그인했는데, 이 테이블을
      추가하면서 실제 계정 기반 로그인(SqlAuthenticationService)으로 바뀝니다.
    - 비밀번호는 평문으로 저장하지 않고 PBKDF2(HMAC-SHA1, 10000회) 해시 + 솔트로 저장합니다.
      아래 admin 계정 시드 값은 비밀번호 "1234"를 위 방식으로 미리 해시한 값입니다
      (ColumbusWeighing.ComnLib.PasswordHasher와 동일한 알고리즘/반복 횟수로 계산했습니다).
    - 실행 전 담당자 검토를 거쳐주세요.
*/

USE COLUMBUS_WEIGH_HUB;
GO

------------------------------------------------------------
-- 8. 사용자(로그인 계정) 마스터 - ColumbusWeighing 로그인/권한 관리
------------------------------------------------------------
IF OBJECT_ID('dbo.APP_USER') IS NULL
BEGIN
    CREATE TABLE dbo.APP_USER
    (
        USER_ID         INT IDENTITY(1,1) PRIMARY KEY,
        LOGIN_ID        NVARCHAR(50)    NOT NULL,
        DISPLAY_NAME    NVARCHAR(50)    NOT NULL,
        PHONE           NVARCHAR(30)    NULL,
        REMARK          NVARCHAR(200)   NULL,
        CAN_PRINT       BIT             NOT NULL DEFAULT 0,
        CAN_EDIT        BIT             NOT NULL DEFAULT 0,
        CAN_DELETE      BIT             NOT NULL DEFAULT 0,
        IS_ADMIN        BIT             NOT NULL DEFAULT 0,
        PASSWORD_HASH   NVARCHAR(200)   NOT NULL,
        PASSWORD_SALT   NVARCHAR(100)   NOT NULL,
        MODIFIED_BY     NVARCHAR(50)    NULL,
        MODIFIED_AT     DATETIME2       NOT NULL DEFAULT SYSDATETIME(),
        CONSTRAINT UQ_APP_USER_LOGIN_ID UNIQUE (LOGIN_ID)
    );
END
GO

-- 기존 고정 계정(admin/1234)을 그대로 시드해, 이 스크립트 실행 뒤에도 로그인이 끊기지 않게 한다.
IF NOT EXISTS (SELECT 1 FROM dbo.APP_USER WHERE LOGIN_ID = N'admin')
BEGIN
    INSERT INTO dbo.APP_USER
        (LOGIN_ID, DISPLAY_NAME, PHONE, REMARK, CAN_PRINT, CAN_EDIT, CAN_DELETE, IS_ADMIN,
         PASSWORD_HASH, PASSWORD_SALT, MODIFIED_BY)
    VALUES
        (N'admin', N'관리자', NULL, N'초기 관리자 계정(비밀번호: 1234, 최초 로그인 후 변경 권장)',
         1, 1, 1, 1,
         N'IUO0r5DITbs3uDYdz/+N83mUbHKInP9nSGyjNGNFqR0=', N'jYnCCJghs1ZyzWUWzIwwLw==', N'system');
END
GO
