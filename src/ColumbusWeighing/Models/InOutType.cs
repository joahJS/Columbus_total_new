namespace ColumbusWeighing.Models
{
    /// <summary>입/출고 구분.</summary>
    public enum InOutType
    {
        In = 0,
        Out = 1
    }

    public static class InOutTypeExtensions
    {
        public static string ToDisplayString(this InOutType value)
        {
            return value == InOutType.In ? "입고" : "출고";
        }

        /// <summary>2차 계량이 아직 안 끝나 입/출고가 정해지지 않은 건(null)은 빈 문자열로
        /// 표시한다 - "출고"로 단정하면 안 된다.</summary>
        public static string ToDisplayString(this InOutType? value)
        {
            return value.HasValue ? value.Value.ToDisplayString() : string.Empty;
        }
    }
}
