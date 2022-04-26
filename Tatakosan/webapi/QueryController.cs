using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Tatakosan.Models;

namespace Tatakosan.webapi
{
    [Route("api/Qu")]
    [ApiController]
    public class QueryController : ControllerBase
    {

        [HttpGet]
        public string Get()
        {


            return "HELLO, I am Tatakosan Query," + Startup._Pool.Count;

        }


        [HttpPost]
        [Route("IsTableExisted")]
        [ProducesResponseType(typeof(ResponseBodyBasic<bool>), 200)]
        public async Task<IActionResult> IsTableExisted([FromBody] RequestIsTableExist src)
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
                    var guid = "donma_system_use";
                    var op = new No2verse.AzureTable.Collections.Operator(Startup._AzRole, src.Table.ToLower(), false);

                    op.Update(new No2verse.AzureTable.Base.DTableEntity { PartitionKey = guid, RowKey = guid });

                    op.Delete(guid, guid);
                });

                var rr = new ResponseBodyBasic<bool>();
                rr.Content = true;
                rr.Code = "200";
                rr.Status = "SUCCESS";
                rr.Sign = JsonConvert.SerializeObject(rr.Content.ToString()).ToMD5();

                return Ok(rr);
            }
            catch (Exception ex)
            {
                var eRes = new ResponseBodyBasic<bool>();
                eRes.Content = false;
                eRes.Code = "200";
                eRes.Status = "SUCCESS";
                eRes.Message = ex.Message;
                eRes.Sign = JsonConvert.SerializeObject(eRes.Content.ToString()).ToMD5();
                return Ok(eRes);
            }



        }



        [HttpPost]
        [Route("GetFullData")]
        [ProducesResponseType(typeof(ResponseBodyBasic<ResponseDataInfo>), 200)]
        public async Task<IActionResult> GetFullData([FromBody] RequestQueryDataFull src)
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

            ResponseDataInfo data = null;
            try
            {
                Task.Run(() => Startup.ResetRecycleTime());

                await Task.Run(() =>
                {
                    data = GetDataFull(src.Table, src.Group, src.Id);

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

            var res = new ResponseBodyBasic<ResponseDataInfo>();
            res.Content = data;
            res.Code = "200";
            res.Status = "SUCCESS";
            res.Sign = JsonConvert.SerializeObject(res.Content).ToMD5();

            return Ok(res);

        }




        [HttpPost]
        [Route("IsDataExist")]
        [ProducesResponseType(typeof(ResponseBodyBasic<ResponseDataExist>), 200)]
        public async Task<IActionResult> IsDataExist([FromBody] RequestIsDataExist src)
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

            ResponseDataExist data = null;

            try
            {

                Task.Run(() => Startup.ResetRecycleTime());

                await Task.Run(() =>
                {
                    data = new ResponseDataExist();

                    data.IsInCache = IsDataInMem(src.Table, src.Group, src.Id);
                    data.IsInStorage = IsDataInAzureTable(src.Table, src.Group, src.Id);

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

            var res = new ResponseBodyBasic<ResponseDataExist>();
            res.Content = data;
            res.Code = "200";
            res.Status = "SUCCESS";
            res.Sign = JsonConvert.SerializeObject(res.Content).ToMD5();

            return Ok(res);

        }


        private string[] GetAllPKs(string table)
        {


            var q = new No2verse.AzureTable.Collections.Query<No2verse.AzureTable.Base.DTableEntity>(Startup._AzRole, table);
            var res = q.AllPartitionKeys();

            return res;
        }

        private bool IsDataInMem(string table, string pk, string rk)
        {
            return Startup._Pool.ContainsKey(table.ToLower() + "|" + pk + "|" + rk);
        }

        private bool IsDataInAzureTable(string table, string pk, string rk)
        {
            var qL = new No2verse.AzureTable.Collections.Query<TatakosanData>(Startup._AzRole, table.ToLower());
            return qL.IsDataExist(pk, rk);
        }
        private ResponseDataInfo GetDataFull(string table, string pk, string rk)
        {

            TatakosanData data = null;

            if (Startup._Pool.ContainsKey(table.ToLower() + "|" + pk + "|" + rk))
            {
                data = Startup._Pool[table.ToLower() + "|" + pk + "|" + rk];
            }


            if (data == null)
            {

                var qL = new No2verse.AzureTable.Collections.Query<TatakosanData>(Startup._AzRole, table.ToLower(), false);
                try
                {
                    data = qL.DataByPRKey(pk, rk);
                }
                catch
                {

                }
            }

            if (data != null)
            {
                Startup._Pool.AddOrUpdate(table.ToLower() + "|" + pk + "|" + rk, data, (oldkey, oldvalue) => data);

            }
            else
            {
                return null;
            }


            data.LastUpdate = DateTime.Now;
            data.ReUpdateToPool(table.ToLower());
            return data.ConvertToResponse();

        }
    }
}
