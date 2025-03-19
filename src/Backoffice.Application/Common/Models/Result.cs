namespace Backoffice.Application.Common.Models;

/// <summary>
/// Servis operasyonları için sonuç modeli
/// </summary>
public class Result
{
    public bool Succeeded { get; }
    public string[] Errors { get; }

    internal Result(bool succeeded, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
    }

    public static Result Success()
    {
        return new Result(true, Array.Empty<string>());
    }

    public static Result Failure(IEnumerable<string> errors)
    {
        return new Result(false, errors);
    }

    public static Result Failure(string error)
    {
        return new Result(false, [error]);
    }
}