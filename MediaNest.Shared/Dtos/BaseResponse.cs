namespace MediaNest.Shared.Dtos;
public class BaseResponse<T> {
    public bool IsSuccess { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public T Data { get; set; } = default!;
}
