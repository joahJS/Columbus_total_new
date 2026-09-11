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

        /// <summary>감량중량(kg)만 다룬다. 감량률(%)은 계근 건이 아니라 품목 마스터에만 있는
        /// 값이라 연결할 데이터가 없어 항목 자체를 숨겼다(계량 화면 설정 팝업/그리드 컬럼 모두).</summary>
        public bool ShowLossWeight { get; set; }

        public bool ShowPriceInfo { get; set; }

        // 비중/환산중량: MDB·허브 DB·코드 전체 어디에도 이 개념 자체가 없어 항목을 숨겼다.
        // public bool ShowSpecificGravity { get; set; }

        public bool ShowInOutType { get; set; } = true;

        // 담당자: 소스 MDB 어디에도 이 데이터가 없어(계근 시 남는 DAMDANG은 이미 "계량자"로
        // 쓰이고 있음) 항목을 숨겼다.
        // public bool ShowPersonInCharge { get; set; }
    }
}
