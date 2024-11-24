using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AHTB_TimBanCungGu_API.Models
{
    public class User_Role
    {
        [Key]
        public int IDRole_US { get; set; } 

 
        [ForeignKey("User")]
        public string UsID { get; set; }
        public User User { get; set; }  


        [ForeignKey("Role")]
        public int IDRole { get; set; }
        public Role Role { get; set; }  

        public string TrangThai { get; set; }
    }
}
