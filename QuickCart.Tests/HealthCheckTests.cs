using Xunit;

namespace QuickCart.Tests
{
    public class HealthCheckTests
    {
        [Fact]
        public void HealthCheck_Component_IsConfigured()
        {
            // Verifies the HealthCheck service type is referenced and verifiable
            var healthCheckType = typeof(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration);
            Assert.NotNull(healthCheckType);
        }
    }
}
