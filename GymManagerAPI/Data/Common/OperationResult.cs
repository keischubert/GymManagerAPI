namespace GymManagerAPI.Data.Common
{
    public class OperationResult<T> where T : class
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public T Data { get; set; }

        public int ErrorStatusCode { get; set; }

        public static OperationResult<T> Ok(T data) => new OperationResult<T>() { Success = true, Data = data };
        public static OperationResult<T> Fail(int errorStatusCode, string errorMessage) => new OperationResult<T> { Success = false, ErrorStatusCode = errorStatusCode, ErrorMessage = errorMessage };
    }
}
