using static System.Runtime.InteropServices.JavaScript.JSType;

namespace nvxapp.server.service.ClientServer.Models
{
   

    public class ModelResult : iMessageResult
    {
        public List<MessageResult> Messages { get; set; }

        public ModelResult()
        {
            Messages = new List<MessageResult>();
        }

        public bool Success
        {
            get
            {
                return Messages.Where(x => x.MsgType == MessageType.Exception).Count() == 0;
            }
        }

        public void AddMessage(string message, MessageType type)
        {
            Messages.Add(new MessageResult(message, type));
        }

    }

   

}
