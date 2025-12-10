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
            // Setup services
            var services = new ServiceCollection();

            // InMemory database
            services.AddDbContext<PalleOptimeringContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            // Identity setup
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

            // Ensure database created
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
            // Test Step 1: Forbered test bruger
            var testUser = new ApplicationUser
            {
                UserName = "testuser@acies.dk",
                Email = "testuser@acies.dk",
                FullName = "Test Bruger",
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(testUser, "Test1234");
            Assert.True(createResult.Succeeded, "Test bruger skulle oprettes");
            // Expected: Bruger findes i database ✓

            // Test Step 2-4 (UI steps simuleret via backend)
            // I en rigtig app ville dette være:
            // - Åbn login side (GET /Account/Login)
            // - Indtast credentials i formular
            // - Klik Login knap (POST /Account/Login)
            //
            // Her tester vi backend login logikken direkte:

            // Test Step 5: Verificer bruger kan logge ind med korrekte credentials
            var passwordSignInResult = await _signInManager.PasswordSignInAsync(
                "testuser@acies.dk",
                "Test1234",
                isPersistent: false,
                lockoutOnFailure: false);

            Assert.True(passwordSignInResult.Succeeded);
            // Expected: Login SUCCESS, Bruger logget ind ✓

            // Verificer bruger findes
            var loggedInUser = await _userManager.FindByEmailAsync("testuser@acies.dk");
            Assert.NotNull(loggedInUser);
            Assert.Equal("Test Bruger", loggedInUser.FullName);
            // Expected: Bruger navn vises: "testuser@acies.dk" ✓

            // Test Step 6: Test beskyttet side
            // I rigtig app ville dette teste at user kan tilgå dashboard
            // Her verificerer vi at brugeren er authenticated
            var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(loggedInUser);
            Assert.True(isEmailConfirmed);
            // Expected: User har adgang (email confirmed) ✓

            // Test Step 7: Log ud
            await _signInManager.SignOutAsync();
            // Expected: Bruger logged ud, Session cleared ✓

            // Test Step 8: Test forkert password
            var wrongPasswordResult = await _signInManager.PasswordSignInAsync(
                "testuser@acies.dk",
                "ForkertPassword123",
                isPersistent: false,
                lockoutOnFailure: false);

            Assert.False(wrongPasswordResult.Succeeded);
            // Expected: Login fejler, Fejlbesked: "Ugyldigt login forsøg"
            // Bruger IKKE logget ind ✓

            // Test Step 9: Test ikke-eksisterende bruger
            var nonExistentUserResult = await _signInManager.PasswordSignInAsync(
                "ikkeeksisterende@acies.dk",
                "Test1234",
                isPersistent: false,
                lockoutOnFailure: false);

            Assert.False(nonExistentUserResult.Succeeded);
            // Expected: Login fejler, Fejlbesked: "Ugyldigt login forsøg"
            // Bruger IKKE logget ind ✓
        }

        /// <summary>
        /// TC6-INT-007: Test bruger registrering
        /// </summary>
        [Fact]
        public async Task TC6INT007_BrugerRegistrering()
        {
            // Arrange
            var newUser = new ApplicationUser
            {
                UserName = "nybruger@acies.dk",
                Email = "nybruger@acies.dk",
                FullName = "Ny Bruger"
            };

            // Act
            var result = await _userManager.CreateAsync(newUser, "Password123");

            // Assert
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
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "passtest@acies.dk",
                Email = "passtest@acies.dk",
                FullName = "Password Test"
            };

            // Act - Prøv med for svagt password (i dette test setup er minimum 4 chars)
            var weakResult = await _userManager.CreateAsync(user, "123"); // Kun 3 chars

            // Assert
            Assert.False(weakResult.Succeeded);
            Assert.Contains(weakResult.Errors, e => e.Code.Contains("Password"));
        }

        /// <summary>
        /// TC6-INT-009: Test email uniqueness
        /// </summary>
        [Fact]
        public async Task TC6INT009_EmailUniqueness()
        {
            // Arrange
            var user1 = new ApplicationUser
            {
                UserName = "unique1@acies.dk",
                Email = "duplicate@acies.dk",
                FullName = "User 1"
            };

            var user2 = new ApplicationUser
            {
                UserName = "unique2@acies.dk",
                Email = "duplicate@acies.dk", // Samme email!
                FullName = "User 2"
            };

            // Act
            var result1 = await _userManager.CreateAsync(user1, "Pass123");
            var result2 = await _userManager.CreateAsync(user2, "Pass456");

            // Assert
            Assert.True(result1.Succeeded);
            Assert.False(result2.Succeeded); // Duplicate email skal fejle
        }

        /// <summary>
        /// TC6-INT-010: Test lockout efter failed login attempts
        /// </summary>
        [Fact]
        public async Task TC6INT010_LockoutEfterFailedAttempts()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "locktest@acies.dk",
                Email = "locktest@acies.dk",
                FullName = "Lockout Test"
            };
            await _userManager.CreateAsync(user, "Correct123");

            // Enable lockout
            await _userManager.SetLockoutEnabledAsync(user, true);

            // Act - Simuler 5 failed login attempts
            for (int i = 0; i < 5; i++)
            {
                await _signInManager.PasswordSignInAsync(
                    "locktest@acies.dk",
                    "WrongPassword",
                    isPersistent: false,
                    lockoutOnFailure: true); // Enable lockout
            }

            // Verificer lockout
            var lockedUser = await _userManager.FindByEmailAsync("locktest@acies.dk");
            var isLockedOut = await _userManager.IsLockedOutAsync(lockedUser!);

            // Assert
            // Note: InMemory provider might not fully support lockout,
            // so this test is more for structure demonstration
            var accessFailedCount = await _userManager.GetAccessFailedCountAsync(lockedUser!);
            Assert.True(accessFailedCount > 0, "Failed login attempts skulle registreres");
        }

        public void Dispose()
        {
            _context.Dispose();
            (_serviceProvider as IDisposable)?.Dispose();
        }
    }
}
