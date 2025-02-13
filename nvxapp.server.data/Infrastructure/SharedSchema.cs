using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Infrastructure
{
    //4 SCHEMA
    public static class SharedSchema
    {

        public static Boolean MigrazioneRunTime { get; set; } = false;

        public static Boolean MultiTenant { get; set; } = false;
        public static string CurrentSchema { get; set; } = "public";
    }
}
