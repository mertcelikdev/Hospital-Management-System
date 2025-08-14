using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Attributes
{
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _allowedRoles;

        public AuthorizeRoleAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Session'dan kullanıcı bilgilerini al
            var userRole = context.HttpContext.Session.GetString("UserRole");
            var userId = context.HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(userId))
            {
                // Kullanıcı giriş yapmamış, login sayfasına yönlendir
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            if (!string.IsNullOrEmpty(userRole))
            {
                if (!_allowedRoles.Contains(userRole))
                {
                    // Yetkisiz erişim
                    context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                    return;
                }
            }
            else
            {
                // Geçersiz rol
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }

    public class RequireDoctorAttribute : AuthorizeRoleAttribute
    {
        public RequireDoctorAttribute() : base("Doctor", "Admin") { }
    }

    public class RequireNurseAttribute : AuthorizeRoleAttribute
    {
        public RequireNurseAttribute() : base("Nurse", "Doctor", "Admin") { }
    }

    public class RequireStaffAttribute : AuthorizeRoleAttribute
    {
        public RequireStaffAttribute() : base("Staff", "Admin") { }
    }

    public class RequireAdminAttribute : AuthorizeRoleAttribute
    {
        public RequireAdminAttribute() : base("Admin") { }
    }

    public class RequireHealthcareAttribute : AuthorizeRoleAttribute
    {
        public RequireHealthcareAttribute() : base("Doctor", "Nurse", "Admin") { }
    }
}
