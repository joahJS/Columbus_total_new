-- 2차계량 화면의 "단가"(관리자 수동입력) + "공급가액" 컬럼을 위한 마이그레이션.
-- 기존 WEIGH_RECORD.UNIT_PRICE는 지점 동기화가 매 주기 원본 값으로 덮어쓰므로, 관리자가
-- 직접 입력/확정하는 단가는 별도 컬럼(ADMIN_UNIT_PRICE)에 저장한다. VENDOR_WEIGHT와 마찬가지로
-- 동기화 UPSERT는 이 컬럼을 건드리지 않으므로 재동기화가 되어도 입력한 값이 사라지지 않는다.
ALTER TABLE dbo.WEIGH_RECORD ADD ADMIN_UNIT_PRICE DECIMAL(15,3) NULL;
GO
