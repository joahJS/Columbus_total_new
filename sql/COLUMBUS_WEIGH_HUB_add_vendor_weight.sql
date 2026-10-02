-- 2차계량 화면에 "업체중량"/"로스" 컬럼을 추가하기 위한 마이그레이션.
-- VENDOR_WEIGHT는 지점 동기화(ColumbusSync.BranchA/BranchBC)로는 절대 채워지지 않고,
-- ColumbusWeighing 뷰어에서 사용자가 직접 입력하는 값만 저장한다. 동기화 UPSERT는 이
-- 컬럼을 건드리지 않으므로, 재동기화가 되어도 입력한 값이 사라지지 않는다.
ALTER TABLE dbo.WEIGH_RECORD ADD VENDOR_WEIGHT DECIMAL(15,3) NULL;
GO
