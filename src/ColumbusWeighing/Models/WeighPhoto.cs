namespace ColumbusWeighing.Models
{
    /// <summary>계근 사진 1장(계근표 출력용). 허브 DB의 dbo.WEIGH_PHOTO 1행과 대응된다.</summary>
    public class WeighPhoto
    {
        public string Stage { get; set; }  // "1" = 1차, "2" = 2차
        public int Seq { get; set; }        // 1 또는 2 (카메라 번호)
        public byte[] Data { get; set; }
    }
}
