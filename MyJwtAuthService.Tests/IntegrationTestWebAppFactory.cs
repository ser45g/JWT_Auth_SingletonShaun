using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Testcontainers.Papercut;
using Testcontainers.PostgreSql;

namespace MyJwtAuthService.Tests
{
    public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:19beta1").WithHostname("localhost").WithDatabase("identitydb-test").WithUsername("postgres").WithPassword("postgres").Build();

        //private readonly IContainer _mailServer = new ContainerBuilder("changemakerstudiosus/papercut-smtp:latest").WithName("papercut-test").WithHostname("localhost").WithPortBinding(2525,2525).WithPortBinding(8096,8080).Build();

        private readonly PapercutContainer _mailServer = new PapercutBuilder("changemakerstudiosus/papercut-smtp:latest").WithHostname("localhost").Build();

        public string DatabaseConnectionString => _dbContainer.GetConnectionString();
        public string MailServerConnectionString => _mailServer.GetConnectionString();

        public const int OutboxDelayMiliseconds = 2000;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:AppIdentityDbContext", _dbContainer.GetConnectionString());
            builder.UseSetting("MailSettings:Port", _mailServer.SmtpPort.ToString());
            builder.UseSetting("MailSettings:Host", _mailServer.Hostname);
            builder.UseSetting("MailSettings:IsAuthenticated", "false");
            builder.UseSetting("Authentication:AccessTokenSecret", "dy#taBV=*Ss8*ecisI*z$+0e$PhFdRUy-X1p#7SKacGjw#CIZP$hpxBamU87H?dovVP1q$QE=iTHMVZ!9-fl12KapdgF*2k9b9s*TS6!L5?aqZPQ-GbSkh+y*Oz6Icx2");
            builder.UseSetting("Authentication:RefreshTokenSecret", "t43Qziswlo!&b-FuCMlONT&1rSG-qqpQJ3pPymL7X+XfnttwV#J&ov?GykxyESa3nE45FIbie7Y#ndBuHCEGuueRn+IGEyc&V758GbJp$uZJO$D6pc#k3dJ#nAM?cZ*!");
            builder.UseSetting("OutboxBackgroundService:IntervalMiliseconds", "400");

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
