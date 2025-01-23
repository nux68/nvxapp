using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Interfaces
{
    public interface ICurrentUser
    {
        string? CurrentUser { get; set; }

    }
}
