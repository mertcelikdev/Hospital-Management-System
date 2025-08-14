using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Attributes
{
    public class RequirePermissionAttribute : ActionFilterAttribute
    {
        private readonly string _permission;

        public RequirePermissionAttribute(string permission)
        {
            _permission = permission;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userRole = context.HttpContext.Session.GetString("UserRole");
            var userId = context.HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(userId))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (!RolePermissions.HasPermission(userRole, _permission))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }

    // Specific permission attributes for common operations
    public class CanViewPatientsAttribute : RequirePermissionAttribute
    {
        public CanViewPatientsAttribute() : base(Permissions.ViewPatients) { }
    }

    public class CanCreatePatientsAttribute : RequirePermissionAttribute
    {
        public CanCreatePatientsAttribute() : base(Permissions.CreatePatients) { }
    }

    public class CanUpdatePatientsAttribute : RequirePermissionAttribute
    {
        public CanUpdatePatientsAttribute() : base(Permissions.UpdatePatients) { }
    }

    public class CanDeletePatientsAttribute : RequirePermissionAttribute
    {
        public CanDeletePatientsAttribute() : base(Permissions.DeletePatients) { }
    }

    public class CanViewAppointmentsAttribute : RequirePermissionAttribute
    {
        public CanViewAppointmentsAttribute() : base(Permissions.ViewAppointments) { }
    }

    public class CanCreateAppointmentsAttribute : RequirePermissionAttribute
    {
        public CanCreateAppointmentsAttribute() : base(Permissions.CreateAppointments) { }
    }

    public class CanUpdateAppointmentsAttribute : RequirePermissionAttribute
    {
        public CanUpdateAppointmentsAttribute() : base(Permissions.UpdateAppointments) { }
    }

    public class CanDeleteAppointmentsAttribute : RequirePermissionAttribute
    {
        public CanDeleteAppointmentsAttribute() : base(Permissions.DeleteAppointments) { }
    }

    public class CanCreatePrescriptionsAttribute : RequirePermissionAttribute
    {
        public CanCreatePrescriptionsAttribute() : base(Permissions.CreatePrescriptions) { }
    }

    public class CanTrackMedicationUsageAttribute : RequirePermissionAttribute
    {
        public CanTrackMedicationUsageAttribute() : base(Permissions.TrackMedicationUsage) { }
    }

    public class CanManageSystemAttribute : RequirePermissionAttribute
    {
        public CanManageSystemAttribute() : base(Permissions.ManageSystem) { }
    }
}
