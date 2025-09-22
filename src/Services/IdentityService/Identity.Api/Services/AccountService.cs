using Identity.Api.DTOs;
using Identity.Api.Model;
using Microsoft.AspNetCore.Identity;
using IdGen;

namespace Identity.Api.Services
{
    public class AccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IdGenerator _idGenerator;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IdGenerator idGenerator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _idGenerator = idGenerator;
        }

        public async Task<IdentityResult> RegisterUserAsync(RegisterUserDto model)
        {
            var user = new ApplicationUser
            {
                Id = _idGenerator.CreateId(),
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                MemberSince = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");
            }

            return result;
        }

        public async Task<ApplicationUser?> ValidateCredentialsAsync(LoginUserDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return null;
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                return null;
            }

            return user;
        }
    }
}