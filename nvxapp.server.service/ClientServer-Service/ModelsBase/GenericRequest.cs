using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.ModelsBase
{
    public class GenericRequest<T>
    {

        public T Data { get; set; }

        // Costruttore senza parametri
        public GenericRequest()
        {
            Data = Activator.CreateInstance<T>();
        }

        // Costruttore con parametro
        public GenericRequest(T data)
        {
            Data = data;
        }
    }

    
}
