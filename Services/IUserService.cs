using CareSphere.Models;

namespace CareSphere.Services
{
    public class UserCreateResult
    {
        public bool Success { get; set; }
        public ApplicationUser? User { get; set; }
        public List<string> Errors { get; set; } = new();
        public static UserCreateResult Ok(ApplicationUser user) => new() { Success = true, User = user };
        public static UserCreateResult Fail(IEnumerable<string> errors) => new() { Success = false, Errors = errors.ToList() };
    }

    public class UserListItem
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Department { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public Guid? DoctorId { get; set; }
    }

    public interface IUserService
    {
        Task<UserCreateResult> CreateUserAsync(Guid tenantId, string fullName, string email, string password, string role, string? department = null, Guid? doctorId = null);
        Task<UserCreateResult> UpdateUserAsync(string userId, string fullName, string? department, bool isActive, string preferredLanguage, Guid? doctorId);
        Task<UserCreateResult> ResetPasswordAsync(string userId, string newPassword);
        Task<UserCreateResult> ToggleUserActiveAsync(string userId, string performedByUserId);
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<List<UserListItem>> GetUsersByTenantAsync(Guid tenantId, string? search = null, string? role = null, bool? isActive = null);
    }
}
