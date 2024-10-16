using Moq;
using GoRideShare;
using Microsoft.Extensions.Logging;

namespace AccountManager.Tests
{
    public class VerifyLoginCredentialsTest
    {
        private readonly Mock<ILogger<VerifyLoginCredentials>> _mockLogger;

        public VerifyLoginCredentialsTest()
        {
            _mockLogger = new Mock<ILogger<VerifyLoginCredentials>>();
        }

        [Fact]
        public void Test()
        {
            VerifyLoginCredentials function = new VerifyLoginCredentials(_mockLogger.Object);
            
            bool result = function.TestFunc();
            Assert.False(result, "Result error");
        }
    }
}