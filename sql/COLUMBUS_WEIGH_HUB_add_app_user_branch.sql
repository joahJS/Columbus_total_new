/*
    COLUMBUS_WEIGH_HUB - dbo.APP_USER에 지점 코드 추가
    ------------------------------------------------------------------
    - COLUMBUS_WEIGH_HUB_add_app_user.sql로 이미 만들어진 dbo.APP_USER에 추가로 실행하는
      증분 스크립트입니다.
    - 배경: B(생곡)/C(녹산)지점은 같은 TS2020 프로그램을 쓰기 때문에 각 지점 mdb의
      TB_USER에 "1", "2", "admin" 같은 단순한 ID가 양쪽에 중복해서 존재합니다. 지금까지는
      LOGIN_ID 하나만으로 전체 유일성을 강제했기 때문에 두 지점의 계정을 동시에 들여올 수
      없었습니다.
    - 해결: BRANCH_CODE 컬럼을 추가해서 유일성 기준을 LOGIN_ID 단독에서 (BRANCH_CODE, LOGIN_ID)
      조합으로 바꿉니다. 로그인 화면에서 지점을 먼저 선택하게 해서, 같은 ID라도 지점이 다르면
      서로 다른 계정으로 로그인할 수 있습니다.
    - BRANCH_CODE를 NULL 허용으로 둔 이유: 기존 admin 계정처럼 특정 지점에 속하지 않는
      전사 공용 관리자 계정도 계속 지원하기 위해서입니다. SQL Server는 복합 UNIQUE 제약에서
      (NULL, 'admin')과 (NULL, 'admin2')처럼 NULL이 섞여 있어도 나머지 컬럼 값이 다르면
      서로 다른 값으로 취급하므로, 공용 admin(NULL) 계정과 지점별 계정이 공존할 수 있습니다.
      (단, 완전히 같은 튜플 (NULL, 'admin')이 두 번 들어가는 것은 여전히 막힙니다.)
    - 실행 전 담당자 검토를 거쳐주세요.
*/

USE COLUMBUS_WEIGH_HUB;
GO

IF COL_LENGTH('dbo.APP_USER', 'BRANCH_CODE') IS NULL
BEGIN
    ALTER TABLE dbo.APP_USER ADD BRANCH_CODE CHAR(1) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_APP_USER_BRANCH'
)
BEGIN
    ALTER TABLE dbo.APP_USER
        ADD CONSTRAINT FK_APP_USER_BRANCH FOREIGN KEY (BRANCH_CODE) REFERENCES dbo.BRANCH (BRANCH_CODE);
END
GO

-- 기존 LOGIN_ID 단독 유일성 제약을 (BRANCH_CODE, LOGIN_ID) 조합 제약으로 교체한다.
IF EXISTS (
    SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_APP_USER_LOGIN_ID' AND parent_object_id = OBJECT_ID('dbo.APP_USER')
)
BEGIN
    ALTER TABLE dbo.APP_USER DROP CONSTRAINT UQ_APP_USER_LOGIN_ID;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_APP_USER_BRANCH_LOGIN_ID' AND parent_object_id = OBJECT_ID('dbo.APP_USER')
)
BEGIN
    ALTER TABLE dbo.APP_USER
        ADD CONSTRAINT UQ_APP_USER_BRANCH_LOGIN_ID UNIQUE (BRANCH_CODE, LOGIN_ID);
END
GO
