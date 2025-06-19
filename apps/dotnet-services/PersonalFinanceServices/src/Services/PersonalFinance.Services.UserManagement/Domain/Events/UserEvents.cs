using PersonalFinance.Shared.Common.Domain;

namespace PersonalFinance.Services.UserManagement.Domain.Events
{
    public class UserRegisteredEvent : DomainEvent
    {
        public UserRegisteredEvent(Guid userId, string email, string fullName)
        {
            UserId = userId;
            Email = email;
            FullName = fullName;
        }

        public Guid UserId { get; }
        public string Email { get; }
        public string FullName { get; }
    }

    public class UserProfileUpdatedEvent : DomainEvent
    {
        public UserProfileUpdatedEvent(Guid userId, string firstName, string lastName)
        {
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
        }

        public Guid UserId { get; }
        public string FirstName { get; }
        public string LastName { get; }
    }

    public class UserEmailConfirmedEvent : DomainEvent
    {
        public UserEmailConfirmedEvent(Guid userId, string email)
        {
            UserId = userId;
            Email = email;
        }

        public Guid UserId { get; }
        public string Email { get; }
    }

    public class UserDeactivatedEvent : DomainEvent
    {
        public UserDeactivatedEvent(Guid userId, string reason)
        {
            UserId = userId;
            Reason = reason;
        }

        public Guid UserId { get; }
        public string Reason { get; }
    }
}