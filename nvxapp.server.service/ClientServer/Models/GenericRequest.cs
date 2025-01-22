using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer.Models
{
    public class GenericRequest<T>
    {

        public T Data { get; set; }

    }
}
