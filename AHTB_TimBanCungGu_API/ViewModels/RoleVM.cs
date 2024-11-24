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
        public string TenRole { get; set; }
    }
    public class RoleListVM
    {
        public List<RoleVM> Roles { get; set; } = new List<RoleVM>();
        public string TenRole { get; set; }
        public List<RoleVMUpdate> RoleUpdate { get; set; } = new List<RoleVMUpdate>();
    }
    public class RoleVMUpdate
    {
        public int IDRole { get; set; }
        public string TenRole { get; set; }
        public List<string> Module { get; set; } = new();
        public List<string> Add { get; set; } = new();
        public List<string> Update { get; set; } = new();
        public List<string> Delete { get; set; } = new();
        public List<string> ReviewDetails { get; set; } = new();
    }
}
