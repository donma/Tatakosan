using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tatakosan.Models;

namespace Tatakosan.webapi
{
    [Route("api/Op")]
    [ApiController]
    public class OpController : ControllerBase
    {


        [HttpGet]
        public async Task<string> Get()
        {

            return "HELLO, I am Tatakosan Opetator," + Startup._Pool.Count;

        }

        [HttpGet]
        [Route("ClearPool")]
        public async Task<string> GetClear(string token)
        {
            if (token != Startup.ClearToken)
            {
                return "Token is Error";
            }
            Startup._Pool = null;
            Startup._Pool = new System.Collections.Concurrent.ConcurrentDictionary<string, TatakosanData>();


            return "HELLO, I am Tatakosan Opetator Clear Pool Already," + Startup._Pool.Count;

        }

        [HttpPost]
        [Route("Update")]
        [ProducesResponseType(typeof(ResponseBodyBasic<string>), 200)]
        public async Task<IActionResult> CreateDataPublic([FromBody] RequestUpdateData src)
        {
            if (string.IsNullOrEmpty(src.Id) || string.IsNullOrEmpty(src.Data) || string.IsNullOrEmpty(src.Group))
            {
                var eRes = new ResponseBodyBasic<string>();
                eRes.Code = "400";
                eRes.Status = "ERROR";
                eRes.Message = "Group , Id or Data null";
                return Ok(eRes);
            }


            if (src.Sign != (src.Table + src.Group + src.Id + "," + Startup.HashToken).ToMD5())
            {
                var eRes = new ResponseBodyBasic<string>();
                eRes.Code = "403";
                eRes.Status = "ERROR";
                eRes.Message = "Not Auth";
                return Ok(eRes);
            }


            try
            {
                Task.Run(() => Startup.ResetRecycleTime());

                if (string.IsNullOrEmpty(src.Table))
                {
                    await Task.Run(() =>
                    {
                        CreateData("maindata", src.Group, src.Id, src.Data, src.Ext1, src.Ext2, src.Ext3);
                    });
                }
                else
                {
                    await Task.Run(() =>
                    {
                        CreateData(src.Table, src.Group, src.Id, src.Data, src.Ext1, src.Ext2, src.Ext3);
                    });
                }

            }
            catch (Exception ex)
            {
                var eRes = new ResponseBodyBasic<string>();
                eRes.Code = "500";
                eRes.Status = "ERROR";
                eRes.Message = ex.Message;
                return Ok(eRes);
            }
            var res = new ResponseBodyBasic<string>();
            res.Content = src.Id.ToUpper();
            res.Code = "200";
            res.Status = "SUCCESS";
            res.Sign = res.Content.ToMD5();

            return Ok(res);

        }


        [HttpPost]
        [Route("Delete")]
        [ProducesResponseType(typeof(ResponseBodyBasic<string>), 200)]
        public async Task<IActionResult> DeleteData([FromBody] RequestDeleteData src)
        {
            if (string.IsNullOrEmpty(src.Id) || string.IsNullOrEmpty(src.Table) || string.IsNullOrEmpty(src.Group))
            {
                var eRes = new ResponseBodyBasic<string>();
                eRes.Code = "400";
                eRes.Status = "ERROR";
                eRes.Message = "Group , Id or Data null";
                return Ok(eRes);
            }


            if (src.Sign != (src.Table + src.Group + src.Id + "," + Startup.HashToken).ToMD5())
            {
                var eRes = new ResponseBodyBasic<string>();
                eRes.Code = "403";
                eRes.Status = "ERROR";
                eRes.Message = "Not Auth";
                return Ok(eRes);
            }


            try
            {
                Task.Run(() => Startup.ResetRecycleTime());


                if (string.IsNullOrEmpty(src.Table))
                {
                    await Task.Run(() =>
                    {
                        DeleteData("maindata", src.Group, src.Id);
                    });
                }
                else
                {
                    await Task.Run(() =>
                    {
                        DeleteData(src.Table, src.Group, src.Id);
                    });
                }

            }
            catch (Exception ex)
            {
                var eRes = new ResponseBodyBasic<string>();
                eRes.Code = "500";
                eRes.Status = "ERROR";
                eRes.Message = ex.Message;
                return Ok(eRes);
            }
            var res = new ResponseBodyBasic<string>();
            res.Content = src.Id.ToUpper();
            res.Code = "200";
            res.Status = "SUCCESS";
            res.Sign = res.Content.ToMD5();

            return Ok(res);

        }



        [HttpPost]
        [Route("CreateTable")]
        [ProducesResponseType(typeof(ResponseBodyBasic<string>), 200)]
        public async Task<IActionResult> CreateTable([FromBody] RequestCreateTable src)
        {
            if (string.IsNullOrEmpty(src.Table))
            {
                var eRes = new ResponseBodyBasic<string>();
                eRes.Code = "400";
                eRes.Status = "ERROR";
                eRes.Message = "TableName null";
                return Ok(eRes);
            }

            if (src.Sign != (src.Table + "," + Startup.HashToken).ToMD5())
            {
                var eRes = new ResponseBodyBasic<string>();
                eRes.Code = "403";
                eRes.Status = "ERROR";
                eRes.Message = "Not Auth";
                return Ok(eRes);
            }


            try
            {
                await Task.Run(() =>
                {
                    CreateTable(src.Table.ToLower());
                });
            }
            catch (Exception ex)
            {
                var eRes = new ResponseBodyBasic<string>();
                eRes.Code = "500";
                eRes.Status = "ERROR";
                eRes.Message = ex.Message;
                return Ok(eRes);
            }
            var res = new ResponseBodyBasic<string>();
            res.Content = src.Table.ToLower();
            res.Code = "200";
            res.Status = "SUCCESS";
            res.Sign = res.Content.ToMD5();

            return Ok(res);

        }


        private void CreateTable(string tableName)
        {
            var op = new No2verse.AzureTable.Collections.Operator(Startup._AzRole, tableName.ToLower(), true);

        }

        private void CreateData(string tableName, string pk, string rk, string data, string ext1 = null, string ext2 = null, string ext3 = null)
        {

            var d = new TatakosanData
            {

                PartitionKey = pk,
                RowKey = rk,
                Data = data,
                Ext1 = ext1,
                Ext2 = ext2,
                Ext3 = ext3,
                LastUpdate = DateTime.Now
            };

            Startup._Pool.AddOrUpdate(tableName.ToLower() + "|" + pk + "|" + rk, d, (oldkey, oldvalue) => d);


            var op = new No2verse.AzureTable.Collections.Operator(Startup._AzRole, tableName.ToLower());

            op.Update(d);

        }


        private void DeleteData(string tableName, string pk, string rk)
        {

            var op = new No2verse.AzureTable.Collections.Operator(Startup._AzRole, tableName.ToLower());

            op.Delete(pk, rk);


            Startup._Pool.Remove(tableName.ToLower() + "|" + pk + "|" + rk, out _);


        }
    }
}
