using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Service.MyTableService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.ChatAI.Models
{
    

    public class ChatAIInModel
    {
        public string Request {  get; set; } = string.Empty;
    }

    public class ChatAIOutModel : ModelResult
    {

        public string Responce { get; set; } = string.Empty;

        public ChatAIOutModel()
        {
            
        }
    }

}
