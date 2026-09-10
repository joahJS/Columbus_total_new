using System;
using System.Collections.Generic;
using System.IO;

namespace ColumbusWeighing.ComnLib
{
    /// <summary>
    /// 사용자 PC에 저장하는 간단한 Section/Key=Value 설정 파일 헬퍼.
    /// VisionIns 솔루션의 gp_GetIniValue/gp_SetIniValue(INI_NAME.USER)와 같은 역할이며,
    /// 로그인 화면의 "접속정보 기억하기" 값을 저장하는 데 사용한다.
    /// </summary>
    public static class IniHelper
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ColumbusWeighing",
            "User.ini");

        public static string GetValue(string section, string key)
        {
            if (!File.Exists(FilePath))
            {
                return string.Empty;
            }

            var prefix = section + "." + key + "=";
            foreach (var line in File.ReadAllLines(FilePath))
            {
                if (line.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return line.Substring(prefix.Length);
                }
            }

            return string.Empty;
        }

        public static void SetValue(string section, string key, string value)
        {
            var directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var lines = File.Exists(FilePath) ? new List<string>(File.ReadAllLines(FilePath)) : new List<string>();
            var prefix = section + "." + key + "=";
            var index = lines.FindIndex(line => line.StartsWith(prefix, StringComparison.Ordinal));
            var newLine = prefix + value;

            if (index >= 0)
            {
                lines[index] = newLine;
            }
            else
            {
                lines.Add(newLine);
            }

            File.WriteAllLines(FilePath, lines);
        }
    }
}
