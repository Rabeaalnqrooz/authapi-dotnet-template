using AuthApi.Application.DTOs;
using AuthApi.Application.Exceptions;
using AuthApi.Application.Interfaces;
using AuthApi.Domain.Enums;

namespace AuthApi.Application.Services;

public class AdminService
{
    private readonly IUserRepository _userRepository;

    public AdminService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // =========================================================
    // جلب كل المستخدمين
    // =========================================================
    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToUserDto);
    }

    // =========================================================
    // جلب مستخدم بالـ Id
    // =========================================================
    public async Task<UserDto> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
            throw new NotFoundException("المستخدم غير موجود");

        return MapToUserDto(user);
    }

    // =========================================================
    // حذف مستخدم
    // =========================================================
    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
            throw new NotFoundException("المستخدم غير موجود");

        await _userRepository.DeleteAsync(user);
    }

    // =========================================================
    // تغيير دور المستخدم
    // =========================================================
    public async Task UpdateUserRoleAsync(Guid id, UpdateUserRoleRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
            throw new NotFoundException("المستخدم غير موجود");

        if (!Enum.TryParse<UserRole>(request.Role, true, out var newRole))
            throw new BadRequestException("الدور غير صالح. يجب أن يكون User أو Admin");

        // ما نسمحش بتحويل المستخدم إلى Disabled من هنا
        if (newRole == UserRole.Disabled)
            throw new BadRequestException("استخدم مسار التعطيل المخصص");

        user.Role = newRole;
        await _userRepository.UpdateAsync(user);
    }

    // =========================================================
    // تعطيل مستخدم
    // =========================================================
    public async Task DisableUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
            throw new NotFoundException("المستخدم غير موجود");

        user.Role = UserRole.Disabled;
        await _userRepository.UpdateAsync(user);
    }

    // =========================================================
    // تفعيل مستخدم
    // =========================================================
    public async Task EnableUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
            throw new NotFoundException("المستخدم غير موجود");

        user.Role = UserRole.User;
        await _userRepository.UpdateAsync(user);
    }

    // =========================================================
    // دالة مساعدة
    // =========================================================
    private static UserDto MapToUserDto(Domain.Entities.User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.ToString().ToLower(),
            IsVerified = user.IsVerified,
            ProfilePic = user.ProfilePic,
            Address = user.Address,
            City = user.City,
            ZipCode = user.ZipCode,
            PhoneNo = user.PhoneNo,
            CreatedAt = user.CreatedAt
        };
    }
    public async Task<PagedUsersResponseDto> GetPagedUsersAsync(int page = 1, int limit = 10)
    {
        var (users, total) = await _userRepository.GetPagedAsync(page, limit);

        return new PagedUsersResponseDto
        {
            Users = users.Select(MapToUserDto),
            Total = total,
            Page = page,
            Limit = limit,
            Pages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit)
        };
    }
}