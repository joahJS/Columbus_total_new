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
    }
}
