# Tatakosan

這是一個實驗計畫，主要就是能夠快速使用 Key , Value 存取資料，主要資料庫我是使用 Azure Storage Table ，因為他很便宜

除了單純寫入 Azure Storage Table 之外，我還有座記憶體快去，讓存取速度可以正常，這裡我是 base on .netcore 3.1

這邊簡單測試過大量寫入下的穩定性，如果有任何問題就在跟我說吧。


Dependency :

[Microsoft.Azure.Cosmos.Table](https://www.nuget.org/packages/Microsoft.Azure.Cosmos.Table)

[Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/)

[No2vers.AzureTable](https://github.com/donma/No2verse.AzureTable)


----
How To Start?
----

in Startup.cs

```C#

        /// <summary>
        /// Azure Blob Table Connection String.
        /// </summary>
        public static readonly string _BlobConnectionString = "DefaultEndpointsProtocol=https;AccountName=...";


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
       


```

Simple Swagger Document: 

https://tatakosandoc.azurewebsites.net/tversion2/


