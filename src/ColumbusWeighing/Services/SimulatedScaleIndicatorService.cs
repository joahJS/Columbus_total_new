using System;
using System.Globalization;
using System.Windows.Forms;

namespace ColumbusWeighing.Services
{
    /// <summary>
    /// 실물 지시계 없이 UI/업무 흐름을 검증하기 위한 모의(simulation) 계량 서비스.
    /// 무작위 차량 중량으로 상승 -> 안정(정지) -> 0점 복귀를 반복한다.
    /// App.config 의 UseSimulatedScale=false 로 바꾸고 실제 시리얼 통신 구현체로 교체하면 된다.
    /// </summary>
    public sealed class SimulatedScaleIndicatorService : IScaleIndicatorService
    {
        private readonly Timer _timer;
        private readonly Random _random = new Random();

        private decimal _target;
        private int _stableTicks;

        public event EventHandler<WeightChangedEventArgs> WeightChanged;

        public decimal CurrentWeight { get; private set; }

        public bool IsStable { get; private set; }

        public SimulatedScaleIndicatorService()
        {
            _timer = new Timer { Interval = 400 };
            _timer.Tick += OnTick;
            PickNewTarget();
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private void PickNewTarget()
        {
            // 0(공차) 또는 15,000~42,000kg 대의 적재 차량 중량을 무작위로 흉내.
            _target = _random.Next(0, 4) == 0 ? 0m : _random.Next(15000, 42000);
        }

        private void OnTick(object sender, EventArgs e)
        {
            var diff = _target - CurrentWeight;

            if (Math.Abs(diff) <= 20)
            {
                CurrentWeight = _target;
                _stableTicks++;
                IsStable = _stableTicks >= 2;

                if (_stableTicks > 6)
                {
                    PickNewTarget();
                    _stableTicks = 0;
                }
            }
            else
            {
                // 완만하게 목표 중량으로 수렴(적재/하차 진행 중을 흉내).
                var step = Math.Max(60, Math.Abs(diff) / 4);
                CurrentWeight += Math.Sign(diff) * Math.Min(Math.Abs(diff), step);
                IsStable = false;
                _stableTicks = 0;
            }

            var raw = FormatRawPacket(CurrentWeight, IsStable);
            WeightChanged?.Invoke(this, new WeightChangedEventArgs(CurrentWeight, IsStable, raw));
        }

        private static string FormatRawPacket(decimal weight, bool stable)
        {
            // 지시계에서 올라오는 원시 데이터 프레임 예시 형태(로그 패널 표시용).
            return string.Format(
                CultureInfo.InvariantCulture,
                "ST,{0}+{1:00000}kg",
                stable ? "S" : "U",
                (int)weight);
        }

        public void Dispose()
        {
            _timer.Tick -= OnTick;
            _timer.Dispose();
        }
    }
}
