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
    /// </summary>
    public static class PhotoFileReader
    {
        /// <summary>needFirstStage/needSecondStage는 이미 허브 DB에 올라가 있어 다시 읽을
        /// 필요가 없는 단계를 건너뛰기 위한 플래그다(SyncOrchestrator가 HubWriter.GetSyncedPhotoKeys
        /// 조회 결과로 결정) - 매 주기마다 이미 동기화된 사진까지 디스크에서 다시 읽는 낭비를
        /// 막는다.</summary>
        public static List<WeighPhoto> ReadPhotos(string imagesFolderRoot, DateTime weighDate, string imgFile, DateTime? secondDateTime, bool needFirstStage, bool needSecondStage)
        {
            var photos = new List<WeighPhoto>();
            if (string.IsNullOrEmpty(imagesFolderRoot) || (!needFirstStage && !needSecondStage))
            {
                return photos;
            }

            var dayFolder = Path.Combine(imagesFolderRoot, weighDate.ToString("yyyyMMdd"));
            if (!Directory.Exists(dayFolder))
            {
                return photos;
            }

            if (needFirstStage && !string.IsNullOrEmpty(imgFile))
            {
                AddPhotosForPrefix(photos, dayFolder, imgFile, "1");
            }

            if (needSecondStage && secondDateTime.HasValue)
            {
                var secondBaseName = FindBaseNameForMinute(dayFolder, secondDateTime.Value, imgFile);
                if (secondBaseName != null)
                {
                    AddPhotosForPrefix(photos, dayFolder, secondBaseName, "2");
                }
            }

            return photos;
        }

        private static void AddPhotosForPrefix(List<WeighPhoto> photos, string dayFolder, string basePrefix, string stage)
        {
            for (var seq = 1; seq <= 2; seq++)
            {
                var path = FindBestMatch(dayFolder, basePrefix, seq);
                if (path == null)
                {
                    continue;
                }

                photos.Add(new WeighPhoto
                {
                    Stage = stage,
                    Seq = seq,
                    Data = File.ReadAllBytes(path),
                });
            }
        }

        /// <summary>basePrefix로 시작하는 "_{seq}.*" 파일을 찾는다. 여러 건이 걸리면(접두사
        /// 겹침) 파일명 기준으로 정렬해 가장 먼저 오는 것 - 즉 끝자리가 가장 작은 것을
        /// 선택한다.</summary>
        private static string FindBestMatch(string dayFolder, string basePrefix, int seq)
        {
            var pattern = string.Format("{0}*_{1}.*", basePrefix, seq);
            var matches = Directory.GetFiles(dayFolder, pattern);
            if (matches.Length == 0)
            {
                return null;
            }

            Array.Sort(matches, StringComparer.OrdinalIgnoreCase);
            return matches[0];
        }

        /// <summary>같은 날짜 폴더 안에서 targetMinute와 같은 분(分)에 촬영된 "_1" 사진을
        /// 찾아 그 베이스 파일명(확장자/순번 제외, 전체 18자리)을 반환한다. excludePrefix(1차
        /// 사진의 IMGFILE 접두사)로 시작하는 파일은 1차 사진 자신이므로 건너뛴다. 여러 건이
        /// 걸리면 그중 하나(정렬 후 첫 매치)를 돌려준다 - 같은 분에 차량이 여러 대 몰리면
        /// 틀린 걸 고를 수 있다는 한계가 있다.</summary>
        private static string FindBaseNameForMinute(string dayFolder, DateTime targetMinute, string excludePrefix)
        {
            var minutePrefix = targetMinute.ToString("yyyyMMddHHmm");
            var files = Directory.GetFiles(dayFolder, minutePrefix + "*_1.*");
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                var fileNameNoExt = Path.GetFileNameWithoutExtension(file);
                if (fileNameNoExt.Length <= "_1".Length)
                {
                    continue;
                }

                var baseName = fileNameNoExt.Substring(0, fileNameNoExt.Length - "_1".Length);
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
