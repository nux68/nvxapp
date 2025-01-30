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
        public string Key { get; set; } = "ChiaveMoltoLungaESicuraPerSviluppo123456!";
        //Il server che genera il token(es.https://mio-dominio.com).
        public string Issuer { get; set; }  = "https://localhost:7146";
        //Chi può usare il token(es.https://mio-client.com).
        public string Audience { get; set; } = "http://localhost:4200";
        public int ExpireMinutes { get; set; } = 0;
    }
}
