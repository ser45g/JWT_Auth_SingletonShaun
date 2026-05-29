using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Testcontainers.Papercut;
using Testcontainers.PostgreSql;

namespace MyJwtAuthService.Tests
{
    public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:latest").WithHostname("localhost").WithDatabase("identitydb-test").WithUsername("postgres").WithPassword("postgres").Build();

        //private readonly IContainer _mailServer = new ContainerBuilder("changemakerstudiosus/papercut-smtp:latest").WithName("papercut-test").WithHostname("localhost").WithPortBinding(2525,2525).WithPortBinding(8096,8080).Build();

        private readonly PapercutContainer _mailServer = new PapercutBuilder("changemakerstudiosus/papercut-smtp:latest").WithHostname("localhost").WithPortBinding(8080, true).Build();

        public string DatabaseConnectionString => _dbContainer.GetConnectionString();
        public string MailServerConnectionString => _mailServer.GetConnectionString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:AppIdentityDbContext", _dbContainer.GetConnectionString());
            builder.UseSetting("MailSettings:Port", _mailServer.SmtpPort.ToString());
            builder.UseSetting("MailSettings:Host", _mailServer.Hostname);
            builder.UseSetting("MailSettings:IsAuthenticated", "false");

            builder.ConfigureTestServices(services => {
                
            });
        }
        public Task InitializeAsync()
        {
            var taskMailServer=_mailServer.StartAsync();
            var taskDb = _dbContainer.StartAsync();

            return Task.WhenAll(taskMailServer, taskDb);
        }

        Task IAsyncLifetime.DisposeAsync()
        {
            var taskMailServer = _mailServer.StopAsync();
            var taskDb = _dbContainer.StopAsync();

            return Task.WhenAll(taskMailServer, taskDb);
        }
    }
}
