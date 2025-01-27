using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

namespace nvxapp.server.service.ClientServer.Models
{
    public class GenericResult<T> : iMessageResult //UserAuth
    {

        public bool Success
        {
            get
            {
                return Messages.Where(x => x.MsgType == MessageType.Exception).Count() == 0;
            }
        }

        public List<MessageResult> GetMessages()
        {
            return Messages;
        }

        public void AddMessages(List<MessageResult> messages)
        {
            Messages.AddRange(messages);
        }

        public void AddMessage(string message, MessageType type)
        {
            Messages.Add(new MessageResult(message, type));
        }

        public List<MessageResult> Messages { get; set; }

        public T? Data { get; set; }

        public GenericResult(T? data)
        {
            Data = data;
            Messages = new List<MessageResult>();

            if(data is iMessageResult && data != null)
            {
                Messages.AddRange( ((iMessageResult)data).GetMessages());
            }


            
        }

        public GenericResult(T? data, string? message, MessageType msgType)
        {
            Data = data;
            Messages = new List<MessageResult>();
            Messages.Add(new MessageResult(message, msgType));
        }

        
    }


 

}
