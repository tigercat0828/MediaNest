namespace MediaNest.ApiService; 
public class BaseResponse<T>{
    public BaseResponse() {
    }
    public BaseResponse(bool isSuccess, string errorMessage, T data) {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Data = data;
    }
    public BaseResponse(bool isSuccess, T data) {
        IsSuccess = isSuccess;
        Data = data;
    }
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } 
    public T Data { get; set; } 
}
