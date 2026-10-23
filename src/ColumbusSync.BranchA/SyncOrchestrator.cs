using System;
using System.Collections.Generic;
using ColumbusSync.BranchA.Config;
using ColumbusSync.BranchA.Hub;
using ColumbusSync.BranchA.Logging;
using ColumbusSync.BranchA.Source;

namespace ColumbusSync.BranchA
{
    /// <summary>
    /// 한 번의 동기화 배치(= 10분마다 한 번)를 수행한다.
    /// "지점별 데이터 차이점 정리" 문서의 규칙(1~3번)을 여기서 실제로 적용한다:
    ///   - 완료 여부는 SecondWeight 존재 여부로 재계산 (원본 CHKYN을 그대로 믿지 않음)
    ///   - 입/출고 구분은 1차/2차 중량 크기 비교로 재계산 (원본 JOBGU를 그대로 믿지 않음)
    ///   - 원본 키(SLINO)는 SOURCE_KEY 컬럼에 그대로 보존
    /// </summary>
    public class SyncOrchestrator
    {
        private readonly MesSourceReader _source;
        private readonly HubWriter _hub;

        public SyncOrchestrator(MesSourceReader source, HubWriter hub)
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

                // MES 로그인 계정도 B/C지점과 같은 방식으로 매 주기 계속 동기화한다.
                foreach (var user in _source.GetUsers())
                {
                    _hub.UpsertUser(user);
                    upserted++;
                }

                // 계근 데이터는 최근 N일치를 매번 다시 훑는다. MEASURE_RETR이 계량일자(TDATE)로만
                // 조회 조건을 받고 수정일시 기준 필터를 지원하지 않기 때문에, 검수/수정이 늦게
                // 들어온 건도 놓치지 않으려면 그 지연 가능성만큼 기간을 넉넉히 잡아야 한다.
                // App.config의 WeighSyncLookbackDays로 조정한다(기본 1년). 매번 전체 기간을
                // 다시 받긴 하지만, 아래 WeighHashCache가 내용이 안 바뀐 완료건의 허브
                // 업서트는 걸러낸다.
                var toDate = DateTime.Today;
                var fromDate = toDate.AddDays(-SyncSettings.WeighSyncLookbackDays);

                // 사진 동기화는 FtpPath가 설정된 경우에만 한다. 이미 올라간 건은
                // GetSyncedPhotoKeys()로 미리 걸러내 매 주기 FTP 재조회를 피한다.
                var ftpPath = SyncSettings.FtpPath;
                var ftpReader = ftpPath != null ? new FtpPhotoReader(ftpPath, SyncSettings.FtpUserId, SyncSettings.FtpPassword) : null;
                var syncedPhotoKeys = ftpReader != null ? _hub.GetSyncedPhotoKeys() : null;

                // 내용이 안 바뀐 완료건은 허브 업서트를 건너뛴다(WeighHashCache 참고) - MES는
                // 매번 전체 기간을 다시 주지만, 그중 실제로 바뀐 것만 허브로 올린다.
                var hashCache = WeighHashCache.Load(SyncSettings.WeighHashCachePath);

                foreach (var raw in _source.GetWeighRecords(fromDate, toDate))
                {
                    var contentHash = WeighHashCache.ComputeHash(BuildHashContent(raw));
                    if (!hashCache.IsUnchanged(raw.Slino, contentHash))
                    {
                        _hub.UpsertWeighRecord(Transform(raw));
                        upserted++;
                    }

                    if (ftpReader != null)
                    {
                        SyncPhotos(raw, ftpReader, syncedPhotoKeys);
                    }
                }

                hashCache.Save();

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

