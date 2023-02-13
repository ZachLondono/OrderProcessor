namespace ApplicationCore.Features.Shared.Domain;

public class ShortGuid {

    private readonly Guid _guid;
    private readonly string _value;

    public ShortGuid(Guid guid) {
        _guid = guid;
        _value = Convert.ToBase64String(guid.ToByteArray())
                        .Substring(0, 22)
                        .Replace("/", "_")
                        .Replace("+", "-");
    }

    public override string ToString() => _value;

    public Guid ToGuid() => _guid;

    public static ShortGuid Parse(string shortGuid) {
        if (shortGuid.Length != 22) {
            throw new FormatException("Input string was not in a correct format.");
        }

        return new ShortGuid(new Guid(Convert.FromBase64String(shortGuid.Replace("_", "/").Replace("-", "+") + "==")));
    }

    public static implicit operator string(ShortGuid guid) => guid.ToString();

    public static implicit operator Guid(ShortGuid shortGuid) => shortGuid._guid;

}
