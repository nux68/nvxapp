using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

namespace nvxapp.server.service.ClientServer.Models
{
    public class GenericResult<T> : IGenericResult<T>, iGenericResult4Server 
    {
        
        public bool Success
        {
            get
            {
                return Messages.Where(x => x.MsgType == MessageType.Exception).Count() == 0;
            }
        }
        public List<Message> Messages { get; set; }
        public T? Data { get; set; }

        //iGenericResult4Server
        public List<Message> GetMessages()
        {
            return Messages;
        }
        public void AddMessages(List<Message> messages)
        {
            Messages.AddRange(messages);
        }
        public void AddMessage(string text, MessageType type)
        {
            Messages.Add(new Message(text, type));
        }

        

        public GenericResult(T? data)
        {
            Data = data;
            Messages = new List<Message>();

            if(data is iGenericResult4Server && data != null)
            {
                Messages.AddRange( ((iGenericResult4Server)data).GetMessages());
            }


            
        }
        public GenericResult(T? data, string? message, MessageType msgType)
        {
            Data = data;
            Messages = new List<Message>();
            Messages.Add(new Message(message, msgType));
        }

        
    }


    public interface IGenericResult<T>
    {
        bool Success { get; }
        
        List<Message> Messages { get; set; }
        
        T? Data { get; set; }
    }


    public interface iGenericResult4Server
    {
        

        public void AddMessage(string message, MessageType type);

        public void AddMessages(List<Message> messages);

        public List<Message> GetMessages();

    }


}
