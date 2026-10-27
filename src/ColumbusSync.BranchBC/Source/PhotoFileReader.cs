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
    /// 1차 계량 사진은 TB_WEIGH.IMGFILE 컬럼으로 연결되는데, 실제 mdb(TSDB.mdb)와 실제
    /// 사진 파일을 직접 대조해본 결과(차량번호까지 사진에서 육안으로 확인) IMGFILE은
    /// "yyyyMMddHHmmssfff"(17자리) 값이고, 실제 파일명은 그 뒤에 구분용 숫자가 하나 더
    /// 붙은 18자리다(예: IMGFILE="20261027071115350" → 파일 "202610270711153501_1.jpg").
    /// 즉 IMGFILE은 파일명 전체가 아니라 "앞부분 접두사"다 - 그래서 정확히 같은 파일명이
    /// 아니라 IMGFILE로 시작하는 파일을 찾아야 한다. 드물게 같은 밀리초에 다른 차량이
    /// 찍혀 접두사가 겹치는 경우가 있는데(정확한 발생 원인은 불명), 그럴 때는 끝자리가
    /// 가장 작은(=가장 먼저 찍힌) 파일을 선택한다 - 실 데이터로 대조해보니 그게 맞는
    /// 파일이었다.
    ///
    /// 2차 계량 사진은 DB에 연결 컬럼이 따로 없다. IMGFILE과 실제 촬영 시각을 대조해보면,
    /// 파일명이 "그 사진을 찍은 정확한 시각"이고 TB_WEIGH의 TIME1/TIME2는 그 시각을 분(分)
    /// 단위로 반올림해 저장한 값이라는 걸 알 수 있었다(1차 기준으로 실제 검증됨). 그래서
    /// 2차 사진은 "2차계량시각(TIME2)과 같은 분에 찍힌, 1차 사진과는 다른 파일"을 찾는
    /// 방식으로 추정 연결한다 - 여러 차량이 같은 분에 몰리면 잘못 연결될 수 있다는 한계는
    /// 있다.
    ///
    /// 같은 배치(RunOnce 1회) 안에서는 같은 날짜 폴더를 여러 계근건이 같이 참조하므로,
    /// 폴더 목록 조회(Directory.GetFiles)를 날짜 폴더당 한 번만 하도록 인스턴스에 캐시한다
    /// (A지점 FtpPhotoReader와 같은 이유) - 이게 없으면 1년치 백필처럼 건수가 많을 때
    /// 같은 날짜 폴더를 계근건 수만큼 반복해서 디스크에서 다시 읽어 매우 느려진다.
    /// </summary>
    public class PhotoFileReader
    {
        private readonly Dictionary<string, List<string>> _dayFolderCache = new Dictionary<string, List<string>>();

        /// <summary>needFirstStage/needSecondStage는 이미 허브 DB에 올라가 있어 다시 읽을
        /// 필요가 없는 단계를 건너뛰기 위한 플래그다(SyncOrchestrator가 HubWriter.GetSyncedPhotoKeys
        /// 조회 결과로 결정) - 매 주기마다 이미 동기화된 사진까지 디스크에서 다시 읽는 낭비를
        /// 막는다.</summary>
        public List<WeighPhoto> ReadPhotos(string imagesFolderRoot, DateTime weighDate, string imgFile, DateTime? secondDateTime, bool needFirstStage, bool needSecondStage)
        {
            var photos = new List<WeighPhoto>();
            if (string.IsNullOrEmpty(imagesFolderRoot) || (!needFirstStage && !needSecondStage))
            {
                return photos;
            }

            var dayFolder = Path.Combine(imagesFolderRoot, weighDate.ToString("yyyyMMdd"));
            var fileNames = GetDayFolderFileNames(dayFolder);
            if (fileNames == null)
            {
                return photos;
            }

            if (needFirstStage && !string.IsNullOrEmpty(imgFile))
            {
                AddPhotosForPrefix(photos, dayFolder, fileNames, imgFile, "1");
            }

            if (needSecondStage && secondDateTime.HasValue)
            {
                var secondBaseName = FindBaseNameForMinute(fileNames, secondDateTime.Value, imgFile);
                if (secondBaseName != null)
                {
                    AddPhotosForPrefix(photos, dayFolder, fileNames, secondBaseName, "2");
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

        private static void AddPhotosForPrefix(List<WeighPhoto> photos, string dayFolder, List<string> fileNames, string basePrefix, string stage)
        {
            for (var seq = 1; seq <= 2; seq++)
            {
                var fileName = FindBestMatch(fileNames, basePrefix, seq);
                if (fileName == null)
                {
                    continue;
                }

                photos.Add(new WeighPhoto
                {
                    Stage = stage,
                    Seq = seq,
                    Data = File.ReadAllBytes(Path.Combine(dayFolder, fileName)),
                });
            }
        }

        /// <summary>basePrefix로 시작하고 "_{seq}"로 끝나는(확장자 제외) 파일을 찾는다.
        /// 여러 건이 걸리면(접두사 겹침) 파일명 기준으로 정렬해 가장 앞에 오는 것 - 즉
        /// 끝자리가 가장 작은 것을 선택한다.</summary>
        private static string FindBestMatch(List<string> fileNames, string basePrefix, int seq)
        {
            var suffix = "_" + seq;
            List<string> matches = null;

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

                if (matches == null)
                {
                    matches = new List<string>();
                }

                matches.Add(name);
            }

            if (matches == null)
            {
                return null;
            }

            matches.Sort(StringComparer.OrdinalIgnoreCase);
            return matches[0];
        }

        /// <summary>targetMinute와 같은 분(分)에 촬영된 "_1" 사진을 찾아 그 베이스 파일명
        /// (확장자/순번 제외, 전체 18자리)을 반환한다. excludePrefix(1차 사진의 IMGFILE
        /// 접두사)로 시작하는 파일은 1차 사진 자신이므로 건너뛴다. 여러 건이 걸리면 그중
        /// 하나(정렬 후 첫 매치)를 돌려준다 - 같은 분에 차량이 여러 대 몰리면 틀린 걸 고를
        /// 수 있다는 한계가 있다.</summary>
        private static string FindBaseNameForMinute(List<string> fileNames, DateTime targetMinute, string excludePrefix)
        {
            var minutePrefix = targetMinute.ToString("yyyyMMddHHmm");
            var candidates = new List<string>();

            foreach (var name in fileNames)
            {
                if (!name.StartsWith(minutePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var nameNoExt = Path.GetFileNameWithoutExtension(name);
                if (nameNoExt.EndsWith("_1", StringComparison.OrdinalIgnoreCase))
                {
                    candidates.Add(name);
                }
            }

            candidates.Sort(StringComparer.OrdinalIgnoreCase);

            foreach (var name in candidates)
            {
                var nameNoExt = Path.GetFileNameWithoutExtension(name);
                if (nameNoExt.Length <= "_1".Length)
                {
                    continue;
                }

                var baseName = nameNoExt.Substring(0, nameNoExt.Length - "_1".Length);
                if (!string.IsNullOrEmpty(excludePrefix) &&
                    baseName.StartsWith(excludePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return baseName;
            }

            return null;
        }
    }
}
