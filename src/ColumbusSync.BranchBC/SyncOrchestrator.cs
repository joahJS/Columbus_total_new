using System;
using System.Collections.Generic;
using ColumbusSync.BranchBC.Config;
using ColumbusSync.BranchBC.Hub;
using ColumbusSync.BranchBC.Logging;
using ColumbusSync.BranchBC.Source;

namespace ColumbusSync.BranchBC
{
    /// <summary>
    /// 한 번의 동기화 배치(= 10분마다 한 번)를 수행한다.
    /// "지점별 데이터 차이점 정리" 문서의 규칙을 여기서 실제로 적용한다:
    ///   - 완료 여부는 SecondWeight 존재 여부로 재계산 (원본 WEIGH_STS를 그대로 믿지 않음)
    ///   - 입/출고 구분은 1차/2차 중량 크기 비교로 재계산 (원본 INOUT을 그대로 믿지 않음)
    ///   - 원본 키(SENO)는 SOURCE_KEY 컬럼에 그대로 보존
    /// A지점(ColumbusSync.BranchA)과 같은 규칙을 그대로 적용해 지점 간 데이터를 일관되게 만든다.
    /// </summary>
    public class SyncOrchestrator
    {
        private readonly MdbSourceReader _source;
        private readonly HubWriter _hub;

        public SyncOrchestrator(MdbSourceReader source, HubWriter hub)
        {
            _source = source;
            _hub = hub;
        }

        public void RunOnce()
        {
            var startedAt = DateTime.Now;
            int upserted = 0;

            try
            {
                FileLogger.Info("동기화 시작");

                foreach (var customer in _source.GetCustomers())
                {
                    _hub.UpsertCustomer(customer);
                    upserted++;
                }

                foreach (var vehicle in _source.GetVehicles())
                {
                    _hub.UpsertVehicle(vehicle);
                    upserted++;
                }

                foreach (var product in _source.GetProducts())
                {
                    _hub.UpsertProduct(product);
                    upserted++;
                }

                // TS2020 로그인 계정도 매 주기 계속 동기화한다(일회성이 아님) - 지점 담당자가
                // 그쪽 프로그램에서 계정을 새로 만들거나 비밀번호를 바꾸면 허브에도 반영되어야
                // ColumbusWeighing에서 같은 계정으로 계속 로그인할 수 있기 때문이다.
                foreach (var user in _source.GetUsers())
                {
                    _hub.UpsertUser(user);
                    upserted++;
                }

                // 계근 데이터: 워터마크(직전 동기화에서 본 가장 최신 WORK_DATE)가 없으면
                // 최초 백필로 WeighSyncLookbackDays 기간(기본 1년) 전체를 훑고, 있으면
                // WORK_DATE가 그 이후인 행만 증분으로 받는다. 이렇게 하면 매 주기 전체
                // 기간을 다시 훑는 낭비 없이도, 아무리 오래된 계근건이라도 수정되면 바로
                // 반영된다(DATE1 범위와 무관하게 WORK_DATE만 본다).
                var watermark = _hub.GetWeighWatermark();
                List<RawWeighRow> weighRows;
                if (watermark == null)
                {
                    var toDate = DateTime.Today;
                    var fromDate = toDate.AddDays(-SyncSettings.WeighSyncLookbackDays);
                    weighRows = _source.GetWeighRecords(fromDate, toDate);
                }
                else
                {
                    weighRows = _source.GetWeighRecordsModifiedSince(watermark);
                }

                // 사진 동기화: mdb에 TB_IMAGE 테이블이 있으면(녹산/C지점처럼 DB에 직접 저장하는
                // 버전) 그걸 쓰고, 없으면 ImagesFolderPath가 설정된 경우(생곡/B지점처럼 로컬
                // 파일로 저장하는 버전) 그걸 쓴다 - HasImageTable() 참고. 이미 올라간 건은
                // GetSyncedPhotoKeys()로 미리 걸러내 매 주기 재탐색을 피한다.
                var imagesFolderPath = SyncSettings.ImagesFolderPath;
                var hasImageTable = _source.HasImageTable();
                var photoSyncEnabled = hasImageTable || imagesFolderPath != null;
                var syncedPhotoKeys = photoSyncEnabled ? _hub.GetSyncedPhotoKeys() : null;

                var maxWorkDate = watermark;
                foreach (var raw in weighRows)
                {
                    _hub.UpsertWeighRecord(Transform(raw));
                    upserted++;

                    if (!string.IsNullOrEmpty(raw.WorkDate) &&
                        (maxWorkDate == null || string.CompareOrdinal(raw.WorkDate, maxWorkDate) > 0))
                    {
                        maxWorkDate = raw.WorkDate;
                    }

                    if (photoSyncEnabled)
                    {
                        SyncPhotos(raw, hasImageTable, imagesFolderPath, syncedPhotoKeys);
                    }
                }

                if (!string.IsNullOrEmpty(maxWorkDate) && maxWorkDate != watermark)
                {
                    _hub.SetWeighWatermark(maxWorkDate);
                }

                var finishedAt = DateTime.Now;
                _hub.WriteSyncLog(startedAt, finishedAt, success: true, inserted: upserted, updated: 0, errorMessage: null);
                FileLogger.Info(string.Format("동기화 완료: {0}건 처리, {1:F1}초 소요", upserted, (finishedAt - startedAt).TotalSeconds));
            }
            catch (Exception ex)
            {
                FileLogger.Error("동기화 실패", ex);

                try
                {
                    _hub.WriteSyncLog(startedAt, DateTime.Now, success: false, inserted: upserted, updated: 0, errorMessage: ex.ToString());
                }
                catch (Exception logEx)
                {
                    // 허브 DB 자체가 응답이 없는 상황일 수 있으므로 로그 기록 실패는 파일 로그로만 남긴다.
                    FileLogger.Error("SYNC_LOG 기록 실패", logEx);
                }
            }
        }

