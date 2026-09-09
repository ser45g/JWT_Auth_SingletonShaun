using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Testcontainers.Papercut;
using Testcontainers.PostgreSql;

namespace MyJwtAuthService.Tests
{
    public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:19beta1").WithDatabase("identitydb-test").WithUsername("postgres").WithPassword("postgres").WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5432)).Build();
        
        private readonly PostgreSqlContainer _quartzDbContainer = new PostgreSqlBuilder("postgres:19beta1").WithDatabase("quartz").WithUsername("postgres").WithPassword("postgres").WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5432)).Build();

        //private readonly IContainer _mailServer = new ContainerBuilder("changemakerstudiosus/papercut-smtp:latest").WithName("papercut-test").WithHostname("localhost").WithPortBinding(2525,2525).WithPortBinding(8096,8080).Build();

        private readonly PapercutContainer _mailServer = new PapercutBuilder("changemakerstudiosus/papercut-smtp:7.7.3-alpha.7").WithHostname("localhost").WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(2525)).Build();

        public string DatabaseConnectionString => _dbContainer.GetConnectionString();
        public string MailServerConnectionString => _mailServer.GetConnectionString();

        public const int OutboxDelayMiliseconds = 3000;
        public const int MaxFailedAccessAttemptsForLockout = 1;
        public const int PasswordRequiredLenght = 8;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:AppIdentityDbContext", _dbContainer.GetConnectionString());
            builder.UseSetting("ConnectionStrings:Quartz", _quartzDbContainer.GetConnectionString());
            builder.UseSetting("MailSettings:Port", _mailServer.SmtpPort.ToString());
            builder.UseSetting("MailSettings:Host", _mailServer.Hostname);
            builder.UseSetting("MailSettings:IsAuthenticated", "false");
            builder.UseSetting("Authentication:AccessTokenSecret", "dy#taBV=*Ss8*ecisI*z$+0e$PhFdRUy-X1p#7SKacGjw#CIZP$hpxBamU87H?dovVP1q$QE=iTHMVZ!9-fl12KapdgF*2k9b9s*TS6!L5?aqZPQ-GbSkh+y*Oz6Icx2");
            builder.UseSetting("Authentication:RefreshTokenSecret", "t43Qziswlo!&b-FuCMlONT&1rSG-qqpQJ3pPymL7X+XfnttwV#J&ov?GykxyESa3nE45FIbie7Y#ndBuHCEGuueRn+IGEyc&V758GbJp$uZJO$D6pc#k3dJ#nAM?cZ*!");
            builder.UseSetting("OutboxBackgroundService:IntervalSeconds", "1");
            builder.UseSetting("IdentityOptions:Lockout:MaxFailedAccessAttempts", MaxFailedAccessAttemptsForLockout.ToString());
            builder.UseSetting("IdentityOptions:Password:RequiredLength", PasswordRequiredLenght.ToString());
        
            builder.ConfigureTestServices(services => {
                
            });
        }
        public Task InitializeAsync()
        {
            var taskMailServerStart=_mailServer.StartAsync();
            var taskQuartzDbStart=_quartzDbContainer.StartAsync();
            var taskDbStart = _dbContainer.StartAsync();

            return Task.WhenAll(taskMailServerStart, taskDbStart, taskQuartzDbStart);
        }

        Task IAsyncLifetime.DisposeAsync()
        {
            var taskMailServerStop = _mailServer.StopAsync();
            var taskQuartzDbStop = _quartzDbContainer.StopAsync();
            var taskDbStop = _dbContainer.StopAsync();

            return Task.WhenAll(taskMailServerStop, taskDbStop, taskQuartzDbStop);
        }
    }
}
