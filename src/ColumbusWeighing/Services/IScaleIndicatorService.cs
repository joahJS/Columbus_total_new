using System;

namespace ColumbusWeighing.Services
{
    public class WeightChangedEventArgs : EventArgs
    {
        public WeightChangedEventArgs(decimal weight, bool isStable, string rawPacket)
        {
            Weight = weight;
            IsStable = isStable;
            RawPacket = rawPacket;
        }

        /// <summary>현재 중량(kg).</summary>
        public decimal Weight { get; }

        /// <summary>계량값 안정(정지) 여부. 트럭이 완전히 멈춰 값이 흔들리지 않을 때 true.</summary>
        public bool IsStable { get; }

        /// <summary>지시계에서 수신한 원본 데이터 패킷(통신 로그 표시용).</summary>
        public string RawPacket { get; }
    }

    /// <summary>
    /// 계근대 중량 지시계(Weighing Indicator) 통신 서비스 추상화.
    /// 실제 현장에서는 RS-232/RS-485 시리얼 포트로 지시계와 통신하는 구현체
    /// (예: SerialScaleIndicatorService)를 이 인터페이스로 교체 투입한다.
    /// </summary>
    public interface IScaleIndicatorService : IDisposable
    {
        event EventHandler<WeightChangedEventArgs> WeightChanged;

        decimal CurrentWeight { get; }

        bool IsStable { get; }

        void Start();

        void Stop();
    }
}
