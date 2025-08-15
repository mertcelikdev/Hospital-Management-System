using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.DTOs
{
    public class DepartmentDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Code { get; set; }
        public string? HeadOfDepartment { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Location { get; set; }
        public int DoctorCount { get; set; }
        public int NurseCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateDepartmentDto
    {
        [Required(ErrorMessage = "Departman adı gereklidir")]
        [StringLength(100, ErrorMessage = "Departman adı en fazla 100 karakter olabilir")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string? Description { get; set; }

        [StringLength(20, ErrorMessage = "Departman kodu en fazla 20 karakter olabilir")]
        public string? Code { get; set; }

        [StringLength(100, ErrorMessage = "Başhekim adı en fazla 100 karakter olabilir")]
        public string? HeadOfDepartment { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        public string? Email { get; set; }

        [StringLength(200, ErrorMessage = "Konum en fazla 200 karakter olabilir")]
        public string? Location { get; set; }

    }

    public class UpdateDepartmentDto
    {
        [StringLength(100, ErrorMessage = "Departman adı en fazla 100 karakter olabilir")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string? Description { get; set; }

        [StringLength(20, ErrorMessage = "Departman kodu en fazla 20 karakter olabilir")]
        public string? Code { get; set; }

        [StringLength(100, ErrorMessage = "Başhekim adı en fazla 100 karakter olabilir")]
        public string? HeadOfDepartment { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        public string? Email { get; set; }

        [StringLength(200, ErrorMessage = "Konum en fazla 200 karakter olabilir")]
        public string? Location { get; set; }

    }
}
