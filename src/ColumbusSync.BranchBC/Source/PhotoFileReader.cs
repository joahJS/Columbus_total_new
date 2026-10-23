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
    /// TS2020의 IMAGE 폴더(날짜별 하위폴더, 파일명 "yyyyMMddHHmmssfff_1.jpg"/"_2.jpg")에서
    /// 계근 사진을 읽어온다.
    ///
    /// 1차 계량 사진은 TB_WEIGH.IMGFILE 컬럼이 그 베이스 파일명을 그대로 가리키고 있어
    /// 확실하게 연결된다. 반면 2차 계량 사진은 DB에 연결 컬럼이 따로 없다 - IMGFILE과
    /// 실제 촬영 시각을 대조해보면, 베이스 파일명이 "그 사진을 찍은 정확한 시각"이고
    /// TB_WEIGH의 TIME1/TIME2는 그 시각을 분(分) 단위로 반올림해 저장한 값이라는 걸 알 수
    /// 있었다(1차 기준으로 실제 검증됨). 그래서 2차 사진은 "2차계량시각(TIME2)과 같은 분에
    /// 찍힌, 1차 사진과는 다른 베이스 파일명"을 찾는 방식으로 추정 연결한다 - 1차와 같은
    /// 원리이므로 신뢰도가 높을 것으로 보이지만, 실제 배포 후 몇 건 직접 대조해 확인이
    /// 필요하다(여러 차량이 같은 분에 몰리면 잘못 연결될 수 있음).
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
                AddPhotosForBaseName(photos, dayFolder, imgFile, "1");
            }

            if (needSecondStage && secondDateTime.HasValue)
            {
                var secondBaseName = FindBaseNameForMinute(dayFolder, secondDateTime.Value, imgFile);
                if (secondBaseName != null)
                {
                    AddPhotosForBaseName(photos, dayFolder, secondBaseName, "2");
                }
            }

            return photos;
        }

        private static void AddPhotosForBaseName(List<WeighPhoto> photos, string dayFolder, string baseName, string stage)
        {
            for (var seq = 1; seq <= 2; seq++)
            {
                var path = FindFile(dayFolder, baseName, seq);
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

        private static string FindFile(string dayFolder, string baseName, int seq)
        {
            var pattern = string.Format("{0}_{1}.*", baseName, seq);
            var matches = Directory.GetFiles(dayFolder, pattern);
            return matches.Length > 0 ? matches[0] : null;
        }

        /// <summary>같은 날짜 폴더 안에서 targetMinute와 같은 분(分)에 촬영된 "_1" 사진을
        /// 찾아 그 베이스 파일명을 반환한다. excludeBaseName(1차 사진)과 같으면 건너뛴다.
        /// 여러 건이 걸리면 그중 하나(첫 매치)를 돌려준다 - 같은 분에 차량이 여러 대 몰리면
        /// 틀린 걸 고를 수 있다는 한계가 있다.</summary>
        private static string FindBaseNameForMinute(string dayFolder, DateTime targetMinute, string excludeBaseName)
        {
            var minutePrefix = targetMinute.ToString("yyyyMMddHHmm");
            var files = Directory.GetFiles(dayFolder, minutePrefix + "*_1.*");
            foreach (var file in files)
            {
                var fileNameNoExt = Path.GetFileNameWithoutExtension(file);
                if (fileNameNoExt.Length <= "_1".Length)
                {
                    continue;
                }

                var baseName = fileNameNoExt.Substring(0, fileNameNoExt.Length - "_1".Length);
                if (!string.Equals(baseName, excludeBaseName, StringComparison.OrdinalIgnoreCase))
                {
                    return baseName;
                }
            }

            return null;
        }
    }
}
