using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tatakosan.Models
{
    public class RequestDeleteData
    {
        public string Table { get; set; }

        public string Group { get; set; }

        public string Id { get; set; }
        public string Sign { get; set; }
    }
}
