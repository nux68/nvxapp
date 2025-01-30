using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ServerModels
{
    public class JwtParameter
    {
        //chiave segreta per firmare il token
        public string Key { get; set; } = "";
        //Il server che genera il token(es.https://mio-dominio.com).
        public string Issuer { get; set; }  = "";
        //Chi può usare il token(es.https://mio-client.com).
        public string Audience { get; set; } = "";
        public int ExpireMinutes { get; set; } = 0;
    }
}
