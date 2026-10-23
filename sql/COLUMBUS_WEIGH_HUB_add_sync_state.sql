-- 지점별 동기화 진행 상태(예: B/C지점의 계근 증분 동기화 워터마크)를 저장하는 범용 테이블.
-- 값 하나만 저장하면 되는 간단한 상태용이라 (BRANCH_CODE, STATE_KEY) 조합을 키로 하는
-- 넓은 테이블 하나로 뺐다 - 상태 종류가 늘어나도 테이블을 또 추가할 필요가 없다.
CREATE TABLE dbo.SYNC_STATE
(
    BRANCH_CODE  CHAR(1)       NOT NULL REFERENCES dbo.BRANCH(BRANCH_CODE),
    STATE_KEY    VARCHAR(50)   NOT NULL,
    STATE_VALUE  NVARCHAR(100) NULL,
    UPDATED_AT   DATETIME2     NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT PK_SYNC_STATE PRIMARY KEY (BRANCH_CODE, STATE_KEY)
);
GO

GRANT SELECT, INSERT, UPDATE ON dbo.SYNC_STATE TO [pineit_ex];
GO
