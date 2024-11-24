using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AHTB_TimBanCungGu_API.Models
{
    public class Role
    {
        [Key]
        public int IDRole { get; set; }
        public string Module { get; set; }
        public string Add { get; set; }
        public string Update { get; set; }
        public string Delete { get; set; }
        public string ReviewDetails { get; set; }
        public string TenRole { get; set; }

        public ICollection<User_Role> User_Role { get; set; }
    }
}
