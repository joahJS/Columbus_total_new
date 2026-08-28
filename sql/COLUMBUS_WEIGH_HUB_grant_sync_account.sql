/*
    COLUMBUS_WEIGH_HUB - 동기화 전용 계정(pineit_ex) 권한 부여
    ------------------------------------------------------------------
    - COLUMBUS_WEIGH_HUB_schema.sql 실행(테이블 생성)이 끝난 뒤, DB 관리자 계정으로
      이 스크립트를 실행하세요. 동기화 전용 계정(pineit_ex) 자체로는 실행할 수 없습니다
      (권한을 받는 쪽이라 아직 권한이 없는 상태이기 때문).
    - ColumbusSync.BranchA(및 이후 만들어질 B/C지점 동기화 프로그램)는 이 계정으로
      COLUMBUS_WEIGH_HUB에 접속해 upsert(있으면 갱신, 없으면 삽입)만 수행합니다.
    - DELETE 권한은 의도적으로 부여하지 않았습니다. 동기화 잡이 실수로 데이터를
      지우는 사고를 원천적으로 막기 위함입니다. 데이터 삭제가 필요하면 관리자가
      직접 수행하세요.
    - 로그인(LOGIN) 자체는 이미 서버에 만들어져 있다는 전제입니다(pineit_ex).
      혹시 로그인 자체가 없다면 아래 CREATE LOGIN 블록의 주석을 해제하고
      비밀번호를 채운 뒤 먼저 실행하세요.
*/

USE COLUMBUS_WEIGH_HUB;
GO

-- 서버에 로그인이 아직 없다면 아래 주석을 해제하고 실행하세요 (비밀번호는 직접 채워넣을 것).
-- CREATE LOGIN [pineit_ex] WITH PASSWORD = N'__CHANGE_ME__', CHECK_POLICY = ON;
-- GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'pineit_ex')
BEGIN
    CREATE USER [pineit_ex] FOR LOGIN [pineit_ex];
END
GO

GRANT SELECT, INSERT, UPDATE ON dbo.BRANCH        TO [pineit_ex];
GRANT SELECT, INSERT, UPDATE ON dbo.CUSTOMER      TO [pineit_ex];
GRANT SELECT, INSERT, UPDATE ON dbo.VEHICLE       TO [pineit_ex];
GRANT SELECT, INSERT, UPDATE ON dbo.PRODUCT       TO [pineit_ex];
GRANT SELECT, INSERT, UPDATE ON dbo.WEIGH_RECORD  TO [pineit_ex];
GRANT SELECT, INSERT, UPDATE ON dbo.CODE_MAP      TO [pineit_ex];
GRANT SELECT, INSERT        ON dbo.SYNC_LOG       TO [pineit_ex];
GO
