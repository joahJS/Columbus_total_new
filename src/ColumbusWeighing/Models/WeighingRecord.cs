using System;
using System.ComponentModel;

namespace ColumbusWeighing.Models
{
    /// <summary>
    /// 1건의 차량 계근(計斤) 기록. 1차 계량 시 생성되고, 2차 계량이 완료되면
    /// SecondDateTime/SecondWeight 가 채워져 2차계량 화면에 표시된다.
    /// </summary>
    public class WeighingRecord : INotifyPropertyChanged
    {
        private decimal? _secondWeight;
        private DateTime? _secondDateTime;
        private decimal? _vendorWeight;

        public int Id { get; set; }

        /// <summary>원본 지점 코드(A/B/C). 화면에는 BranchCodeExtensions.ToDisplayString()으로 지역명을 표시한다.</summary>
        public string BranchCode { get; set; }

        /// <summary>당일 계량 순번.</summary>
        public int WeighSeq { get; set; }

        public string VehicleNo { get; set; }

        public string CustomerName { get; set; }

        public string ProductName { get; set; }

        public InOutType InOutType { get; set; }

        /// <summary>계량자(로그인 사용자/회사).</summary>
        public string WeigherName { get; set; }

        /// <summary>차량 소속 회사.</summary>
        public string OwnerCompany { get; set; }

        public string DriverName { get; set; }

        public decimal? UnitPrice { get; set; }

        public decimal? SpecificGravity { get; set; }

        public string PersonInCharge { get; set; }

        public string Remark { get; set; }

        public DateTime FirstDateTime { get; set; }

        public decimal FirstWeight { get; set; }

        public DateTime? SecondDateTime
        {
            get { return _secondDateTime; }
            set { _secondDateTime = value; OnPropertyChanged(nameof(SecondDateTime)); OnPropertyChanged(nameof(IsCompleted)); }
        }

        public decimal? SecondWeight
        {
            get { return _secondWeight; }
            set { _secondWeight = value; OnPropertyChanged(nameof(SecondWeight)); OnPropertyChanged(nameof(NetWeight)); OnPropertyChanged(nameof(Loss)); }
        }

        /// <summary>업체중량. 동기화 대상이 아니라 이 프로그램에서 사용자가 직접 입력하는 값이다
        /// (DB의 WEIGH_RECORD.VENDOR_WEIGHT, SqlWeighingRepository.UpdateVendorWeight 참고).</summary>
        public decimal? VendorWeight
        {
            get { return _vendorWeight; }
            set { _vendorWeight = value; OnPropertyChanged(nameof(VendorWeight)); OnPropertyChanged(nameof(Loss)); }
        }

        /// <summary>2차 계량까지 완료된 건인지 여부. true 이면 2차계량 화면 대상.</summary>
        public bool IsCompleted
        {
            get { return SecondDateTime.HasValue; }
        }

        /// <summary>순중량 = |2차중량 - 1차중량|.</summary>
        public decimal? NetWeight
        {
            get
            {
                if (!SecondWeight.HasValue)
                {
                    return null;
                }

                return Math.Abs(SecondWeight.Value - FirstWeight);
            }
        }

        public decimal? LossRate { get; set; }

        public decimal? LossWeight { get; set; }

        /// <summary>로스 = 당사중량(NetWeight) - 업체중량. 업체중량이 입력된 건만 계산된다.</summary>
        public decimal? Loss
        {
            get
            {
                if (!VendorWeight.HasValue || !NetWeight.HasValue)
                {
                    return null;
                }

                return NetWeight.Value - VendorWeight.Value;
            }
        }

        public decimal? Amount
        {
            get
            {
                if (!UnitPrice.HasValue || !NetWeight.HasValue)
                {
                    return null;
                }

                return UnitPrice.Value * NetWeight.Value;
            }
        }

        public decimal? ConvertedWeight
        {
            get
            {
                if (!SpecificGravity.HasValue || !NetWeight.HasValue)
                {
                    return null;
                }

                return NetWeight.Value * SpecificGravity.Value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
