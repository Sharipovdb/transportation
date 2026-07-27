namespace Transportation.Application.Common.Dtos;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public string? Message {get; set;}
    public T? Data { get; set; }

    public static ApiResponse<T> Success(T data, string? message = null, int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Message = message,
            Data = data,
            StatusCode = statusCode
        };
    }

    public static ApiResponse<T> Success(string? message = null, int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Message = message,
            StatusCode = statusCode,
            Data = default
        };
    }
    
    public static ApiResponse<T> Failure(string message, int statusCode = 400) 
    {
        return new ApiResponse<T> 
        {
            IsSuccess = false,
            Message = message,
            StatusCode = statusCode,
            Data = default
        };
    }
}