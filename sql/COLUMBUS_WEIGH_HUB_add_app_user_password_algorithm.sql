/*
    COLUMBUS_WEIGH_HUB - dbo.APP_USER에 비밀번호 해시 방식 컬럼 추가
    ------------------------------------------------------------------
    - COLUMBUS_WEIGH_HUB_add_app_user.sql로 이미 만들어진 dbo.APP_USER에 추가로 실행하는
      증분 스크립트입니다.
    - 배경: A지점(MES)의 로그인 계정(zUSRLST)을 허브로 동기화하면서, 비밀번호를 B/C지점
      (TS2020, 평문 저장)과 다르게 처리해야 하는 문제가 생겼습니다. MES는 비밀번호를
      HASHBYTES('SHA2_256', 평문)로만 저장하고 평문을 어디에도 남기지 않기 때문에
      (usp_Comn_Login 저장 프로시저 확인), 우리가 쓰는 PBKDF2 방식으로 다시 해시할 수 없습니다.
      대신 MES가 이미 계산해 둔 SHA-256 해시 바이트를 그대로 복사해서 저장하고, 로그인 시에도
      같은 방식(HASHBYTES SHA2_256)으로 검증합니다 (ColumbusSync.BranchA/Hub/HubWriter.cs의
      UpsertUser, ColumbusWeighing.Services.SqlAuthenticationService 참고).
    - PASSWORD_ALGORITHM 값:
        'PBKDF2' (기본값) - 지금까지 쓰던 방식. admin 계정과 B/C지점 동기화 계정,
                            ColumbusWeighing 관리자가 직접 등록/재설정한 모든 계정이 해당.
        'SHA256'          - A지점(MES)에서 동기화된 계정. ColumbusWeighing 관리자가 이 계정의
                            비밀번호를 직접 재설정하면 그 순간부터 'PBKDF2'로 바뀐다.
    - 실행 전 담당자 검토를 거쳐주세요.
*/

USE COLUMBUS_WEIGH_HUB;
GO

IF COL_LENGTH('dbo.APP_USER', 'PASSWORD_ALGORITHM') IS NULL
BEGIN
    ALTER TABLE dbo.APP_USER ADD PASSWORD_ALGORITHM NVARCHAR(20) NOT NULL DEFAULT 'PBKDF2';
END
GO
