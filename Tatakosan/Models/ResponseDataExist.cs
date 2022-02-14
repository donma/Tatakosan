using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tatakosan.Models
{
    public class ResponseDataExist
    {
        public bool IsInCache { get; set; }

        public bool IsInStorage { get; set; }
    }
}
