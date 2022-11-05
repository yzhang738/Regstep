using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Logging;

namespace RSToolKit.Domain.Entities.Clients
{
    public class AppUserManager : UserManager<User>
    {

        public override async Task<IdentityResult> ResetPasswordAsync(string userId, string token, string newPassword)
        {
            var user = await FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed("User does not exist.");
            if (user.PasswordResetToken.ToString() != token)
                return IdentityResult.Failed("Reset tokens do not match.");
            if (user.PasswordResetTokenExpiration < DateTimeOffset.UtcNow)
                return IdentityResult.Failed("Reset token is expired.");
            user.PasswordHash = PasswordHasher.HashPassword(newPassword);
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.PasswordResetToken = Guid.NewGuid();
            user.PasswordResetTokenExpiration = DateTimeOffset.UtcNow;
            user.IsConfirmed = true;
            return await UpdateAsync(user);
        }

        public override async Task<string> GenerateEmailConfirmationTokenAsync(string userId)
        {
            var user = await FindByIdAsync(userId);
            if (user == null)
                return null;
            user.ValidationToken = Guid.NewGuid();
            var result = await UpdateAsync(user);
            if (result.Succeeded)
                return user.PasswordResetToken.ToString();
            return null;
        }

        public override async Task<IdentityResult> ConfirmEmailAsync(string userId, string token)
        {
            var user = await FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed("User does not exist.");
            if (user.ValidationToken.ToString() != token)
                return IdentityResult.Failed("Validation tokens do not match.");
            user.ValidationToken = Guid.NewGuid();
            user.EmailConfirmed = true;
            return await UpdateAsync(user);
        }

        public async Task<string> ConfirmAccountAsync(string userId, Guid token)
        {
            var emailResult = await ConfirmEmailAsync(userId, token.ToString());
            if (emailResult.Succeeded)
                return await GeneratePasswordResetTokenAsync(userId);
            return null;
        }

        public override async Task<IdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed("User does not exist.");
            var passwordMatch = await CheckPasswordAsync(user, currentPassword);
            if (!passwordMatch)
                return IdentityResult.Failed("Current Password is incorrect.");
            if (newPassword.Length < 3)
                return IdentityResult.Failed("New password must be at least 3 characters.");
            user.PasswordHash = PasswordHasher.HashPassword(newPassword);
            user.SecurityStamp = Guid.NewGuid().ToString();
            return await UpdateAsync(user);
        }

        public override async Task<string> GeneratePasswordResetTokenAsync(string userId)
        {
            var user = await FindByIdAsync(userId);
            if (user == null)
                return null;
            user.PasswordResetToken = Guid.NewGuid();
            user.PasswordResetTokenExpiration = DateTimeOffset.UtcNow.AddMinutes(30);
            var result = await UpdateAsync(user);
            if (result.Succeeded)
                return user.PasswordResetToken.ToString();
            return null;
        }

        public override async Task<IdentityResult> SetEmailAsync(string userId, string email)
        {
            var user = await FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed("User does not exist.");
            user.Email = email;
            user.EmailConfirmed = false;
            return await UpdateAsync(user);
        }
       
        public AppUserManager(IUserStore<User> store) : base(store)
        { }

        public IdentityResult Delete(User user, FormsRepository Repository)
        {
            foreach (var log in Repository.Search<Log>(l => l.UserKey == user.Id))
                Repository.Remove(log);
            Repository.Commit();
            var result = UserManagerExtensions.Delete(this, user);
            return result;
        }

        public static AppUserManager Create(IdentityFactoryOptions<AppUserManager> options, IOwinContext context)
        {
            var db = context.Get<EFDbContext>();
            var manager = new AppUserManager(new UserStore<User>(db));
            return manager;
        }
    }
}
