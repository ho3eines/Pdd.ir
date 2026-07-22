using Xunit;
using Pdd.ir.Business.Services;

namespace Pdd.ir.Tests.Services
{
    public class AuthBusinessServiceTests
    {
        [Fact]
        public void HashPassword_ReturnsDifferentHashes()
        {
            // Arrange
            var password = "testPassword123";
            
            // Act
            var hash1 = AuthBusinessService.HashPassword(password);
            var hash2 = AuthBusinessService.HashPassword(password);
            
            // Assert
            Assert.NotEqual(hash1, hash2); // Different salts
        }

        [Fact]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            var password = "testPassword123";
            var hash = AuthBusinessService.HashPassword(password);
            
            // Act
            var result = AuthBusinessService.VerifyPassword(password, hash);
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_WrongPassword_ReturnsFalse()
        {
            // Arrange
            var password = "testPassword123";
            var wrongPassword = "wrongPassword";
            var hash = AuthBusinessService.HashPassword(password);
            
            // Act
            var result = AuthBusinessService.VerifyPassword(wrongPassword, hash);
            
            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HashPassword_ReturnsBase64String()
        {
            // Arrange
            var password = "testPassword123";
            
            // Act
            var hash = AuthBusinessService.HashPassword(password);
            
            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
            // Should be valid Base64
            var bytes = Convert.FromBase64String(hash);
            Assert.Equal(48, bytes.Length); // 16 salt + 32 hash
        }

        [Fact]
        public void VerifyPassword_InvalidHash_ReturnsFalse()
        {
            // Arrange
            var password = "testPassword123";
            var invalidHash = "invalidHashString";
            
            // Act
            var result = AuthBusinessService.VerifyPassword(password, invalidHash);
            
            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GenerateAesKey_ReturnsValidBase64()
        {
            // Act
            var key = AuthBusinessService.GenerateAesKey();
            
            // Assert
            Assert.NotNull(key);
            Assert.NotEmpty(key);
            // Should be valid Base64 (32 bytes = 44 chars)
            var bytes = Convert.FromBase64String(key);
            Assert.Equal(32, bytes.Length);
        }
    }
}
