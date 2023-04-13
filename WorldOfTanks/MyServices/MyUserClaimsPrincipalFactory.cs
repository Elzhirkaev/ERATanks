using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using WorldOfTanks.Models.Register;

namespace WorldOfTanks.MyServices
{
    public class MyUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public MyUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            string emailConfirmed;
            var identity = await base.GenerateClaimsAsync(user);
            if (user.EmailConfirmed)
            {
                emailConfirmed = "true";
            }
            else
            {
                emailConfirmed = "false";
            }
            identity.AddClaim(new Claim("EmailConfirmed", emailConfirmed));
            return identity;
        }
    }
}
