namespace MediaCloud.WebApp.Services.UserProvider
{
    public class RegistrationResult
    {
        public bool IsSuccess;
        public string Message;

        public RegistrationResult()
        {
            IsSuccess = true;
            Message = "";
        }

        public RegistrationResult(bool success, string message)
        {
            IsSuccess = success;
            Message = message;
        }
    }
}
