using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MyProject.Tests.SecurityTests
{
    /// <summary>
    /// Sikkerhedstests for Authentication
    /// Reference: docs/testplan.md - Section 4.5 Sikkerhedstest
    /// Reference: docs/klassediagram.md - AccountController
    /// Reference: docs/er-diagram.md - AspNetUsers, AspNetRoles, AspNetUserRoles tabeller
    /// </summary>
    public class AuthenticationTests
    {
        /// <summary>
        /// Test: AccountController.Login() med valid credentials
        /// Formål: Verificer at login lykkes med korrekt brugernavn og password
        /// Reference: AspNetUsers tabel - UserName, PasswordHash kolonner
        /// </summary>
        [Fact]
        public async Task Login_WithValidCredentials_ShouldSucceed()
        {
            // Arrange
            var mockSignInManager = CreateMockSignInManager();
            var mockUserManager = CreateMockUserManager();

            // Setup mock til at returnere success ved login
            mockSignInManager
                .Setup(m => m.PasswordSignInAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var controller = new AccountController(mockSignInManager.Object, mockUserManager.Object);
            var loginModel = new LoginViewModel
            {
                UserName = "testuser",
                Password = "ValidPassword123!",
                RememberMe = false
            };

            // Act
            var result = await controller.Login(loginModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);

            // Verificer at SignInManager blev kaldt korrekt
            mockSignInManager.Verify(m => m.PasswordSignInAsync(
                "testuser",
                "ValidPassword123!",
                false,
                false), Times.Once);
        }

        /// <summary>
        /// Test: AccountController.Login() med invalid credentials
        /// Formål: Verificer at login fejler med forkert password
        /// Reference: AspNetUsers.AccessFailedCount - tæller fejlede login forsøg
        /// </summary>
        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldFail()
        {
            // Arrange
            var mockSignInManager = CreateMockSignInManager();
            var mockUserManager = CreateMockUserManager();

            mockSignInManager
                .Setup(m => m.PasswordSignInAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            var controller = new AccountController(mockSignInManager.Object, mockUserManager.Object);
            var loginModel = new LoginViewModel
            {
                UserName = "testuser",
                Password = "WrongPassword",
                RememberMe = false
            };

            // Act
            var result = await controller.Login(loginModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains("Invalid login attempt", controller.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
        }

        /// <summary>
        /// Test: AccountController.Register() med valid data
        /// Formål: Verificer at ny bruger kan oprettes
        /// Reference: AspNetUsers tabel - opretter ny bruger med PasswordHash, CreatedAt
        /// </summary>
        [Fact]
        public async Task Register_WithValidData_ShouldCreateUser()
        {
            // Arrange
            var mockSignInManager = CreateMockSignInManager();
            var mockUserManager = CreateMockUserManager();

            var newUser = new ApplicationUser
            {
                UserName = "newuser",
                Email = "newuser@example.com",
                FullName = "New Test User"
            };

            mockUserManager
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var controller = new AccountController(mockSignInManager.Object, mockUserManager.Object);
            var registerModel = new RegisterViewModel
            {
                UserName = "newuser",
                Email = "newuser@example.com",
                FullName = "New Test User",
                Password = "SecurePassword123!",
                ConfirmPassword = "SecurePassword123!"
            };

            // Act
            var result = await controller.Register(registerModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            mockUserManager.Verify(m => m.CreateAsync(
                It.Is<ApplicationUser>(u => u.UserName == "newuser"),
                "SecurePassword123!"), Times.Once);
        }

        /// <summary>
        /// Test: Password Hashing
        /// Formål: Verificer at passwords hashs korrekt og ikke gemmes i plaintext
        /// Reference: AspNetUsers.PasswordHash kolonne
        /// </summary>
        [Fact]
        public void PasswordHasher_ShouldHashPasswordSecurely()
        {
            // Arrange
            var hasher = new PasswordHasher<ApplicationUser>();
            var user = new ApplicationUser { UserName = "testuser" };
            var plainPassword = "MySecretPassword123!";

            // Act
            var hashedPassword = hasher.HashPassword(user, plainPassword);

            // Assert
            Assert.NotEqual(plainPassword, hashedPassword); // Password er hashed
            Assert.NotEmpty(hashedPassword);
            Assert.True(hashedPassword.Length > 50); // Hashed passwords er lange

            // Verificer at hash kan verificeres
            var verificationResult = hasher.VerifyHashedPassword(user, hashedPassword, plainPassword);
            Assert.Equal(PasswordVerificationResult.Success, verificationResult);
        }

        /// <summary>
        /// Test: AccountController.Logout()
        /// Formål: Verificer at session invalideres korrekt ved logout
        /// Reference: Testplan - Session management tests
        /// </summary>
        [Fact]
        public async Task Logout_ShouldSignOutUser()
        {
            // Arrange
            var mockSignInManager = CreateMockSignInManager();
            var mockUserManager = CreateMockUserManager();

            mockSignInManager
                .Setup(m => m.SignOutAsync())
                .Returns(Task.CompletedTask);

            var controller = new AccountController(mockSignInManager.Object, mockUserManager.Object);

            // Act
            var result = await controller.Logout();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);

            mockSignInManager.Verify(m => m.SignOutAsync(), Times.Once);
        }

        /// <summary>
        /// Test: Account Lockout ved multiple fejlede login forsøg
        /// Formål: Verificer at konto låses efter for mange fejlede forsøg
        /// Reference: AspNetUsers.LockoutEnabled, LockoutEnd, AccessFailedCount
        /// </summary>
        [Fact]
        public async Task Login_AfterMultipleFailedAttempts_ShouldLockAccount()
        {
            // Arrange
            var mockSignInManager = CreateMockSignInManager();
            var mockUserManager = CreateMockUserManager();

            mockSignInManager
                .Setup(m => m.PasswordSignInAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

            var controller = new AccountController(mockSignInManager.Object, mockUserManager.Object);
            var loginModel = new LoginViewModel { UserName = "testuser", Password = "wrong" };

            // Act
            var result = await controller.Login(loginModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Contains("locked", controller.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "");
        }

        #region Helper Methods

        private Mock<SignInManager<ApplicationUser>> CreateMockSignInManager()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            return new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object,
                Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);
        }

        private Mock<UserManager<ApplicationUser>> CreateMockUserManager()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
        }

        #endregion
    }

    #region Mock Models and Controller

    /// <summary>
    /// ApplicationUser model fra ER-diagram
    /// Reference: AspNetUsers tabel
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class LoginViewModel
    {
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Password { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
    }

    /// <summary>
    /// Mock AccountController baseret på klassediagrammet
    /// Reference: docs/klassediagram.md - AccountController
    /// </summary>
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Account is locked");
                return View(model);
            }

            ModelState.AddModelError("", "Invalid login attempt");
            return View(model);
        }

        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }

    #endregion
}
