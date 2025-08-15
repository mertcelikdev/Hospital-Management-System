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
            // Önce JWT Claims üzerinden oku
            var claimsUser = context.HttpContext.User;
            if (claimsUser?.Identity?.IsAuthenticated != true)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            var userRole = claimsUser.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            var userId = claimsUser.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userRole) || string.IsNullOrWhiteSpace(userId))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            if (!_allowedRoles.Contains(userRole))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                return;
            }

            context.HttpContext.Items["CurrentUserRole"] = userRole;

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
