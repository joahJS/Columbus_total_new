using System.Drawing;
using DevExpress.XtraEditors;

namespace ColumbusWeighing.Controls
{
    /// <summary>
    /// 계근대 현재 중량을 크게 표시하는 LED 스타일 표시기.
    /// 값 안정(정지) 시 녹색, 변동(적재/이동 중) 시 호박색으로 색이 바뀐다.
    /// </summary>
    public partial class WeightDisplayControl : XtraUserControl
    {
        private static readonly Color StableColor = Color.Lime;
        private static readonly Color UnstableColor = Color.FromArgb(255, 176, 0);

        private decimal _value;

        public WeightDisplayControl()
        {
            InitializeComponent();
        }

        public decimal Value
        {
            get { return _value; }
            set
            {
                _value = value;
                _valueLabel.Text = string.Format("{0:N0} kg", _value);
            }
        }

        public void SetStable(bool isStable)
        {
            _valueLabel.Appearance.ForeColor = isStable ? StableColor : UnstableColor;
        }
    }
}
