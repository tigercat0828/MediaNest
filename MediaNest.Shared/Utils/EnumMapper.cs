using System.Reflection;

namespace MediaNest.Shared.Utils;

[AttributeUsage(AttributeTargets.Field)]
public class DbCodeAttribute : Attribute {
    public string Code { get; }
    public DbCodeAttribute(string code) {
        Code = code;
    }
}
public static class EnumExtensions {
    public static string ToDb<TEnum>(this TEnum value) where TEnum : struct, Enum
        => EnumMapper<TEnum>.ToDb(value);

    public static TEnum FromDb<TEnum>(this string code) where TEnum : struct, Enum
        => EnumMapper<TEnum>.FromDb(code);
}
public static class EnumMapper<TEnum> where TEnum : struct, Enum {
    private static readonly Dictionary<string, TEnum> _fromDb = new();
    private static readonly Dictionary<TEnum, string> _toDb = new();

    static EnumMapper() {
        foreach (var value in Enum.GetValues<TEnum>()) {
            var field = typeof(TEnum).GetField(value.ToString())!;
            var attr = field.GetCustomAttribute<DbCodeAttribute>()
                       ?? throw new Exception($"{typeof(TEnum).Name}.{value} is missing DbCodeAttribute");

            if (_fromDb.ContainsKey(attr.Code))
                throw new Exception($"Duplicate DbCode '{attr.Code}' in {typeof(TEnum).Name}");

            _fromDb[attr.Code] = value;
            _toDb[value] = attr.Code;
        }
    }

    public static TEnum FromDb(string code) {
        if (_fromDb.TryGetValue(code, out var value))
            return value;
        throw new Exception($"Unknown code '{code}' for {typeof(TEnum).Name}");
    }

    public static string ToDb(TEnum value) {
        if (_toDb.TryGetValue(value, out var code))
            return code;
        throw new Exception($"Unknown enum value '{value}'");
    }
}
