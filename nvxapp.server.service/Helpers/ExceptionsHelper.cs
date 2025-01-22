using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.Helpers
{
    public enum HttpErrors
    {
        None,
        NotAuthorized,
        InvalidParameters
    }


    public static class ExceptionsHelper
    {
        public static string ToErrorString(this Exception e)
        {
            var erMsg = e.Message;

            if (e.InnerException != null)
            {
                erMsg += " INNER: " + e.InnerException.Message;
            }

            return erMsg;
        }




        public static string ToErrorString(this HttpErrors error)
        {
            switch (error)
            {
                case HttpErrors.NotAuthorized:
                    return "NotAuthorized"; // HttpErr.NotAuthorized;
                case HttpErrors.InvalidParameters:
                    return "InvalidParameters"; // HttpErr.InvalidParameters;
                case HttpErrors.None:
                default:
                    return ""; //string.Empty;
            }
        }
    }
}
