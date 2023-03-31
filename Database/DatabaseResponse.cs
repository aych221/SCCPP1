namespace SCCPP1.Database
{
    public class DatabaseResponse
    {

        public bool Success { get; }
        public string ErrorMessage { get; }
        public object? Result { get; }


        public DatabaseResponse(bool success, object? result = null, string errorMessage = "")
        {
            Success = success;
            Result = result;
            ErrorMessage = errorMessage;
        }

    }

}
