using System;
using System.ComponentModel;
using System.Linq;
using ColumbusWeighing.Models;

namespace ColumbusWeighing.Services
{
    /// <summary>
    /// 메모리 기반 계근 기록 저장소. 프로그램 시작 시 데모/개발용 샘플 데이터를 적재한다.
    /// </summary>
    public sealed class InMemoryWeighingRepository : IWeighingRepository
    {
        private const string CompanyName = "콜럼버스 주식회사";

        public BindingList<WeighingRecord> Records { get; } = new BindingList<WeighingRecord>();

        private int _nextId = 1;

        public InMemoryWeighingRepository()
        {
            SeedSampleData();
        }

        public WeighingRecord AddFirstWeighing(
            string vehicleNo,
            string customerName,
            string productName,
            InOutType inOutType,
            decimal firstWeight,
            string weigherName)
        {
            var record = new WeighingRecord
            {
                Id = _nextId++,
                WeighSeq = GetNextSeq(DateTime.Today),
                VehicleNo = vehicleNo,
                CustomerName = customerName,
                ProductName = productName,
                InOutType = inOutType,
                FirstDateTime = DateTime.Now,
                FirstWeight = firstWeight,
                WeigherName = weigherName,
                OwnerCompany = CompanyName
            };

            Records.Add(record);
            return record;
        }

        public void CompleteSecondWeighing(WeighingRecord record, decimal secondWeight, string weigherName)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            record.SecondDateTime = DateTime.Now;
            record.SecondWeight = secondWeight;

            if (!string.IsNullOrEmpty(weigherName))
            {
                record.WeigherName = weigherName;
            }

            // BindingList 는 속성 변경만으로는 그리드 정렬/필터를 갱신하지 않으므로
            // 리스트 변경 이벤트를 강제로 한 번 발생시켜 화면(1차/2차 그리드 이동)을 갱신한다.
            var index = Records.IndexOf(record);
            if (index >= 0)
            {
                Records.ResetItem(index);
            }
        }

        public WeighingRecord AddSingleWeighing(
            string vehicleNo,
            string customerName,
            string productName,
            InOutType inOutType,
            decimal firstWeight,
            decimal secondWeight,
            string weigherName)
        {
            var now = DateTime.Now;
            var record = new WeighingRecord
            {
                Id = _nextId++,
                WeighSeq = GetNextSeq(DateTime.Today),
                VehicleNo = vehicleNo,
                CustomerName = customerName,
                ProductName = productName,
                InOutType = inOutType,
                FirstDateTime = now,
                FirstWeight = firstWeight,
                SecondDateTime = now,
                SecondWeight = secondWeight,
                WeigherName = weigherName,
                OwnerCompany = CompanyName
            };

            Records.Add(record);
            return record;
        }

        public int GetNextSeq(DateTime date)
        {
            var todays = Records.Where(r => r.FirstDateTime.Date == date.Date).ToList();
            return todays.Count == 0 ? 1 : todays.Max(r => r.WeighSeq) + 1;
        }

