using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SportsEquipment.Domain.Enums;
using SportsEquipment.Domain.Common;
using SportsEquipment.Domain.Entities;
using SportsEquipment.Application.Commands.Login;
using SportsEquipment.Application.Commands.Users;
using SportsEquipment.Application.Security.Interfaces;
using SportsEquipment.Application.Interfaces.Repositories;
using SportsEquipment.Application.Services.Implementation.Users;

namespace SportsEquipment.Tests.Unit.Application
{
    public class UserServiceTests
    {
        [Fact]
        public async Task RegisterAsync_WhenEmailExists_Throws()
        {
            var mockRepo = new Mock<IUserRepository>();
            var mockHasher = new Mock<IPasswordHasher>();
            var mockUow = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<UserService>>();

            mockRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(new User("x", "a@b.com", "hash", UserType.Seller));

            var service = new UserService(mockRepo.Object, mockHasher.Object, mockUow.Object, mockLogger.Object);

            var cmd = new CreateUserCommand { Name = "Test", Email = "a@b.com", Password = "123456", Type = UserType.Seller };

            await Assert.ThrowsAsync<DomainException>(() => service.RegisterAsync(cmd));
        }

        [Fact]
        public async Task AuthenticateAsync_WithValidCredentials_ReturnsTokenWhenProviderPresent()
        {
            var mockRepo = new Mock<IUserRepository>();
            var mockHasher = new Mock<IPasswordHasher>();
            var mockUow = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<UserService>>();
            var mockTokenProvider = new Mock<ITokenProvider>();

            var user = new User("U", "u@test.com", "hashed", UserType.Seller);

            mockRepo.Setup(r => r.GetByEmailAsync("u@test.com")).ReturnsAsync(user);
            mockHasher.Setup(h => h.Verify("pass", "hashed")).Returns(true);
            mockTokenProvider.Setup(tp => tp.GenerateToken(It.IsAny<User>())).Returns("tok");
            mockTokenProvider.SetupGet(tp => tp.TokenLifetime).Returns(TimeSpan.FromMinutes(60));

            var service = new UserService(mockRepo.Object, mockHasher.Object, mockUow.Object, mockLogger.Object, mockTokenProvider.Object);

            var res = await service.AuthenticateAsync(new LoginRequest { Email = "u@test.com", Password = "pass" });

            res.Token.Should().Be("tok");
            res.User.Email.Should().Be("u@test.com");
        }

        [Fact]
        public async Task AuthenticateAsync_InvalidPassword_Throws()
        {
            var mockRepo = new Mock<IUserRepository>();
            var mockHasher = new Mock<IPasswordHasher>();
            var mockUow = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<UserService>>();

            var user = new User("U", "u@test.com", "hashed", UserType.Seller);

            mockRepo.Setup(r => r.GetByEmailAsync("u@test.com")).ReturnsAsync(user);
            mockHasher.Setup(h => h.Verify("wrong", "hashed")).Returns(false);

            var service = new UserService(mockRepo.Object, mockHasher.Object, mockUow.Object, mockLogger.Object);

            await Assert.ThrowsAsync<DomainException>(() => service.AuthenticateAsync(new LoginRequest { Email = "u@test.com", Password = "wrong" }));
        }
    }
}