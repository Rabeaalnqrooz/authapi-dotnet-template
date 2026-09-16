using AuthApi.Application.DTOs;

using AuthApi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.API.Controllers;

[ApiController]
[Route("api/v1/user")]
[Authorize(Roles = "Admin")]   // كل الدوال هنا للأدمن فقط
public class AdminController : ControllerBase
{
    private readonly AdminService _adminService;

    public AdminController(AdminService adminService)
    {
        _adminService = adminService;
    }

    // =========================================================
    // جلب كل المستخدمين
    // =========================================================
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers(
     [FromQuery] int page = 1,
     [FromQuery] int limit = 10)
    {
        try
        {
            var result = await _adminService.GetPagedUsersAsync(page, limit);
            return Ok(new
            {
                success = true,
                users = result.Users,
                total = result.Total,
                page = result.Page,
                limit = result.Limit,
                pages = result.Pages
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // =========================================================
    // جلب مستخدم واحد
    // =========================================================
    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        try
        {
            var user = await _adminService.GetUserByIdAsync(id);
            return Ok(new { success = true, user });
        }
        catch (Exception ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    // =========================================================
    // حذف مستخدم
    // =========================================================
    [HttpDelete("users/{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            await _adminService.DeleteUserAsync(id);
            return Ok(new { success = true, message = "تم حذف المستخدم بنجاح" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // =========================================================
    // تغيير دور المستخدم
    // =========================================================
    [HttpPut("users/{id:guid}/role")]
    public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleRequestDto request)
    {
        try
        {
            await _adminService.UpdateUserRoleAsync(id, request);
            return Ok(new { success = true, message = "تم تحديث الدور بنجاح" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // =========================================================
    // تعطيل مستخدم
    // =========================================================
    [HttpPut("users/{id:guid}/disable")]
    public async Task<IActionResult> DisableUser(Guid id)
    {
        try
        {
            await _adminService.DisableUserAsync(id);
            return Ok(new { success = true, message = "تم تعطيل المستخدم" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // =========================================================
    // تفعيل مستخدم
    // =========================================================
    [HttpPut("users/{id:guid}/enable")]
    public async Task<IActionResult> EnableUser(Guid id)
    {
        try
        {
            await _adminService.EnableUserAsync(id);
            return Ok(new { success = true, message = "تم تفعيل المستخدم" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}