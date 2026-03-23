using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace nvxapp.server.service.ClientServer_Service.ModelsBase
{


    public class ModelResult : iModelResult, iGenericResult4Server
    {
        //iModelResult
        public List<Message> Messages { get; set; }
        public bool Success
        {
            get
            {
                return Messages.Where(x => x.MsgType == MessageType.Exception).Count() == 0;
            }
        }


        //iGenericResult4Server
        public List<Message> GetMessages()
        {
            return Messages;
        }
        public void AddMessage(string message, MessageType type)
        {
            Messages.Add(new Message(message, type));
        }
        public void AddMessages(List<Message> messages)
        {
            Messages.AddRange(messages);
        }


        public ModelResult()
        {
            Messages = new List<Message>();
        }


    }


    public interface iModelResult
    {
        bool Success { get; }
        List<Message> Messages { get; set; }
    }



    public class HashModel
    {
        public string Hash { get; set; }

        public string CalcolaHash<T>(T instance, params Expression<Func<T, object>>[] campi)
        {
            var values = new List<string>();

            foreach (var campo in campi)
            {
                var compiled = campo.Compile();
                var value = compiled(instance);

                values.Add(value?.ToString() ?? "null");
            }

            var raw = string.Join("|", values);

            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(raw);
            var hashBytes = sha.ComputeHash(bytes);

            return Convert.ToHexString(hashBytes);
        }
    }

}
