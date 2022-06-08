using Microsoft.Extensions.Configuration;

namespace _3DPOSRegistrationApp.Database.ModelClass
{
    public class LocalSQLDBCreation
    {
        private readonly IConfiguration _configuration;
        private readonly string DataSource;
        private readonly string UserId;
        private readonly string Password;
        private readonly string Server;

        public LocalSQLDBCreation(IConfiguration configuration)
        {
            this._configuration = configuration;
            this.Server = configuration.GetSection("LocalSQLDBCreation").GetSection("Server").Value;
            this.DataSource = configuration.GetSection("LocalSQLDBCreation").GetSection("DataSource").Value;
            this.UserId = configuration.GetSection("LocalSQLDBCreation").GetSection("UserId").Value;
            this.Password = configuration.GetSection("LocalSQLDBCreation").GetSection("Password").Value;
        }

        public LocalSQLDBCreationData StoreLocalSQLDBCreationData()
        {
            LocalSQLDBCreationData localSQLDBCreationData = new LocalSQLDBCreationData();
            localSQLDBCreationData.Server = Server;
            localSQLDBCreationData.DataSource = DataSource;
            localSQLDBCreationData.UserId = UserId;
            localSQLDBCreationData.Password = Password;

            return localSQLDBCreationData;
        }

    }
}
