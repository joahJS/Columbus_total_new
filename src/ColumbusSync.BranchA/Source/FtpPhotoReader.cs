using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using ColumbusSync.BranchA.Logging;

namespace ColumbusSync.BranchA.Source
{
    /// <summary>계근 사진 1장(사진 바이트 + 어느 단계/순번인지).</summary>
    public class WeighPhoto
    {
        public string Stage { get; set; }  // "1" = 1차, "2" = 2차
        public int Seq { get; set; }        // 1 또는 2
        public byte[] Data { get; set; }
    }

    /// <summary>
    /// Columbus_MES(Demo_weigh_adm/Columbus_MES/03SA/SA015F00.cs, SetPictures())가 쓰는 것과
    /// 동일한 FTP 서버에서 계근 사진을 읽어온다. A지점은 B/C지점과 달리 사진 파일명이
    /// SLINO(원본 PK)로 완전히 결정적이다(예: "{SLINO}_1_1.jpg") - B/C지점처럼 촬영 시각으로
    /// 추정해 연결할 필요가 없다.
    ///
    /// 경로 규칙: {FtpPath}Images/{yyyy}/{MM}/{yyyy-MM-dd}/{SLINO}_{1|2}_{1|2}.jpg
    /// (1번째 숫자 = 1차/2차 계량, 2번째 숫자 = 그 단계 안에서의 사진 순번)
    /// </summary>
    public class FtpPhotoReader
    {
        private readonly string _ftpPath;
        private readonly string _userId;
        private readonly string _password;

        // 같은 배치(RunOnce 1회) 안에서는 같은 날짜 폴더를 여러 계근건이 같이 참조하므로,
        // 폴더 목록 조회(FTP ListDirectory)를 날짜 폴더당 한 번만 하도록 캐시한다.
        private readonly Dictionary<string, HashSet<string>> _directoryCache = new Dictionary<string, HashSet<string>>();

        public FtpPhotoReader(string ftpPath, string userId, string password)
        {
            _ftpPath = ftpPath;
            _userId = userId;
            _password = password;
        }

        public List<WeighPhoto> ReadPhotos(DateTime tDate, string slino, bool needFirstStage, bool needSecondStage)
        {
            var photos = new List<WeighPhoto>();
            if (string.IsNullOrEmpty(slino) || (!needFirstStage && !needSecondStage))
            {
                return photos;
            }

            var folderUrl = string.Format("{0}Images/{1}/{2}/{3}/", _ftpPath, tDate.ToString("yyyy"), tDate.ToString("MM"), tDate.ToString("yyyy-MM-dd"));

            HashSet<string> filesInFolder;
            if (!_directoryCache.TryGetValue(folderUrl, out filesInFolder))
            {
                filesInFolder = ListDirectory(folderUrl);
                _directoryCache[folderUrl] = filesInFolder;
            }

            if (needFirstStage)
            {
                TryAddPhoto(photos, folderUrl, filesInFolder, slino, "1", 1);
                TryAddPhoto(photos, folderUrl, filesInFolder, slino, "1", 2);
            }

            if (needSecondStage)
            {
                TryAddPhoto(photos, folderUrl, filesInFolder, slino, "2", 1);
                TryAddPhoto(photos, folderUrl, filesInFolder, slino, "2", 2);
            }

            return photos;
        }

        private void TryAddPhoto(List<WeighPhoto> photos, string folderUrl, HashSet<string> filesInFolder, string slino, string stage, int seq)
        {
            var fileName = string.Format("{0}_{1}_{2}.jpg", slino, stage, seq);
            if (!filesInFolder.Contains(fileName))
            {
                return;
            }

            var data = DownloadFile(folderUrl + fileName);
            if (data != null)
            {
                photos.Add(new WeighPhoto { Stage = stage, Seq = seq, Data = data });
            }
        }

        private HashSet<string> ListDirectory(string folderUrl)
        {
            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var request = CreateRequest(folderUrl, WebRequestMethods.Ftp.ListDirectory);

                    using (var response = (FtpWebResponse)request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            // 서버가 파일명만 돌려주는 걸 기준으로 한다(SA015F00.cs와 동일한 가정).
                            result.Add(Path.GetFileName(line.Trim()));
                        }
                    }

                    return result;
                }
                catch (WebException ex)
                {
                    // "550"(파일/폴더 없음)은 그날 사진 폴더가 아예 없는 정상적인 경우일 수 있다
                    // (예: 그 날짜엔 카메라 설정이 꺼져 있었음) - 조용히 빈 목록으로 넘어간다.
                    if (IsFileUnavailable(ex))
                    {
                        return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    }

                    // 그 외(연결 끊김, 인증 실패, 타임아웃 등)는 한 번 재시도해보고, 그래도 안
                    // 되면 원인을 알아야 하므로 로그를 남긴다 - 안 그러면 사진이 하나도 안
                    // 쌓여도 어디서 막히는지 전혀 알 수 없다.
                    if (attempt >= MaxAttempts)
                    {
                        FileLogger.Error(string.Format("FTP 디렉토리 조회 실패: {0}", folderUrl), ex);
                        return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    }
                }
            }

            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsFileUnavailable(WebException ex)
        {
            var response = ex.Response as FtpWebResponse;
            return response != null && response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable;
        }

        private const int MaxAttempts = 2;

        private byte[] DownloadFile(string fileUrl)
        {
            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    var request = CreateRequest(fileUrl, WebRequestMethods.Ftp.DownloadFile);

                    using (var response = (FtpWebResponse)request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    using (var memory = new MemoryStream())
                    {
                        stream.CopyTo(memory);
                        return memory.ToArray();
                    }
                }
                catch (WebException ex)
                {
                    if (IsFileUnavailable(ex))
                    {
                        return null;
                    }

                    if (attempt >= MaxAttempts)
                    {
                        FileLogger.Error(string.Format("FTP 파일 다운로드 실패: {0}", fileUrl), ex);
                        return null;
                    }
                }
            }

            return null;
        }

        /// <summary>.NET의 FtpWebRequest는 기본적으로 요청 간 연결을 재사용하는데(KeepAlive),
        /// 이 연결 재사용 경로에 "연결이 닫혔습니다 / 프로토콜 위반" 같은 오류를 간헐적으로
        /// 일으키는 오래된 버그가 있다 - 짧은 시간에 여러 파일을 연속으로 받을 때 특히 잘
        /// 드러난다. 매 요청마다 새 연결을 쓰도록 강제해서 피한다.</summary>
        private FtpWebRequest CreateRequest(string url, string method)
        {
            var request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.Credentials = new NetworkCredential(_userId, _password);
            request.UsePassive = true;
            request.KeepAlive = false;
            return request;
        }
    }
}
