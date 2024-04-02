namespace MediaCloud.WebApp.Services.ConfigurationProvider 
{
    [Serializable]
    public class EnvironmentSettings
    {
        public string DatabaseConnectionString {get; set;}

        public EnvironmentSettings(IConfiguration configuration)
        {
            var dbPath = configuration?["ConnectionStrings:Database"];
            DatabaseConnectionString =  dbPath?.Split(";").First().Split("=").Last() ?? "";


        }
    }
}