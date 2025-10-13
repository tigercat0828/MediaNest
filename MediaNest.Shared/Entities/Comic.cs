using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace MediaNest.Shared.Entities;

public class Comic {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;


    [Required]
    [RegularExpression(@"^[^\/:*?""<>|]+$", ErrorMessage = "Title 不可包含 / \\ : * ? \" < > | 這些字元")]
    public string Title {
        get => _title;
        set { _title = SanitizeTitle(value); }
    }

    private string _title = string.Empty;

    public string SubTitle { get; set; } = string.Empty;
    public string Series { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public List<string> Characters { get; set; } = [];
    public List<int> Bookmarks { get; set; } = [];
    public string Uploader { get; set; } = string.Empty;
    public string Code { get; set; } = "xxxxxx";
    [BsonIgnore]
    public string FolderName => Path.Combine(Code[..3], $"[{Code}]{Title}");    // retrieve asset path (physical hierarchy directories)

    private static string SanitizeTitle(string raw) {
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
