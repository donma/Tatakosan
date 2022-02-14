
using No2verse.AzureTable.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tatakosan.Models
{
    public class TatakosanData : DTableEntity
    {
        public string Data { get; set; }
        public string Ext1 { get; set; }
        public string Ext2 { get; set; }
        public string Ext3 { get; set; }

        public string Group { get; set; }
        public DateTime LastUpdate { get; set; }

        
    }

   
}
