namespace LauncherMiddleware.Models;

public enum GameName
{
    [StringValue("warhammer3")] warhammer3,
    [StringValue("warhammer2")] warhammer2,
    [StringValue("threekingdoms")] threekingdoms
}

public class StringValueAttribute : Attribute
{
    public StringValueAttribute ()
    {
    }

    public StringValueAttribute (string value)
    {
        StringValue = value;
    }

    private string StringValue { get; }

    public override string ToString ()
    {
        return StringValue;
    }
}

public static class EnumGamesHelper
{
    public static TAttribute GetAttribute <TAttribute> (this Enum value) where TAttribute : Attribute
    {
        var enumType = value.GetType();
        string name = Enum.GetName(enumType, value) ?? string.Empty;
        var a = enumType.GetField(name).GetCustomAttributes(false).OfType<TAttribute>().SingleOrDefault();
        return a ?? string.Empty; // TODO
    }
}
