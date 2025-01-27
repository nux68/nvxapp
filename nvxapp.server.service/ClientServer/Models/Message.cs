namespace nvxapp.server.service.ClientServer.Models
{
   

    public class Message
    {
        public string? Text { get; set; }
        public MessageType MsgType { get; set; }

        public Message(string? text, MessageType msgType)
        {
            Text = text;
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



}
