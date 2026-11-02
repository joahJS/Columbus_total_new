using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ColumbusSync.BranchBC.Source
{
    /// <summary>계근 사진 1장(사진 바이트 + 어느 단계/순번인지).</summary>
    public class WeighPhoto
    {
        public string Stage { get; set; }  // "1" = 1차, "2" = 2차
        public int Seq { get; set; }        // 1 또는 2
        public byte[] Data { get; set; }
    }

    /// <summary>
    /// TS2020의 IMAGE 폴더(날짜별 하위폴더)에서 계근 사진을 읽어온다.
    ///
    /// 실제 mdb(TSDB.mdb)와 실제 사진 파일, 사진 속 차량번호/적재상태까지 직접 대조해서
    /// 확인한 규칙: TB_WEIGH.IMGFILE은 17자리(yyyyMMddHHmmssfff, 1차 계량 시점의 정밀
    /// 타임스탬프) 값이고, 실제 파일명은 "{IMGFILE}{식별숫자1자리}_{카메라번호}.jpg" 형태다.
    /// 1차 사진은 "{IMGFILE}1_1.jpg"/"{IMGFILE}1_2.jpg"(카메라 1/2), 2차 사진은
    /// "{IMGFILE}2_1.jpg"/"{IMGFILE}2_2.jpg"로, **2차 사진도 1차와 똑같은 IMGFILE
    /// 타임스탬프를 그대로 재사용**하고 그 뒤에 붙는 한 자리 숫자만 1(1차)/2(2차)로
    /// 바뀐다 - 2차 사진 파일명이 "2차 계량이 실제로 일어난 시각"을 반영하는 게 아니다.
    /// (처음에는 "같은 밀리초에 다른 차량이 찍혔다"고 오판했었는데, 차량번호와 적재상태를
    /// 직접 대조해보니 같은 차량의 1차(적재 전/후)와 2차였다.)
    ///
    /// 그래서 2차 사진을 찾을 때 "2차계량시각과 같은 분" 같은 추정이 필요 없다 - 같은
    /// IMGFILE 접두사를 가진 카메라별 파일들을 끝자리 순으로 정렬하면, 첫 번째가 1차,
    /// 두 번째가 2차다(2차가 아직 없으면 그 카메라의 파일이 1개만 있다).
    ///
    /// 같은 배치(RunOnce 1회) 안에서는 같은 날짜 폴더를 여러 계근건이 같이 참조하므로,
    /// 폴더 목록 조회(Directory.GetFiles)를 날짜 폴더당 한 번만 하도록 인스턴스에 캐시한다
    /// (A지점 FtpPhotoReader와 같은 이유).
    /// </summary>
    public class PhotoFileReader
    {
        // 네트워크 드라이브 문제나 다른 프로그램의 파일 잠금 등으로 디스크 I/O가 멈춰버리는
        // 경우를 대비한 보호장치. OleDbHelper의 mdb 쿼리 타임아웃과 같은 이유다 - 실제로
        // "동기화 시작" 로그만 남긴 채 며칠씩 멈추는 사례가 있었는데, mdb 쪽은 이미 보호돼
        // 있어서 이 파일 I/O 쪽이 의심된다.
        private static readonly TimeSpan IoTimeout = TimeSpan.FromSeconds(30);

        private readonly Dictionary<string, List<string>> _dayFolderCache = new Dictionary<string, List<string>>();

        /// <summary>needFirstStage/needSecondStage는 이미 허브 DB에 올라가 있어 다시 읽을
        /// 필요가 없는 단계를 건너뛰기 위한 플래그다(SyncOrchestrator가 HubWriter.GetSyncedPhotoKeys
        /// 조회 결과로 결정) - 매 주기마다 이미 동기화된 사진까지 디스크에서 다시 읽는 낭비를
        /// 막는다.</summary>
        public List<WeighPhoto> ReadPhotos(string imagesFolderRoot, DateTime weighDate, string imgFile, bool needFirstStage, bool needSecondStage)
        {
            var photos = new List<WeighPhoto>();
            if (string.IsNullOrEmpty(imagesFolderRoot) || string.IsNullOrEmpty(imgFile) || (!needFirstStage && !needSecondStage))
            {
                return photos;
            }

            var dayFolder = Path.Combine(imagesFolderRoot, weighDate.ToString("yyyyMMdd"));
            var fileNames = GetDayFolderFileNames(dayFolder);
            if (fileNames == null)
            {
                return photos;
            }

            // 카메라(seq) 1, 2 각각에 대해, 같은 IMGFILE 접두사를 가진 파일을 끝자리 순으로
            // 정렬하면 [0]=1차, [1]=2차다.
            for (var seq = 1; seq <= 2; seq++)
            {
                var matches = FindMatches(fileNames, imgFile, seq);
                if (matches.Count == 0)
                {
                    continue;
                }

                if (needFirstStage)
                {
                    AddPhoto(photos, dayFolder, matches, 0, "1", seq);
                }

                if (needSecondStage && matches.Count > 1)
                {
                    AddPhoto(photos, dayFolder, matches, 1, "2", seq);
                }
            }

            return photos;
        }

        /// <summary>dayFolder 안의 파일명(경로 제외) 목록을 캐시에서 가져오거나, 없으면
        /// 한 번만 디스크에서 읽어 캐시에 넣는다. 폴더가 없으면 null을 캐시해서, 존재하지
        /// 않는 폴더를 매번 다시 확인하는 것도 피한다.</summary>
        private List<string> GetDayFolderFileNames(string dayFolder)
        {
            List<string> fileNames;
            if (_dayFolderCache.TryGetValue(dayFolder, out fileNames))
            {
                return fileNames;
            }

            fileNames = RunWithTimeout(() =>
            {
                if (!Directory.Exists(dayFolder))
                {
                    return null;
                }

                var paths = Directory.GetFiles(dayFolder);
                var names = new List<string>(paths.Length);
                foreach (var path in paths)
                {
                    names.Add(Path.GetFileName(path));
                }

                return names;
            });

            _dayFolderCache[dayFolder] = fileNames;
            return fileNames;
        }

        /// <summary>디스크 I/O 하나를 시간제한 안에서 실행한다. 시간 안에 못 끝나면
        /// TimeoutException을 던진다 - 이 클래스를 쓰는 SyncOrchestrator.SyncPhotos가
        /// 이미 사진 하나 처리 실패를 전부 잡아서 로그만 남기고 다음 건으로 넘어가므로,
        /// 여기서 멈춰버리는 건이 하나 있어도 전체 동기화가 멈추지 않는다.</summary>
        private static T RunWithTimeout<T>(Func<T> action)
        {
            var task = Task.Run(action);
            if (!task.Wait(IoTimeout))
            {
                throw new TimeoutException(string.Format("사진 폴더/파일 I/O가 {0}초 안에 끝나지 않았습니다.", IoTimeout.TotalSeconds));
            }

            return task.Result;
        }

        private static void AddPhoto(List<WeighPhoto> photos, string dayFolder, List<string> matches, int index, string stage, int seq)
        {
            if (index >= matches.Count)
            {
                return;
            }

            var path = Path.Combine(dayFolder, matches[index]);
            photos.Add(new WeighPhoto
            {
                Stage = stage,
                Seq = seq,
                Data = RunWithTimeout(() => File.ReadAllBytes(path)),
            });
        }

        /// <summary>basePrefix(IMGFILE)로 시작하고 "_{seq}"로 끝나는(확장자 제외) 파일을
        /// 전부 찾아 정렬해서 돌려준다. 보통 0~2개가 나온다(없음=1차 사진도 아직 없음,
        /// 1개=1차만, 2개=1차+2차 둘 다).</summary>
        private static List<string> FindMatches(List<string> fileNames, string basePrefix, int seq)
        {
            var suffix = "_" + seq;
            var matches = new List<string>();

            foreach (var name in fileNames)
            {
                if (!name.StartsWith(basePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var nameNoExt = Path.GetFileNameWithoutExtension(name);
                if (!nameNoExt.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                matches.Add(name);
            }

            matches.Sort(StringComparer.OrdinalIgnoreCase);
            return matches;
        }
    }
}
