-- B/C지점 계근 데이터 워터마크 증분 동기화 버그로 과거에 누락된 데이터를 복구하기 위한
-- 1회성 조치. SyncOrchestrator의 WORK_DATE 워터마크를 지워서, 다음 동기화 주기에 B/C
-- 지점 둘 다 WeighSyncLookbackDays(기본 1년) 전체를 DATE1 기준으로 다시 훑게 만든다.
--
-- 안전한 이유:
--   - UpsertWeighRecord/UpsertWeighPhoto 둘 다 MERGE 기반 업서트라 멱등이다. 이미 올라간
--     정상 데이터는 그대로 덮어써질 뿐 중복되지 않는다.
--   - 사진은 GetSyncedPhotoKeys()로 이미 올라간 (SOURCE_KEY, STAGE)를 따로 추적하므로,
--     이 워터마크 리셋과 무관하게 이미 동기화된 사진을 다시 내려받지 않는다.
--   - 백필 범위는 DATE1(Text, yyyy-mm-dd) 기준 문자열 비교로, 이번 버그(WORK_DATE
--     워터마크)와 무관하게 전부터 안정적으로 동작해온 경로다.
--
-- 주의: 다음 동기화 주기가 B/C지점 각각 1년치를 다시 훑으므로, 평소보다 그 한 번은
-- 오래 걸릴 수 있다(사진까지 전부 다시 확인하지만, 이미 올라간 사진은 건너뛰므로 처음
-- 백필 때보다는 훨씬 빠를 것이다).

DELETE FROM dbo.SYNC_STATE
WHERE STATE_KEY = 'WEIGH_WATERMARK' AND BRANCH_CODE IN ('B', 'C');
