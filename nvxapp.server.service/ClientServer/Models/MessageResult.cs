namespace nvxapp.server.service.ClientServer.Models
{
   

    public class MessageResult
    {
        public string? Message { get; set; }
        public MessageType MsgType { get; set; }

        public MessageResult(string? message, MessageType msgType)
        {
            Message = message;
            MsgType = msgType;
        }

    }

    public enum MessageType
    {
        Exception = 0,
        Error = 1,
        Warning = 2,
        Information = 3,
    }

    public interface iMessageResult
    {
        public bool Success { get; }

        public void AddMessage(string message, MessageType type);

        public void AddMessages(List<MessageResult> messages);

        public List<MessageResult> GetMessages(); 

    }

}
