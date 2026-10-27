using System;
using System.Collections.Generic;
using System.IO;

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

            if (Directory.Exists(dayFolder))
            {
                var paths = Directory.GetFiles(dayFolder);
                fileNames = new List<string>(paths.Length);
                foreach (var path in paths)
                {
                    fileNames.Add(Path.GetFileName(path));
                }
            }
            else
            {
                fileNames = null;
            }

            _dayFolderCache[dayFolder] = fileNames;
            return fileNames;
        }

        private static void AddPhoto(List<WeighPhoto> photos, string dayFolder, List<string> matches, int index, string stage, int seq)
        {
            if (index >= matches.Count)
            {
                return;
            }

            photos.Add(new WeighPhoto
            {
                Stage = stage,
                Seq = seq,
                Data = File.ReadAllBytes(Path.Combine(dayFolder, matches[index])),
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
