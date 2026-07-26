namespace RoomBooking.Api.Shared.Http;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public ApiError? Error { get; init; }

    public static ApiResponse<T> Ok(T? data = default) => new()
    {
        Success = true,
        Data = data
    };

    public static ApiResponse<T> Fail(string detail, string? code = null) => new()
    {
        Success = false,
        Error = new ApiError(detail, code)
    };
}
