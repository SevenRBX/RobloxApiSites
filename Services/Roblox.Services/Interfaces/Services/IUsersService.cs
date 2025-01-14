﻿using System;
using System.Threading.Tasks;
using Roblox.Services.Models.Users;

namespace Roblox.Services.Services
{
    public interface IUsersService
    {
        Task<Models.Users.UserDescriptionEntry> GetDescription(long userId);
        Task SetUserDescription(long userId, string description);
        Task SetUserBirthDate(long userId, DateTime birthDate);
        Task SetUserGender(long userId, byte gender);
        Task<Models.Users.UserInformationResponse> GetUserById(long userId);
        DateTime GetDateTimeFromBirthDate(int birthYear, int birthMonth, int birthDay);
        Task<Models.Users.UserAccountEntry> CreateUser(string username);
        Task DeleteUser(long userId);
        Task<Models.Users.SkinnyUserAccountEntry> GetUserByUsername(string username);
    }
}