using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Tatakosan.Models;

namespace Tatakosan
{
    public static class ExtUtil
    {

        public static string ToMD5(this string input, string salt = "no2don")
        {
            var x = new MD5CryptoServiceProvider();
            byte[] bs = Encoding.UTF8.GetBytes(input + salt);
            bs = x.ComputeHash(bs);
            var s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            string password = s.ToString();
            return password;
        }

        public static ResponseDataInfo ConvertToResponse(this TatakosanData data)
        {


            return new ResponseDataInfo
            {
                Data = data.Data,
                Ext1 = data.Ext1,
                Ext2 = data.Ext2,
                Ext3 = data.Ext3,
                Group = data.PartitionKey,
                Id = data.RowKey
            };
        }

        public static void ReUpdateToPool(this TatakosanData data,string tableName)
        {

            Startup._Pool.AddOrUpdate(tableName + "|" + data.PartitionKey + "|" + data.RowKey, data, (oldkey, oldvalue) => data);



        }
    }
}