        /// <summary>계근건 1건의 사진을 FTP에서 읽어 올린다. 이미 올라간 단계는 건너뛰고,
        /// 1차는 언제든(완료 전이라도) 시도하되 2차는 2차 계량이 끝난 건만 시도한다.
        /// 사진 하나 처리 실패(FTP 일시 오류 등)가 전체 동기화를 중단시키지 않도록 이
        /// 메서드 안에서만 예외를 잡는다.</summary>
        private void SyncPhotos(RawWeighRow raw, FtpPhotoReader ftpReader, HashSet<string> syncedPhotoKeys)
        {
            var needFirst = !syncedPhotoKeys.Contains(PhotoKey(raw.Slino, "1"));
            var needSecond = IsCompleted(raw) && !syncedPhotoKeys.Contains(PhotoKey(raw.Slino, "2"));

            if (!needFirst && !needSecond)
            {
                return;
            }

            try
            {
                var photos = ftpReader.ReadPhotos(raw.TDate, raw.Slino, needFirst, needSecond);
                foreach (var photo in photos)
                {
                    _hub.UpsertWeighPhoto(raw.Slino, photo);
                    syncedPhotoKeys.Add(PhotoKey(raw.Slino, photo.Stage));
                }
            }
            catch (Exception ex)
            {
                FileLogger.Error(string.Format("사진 동기화 실패(SLINO={0})", raw.Slino), ex);
            }
        }

        private static string PhotoKey(string sourceKey, string stage)
        {
            return sourceKey + "|" + stage;
        }

        private static bool IsCompleted(RawWeighRow raw)
        {
            return raw.SWeit.HasValue && raw.SWeit.Value != 0;
        }

        /// <summary>WeighHashCache와 비교할 "내용"을 만든다. 허브에 실제로 올라가는 값에
        /// 영향을 주는 필드만 모으면 되고, 필드 추가/순서 변경 시에는 캐시가 전부
        /// "변경됨"으로 보여 한 번 더 업서트될 뿐이라 안전하다(멱등적인 MERGE).</summary>
        private static string BuildHashContent(RawWeighRow raw)
        {
            return string.Join("|", new[]
            {
                raw.TDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                raw.SeqNo, raw.JobGu, raw.CarNo, raw.CvCod, raw.CvNam, raw.ItCod, raw.ItNam,
                raw.FTime.HasValue ? raw.FTime.Value.ToString("o") : null,
                raw.STime.HasValue ? raw.STime.Value.ToString("o") : null,
                raw.FWeit.HasValue ? raw.FWeit.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : null,
                raw.SWeit.HasValue ? raw.SWeit.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : null,
                raw.EWeit.HasValue ? raw.EWeit.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : null,
                raw.RWeit.HasValue ? raw.RWeit.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : null,
                raw.LWeit.HasValue ? raw.LWeit.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : null,
                raw.LosGu,
                raw.AWeit.HasValue ? raw.AWeit.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : null,
                raw.UCost.HasValue ? raw.UCost.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : null,
                raw.ChkYn, raw.PlnNm, raw.InsRk,
            });
        }

        /// <summary>MES 원본 1건을 통합 스키마 규칙에 맞게 변환한다.</summary>
        private static WeighRecordForHub Transform(RawWeighRow raw)
        {
            var isCompleted = IsCompleted(raw);

            string inOutType = null;
            if (raw.FWeit.HasValue && raw.SWeit.HasValue && raw.FWeit.Value != 0 && raw.SWeit.Value != 0)
            {
                // SA015F00.cs CalculateWeight()와 동일한 규칙: 1차 >= 2차면 입고, 아니면 출고.
                inOutType = raw.FWeit.Value >= raw.SWeit.Value ? "I" : "O";
            }

            decimal? netWeight = null;
            if (raw.FWeit.HasValue && raw.SWeit.HasValue)
            {
                netWeight = Math.Abs(raw.FWeit.Value - raw.SWeit.Value);
            }

            int? weighSeq;
            int seqParsed;
            weighSeq = int.TryParse(raw.SeqNo, out seqParsed) ? seqParsed : (int?)null;

            return new WeighRecordForHub
            {
                SourceKey = raw.Slino,
                WeighDate = raw.TDate.Date,
                WeighSeq = weighSeq,
                VehicleNo = raw.CarNo,
                CustomerSourceCode = raw.CvCod,
                CustomerName = raw.CvNam,
                ProductSourceCode = raw.ItCod,
                ProductName = raw.ItNam,
                FirstDateTime = raw.FTime,
                FirstWeight = raw.FWeit,
                SecondDateTime = raw.STime,
                SecondWeight = raw.SWeit,
                NetWeight = netWeight,
                LossWeight = raw.LWeit,
                UnitPrice = raw.UCost,
                InOutType = inOutType,
                IsCompleted = isCompleted,
                WeigherName = raw.PlnNm,
                Remark = raw.InsRk,
                SourceRawStatus = raw.ChkYn,
            };
        }
    }
}
