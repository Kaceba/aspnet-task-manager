namespace TaskManager.Core.DTOs.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> ErrorResponse(string error, T? data = default)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = data,
            Errors = new List<string> { error }
        };
    }

    public static ApiResponse<T> ErrorResponse(List<string> errors, T? data = default)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = data,
            Errors = errors
        };
    }
}
