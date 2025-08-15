using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HospitalManagementSystem.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userRole = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            
            if (string.IsNullOrEmpty(userRole) || userRole != "Admin")
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireDoctorAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userRole = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            
            if (string.IsNullOrEmpty(userRole) || userRole != "Doctor")
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireNurseAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userRole = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            
            if (string.IsNullOrEmpty(userRole) || userRole != "Nurse")
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireStaffAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userRole = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            
            if (string.IsNullOrEmpty(userRole) || (userRole != "Doctor" && userRole != "Nurse" && userRole != "Admin" && userRole != "Staff"))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireAuthenticationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
