using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SportsEquipment.Domain.Enums;
using SportsEquipment.Application.DTOs.Login;
using SportsEquipment.Application.DTOs.Users;
using SportsEquipment.Application.Commands.Login;
using SportsEquipment.Application.Commands.Users;
using SportsEquipment.Api.Presentation.Controllers;
using SportsEquipment.Application.Interfaces.Services;

namespace SportsEquipment.Tests.Unit.Controllers
{
    public class UsersControllerTests
    {
        [Fact]
        public async Task RegisterAsync_ReturnsCreated()  
        {
            var mockService = new Mock<IUserService>();
            var dto = new UserDto { Id = Guid.NewGuid(), Name = "A", Email = "a@x.com" };

            mockService.Setup(s => s.RegisterAsync(It.IsAny<CreateUserCommand>())).ReturnsAsync(dto);

            var controller = new UsersController(mockService.Object);

            var result = await controller.RegisterAsync(new CreateUserCommand
            {
                Name = "A",
                Email = "a@x.com",
                Password = "123456",
                Type = UserType.Seller
            });

            result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        public async Task LoginAsync_ReturnsOk() 
        {
            var mockService = new Mock<IUserService>();
            var auth = new AuthenticateResult
            {
                Token = "t",
                User = new UserDto
                {
                    Id = Guid.NewGuid(),
                    Email = "a@x.com"
                }
            };

            mockService.Setup(s => s.AuthenticateAsync(It.IsAny<LoginRequest>())).ReturnsAsync(auth);

            var controller = new UsersController(mockService.Object);

            var result = await controller.LoginAsync(new LoginRequest
            {
                Email = "a@x.com",
                Password = "123456"
            });

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
