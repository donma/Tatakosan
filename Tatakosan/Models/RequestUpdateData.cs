using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tatakosan.Models
{

    public class RequestUpdateData
    {

        public string Id { get; set; }

        public string Data { get; set; }
        public string Ext1 { get; set; }
        public string Ext2 { get; set; }
        public string Ext3 { get; set; }

        public string Group { get; set; }

        public string Table { get; set; }
        public string Sign { get; set; }

    }
}
