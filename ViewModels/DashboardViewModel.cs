using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalMedicines { get; set; }
        public int LowStockMedicines { get; set; }
        public int TodayAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        
        public List<User> RecentUsers { get; set; } = new();
        public List<Appointment> RecentAppointments { get; set; } = new();
        
        public List<int> AppointmentChartData { get; set; } = new();
        public List<int> UserRegistrationChartData { get; set; } = new();
    }
}
