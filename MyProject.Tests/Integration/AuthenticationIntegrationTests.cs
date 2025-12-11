using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyProject.Data;
using MyProject.Models;
using Xunit;

namespace MyProject.Tests.Integration
{
    /// <summary>
    /// Authentication Integration Tests
    /// Tester login/authentication flow med ASP.NET Core Identity
    /// </summary>
    public class AuthenticationIntegrationTests : IDisposable
    {
        private readonly PalleOptimeringContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IServiceProvider _serviceProvider;

        public AuthenticationIntegrationTests()
        {
           
            var services = new ServiceCollection();

           
            services.AddDbContext<PalleOptimeringContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

           
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 4;
            })
            .AddEntityFrameworkStores<PalleOptimeringContext>()
            .AddDefaultTokenProviders();

            services.AddLogging();

            _serviceProvider = services.BuildServiceProvider();
            _context = _serviceProvider.GetRequiredService<PalleOptimeringContext>();
            _userManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            _signInManager = _serviceProvider.GetRequiredService<SignInManager<ApplicationUser>>();

           
            _context.Database.EnsureCreated();
        }

        /// <summary>
        /// SCRUM-76: TC6-INT-002 - Test login flow
        /// Test Step 1-9: Login authentication med korrekt og forkert credentials
        ///
        /// NOTE: Dette er en backend integration test.
        /// For fuld UI test (step 2-3: "Åbn login side", "Klik Login knap"),
        /// brug E2E test framework som Selenium eller Playwright.
        /// </summary>
        [Fact]
        public async Task SCRUM76_TestLoginFlow()
        {
           
            var testUser = new ApplicationUser
            {
                UserName = "testuser@acies.dk",
                Email = "testuser@acies.dk",
                FullName = "Test Bruger",
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(testUser, "Test1234");
            Assert.True(createResult.Succeeded, "Test bruger skulle oprettes");
           

           
           
           
           
           
            //
           

           
            var passwordSignInResult = await _signInManager.PasswordSignInAsync(
                "testuser@acies.dk",
                "Test1234",
                isPersistent: false,
                lockoutOnFailure: false);

            Assert.True(passwordSignInResult.Succeeded);
           

           
            var loggedInUser = await _userManager.FindByEmailAsync("testuser@acies.dk");
            Assert.NotNull(loggedInUser);
            Assert.Equal("Test Bruger", loggedInUser.FullName);
           

           
           
           
            var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(loggedInUser);
            Assert.True(isEmailConfirmed);
           

           
            await _signInManager.SignOutAsync();
           

           
            var wrongPasswordResult = await _signInManager.PasswordSignInAsync(
                "testuser@acies.dk",
                "ForkertPassword123",
                isPersistent: false,
                lockoutOnFailure: false);

            Assert.False(wrongPasswordResult.Succeeded);
           
           

           
            var nonExistentUserResult = await _signInManager.PasswordSignInAsync(
                "ikkeeksisterende@acies.dk",
                "Test1234",
                isPersistent: false,
                lockoutOnFailure: false);

            Assert.False(nonExistentUserResult.Succeeded);
           
           
        }

        /// <summary>
        /// TC6-INT-007: Test bruger registrering
        /// </summary>
        [Fact]
        public async Task TC6INT007_BrugerRegistrering()
        {
           
            var newUser = new ApplicationUser
            {
                UserName = "nybruger@acies.dk",
                Email = "nybruger@acies.dk",
                FullName = "Ny Bruger"
            };

           
            var result = await _userManager.CreateAsync(newUser, "Password123");

           
            Assert.True(result.Succeeded);

            var savedUser = await _userManager.FindByEmailAsync("nybruger@acies.dk");
            Assert.NotNull(savedUser);
            Assert.Equal("Ny Bruger", savedUser.FullName);
        }

        /// <summary>
        /// TC6-INT-008: Test password krav
        /// </summary>
        [Fact]
        public async Task TC6INT008_PasswordKrav()
        {
           
            var user = new ApplicationUser
            {
                UserName = "passtest@acies.dk",
                Email = "passtest@acies.dk",
                FullName = "Password Test"
            };

           
            var weakResult = await _userManager.CreateAsync(user, "123");

           
            Assert.False(weakResult.Succeeded);
            Assert.Contains(weakResult.Errors, e => e.Code.Contains("Password"));
        }

        /// <summary>
        /// TC6-INT-009: Test email uniqueness
        /// </summary>
        [Fact]
        public async Task TC6INT009_EmailUniqueness()
        {
           
            var user1 = new ApplicationUser
            {
                UserName = "unique1@acies.dk",
                Email = "duplicate@acies.dk",
                FullName = "User 1"
            };

            var user2 = new ApplicationUser
            {
                UserName = "unique2@acies.dk",
                Email = "duplicate@acies.dk",
                FullName = "User 2"
            };

           
            var result1 = await _userManager.CreateAsync(user1, "Pass123");
            var result2 = await _userManager.CreateAsync(user2, "Pass456");

           
            Assert.True(result1.Succeeded);
            Assert.False(result2.Succeeded);
        }

        }
    }
}
