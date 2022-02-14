using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tatakosan.Models
{
    public class ResponseBody
    {

        /// <summary>
        /// error,warning
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public string Code { get; set; }


        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string Message { get; set; }


        public string TimeStamp { get; set; }


    }

    public class ResponseBodyBasic<T> : ResponseBody
    {

        public T Content { get; set; }


        public string Sign
        {
            get; set;
        }

    }

}
