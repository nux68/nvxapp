using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;

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




        /// <summary>
        /// errore previsto, con un messaggio per l'utente (gia' nella sua lingua): UserMessageException
        /// o applicativo non attivo per l'azienda. Le altre eccezioni sono errori tecnici.
        /// </summary>
        public static bool IsUserMessage(this Exception e)
        {
            return e is UserMessageException || e is ApplicationNotActiveException;
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
