namespace GymManagerAPI.Interfaces
{
    public interface IOperationResult<T> where T : class
    {
        int StatusCode { get; }
        bool IsSuccess { get; }
        string Message { get; }
        T Data { get; }

    }
}
