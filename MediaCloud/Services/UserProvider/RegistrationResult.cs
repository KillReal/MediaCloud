namespace MediaCloud.WebApp.Services.UserProvider
{
    public class RegistrationResult(bool success, string message)
    {
        public readonly bool IsSuccess = success;
        public readonly string Message = message;

        public RegistrationResult() : this(true, "")
        {
        }
    }
}
