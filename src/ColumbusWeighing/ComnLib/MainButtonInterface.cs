namespace ColumbusWeighing.ComnLib
{
    /// <summary>
    /// 공용 툴바 버튼(조회/추가/저장/삭제/출력/엑셀/닫기)의 동작을 화면마다 위임 구현하기 위한 인터페이스.
    /// VisionIns 솔루션의 MainButtonInterface와 동일하다. Columbus는 아직 MDI 다중 화면 구조가 아니라
    /// 구현체는 없지만, 추후 화면이 늘어나 공용 툴바로 전환할 때를 대비해 구조만 맞춰 둔다.
    /// </summary>
    public interface MainButtonInterface
    {
        void RETR();

        void ADD();

        void SAVE();

        void DELETE();

        void PRINT();

        void XLS();

        void CLOSE();
    }
}
