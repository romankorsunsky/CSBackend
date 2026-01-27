namespace b1.Main
{
    public class Result<T>
    {
        public T? Value { get; init; }
        public int Code { get; init; }
        public string? ErrMsg { get; set; }
        public bool Success { get; init; }
        private Result(T? val, int errorCode = 0, bool success = true, string? errMsg = null)
        {
            Value = val;
            Code = errorCode;
            Success = success;
            ErrMsg = errMsg;
        }
        public static Result<T> SuccessResult(T value)
        {
            return new Result<T>(value);
        }
        public static Result<T> FailedResult(int errorCode, bool success, string msg)
        {
            return new Result<T>(default, errorCode: errorCode, success: false, errMsg: msg);
        }
    }
}