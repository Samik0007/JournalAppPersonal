using JournalPersonalApp.Data.Abstractions;
using JournalPersonalApp.Data.Models;
using JournalPersonalApp.Data.Utils;
using Microsoft.EntityFrameworkCore;

namespace JournalPersonalApp.Data.Services;

public sealed class UserService(DBcontext context) : IUserService
{
    private readonly DBcontext _context = context;

    public async Task<UserAuthMode> GetAuthModeAsync()
    {
        var hasUser = await _context.Users.AnyAsync();
        return hasUser ? UserAuthMode.LoginPin : UserAuthMode.SetupPin;
    }

    public async Task<User> CreateUserWithPinAsync(string pin)
    {
        if (pin.Length != 4 || !pin.All(char.IsDigit))
            throw new ArgumentException("PIN must be 4 digits.", nameof(pin));

        if (await _context.Users.AnyAsync())
            throw new InvalidOperationException("User already exists.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            pin = int.Parse(pin),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> VerifyPinAsync(string pin)
    {
        if (pin.Length != 4 || !pin.All(char.IsDigit))
            return false;

        var pinValue = int.Parse(pin);
        return await _context.Users.AnyAsync(u => u.pin == pinValue);
    }

    public Task SetPasswordAsync(string password) => Task.CompletedTask;

    public Task<bool> VerifyPasswordAsync(string password) => Task.FromResult(true);

    public async Task ChangePasswordAsync(string currentPassword, string newPassword)
    {
        // Treat "password" as PIN here, since the model stores PIN only.
        await ChangePinAsync(currentPassword, newPassword);
    }

    public async Task ChangePinAsync(string currentPin, string newPin)
    {
        try
        {
            if (currentPin.Length != 4 || !currentPin.All(char.IsDigit))
                throw new ArgumentException("Current PIN must be 4 digits.", nameof(currentPin));

            if (newPin.Length != 4 || !newPin.All(char.IsDigit))
                throw new ArgumentException("New PIN must be 4 digits.", nameof(newPin));

            var user = await _context.Users.OrderBy(u => u.CreatedAt).FirstOrDefaultAsync();
            if (user is null)
                throw new InvalidOperationException("No user exists.");

            if (user.pin != int.Parse(currentPin))
                throw new InvalidOperationException("Current PIN is incorrect.");

            user.pin = int.Parse(newPin);
            await _context.SaveChangesAsync();
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to change PIN.", ex);
        }
    }

    public Task LogoutAsync()
    {
        Preferences.Set("IsLoggedIn", false);
        Preferences.Remove("LastLoginTime");
        
        return Task.CompletedTask;
    }
}