        private void SeedSampleData()
        {
            var today = DateTime.Today;

            // (seq, 1차시간, 2차시간, 차량번호, 거래처, 제품, 1차중량, 2차중량, 입출고, 계량자, 비고)
            var completed = new[]
            {
                new { Seq = 1,  T1 = "16:40", T2 = "07:06", Car = "3812", Cust = "생곡>>>녹산이송", Prod = "왕문철A",       W1 = 32556m, W2 = 15708m, InOut = InOutType.Out, Remark = "납품" },
                new { Seq = 2,  T1 = "07:19", T2 = "07:45", Car = "0836", Cust = "삼호스틸",        Prod = "왕문철A",       W1 = 34260m, W2 = 6120m,  InOut = InOutType.Out, Remark = "" },
                new { Seq = 3,  T1 = "08:01", T2 = "08:17", Car = "5483", Cust = "한국특강",        Prod = "왕문철A",       W1 = 30480m, W2 = 5980m,  InOut = InOutType.Out, Remark = "" },
                new { Seq = 4,  T1 = "08:07", T2 = "08:30", Car = "9044", Cust = "한국특강",        Prod = "왕문철A",       W1 = 29940m, W2 = 6260m,  InOut = InOutType.Out, Remark = "" },
                new { Seq = 5,  T1 = "08:27", T2 = "08:35", Car = "1870", Cust = "명성",            Prod = "일반분철",      W1 = 21870m, W2 = 6900m,  InOut = InOutType.Out, Remark = "" },
                new { Seq = 6,  T1 = "08:37", T2 = "08:59", Car = "5492", Cust = "주철관",          Prod = "생철A(일반)",   W1 = 25460m, W2 = 6180m,  InOut = InOutType.Out, Remark = "" },
                new { Seq = 7,  T1 = "09:13", T2 = "09:35", Car = "3725", Cust = "코아텍",          Prod = "생철A(일반)",   W1 = 32120m, W2 = 5680m,  InOut = InOutType.Out, Remark = "" },
                new { Seq = 8,  T1 = "09:18", T2 = "09:57", Car = "2843", Cust = "삼일",            Prod = "왕문철A",       W1 = 28260m, W2 = 6040m,  InOut = InOutType.Out, Remark = "" },
                new { Seq = 9,  T1 = "09:37", T2 = "10:16", Car = "3812", Cust = "금강상사",        Prod = "일반분철",      W1 = 27480m, W2 = 6280m,  InOut = InOutType.Out, Remark = "" },
                new { Seq = 10, T1 = "10:22", T2 = "10:36", Car = "9921", Cust = "에스케이엠씨",    Prod = "왕문철A",       W1 = 24180m, W2 = 6080m,  InOut = InOutType.Out, Remark = "" },
                new { Seq = 11, T1 = "11:48", T2 = "12:09", Car = "1517", Cust = "정산",            Prod = "일반분철",      W1 = 33460m, W2 = 6420m,  InOut = InOutType.Out, Remark = "" },
                new { Seq = 12, T1 = "13:00", T2 = "13:06", Car = "2393", Cust = "에스케이엠씨",    Prod = "일반분철",      W1 = 30040m, W2 = 6100m,  InOut = InOutType.Out, Remark = "" },
                new { Seq = 13, T1 = "13:06", T2 = "13:35", Car = "2393", Cust = "에스케이엠씨",    Prod = "왕문철A",       W1 = 31980m, W2 = 6180m,  InOut = InOutType.Out, Remark = "" },
                new { Seq = 14, T1 = "13:28", T2 = "13:35", Car = "9921", Cust = "에스케이엠씨",    Prod = "일반분철",      W1 = 29760m, W2 = 6040m,  InOut = InOutType.Out, Remark = "" },
                new { Seq = 15, T1 = "13:32", T2 = "13:40", Car = "3930", Cust = "금강스틸",        Prod = "일반분철",      W1 = 26380m, W2 = 5960m,  InOut = InOutType.Out, Remark = "" },
                new { Seq = 16, T1 = "13:36", T2 = "14:00", Car = "9921", Cust = "에스케이엠씨",    Prod = "왕문철A",       W1 = 28540m, W2 = 6060m,  InOut = InOutType.Out, Remark = "" },
                new { Seq = 17, T1 = "14:08", T2 = "14:35", Car = "1517", Cust = "정산",            Prod = "일반분철",      W1 = 32860m, W2 = 6260m,  InOut = InOutType.Out, Remark = "" },
                new { Seq = 18, T1 = "14:40", T2 = "14:58", Car = "5483", Cust = "한국철강",        Prod = "왕문철A",       W1 = 27980m, W2 = 6120m,  InOut = InOutType.Out, Remark = "" }
            };

            foreach (var row in completed)
            {
                Records.Add(new WeighingRecord
                {
                    Id = _nextId++,
                    WeighSeq = row.Seq,
                    VehicleNo = row.Car,
                    CustomerName = row.Cust,
                    ProductName = row.Prod,
                    InOutType = row.InOut,
                    FirstDateTime = today.Add(TimeSpan.Parse(row.T1)),
                    FirstWeight = row.W1,
                    SecondDateTime = today.Add(TimeSpan.Parse(row.T2)),
                    SecondWeight = row.W2,
                    WeigherName = CompanyName,
                    OwnerCompany = CompanyName,
                    Remark = row.Remark
                });
            }

            // 아직 2차 계량 전(대기) 건 - 1차 계량 화면에 표시됨.
            Records.Add(new WeighingRecord
            {
                Id = _nextId++,
                WeighSeq = 19,
                VehicleNo = "2393",
                CustomerName = "에스케이엠씨",
                ProductName = "왕문철A",
                InOutType = InOutType.Out,
                FirstDateTime = today.Add(TimeSpan.Parse("15:03")),
                FirstWeight = 38120m,
                WeigherName = CompanyName,
                OwnerCompany = CompanyName
            });

            Records.Add(new WeighingRecord
            {
                Id = _nextId++,
                WeighSeq = 20,
                VehicleNo = "5492",
                CustomerName = "한국철강",
                ProductName = "왕문철A",
                InOutType = InOutType.Out,
                FirstDateTime = today.Add(TimeSpan.Parse("14:42")),
                FirstWeight = 16040m,
                WeigherName = CompanyName,
                OwnerCompany = CompanyName
            });
        }
    }
}
