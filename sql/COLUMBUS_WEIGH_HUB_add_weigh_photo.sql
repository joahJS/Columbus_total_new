-- 계근 사진(1차/2차, 각 최대 2장) 저장용 테이블 추가.
-- A지점(FTP)/B,C지점(로컬 IMAGE 폴더) 양쪽 동기화 잡(ColumbusSync.BranchA/BranchBC)이
-- 이 테이블에 사진을 올린다.
-- PHOTO_DATA는 사진 1장당 보통 수백KB~수MB. 계근 건수가 많으면 DB 용량이 빠르게 늘어날 수
-- 있으니, 운영 중 DB 용량 추이를 지켜볼 것.
CREATE TABLE dbo.WEIGH_PHOTO
(
    PHOTO_ID    BIGINT IDENTITY(1,1) PRIMARY KEY,
    BRANCH_CODE CHAR(1)         NOT NULL REFERENCES dbo.BRANCH(BRANCH_CODE),
    SOURCE_KEY  NVARCHAR(30)    NOT NULL,   -- WEIGH_RECORD.SOURCE_KEY와 동일한 값/길이(원본 PK)
    STAGE       CHAR(1)         NOT NULL,   -- '1' = 1차 계량, '2' = 2차 계량
    SEQ         TINYINT         NOT NULL,   -- 같은 단계 안에서의 사진 순번(1, 2)
    PHOTO_DATA  VARBINARY(MAX)  NOT NULL,
    SYNCED_AT   DATETIME2       NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT UQ_WEIGH_PHOTO UNIQUE (BRANCH_CODE, SOURCE_KEY, STAGE, SEQ),
    CONSTRAINT CK_WEIGH_PHOTO_STAGE CHECK (STAGE IN ('1', '2')),
    CONSTRAINT CK_WEIGH_PHOTO_SEQ CHECK (SEQ IN (1, 2))
);
GO

-- 거래처/제품 등과 동일하게, 동기화 전용 계정(pineit_ex)에 SELECT/INSERT/UPDATE만 부여한다
-- (DELETE는 의도적으로 주지 않음 - 동기화 잡이 실수로 사진을 지우는 사고를 막기 위함).
GRANT SELECT, INSERT, UPDATE ON dbo.WEIGH_PHOTO TO [pineit_ex];
GO
