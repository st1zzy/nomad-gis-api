using System.ComponentModel.DataAnnotations;

namespace nomad_gis_V2.DTOs.Users
{
    public class UpdateRoleRequest
    {
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}