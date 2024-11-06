using System.Collections.Generic;

namespace AHTB_TimBanCungGu_API.ViewModels
{
    public class RoleVM
    {
        public string User {  get; set; }
        public int IDRole { get; set; }
        public string Module { get; set; }
        public string Add { get; set; }
        public string Update { get; set; }
        public string Delete { get; set; }
        public string ReviewDetails { get; set; }
    }
    public class RoleListVM
    {
        public List<RoleVM> Roles { get; set; } = new List<RoleVM>();
    }

}
