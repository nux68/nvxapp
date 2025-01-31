using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

namespace nvxapp.server.service.ClientServer_Service.ModelsBase
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
        public String? Token { get; set; }

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

        

        public GenericResult(T? data,string? token=null)
        {
            Data = data;
            Messages = new List<Message>();
            Token = token;

            if(data is iGenericResult4Server && data != null)
            {
                Messages.AddRange( ((iGenericResult4Server)data).GetMessages());
            }


            
        }
        public GenericResult(T? data, string? message, MessageType msgType, string? token = null)
        {
            Data = data;
            Token = token;
            Messages = new List<Message>();
            Messages.Add(new Message(message, msgType));
        }

        
    }


    public interface IGenericResult<T>
    {
        bool Success { get; }
        
        List<Message> Messages { get; set; }
        
        T? Data { get; set; }

        String? Token { get; set; }
    }


    public interface iGenericResult4Server
    {
        

        public void AddMessage(string message, MessageType type);

        public void AddMessages(List<Message> messages);

        public List<Message> GetMessages();

    }


}
