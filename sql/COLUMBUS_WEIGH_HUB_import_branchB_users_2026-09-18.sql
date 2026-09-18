/*
    COLUMBUS_WEIGH_HUB - B지점(생곡) TS2020 TB_USER 수동 반영 (2026-09-18)
    ------------------------------------------------------------------
    - 현장에 바로 원격접속할 수 없어 ColumbusSync.BranchBC를 직접 돌리지 못하는 상황이라,
      전달받은 B지점 TSDB.mdb의 TB_USER 내용을 이 스크립트로 대신 반영합니다.
    - 반드시 다음 두 스크립트를 먼저 실행한 뒤에 이 스크립트를 실행하세요.
        1) COLUMBUS_WEIGH_HUB_add_app_user_branch.sql   (BRANCH_CODE 컬럼)
        2) COLUMBUS_WEIGH_HUB_add_app_user_password.sql (dbo.APP_USER_PASSWORD 테이블)
    - mdb TB_USER 원본 내용 (2026-09-18 확인):
        ID1="1",     USER1="콜럼버스", PASS1="1"
        ID1="2",     USER1="이재현",   PASS1=(비어있음)
        ID1="admin", USER1="admin",   PASS1="123"
      권한 컬럼(C_PRINT/C_MODIFY/C_DELETE/C_ADMINISTRATOR)은 실제 mdb 값을 그대로 반영했습니다.
    - "admin" 아이디는 기존 최상위 관리자 계정(admin/1234)과 겹칩니다. 요청에 따라 계정을
      합치지 않고 비밀번호 두 개를 모두 허용합니다 - 그래서 이 스크립트는 admin 행을
      덮어쓰기 전에 현재 비밀번호(1234)를 dbo.APP_USER_PASSWORD에 먼저 백업해 둡니다.
      (이미 백업되어 있으면 다시 넣지 않도록 NOT EXISTS로 막아뒀습니다 - 스크립트를
      실수로 두 번 실행해도 안전합니다.)
    - 이재현(ID "2") 계정은 mdb에 비밀번호가 없어서, 요청하신 대로 "빈 비밀번호로 로그인"이
      되도록 빈 문자열("")을 해시해서 넣었습니다 - 로그인 화면에서 비밀번호 칸을 비워두고
      로그인하면 됩니다. 보안상 원치 않으시면 나중에 UserManagementForm에서 직접 비밀번호를
      설정해주세요.
    - 아래 PASSWORD_HASH/SALT 값은 ColumbusWeighing.ComnLib.PasswordHasher와 동일한 알고리즘
      (PBKDF2, HMAC-SHA1, 16바이트 솔트, 32바이트 해시, 10000회 반복)으로 미리 계산해 둔
      값입니다. 실행 전 담당자 검토를 거쳐주세요.
*/

USE COLUMBUS_WEIGH_HUB;
GO

-- 1) 기존 admin(비밀번호 1234)이 이번에 덮어써지기 전에, 그 비밀번호를 보조 비밀번호로 백업.
INSERT INTO dbo.APP_USER_PASSWORD (USER_ID, PASSWORD_HASH, PASSWORD_SALT, PASSWORD_ALGORITHM, REMARK, CREATED_BY)
SELECT USER_ID, PASSWORD_HASH, PASSWORD_SALT, PASSWORD_ALGORITHM,
       N'B지점 TS2020 admin(123) 계정과 아이디가 겹쳐서 백업한 기존 admin 비밀번호(1234)',
       N'manual:B-import-2026-09-18'
FROM dbo.APP_USER
WHERE LOGIN_ID = N'admin'
  AND NOT EXISTS (
      SELECT 1 FROM dbo.APP_USER_PASSWORD p
      INNER JOIN dbo.APP_USER u ON u.USER_ID = p.USER_ID
      WHERE u.LOGIN_ID = N'admin'
        AND p.REMARK = N'B지점 TS2020 admin(123) 계정과 아이디가 겹쳐서 백업한 기존 admin 비밀번호(1234)'
  );
GO

-- 2) B지점 TB_USER 3건을 dbo.APP_USER에 반영 (LOGIN_ID 기준 upsert - 동기화 프로그램과 동일한 방식).
MERGE dbo.APP_USER AS target
USING (VALUES
    (N'1',     N'콜럼버스', N'cm8SeAKtsRrNw9wornR5G0Z7kWKSmgFEnpI+Gqb53So=', N'nkm9w2VoTYYYvwYDaPUIFg==', 1, 0, 0, 0),
    (N'2',     N'이재현',   N'kiHozsfwmCKGNWyNsaspJWGle6uS8eE3yMEyuMjWuqY=', N'jHecBDTOubwyKiHHyPAmIQ==', 1, 1, 1, 1),
    (N'admin', N'admin',   N'8lHb6YCm+XenQiHrHtC0SUAikDYJ9lxoAiL4lRZUG0Y=', N'chj8rUEApfXeLZv7zFvZ5g==', 1, 1, 1, 1)
) AS src (LOGIN_ID, DISPLAY_NAME, PASSWORD_HASH, PASSWORD_SALT, CAN_PRINT, CAN_EDIT, CAN_DELETE, IS_ADMIN)
ON target.LOGIN_ID = src.LOGIN_ID
WHEN MATCHED THEN UPDATE SET
    BRANCH_CODE = N'B', DISPLAY_NAME = src.DISPLAY_NAME,
    CAN_PRINT = src.CAN_PRINT, CAN_EDIT = src.CAN_EDIT, CAN_DELETE = src.CAN_DELETE, IS_ADMIN = src.IS_ADMIN,
    PASSWORD_HASH = src.PASSWORD_HASH, PASSWORD_SALT = src.PASSWORD_SALT, PASSWORD_ALGORITHM = N'PBKDF2',
    MODIFIED_BY = N'manual:B-import-2026-09-18', MODIFIED_AT = SYSDATETIME()
WHEN NOT MATCHED THEN INSERT
    (BRANCH_CODE, LOGIN_ID, DISPLAY_NAME, CAN_PRINT, CAN_EDIT, CAN_DELETE, IS_ADMIN,
     PASSWORD_HASH, PASSWORD_SALT, PASSWORD_ALGORITHM, MODIFIED_BY)
    VALUES
    (N'B', src.LOGIN_ID, src.DISPLAY_NAME, src.CAN_PRINT, src.CAN_EDIT, src.CAN_DELETE, src.IS_ADMIN,
     src.PASSWORD_HASH, src.PASSWORD_SALT, N'PBKDF2', N'manual:B-import-2026-09-18');
GO
