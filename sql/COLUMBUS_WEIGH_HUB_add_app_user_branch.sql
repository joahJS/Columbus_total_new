/*
    COLUMBUS_WEIGH_HUB - dbo.APP_USER에 지점 코드 컬럼 추가 (참고용/추후 권한 기능 대비)
    ------------------------------------------------------------------
    - COLUMBUS_WEIGH_HUB_add_app_user.sql로 이미 만들어진 dbo.APP_USER에 추가로 실행하는
      증분 스크립트입니다.
    - 배경: B(생곡)/C(녹산)지점 TS2020의 TB_USER 계정을 허브로 동기화하면서, 그 계정이 어느
      지점 mdb에서 왔는지(또는 마지막으로 어느 지점에서 동기화됐는지) 기록해두기 위한
      컬럼입니다.
    - 로그인 자체는 지점과 무관하게 LOGIN_ID+비밀번호만으로 동작합니다 (지점 선택 UI는
      추가했다가 다시 뺐습니다 - "일단 보류"). LOGIN_ID의 전체 유일성 제약(UQ_APP_USER_LOGIN_ID)은
      그대로 유지합니다. B/C지점이 같은 TS2020 프로그램을 써서 "1"/"admin" 같은 아이디가
      양쪽에 있을 수 있는데, 지금은 나중에 동기화되는 쪽이 그 아이디를 그대로 덮어씁니다
      (ColumbusSync.BranchBC/Hub/HubWriter.cs의 UpsertUser 참고).
    - 이 컬럼은 나중에 "최고관리자가 아이디별로 어느 지점까지 조회 가능한지 권한을 부여"하는
      기능을 만들 때 참고 정보 또는 시작점으로 쓰일 수 있습니다.
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
