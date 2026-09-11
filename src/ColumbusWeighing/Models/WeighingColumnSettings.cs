namespace ColumbusWeighing.Models
{
    /// <summary>
    /// "계량 화면 설정" 팝업에서 켜고 끄는 1차/2차 계량 그리드의 컬럼 표시 여부.
    /// 차량번호/1차·2차·순중량/계량자/비고처럼 항상 보여줘야 하는 핵심 컬럼은 여기 포함하지
    /// 않고, 선택적으로 켜고 끌 수 있는 부가 컬럼만 담는다. 기본값은 참고 화면의 초기상태와
    /// 동일하다.
    /// </summary>
    public class WeighingColumnSettings
    {
        public bool ShowWeighSeq { get; set; } = true;
        public bool ShowFirstDateTime { get; set; } = true;
        public bool ShowSecondDateTime { get; set; } = true;
        public bool ShowOwnerCompany { get; set; }
        public bool ShowProductName { get; set; } = true;
        public bool ShowCustomerName { get; set; } = true;
        public bool ShowDriverName { get; set; }
        public bool ShowLossInfo { get; set; }
        public bool ShowPriceInfo { get; set; }
        public bool ShowSpecificGravity { get; set; }
        public bool ShowInOutType { get; set; } = true;
        public bool ShowPersonInCharge { get; set; }
    }
}
