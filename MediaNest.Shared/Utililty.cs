using System.Text.RegularExpressions;

namespace MediaNest.Shared;

public static class Utility {
    public static string GenerateSixDigitCode() {
        int hash = BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0);
        int sixDigit = Math.Abs(hash % 1000000);
        return sixDigit.ToString("000000");
    }
    public static string SanitizeTitle(string raw) {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        // Trim 首尾空白
        string result = raw.Trim();

        // 移除非法字元
        char[] invalid = Path.GetInvalidFileNameChars();
        foreach (char c in invalid)
            result = result.Replace(c, '_');

        // 移除控制字元（0x00–0x1F）
        result = new string([.. result.Where(c => !char.IsControl(c))]);

        // 移除尾端的句點或空白（Windows 不允許）
        result = Regex.Replace(result, @"[\. ]+$", "");

        return result;
    }
}
