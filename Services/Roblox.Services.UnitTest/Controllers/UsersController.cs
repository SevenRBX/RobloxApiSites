﻿using System;
using System.Threading.Tasks;
using Roblox.Services.Controllers;
using Xunit;
using Moq;
using Roblox.Platform.Membership;
using Roblox.Services.Exceptions.Services;
using Roblox.Services.Models.Users;
using Roblox.Services.Services;

namespace Roblox.Services.UnitTest.Controllers
{
    public class UnitTestUsersController
    {
        
        [Fact]
        public async Task Get_User_Description()
        {
            var userId = 1;
            var expectedDescription = "This is an example description for a unit test.";
            var mock = new Mock<IUsersService>();
            mock.Setup(foo => foo.GetDescription(userId)).ReturnsAsync(new UserDescriptionEntry()
            {
                userId = userId,
                description = expectedDescription,
                created = DateTime.Now,
                updated = DateTime.Now,
            });
            UsersController controller = new(mock.Object);
            var result = await controller.GetUserDescription(1);
            Assert.Equal(expectedDescription, result.description);
            Assert.Equal(userId, result.userId);
            
            mock.VerifyAll();
        }
        
        [Fact]
        public async Task Get_User_Description_For_Invalid_Account_And_Throw()
        {
            var userId = 1;
            var mock = new Mock<IUsersService>();
            mock.Setup(foo => foo.GetDescription(userId)).Throws(new RecordNotFoundException(userId));
            UsersController controller = new(mock.Object);
            await Assert.ThrowsAsync<RecordNotFoundException>(async () =>
            {
                await controller.GetUserDescription(1);
            });
            
            mock.VerifyAll();
        }

        [Fact]
        public async Task Set_User_Description()
        {
            var userId = 1;
            var newDescription = "This is a new test description";
            var mock = new Mock<IUsersService>();
            mock.Setup(c => c.SetUserDescription(userId, newDescription));
            var controller = new UsersController(mock.Object);
            await controller.SetUserDescription(new()
            {
                userId = userId,
                description = newDescription,
            });
            
            mock.VerifyAll();
        }

        [Fact]
        public async Task Get_User_By_Id()
        {
            var userId = 1;
            var name = "UnitTest5356";
            var mock = new Mock<IUsersService>();
            mock.Setup(c => c.GetUserById(userId)).ReturnsAsync(new UserInformationResponse()
            {
                username = name,
                userId = userId,
            });
            var controller = new UsersController(mock.Object);
            var result = await controller.GetUserById(userId);
            Assert.Equal(name, result.username);
            Assert.Equal(userId, result.userId);
        }
        
        [Fact]
        public async Task Get_User_By_Id_Non_Existent()
        {
            var userId = 1;
            var mock = new Mock<IUsersService>();
            mock.Setup(c => c.GetUserById(userId)).ThrowsAsync(new RecordNotFoundException());
            var controller = new UsersController(mock.Object);
            await Assert.ThrowsAsync<RecordNotFoundException>(async () =>
            {
                await controller.GetUserById(userId);
            });
        }

        [Fact]
        public async Task Create_User_With_Valid_Args()
        {
            // args
            var expectedId = 123;
            var req = new CreateUserRequest()
            {
                username = "GoodName123",
                birthDay = 1,
                birthMonth = 1,
                birthYear = 2000,
            };
            var birthDateAsTime = new DateTime(req.birthYear, req.birthMonth, req.birthDay);
            // mocks
            var mock = new Mock<IUsersService>();
            mock.Setup(c => c.GetDateTimeFromBirthDate(req.birthYear, req.birthMonth, req.birthDay))
                .Returns(birthDateAsTime);
            mock.Setup(c => c.CreateUser(req.username)).ReturnsAsync(new UserAccountEntry()
            {
                username = req.username,
                userId = expectedId,
                accountStatus = AccountStatus.Ok,
            });
            mock.Setup(c => c.SetUserBirthDate(expectedId, birthDateAsTime));
            // test code
            var controller = new UsersController(mock.Object);
            var result = await controller.CreateUser(req);
            // assertions
            Assert.Equal(expectedId, result.userId);
            Assert.Equal(req.username, result.username);
            Assert.Equal(AccountStatus.Ok, result.accountStatus);
            // mock verify
            mock.Verify(c => c.GetDateTimeFromBirthDate(req.birthYear, req.birthMonth, req.birthDay), Times.Once);
            mock.Verify(c => c.CreateUser(req.username), Times.Once);
            mock.Verify(c => c.SetUserBirthDate(expectedId, birthDateAsTime), Times.Once);
        }
        
        [Fact]
        public async Task Create_User_With_Bad_Username()
        {
            // args
            var req = new CreateUserRequest()
            {
                username = "A",
                birthDay = 1,
                birthMonth = 1,
                birthYear = 2000,
            };
            // mocks
            var mock = new Mock<IUsersService>();
            // test code
            var controller = new UsersController(mock.Object);
            // assertions
            await Assert.ThrowsAsync<ParameterException>(async () =>
            {
                await controller.CreateUser(req);
            });
        }
    }
}