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
        }

        public GenericResult(T? data, string? message, MessageType msgType)
        {
            Data = data;
            Messages = new List<MessageResult>();
            Messages.Add(new MessageResult(message, msgType));
        }

        //public void AddMessagesRecursively<U>(GenericResult<U> result)
        //{
        //    // Somma i messaggi dell'oggetto passato e poi li elimina
        //    Messages.AddRange(result.Messages);
        //    result.Messages.Clear();

        //    // Se il tipo U è un GenericResult, chiama ricorsivamente il metodo
        //    if (result.Data is GenericResult<U> nestedResult)
        //    {
        //        AddMessagesRecursively(nestedResult);
        //    }
        //    else if (result.Data is GenericResult<object> nestedGenericResult)
        //    {
        //        AddMessagesRecursively(nestedGenericResult);
        //    }
        //}

        //public void AddMessagesRecursively<U>(GenericResult<U> result)
        //{
        //    // Somma i messaggi dell'oggetto passato e poi li elimina
        //    Messages.AddRange(result.Messages);
        //    result.Messages.Clear();

        //    // Se il tipo U è un GenericResult, chiama ricorsivamente il metodo
        //    if (result.Data is GenericResult<U> nestedResult)
        //    {
        //        AddMessagesRecursively(nestedResult);
        //    }
        //    else if (result.Data is GenericResult<object> nestedGenericResult)
        //    {
        //        AddMessagesRecursively(nestedGenericResult);
        //    }
        //    else
        //    {
        //        // Scansiona le proprietà dell'oggetto data che potrebbero essere di tipo GenericResult
        //        var properties = result.Data?.GetType().GetProperties();
        //        if (properties != null)
        //        {
        //            foreach (var property in properties)
        //            {
        //                var value = property.GetValue(result.Data);
        //                if (value is GenericResult<object> nestedPropertyResult)
        //                {
        //                    AddMessagesRecursively(nestedPropertyResult);
        //                }
        //            }
        //        }
        //    }
        //}

        //public void AddMessagesRecursively<U>(GenericResult<U> result)
        //{
        //    // Somma i messaggi dell'oggetto passato e poi li elimina
        //    Messages.AddRange(result.Messages);
        //    //result.Messages.Clear();

        //    // Se il tipo U è un GenericResult, chiama ricorsivamente il metodo
        //    if (result.Data is GenericResult<U> nestedResult)
        //    {
        //        AddMessagesRecursively(nestedResult);
        //    }
        //    else if (result.Data is GenericResult<object> nestedGenericResult)
        //    {
        //        AddMessagesRecursively(nestedGenericResult);
        //    }
        //    else
        //    {
        //        // Scansiona le proprietà dell'oggetto data che potrebbero essere di tipo GenericResult
        //        var properties = result.Data?.GetType().GetProperties();
        //        if (properties != null)
        //        {
        //            foreach (var property in properties)
        //            {
        //                var value = property.GetValue(result.Data);
        //                if (value is GenericResult<object> nestedPropertyResult)
        //                {
        //                    AddMessagesRecursively(nestedPropertyResult);
        //                }
        //            }
        //        }
        //    }
        //}

        //public void AddMessagesRecursively<U>(GenericResult<U> result)
        //{
        //    // Somma i messaggi dell'oggetto passato e poi li elimina
        //    Messages.AddRange(result.Messages);
        //    result.Messages.Clear();

        //    // Processa i dati contenuti
        //    ProcessData(result.Data);
        //}

        //private void ProcessData(object? data)
        //{
        //    if (data == null) return;

        //    if (data is GenericResult<object> nestedGenericResult)
        //    {
        //        Messages.AddRange(nestedGenericResult.Messages);
        //        nestedGenericResult.Messages.Clear();
        //        ProcessData(nestedGenericResult.Data);
        //    }

        //    // Scansiona le proprietà dell'oggetto data che potrebbero essere di tipo GenericResult
        //    var properties = data.GetType().GetProperties();
        //    foreach (var property in properties)
        //    {
        //        var value = property.GetValue(data);
        //        if (value is GenericResult<object> nestedPropertyResult)
        //        {
        //            Messages.AddRange(nestedPropertyResult.Messages);
        //            nestedPropertyResult.Messages.Clear();
        //            ProcessData(nestedPropertyResult.Data);
        //        }
        //    }
        //}

        //public void MergeAllMessagesRecursively()
        //{
        //    // Lista di messaggi combinati
        //    var combinedMessages = new List<MessageResult>(Messages);

        //    // Cerca messaggi ricorsivamente nei figli
        //    CollectMessagesRecursively(Data, combinedMessages);

        //    // Aggiorna la lista Messages dell'istanza principale
        //    Messages = combinedMessages;
        //}

        // Metodo di supporto per la ricerca ricorsiva
        //private void CollectMessagesRecursively(object? data, List<MessageResult> combinedMessages)
        //{
        //    if (data == null) return;

        //    // Se il dato è un GenericResult, aggiungi i suoi messaggi
        //    if (data is GenericResult<object> genericData)
        //    {
        //        combinedMessages.AddRange(genericData.Messages);
        //        CollectMessagesRecursively(genericData.Data, combinedMessages);
        //    }

        //    // Se il dato è una lista di GenericResult, esegui la ricerca su ogni elemento
        //    if (data is IEnumerable<GenericResult<object>> genericList)
        //    {
        //        foreach (var item in genericList)
        //        {
        //            CollectMessagesRecursively(item, combinedMessages);
        //        }
        //    }

        //    // Se il dato è una lista generica, esplora i suoi elementi
        //    if (data is IEnumerable<object> genericEnumerable)
        //    {
        //        foreach (var item in genericEnumerable)
        //        {
        //            CollectMessagesRecursively(item, combinedMessages);
        //        }
        //    }
        //    // Cerca ricorsivamente in tutte le proprietà pubbliche di un oggetto
        //    var type = data.GetType();
        //    foreach (var property in type.GetProperties())
        //    {
        //        if (property.CanRead) // Assicurati che la proprietà sia leggibile
        //        {
        //            var value = property.GetValue(data);
        //            CollectMessagesRecursively(value, combinedMessages);
        //        }
        //    }
        //}
    }


 

}
