namespace PrecastTracker.Business.Core;

public class BusinessResult<T>
{
    public bool Succeeded { get; }
    public T? Data { get; }
    public IReadOnlyList<BusinessError> Errors { get; }

    private BusinessResult(bool succeeded, T? data, IReadOnlyList<BusinessError> errors)
    {
        Succeeded = succeeded;
        Data = data;
        Errors = errors;
    }

    public static BusinessResult<T> Success(T data)
        => new(true, data, new List<BusinessError>());

    public static BusinessResult<T> Failure(BusinessError error)
        => new(false, default, new List<BusinessError> { error });

    public static BusinessResult<T> Failure(IEnumerable<BusinessError> errors)
        => new(false, default, errors.ToList());
}

public class BusinessResult
{
    public bool Succeeded { get; }
    public IReadOnlyList<BusinessError> Errors { get; }

    private BusinessResult(bool succeeded, IReadOnlyList<BusinessError> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public static BusinessResult Success()
        => new(true, new List<BusinessError>());

    public static BusinessResult Failure(BusinessError error)
        => new(false, new List<BusinessError> { error });

    public static BusinessResult Failure(IEnumerable<BusinessError> errors)
        => new(false, errors.ToList());
}
