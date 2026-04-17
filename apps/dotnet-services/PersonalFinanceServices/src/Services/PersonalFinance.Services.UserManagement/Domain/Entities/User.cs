using PersonalFinance.Services.UserManagement.Domain.Events;
using PersonalFinance.Shared.Common.Domain;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.UserManagement.Domain.Entities
{
    public class User : AggregateRoot
    {
        public string UserName { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public Email Email { get; private set; }
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public bool IsEmailConfirmed { get; private set; } = false;
        public string? PhoneNumber { get; private set; }
        public string? GoogleId { get; private set; }
        public DateTime? LastLoginAt { get; private set; }

        // Gmail API access tokens (separate from app JWT tokens)
        public string? GmailAccessToken { get; private set; }
        public string? GmailRefreshToken { get; private set; }
        public DateTime? GmailTokenExpiresAt { get; private set; }
        public bool HasGmailAccess => !string.IsNullOrEmpty(GmailRefreshToken);

        private User() { }

        private readonly List<UserRole> _userRoles = new();
        private readonly List<RefreshToken> _refreshTokens = new();

        public User(string email, string userName, string firstName, string lastName)
        {
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
            Email = new Email(email);
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            IsEmailConfirmed = false;

            AddDomainEvent(new UserRegisteredEvent(Id, Email, $"{FirstName} {LastName}"));
        }

        // Navigation properties
        public UserProfile? Profile { get; private set; }
        public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
        public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

        // Business methods
        public void UpdateProfile(string firstName, string lastName, string? phoneNumber)
        {
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            PhoneNumber = phoneNumber;

            AddDomainEvent(new UserProfileUpdatedEvent(Id, firstName, lastName));
        }

        public void SetPasswordHash(string passwordHash)
        {
            PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        }

        public void ConfirmEmail()
        {
            IsEmailConfirmed = true;
            AddDomainEvent(new UserEmailConfirmedEvent(Id, Email));
        }

        public void RecordLogin()
        {
            LastLoginAt = DateTime.UtcNow;
        }

        public void AddRole(Role role)
        {
            if (_userRoles.Any(ur => ur.RoleId == role.Id))
                return;

            _userRoles.Add(new UserRole(Id, role.Id));
        }

        public void RemoveRole(Role role)
        {
            var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == role.Id);
            if (userRole != null)
            {
                _userRoles.Remove(userRole);
            }
        }

        public bool HasRole(string roleName)
        {
            return _userRoles.Any(ur => ur.Role?.Name == roleName);
        }

        public void CreateProfile(UserProfile userProfile)
        {
            Profile = userProfile ?? throw new ArgumentNullException(nameof(userProfile));
        }

        public void Activate(string reason)
        {
            IsActive = true;
            AddDomainEvent(new UserDeactivatedEvent(Id, reason));
        }

        public void Deactivate(string reason)
        {
            // Logic to deactivate user, e.g., setting a flag or removing roles
            IsActive = false;
            AddDomainEvent(new UserDeactivatedEvent(Id, reason));
        }

        public void SetGoogleId(string googleId)
        {
            GoogleId = googleId;
        }

        public void SetGmailTokens(string accessToken, string refreshToken, DateTime expiresAt)
        {
            GmailAccessToken = accessToken;
            GmailRefreshToken = refreshToken;
            GmailTokenExpiresAt = expiresAt;
        }

        public void ClearGmailTokens()
        {
            GmailAccessToken = null;
            GmailRefreshToken = null;
            GmailTokenExpiresAt = null;
        }

        public void AddRefreshToken(string token, DateTime expiresAt, string? createdByIp = null)
        {
            var refreshToken = new RefreshToken(token, expiresAt, Id, createdByIp);
            _refreshTokens.Add(refreshToken);
        }

        public void RevokeRefreshToken(string token, string? revokedByIp = null, string? replacedByToken = null)
        {
            var refreshToken = _refreshTokens.FirstOrDefault(t => t.Token == token);
            refreshToken?.Revoke(revokedByIp, replacedByToken);
        }

        public void RemoveOldRefreshTokens()
        {
            _refreshTokens.RemoveAll(t => !t.IsActive && t.CreatedAt.AddDays(7) <= DateTime.UtcNow);
        }
    }
}
