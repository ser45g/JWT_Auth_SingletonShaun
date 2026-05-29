
using Microsoft.Extensions.DependencyInjection;
using MyJwtAuthService.Data;
using MyJwtAuthService.Tests.Services;

namespace MyJwtAuthService.Tests
{
    public class BaseIntegrationTest:IClassFixture<IntegrationTestWebAppFactory>
    {
        private readonly IServiceScope _scope;
        protected AppIdentityDbContext _dbContext;
        protected IntegrationTestWebAppFactory _factory;
        protected AuthenticationService authenticationService;
        protected PapercutService papercutService;
        protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
        {
            _factory = factory;
            _scope = _factory.Services.CreateScope();
            _dbContext = _scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();

            papercutService = new PapercutService(_factory);
            authenticationService = new AuthenticationService(_factory,papercutService);
            
        }
    }
}