        /// <summary>계근건 1건의 사진을 읽어 올린다. 이미 올라간 단계는 건너뛰고, 1차는
        /// 언제든(완료 전이라도) 시도하되 2차는 2차 계량이 끝난 건만 시도한다 - 로컬 파일
        /// 방식(hasImageTable=false)에서는 2차가 아직 안 끝났으면 TIME2가 아직 확정되지
        /// 않아 분(分) 매칭이 의미가 없기 때문이고, DB 테이블 방식도 2차가 끝나야 2차
        /// 컬럼이 채워지는 건 마찬가지라 동일하게 적용한다.
        /// 사진 하나 처리 실패가 전체 동기화를 중단시키지 않도록 이 메서드 안에서만 예외를 잡는다.</summary>
        private void SyncPhotos(RawWeighRow raw, bool hasImageTable, string imagesFolderPath, HashSet<string> syncedPhotoKeys)
        {
            var needFirst = !syncedPhotoKeys.Contains(PhotoKey(raw.Seno, "1"));
            var needSecond = IsCompleted(raw) && !syncedPhotoKeys.Contains(PhotoKey(raw.Seno, "2"));

            if (!needFirst && !needSecond)
            {
                return;
            }

            try
            {
                var photos = hasImageTable
                    ? _source.GetImagesByImgFile(raw.ImgFile)
                    : PhotoFileReader.ReadPhotos(imagesFolderPath, raw.WeighDate, raw.ImgFile, raw.SecondDateTime, needFirst, needSecond);

                foreach (var photo in photos)
                {
                    if ((photo.Stage == "1" && !needFirst) || (photo.Stage == "2" && !needSecond))
                    {
                        continue;
                    }

                    _hub.UpsertWeighPhoto(raw.Seno, photo);
                    syncedPhotoKeys.Add(PhotoKey(raw.Seno, photo.Stage));
                }
            }
            catch (Exception ex)
            {
                FileLogger.Error(string.Format("사진 동기화 실패(SENO={0})", raw.Seno), ex);
            }
        }

        private static string PhotoKey(string sourceKey, string stage)
        {
            return sourceKey + "|" + stage;
        }

        private static bool IsCompleted(RawWeighRow raw)
        {
            return raw.SecondWeight.HasValue && raw.SecondWeight.Value != 0;
        }

        /// <summary>mdb 원본 1건을 통합 스키마 규칙에 맞게 변환한다.</summary>
        private static WeighRecordForHub Transform(RawWeighRow raw)
        {
            var isCompleted = IsCompleted(raw);

            string inOutType = null;
            if (raw.FirstWeight.HasValue && raw.SecondWeight.HasValue && raw.FirstWeight.Value != 0 && raw.SecondWeight.Value != 0)
            {
                // A지점(SA015F00.cs CalculateWeight())과 동일한 규칙: 1차 >= 2차면 입고, 아니면 출고.
                inOutType = raw.FirstWeight.Value >= raw.SecondWeight.Value ? "I" : "O";
            }

            decimal? netWeight = null;
            if (raw.FirstWeight.HasValue && raw.SecondWeight.HasValue)
            {
                netWeight = Math.Abs(raw.FirstWeight.Value - raw.SecondWeight.Value);
            }

            int? weighSeq;
            int seqParsed;
            weighSeq = int.TryParse(raw.Bunho, out seqParsed) ? seqParsed : (int?)null;

            return new WeighRecordForHub
            {
                SourceKey = raw.Seno,
                WeighDate = raw.WeighDate.Date,
                WeighSeq = weighSeq,
                VehicleNo = raw.CarNo,
                CustomerSourceCode = raw.CustoNo,
                CustomerName = raw.CustoNm,
                ProductSourceCode = raw.PumNo,
                ProductName = raw.PumNm,
                FirstDateTime = raw.FirstDateTime,
                FirstWeight = raw.FirstWeight,
                SecondDateTime = raw.SecondDateTime,
                SecondWeight = raw.SecondWeight,
                NetWeight = netWeight,
                LossWeight = raw.LossWeight,
                UnitPrice = raw.UnitPrice,
                InOutType = inOutType,
                IsCompleted = isCompleted,
                WeigherName = raw.WeigherName,
                Remark = raw.Remark,
                SourceRawStatus = raw.WeighStatus,
            };
        }
    }
}
