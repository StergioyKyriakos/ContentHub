namespace ContentHub.Data.Entities.Common;

public sealed class DomainError
{
    public string Code { get; }

    public string Message { get; }

    private DomainError(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public static DomainError Create(string code, string message)
    {
        return new DomainError(code, message);
    }
}