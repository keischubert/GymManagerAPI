using GymManagerAPI.Interfaces;

namespace GymManagerAPI.Data.Common
{
    public class OperationResult<T> : IOperationResult<T> where T : class
    {
        public bool IsSuccess { get; private set; }
        public int StatusCode { get; private set; }
        public string Message { get; private set; }
        public T Data { get; private set; }

        public OperationResult(bool success, int statusCode, string message, T data)
        {
            IsSuccess = success;
            StatusCode = statusCode;
            Message = message;
            Data = data;
        }

        public static OperationResult<T> Fail(int statusCode, string message)
        {
            return new OperationResult<T>(false, statusCode, message, null);
        }

        public static OperationResult<T> Ok(int statusCode = 200, string message = "Success", T data = null)
        {
            return new OperationResult<T>(true, statusCode, message, data);
        }
    }
}
