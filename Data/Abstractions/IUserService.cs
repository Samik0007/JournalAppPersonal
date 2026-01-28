using JournalPersonalApp.Data.Models;

namespace JournalPersonalApp.Data.Abstractions;

public enum UserAuthMode
{
    SetupPin,
    LoginPin
}

public interface IUserService
{
    Task<UserAuthMode> GetAuthModeAsync();

    Task<User> CreateUserWithPinAsync(string pin);

    Task<bool> VerifyPinAsync(string pin);

    Task ChangePinAsync(string currentPin, string newPin);

    Task LogoutAsync();
}
