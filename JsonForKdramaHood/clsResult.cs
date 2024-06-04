namespace KdramaHoodJsonizer
{
    public class clsResult<T>
    {
        public T Value { get; }
        public string Error { get; }
        public bool IsSuccess => Error == null;

        private clsResult(T value, string error)
        {
            Value = value;
            Error = error;
        }

        public static clsResult<T> Success(T value) => new clsResult<T>(value, null);
        public static clsResult<T> Failure(string error) => new clsResult<T>(default, error);
    }
}
