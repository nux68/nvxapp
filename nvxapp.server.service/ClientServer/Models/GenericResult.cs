using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer.Models
{
    public class GenericResult<T> //: UserAuth
    {

        public bool Success { get; set; }

        public string Error { get; set; }

        public T Data { get; set; }

    }
}
