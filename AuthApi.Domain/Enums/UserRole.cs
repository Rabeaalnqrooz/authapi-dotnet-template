using System;
using System.Collections.Generic;
using System.Text;

namespace AuthApi.Domain.Enums
{
    /// <summary>
    /// أدوار المستخدم في النظام.
    /// نستخدم enum عشان نمنع أي قيمة غير صحيحة تدخل قاعدة البيانات.
    /// </summary>
    public enum UserRole
    {
        User = 0,
        Admin = 1,
        Disabled = 2
    }
}
