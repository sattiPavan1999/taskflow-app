using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TaskApi.DTOs;
using TaskApi.Exceptions;
using TaskApi.Models;
using TaskApi.Services;
using TaskApi.Settings;

namespace TaskApi.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        var jwtSettings = Options.Create(new JwtSettings
        {
            SecretKey = "TestSecretKeyThatIsAtLeast32CharactersLong!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationMinutes = 60
        });

        _service = new AuthService(_context, jwtSettings, new Mock<ILogger<AuthService>>().Object);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task RegisterAsync_ReturnsTokenAndEmail_WhenValid()
    {
        var dto = new RegisterDto { Email = "user@test.com", Password = "password123" };

        var result = await _service.RegisterAsync(dto);

        Assert.NotEmpty(result.Token);
        Assert.Equal("user@test.com", result.Email);
    }

    [Fact]
    public async Task RegisterAsync_PersistsUser_WithHashedPassword()
    {
        var dto = new RegisterDto { Email = "user@test.com", Password = "password123" };

        await _service.RegisterAsync(dto);

        var user = await _context.Users.SingleAsync(u => u.Email == "user@test.com");
        Assert.NotEqual("password123", user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("password123", user.PasswordHash));
    }

    [Fact]
    public async Task RegisterAsync_ThrowsValidationException_OnDuplicateEmail()
    {
        var dto = new RegisterDto { Email = "user@test.com", Password = "password123" };
        await _service.RegisterAsync(dto);

        await Assert.ThrowsAsync<ValidationException>(() => _service.RegisterAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_ReturnsToken_WithCorrectCredentials()
    {
        await _service.RegisterAsync(new RegisterDto { Email = "user@test.com", Password = "password123" });

        var result = await _service.LoginAsync(new LoginDto { Email = "user@test.com", Password = "password123" });

        Assert.NotEmpty(result.Token);
        Assert.Equal("user@test.com", result.Email);
    }

    [Fact]
    public async Task LoginAsync_ThrowsValidationException_WithWrongPassword()
    {
        await _service.RegisterAsync(new RegisterDto { Email = "user@test.com", Password = "password123" });

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.LoginAsync(new LoginDto { Email = "user@test.com", Password = "wrongpassword" }));
    }

    [Fact]
    public async Task LoginAsync_ThrowsValidationException_WithUnknownEmail()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.LoginAsync(new LoginDto { Email = "nobody@test.com", Password = "password123" }));
    }
}
