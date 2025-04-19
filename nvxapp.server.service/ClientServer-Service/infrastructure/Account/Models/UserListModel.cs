using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nvxapp.server.data.Entities.AspNetUsersDataUtil;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models
{

    public class UserListModel
    {
        public string? IdAspNetUsers { get; set; }
        public string? Descrizione { get; set; }
        public string? RoleId { get; set; }
    }
    public class UserListInModel
    {
    }
    public class UserListOutModel : ModelResult
    {
        public List<UserListModel> UserList { get; set; }

        public UserListOutModel()
        {
            UserList = new List<UserListModel>();
        }
    }
    public class UserEditModel
    {
        public string IdAspNetUsers { get; set; } = string.Empty;
        public string? Descrizione { get; set; } = string.Empty;
        public string? Mail { get; set; } = string.Empty;
        public string? Pw { get; set; } = string.Empty;
        public string? RoleId { get; set; }
    }
    public class UserGetInModel
    {
        public string Id { get; set; } = string.Empty;
    }
    public class UserGetOutModel : ModelResult
    {
        public UserEditModel UserEdit { get; set; } = new UserEditModel();
    }
    public class UserPutInModel : ModelResult
    {
        public UserEditModel UserEdit { get; set; } = new UserEditModel();
    }
    public class UserPutOutModel : ModelResult
    {
        public UserEditModel UserEdit { get; set; } = new UserEditModel();
    }

}
