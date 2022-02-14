using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Timers;
using No2verse.AzureTable.Base;

namespace Tatakosan
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }



        public IConfiguration Configuration { get; }



        //-- Setting --//

        /// <summary>
        /// Azure Blob Table Connection String.
        /// </summary>
        public static readonly string _BlobConnectionString = "";


        /// <summary>
        /// /api/Op/ClearPool?token=token  的token
        /// </summary>
        public readonly static string ClearToken = "token";


        public readonly static string HashToken = "TATAKOSAN";

        /// <summary>
        /// 五分鐘請檢查一下快取有沒有過期
        /// </summary>
        private readonly static int RecycleMinutes = 5;

        /// <summary>
        ///  如果 60 秒，都沒有人請求，在進行回收
        /// </summary>
        private readonly static int HowManySecondsNoDataRequest = 60;

        /// <summary>
        /// 如果多少分鐘沒有人存取就回收
        /// default : 4hr * 60=2400;
        /// </summary>
        private readonly static int KeepMinutesInMemory = 2400;

        //-- Setting End --//



        public static Timer _TimerChecker { get; set; }

        public static bool IsRunningChecker { get; set; }

        public static DateTime _RecyleStamp { get; set; }

     

        public static ConcurrentDictionary<string, Tatakosan.Models.TatakosanData> _Pool;

        public static AzureTableRole _AzRole = new No2verse.AzureTable.Base.AzureTableRole("TATAKOSAN", new No2verse.AzureTable.AzureStorageSettings
        {
            ConnectionString = _BlobConnectionString

        });


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

            services.AddRazorPages();


            services.AddSwaggerDocument(settings =>
            {
                settings.Title = "TATAKOSAN Dev API";
            });
        }





        /// <summary>
        /// 檢查清除記憶體中的快取
        /// </summary>
        public static void RestartTimerChecker()
        {

            _TimerChecker = new Timer();
            _TimerChecker.Elapsed += (sender, args) =>
            {
                if ((DateTime.Now - _RecyleStamp).TotalSeconds > HowManySecondsNoDataRequest)
                {
                    if (IsRunningChecker) return;

                    IsRunningChecker = true;



                    CheckDataToExpire();

                    IsRunningChecker = false;
                }
            };

            _TimerChecker.Interval = 1000 * 60 * RecycleMinutes;

            _TimerChecker.Start();


        }

        public static void ResetRecycleTime()
        {
            _RecyleStamp = DateTime.Now;
        }

        public static void CheckDataToExpire()
        {


            var ids = _Pool.Select(x => x.Key).ToArray();

            if (ids != null)
            {

                Parallel.ForEach(ids, id =>
                {
                    try
                    {
                        var c = _Pool[id];
                        if (c != null)
                        {

                            if (c.LastUpdate.AddSeconds(KeepMinutesInMemory) <= DateTime.Now)
                            {
                                _Pool.TryRemove(id, out _);
                            }
                        }
                    }
                    catch
                    {

                    }
                });

            }


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
           
            _Pool = new ConcurrentDictionary<string, Models.TatakosanData>();

            RestartTimerChecker();

            var op = new No2verse.AzureTable.Collections.Operator(Startup._AzRole, "maindata", true);


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}
