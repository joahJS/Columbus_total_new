using System;
using System.ComponentModel;
using System.Reflection;
using ColumbusWeighing.Models;

namespace ColumbusWeighing.Services
{
    /// <summary>
    /// 메모리 기반 버전 이력 저장소. 프로그램 시작 시 현재 배포 버전을 최초 이력으로 적재한다.
    /// </summary>
    public sealed class InMemoryVersionRepository : IVersionRepository
    {
        public BindingList<VersionRecord> Records { get; } = new BindingList<VersionRecord>();

        private int _nextId = 1;

        public InMemoryVersionRepository()
        {
            SeedSampleData();
        }

        public VersionRecord AddVersion(
            string versionId,
            DateTime uploadDate,
            string fileName,
            byte[] fileData,
            string remark,
            string uploadedBy)
        {
            var record = new VersionRecord
            {
                Id = _nextId++,
                VersionId = versionId,
                UploadDate = uploadDate,
                FileName = fileName,
                FileSize = fileData?.LongLength ?? 0,
                FileData = fileData,
                Remark = remark,
                UploadedBy = uploadedBy
            };

            Records.Add(record);
            return record;
        }

        private void SeedSampleData()
        {
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            Records.Add(new VersionRecord
            {
                Id = _nextId++,
                VersionId = currentVersion,
                UploadDate = DateTime.Today,
                FileName = "ColumbusWeighing.exe",
                FileSize = 0,
                Remark = "최초 배포 버전",
                UploadedBy = "admin"
            });
        }
    }
}
