namespace HospitalManagementSystem.Security;

public static class PermissionCatalog
{
	public static class Users
	{
		public const string View = "users.view";
		public const string Create = "users.create";
		public const string Update = "users.update";
		public const string Delete = "users.delete";
	}

	public static class Appointments
	{
		public const string View = "appointments.view";
		public const string Create = "appointments.create";
		public const string Update = "appointments.update";
		public const string Cancel = "appointments.cancel";
	}

	public static class Medications
	{
		public const string View = "medications.view";
		public const string Create = "medications.create";
		public const string Update = "medications.update";
		public const string Delete = "medications.delete";
	}

	public static readonly Dictionary<string, List<string>> DefaultRolePermissions = new()
	{
		["Admin"] = new List<string>{ Users.View, Users.Create, Users.Update, Users.Delete, Appointments.View, Appointments.Create, Appointments.Update, Appointments.Cancel, Medications.View, Medications.Create, Medications.Update, Medications.Delete },
		["Doctor"] = new List<string>{ Appointments.View, Appointments.Create, Appointments.Update, Medications.View },
		["Nurse"] = new List<string>{ Appointments.View, Medications.View },
		["Staff"] = new List<string>{ Appointments.View },
		["Patient"] = new List<string>{ Appointments.View, Appointments.Create, Appointments.Cancel }
	};
}
