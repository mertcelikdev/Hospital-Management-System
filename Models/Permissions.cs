namespace HospitalManagementSystem.Models
{
    // Permission constants
    public static class Permissions
    {
    // General
    public const string ViewDashboard = "dashboard:view";
        // Patient Permissions
        public const string ViewPatients = "patients:view";
        public const string CreatePatients = "patients:create";
        public const string UpdatePatients = "patients:update";
        public const string DeletePatients = "patients:delete";

        // Appointment Permissions
        public const string ViewAppointments = "appointments:view";
        public const string CreateAppointments = "appointments:create";
        public const string UpdateAppointments = "appointments:update";
        public const string DeleteAppointments = "appointments:delete";

        // Medical Records Permissions
        public const string ViewMedicalRecords = "medical_records:view";
        public const string CreateMedicalRecords = "medical_records:create";
        public const string UpdateMedicalRecords = "medical_records:update";
        public const string DeleteMedicalRecords = "medical_records:delete";

        // Prescription Permissions
        public const string ViewPrescriptions = "prescriptions:view";
        public const string CreatePrescriptions = "prescriptions:create";
        public const string UpdatePrescriptions = "prescriptions:update";
        public const string DeletePrescriptions = "prescriptions:delete";

        // Medication Stock Permissions
        public const string ViewMedications = "medications:view";
        public const string CreateMedications = "medications:create";
        public const string UpdateMedications = "medications:update";
        public const string DeleteMedications = "medications:delete";
        public const string TrackMedicationUsage = "medications:track_usage";

        // Department Permissions
        public const string ViewDepartments = "departments:view";
        public const string CreateDepartments = "departments:create";
        public const string UpdateDepartments = "departments:update";
        public const string DeleteDepartments = "departments:delete";

        // Doctor Permissions
        public const string ViewDoctors = "doctors:view";
        public const string CreateDoctors = "doctors:create";
        public const string UpdateDoctors = "doctors:update";
        public const string DeleteDoctors = "doctors:delete";

        // Nurse Permissions
        public const string ViewNurses = "nurses:view";
        public const string CreateNurses = "nurses:create";
        public const string UpdateNurses = "nurses:update";
        public const string DeleteNurses = "nurses:delete";

        // Staff Permissions
        public const string ViewStaff = "staff:view";
        public const string CreateStaff = "staff:create";
        public const string UpdateStaff = "staff:update";
        public const string DeleteStaff = "staff:delete";

        // Reports Permissions
        public const string ViewReports = "reports:view";
        public const string CreateReports = "reports:create";

        // Admin Permissions
        public const string ManageSystem = "system:manage";
        public const string ViewLogs = "logs:view";
        public const string ManageRoles = "roles:manage";
    }

    // Role-based permission mapping
    public static class RolePermissions
    {
        public static readonly Dictionary<string, List<string>> PermissionsByRole = new()
        {
            {
                "Staff", new List<string>
                {
                    Permissions.ViewDashboard,
                    // Staff can register patients and manage appointments but cannot delete
                    Permissions.ViewPatients,
                    Permissions.CreatePatients,
                    Permissions.UpdatePatients,
                    
                    Permissions.ViewAppointments,
                    Permissions.CreateAppointments,
                    Permissions.UpdateAppointments,
                    
                    // Basic viewing permissions
                    Permissions.ViewDepartments,
                    Permissions.ViewDoctors
                }
            },
            {
                "Nurse", new List<string>
                {
                    Permissions.ViewDashboard,
                    // Nurses have view-only access to most things + medication tracking
                    Permissions.ViewPatients,
                    Permissions.ViewAppointments,
                    Permissions.ViewMedicalRecords,
                    Permissions.ViewPrescriptions,
                    
                    // Medication management
                    Permissions.ViewMedications,
                    Permissions.UpdateMedications,
                    Permissions.TrackMedicationUsage,
                    
                    // Can add notes to medical records
                    Permissions.UpdateMedicalRecords,
                    
                    // Basic viewing permissions
                    Permissions.ViewDepartments,
                    Permissions.ViewDoctors,
                    Permissions.ViewNurses
                }
            },
            {
                "Doctor", new List<string>
                {
                    Permissions.ViewDashboard,
                    // Doctors have full control over medical aspects
                    Permissions.ViewPatients,
                    Permissions.UpdatePatients,
                    
                    // Full appointment control
                    Permissions.ViewAppointments,
                    Permissions.CreateAppointments,
                    Permissions.UpdateAppointments,
                    Permissions.DeleteAppointments,
                    
                    // Full medical records control
                    Permissions.ViewMedicalRecords,
                    Permissions.CreateMedicalRecords,
                    Permissions.UpdateMedicalRecords,
                    Permissions.DeleteMedicalRecords,
                    
                    // Full prescription control
                    Permissions.ViewPrescriptions,
                    Permissions.CreatePrescriptions,
                    Permissions.UpdatePrescriptions,
                    Permissions.DeletePrescriptions,
                    
                    // Medication viewing and usage tracking
                    Permissions.ViewMedications,
                    Permissions.TrackMedicationUsage,
                    
                    // Basic viewing permissions
                    Permissions.ViewDepartments,
                    Permissions.ViewDoctors,
                    Permissions.ViewNurses,
                    
                    // Reports
                    Permissions.ViewReports,
                    Permissions.CreateReports
                }
            },
            {
                "Admin", new List<string>
                {
                    Permissions.ViewDashboard,
                    // Admin has all permissions
                    Permissions.ViewPatients,
                    Permissions.CreatePatients,
                    Permissions.UpdatePatients,
                    Permissions.DeletePatients,
                    
                    Permissions.ViewAppointments,
                    Permissions.CreateAppointments,
                    Permissions.UpdateAppointments,
                    Permissions.DeleteAppointments,
                    
                    Permissions.ViewMedicalRecords,
                    Permissions.CreateMedicalRecords,
                    Permissions.UpdateMedicalRecords,
                    Permissions.DeleteMedicalRecords,
                    
                    Permissions.ViewPrescriptions,
                    Permissions.CreatePrescriptions,
                    Permissions.UpdatePrescriptions,
                    Permissions.DeletePrescriptions,
                    
                    Permissions.ViewMedications,
                    Permissions.CreateMedications,
                    Permissions.UpdateMedications,
                    Permissions.DeleteMedications,
                    Permissions.TrackMedicationUsage,
                    
                    Permissions.ViewDepartments,
                    Permissions.CreateDepartments,
                    Permissions.UpdateDepartments,
                    Permissions.DeleteDepartments,
                    
                    Permissions.ViewDoctors,
                    Permissions.CreateDoctors,
                    Permissions.UpdateDoctors,
                    Permissions.DeleteDoctors,
                    
                    Permissions.ViewNurses,
                    Permissions.CreateNurses,
                    Permissions.UpdateNurses,
                    Permissions.DeleteNurses,
                    
                    Permissions.ViewStaff,
                    Permissions.CreateStaff,
                    Permissions.UpdateStaff,
                    Permissions.DeleteStaff,
                    
                    Permissions.ViewReports,
                    Permissions.CreateReports,
                    
                    Permissions.ManageSystem,
                    Permissions.ViewLogs,
                    Permissions.ManageRoles
                }
            }
        };

        public static bool HasPermission(string role, string permission)
        {
            if (PermissionsByRole.TryGetValue(role, out var rolePermissions))
            {
                return rolePermissions.Contains(permission);
            }
            return false;
        }

        public static List<string> GetPermissions(string role)
        {
            return PermissionsByRole.TryGetValue(role, out var rolePermissions) 
                ? rolePermissions 
                : new List<string>();
        }
    }
}
