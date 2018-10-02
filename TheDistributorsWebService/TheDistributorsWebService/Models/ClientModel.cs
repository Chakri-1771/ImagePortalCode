using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheDistributorsWebService.Models
{
    public class ClientModel
    {
        public string ClientID { get; set; }
        public string ClientKey { get; set; }

        public ClientModel(string _ClientID, string _ClientKey)
        {
            ClientID = _ClientID;
            ClientKey = _ClientKey;
        }
    }
}