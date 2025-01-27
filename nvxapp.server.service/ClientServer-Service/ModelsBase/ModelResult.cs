using static System.Runtime.InteropServices.JavaScript.JSType;

namespace nvxapp.server.service.ClientServer_Service.ModelsBase
{
   

    public class ModelResult : iModelResult,iGenericResult4Server
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


}
