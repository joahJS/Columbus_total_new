using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ColumbusSync.BranchA
{
    /// <summary>
    /// 완료된(2차계량까지 끝난) 계근건은 한번 동기화되면 거의 바뀌지 않는다. 그런데 MES
    /// MEASURE_RETR 프로시저는 날짜 범위로만 조회되고 수정시각 기준 필터를 지원하지 않아서
    /// (B/C지점 mdb의 WORK_DATE 같은 컬럼이 없음), 기본 조회기간(WeighSyncLookbackDays,
    /// 1년) 안의 완료건을 매 주기(10분)마다 통째로 다시 받게 된다. MES 조회 자체는 사내망이라
    /// 빠르지만, 그걸 매번 인터넷 건너 허브 DB(COLUMBUS_WEIGH_HUB)에 업서트하는 왕복은
    /// 비용이 크다.
    ///
    /// 그래서 "일단 MES에서는 다 받고, 내용이 실제로 바뀐 건만 허브에 올린다" 방식을 쓴다.
    /// 직전 동기화 때 계산해둔 내용 해시를 로컬 파일에 저장해두고, 이번에 받은 레코드의
    /// 내용 해시가 그대로면 허브 업서트를 건너뛴다 - B/C지점의 WORK_DATE 방식보다는 원본
    /// 조회 비용을 줄이지 못하지만(매번 전체 기간을 다시 받음), 가장 비용이 큰 허브 왕복은
    /// 똑같이 줄여준다.
    ///
    /// 캐시 파일이 없거나(최초 실행, 재배포 등) 읽기에 실패하면 그냥 빈 캐시로 시작한다.
    /// 최악의 경우 그 한 번만 전체 업서트를 다시 하게 될 뿐이고(멱등적인 MERGE라서 데이터가
    /// 틀리게 되는 일은 없다), 사진 동기화는 HubWriter.GetSyncedPhotoKeys로 따로 걸러지므로
    /// 이 캐시와 무관하게 항상 자체적으로 누락 여부를 다시 확인한다.
    /// </summary>
    public class WeighHashCache
    {
        private readonly string _filePath;
        private readonly Dictionary<string, string> _hashes;

        private WeighHashCache(string filePath, Dictionary<string, string> hashes)
        {
            _filePath = filePath;
            _hashes = hashes;
        }

        public static WeighHashCache Load(string filePath)
        {
            var hashes = new Dictionary<string, string>();

            try
            {
                if (File.Exists(filePath))
                {
                    foreach (var line in File.ReadAllLines(filePath))
                    {
                        var sep = line.IndexOf('\t');
                        if (sep > 0)
                        {
                            hashes[line.Substring(0, sep)] = line.Substring(sep + 1);
                        }
                    }
                }
            }
            catch (IOException)
            {
                hashes.Clear();
            }

            return new WeighHashCache(filePath, hashes);
        }

        /// <summary>sourceKey의 내용이 마지막으로 저장해둔 해시와 같으면 true(= 허브 업서트를
        /// 건너뛰어도 됨)를 반환한다. 호출과 동시에 캐시를 이번에 본 내용(contentHash)으로
        /// 갈아 끼운다 - 다음 비교 기준은 항상 "가장 최근에 실제로 본 내용"이어야 한다.</summary>
        public bool IsUnchanged(string sourceKey, string contentHash)
        {
            string existing;
            var unchanged = _hashes.TryGetValue(sourceKey, out existing) && existing == contentHash;
            _hashes[sourceKey] = contentHash;
            return unchanged;
        }

        public void Save()
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var tempPath = _filePath + ".tmp";
            using (var writer = new StreamWriter(tempPath, false, Encoding.UTF8))
            {
                foreach (var pair in _hashes)
                {
                    writer.Write(pair.Key);
                    writer.Write('\t');
                    writer.WriteLine(pair.Value);
                }
            }

            File.Copy(tempPath, _filePath, true);
            File.Delete(tempPath);
        }

        public static string ComputeHash(string content)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(content ?? string.Empty));
                var sb = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes)
                {
                    sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
                }

                return sb.ToString();
            }
        }
    }
}
