namespace ColumbusWeighing.Models
{
    /// <summary>IP 카메라 1대의 접속 정보. 시스템 설정 화면의 "IP 카메라 설정" 표 한 행에 대응한다.</summary>
    public class IpCameraSetting
    {
        public int No { get; set; }

        public string Ip { get; set; }

        public int VnpPort { get; set; }

        public int HttpPort { get; set; }

        public string UserId { get; set; }

        public string Password { get; set; }

        public string Model { get; set; }
    }
}
