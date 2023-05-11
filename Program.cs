using Bc2BlazeSvr.Core;
using System.Runtime.Versioning;
using static Bc2BlazeSvr.Bc2.Bc2Pub;
using Task = System.Threading.Tasks.Task;
using static Bc2BlazeSvr.Myfunc;

namespace Bc2BlazeSvr
{
    internal class Program
    {
        [SupportedOSPlatform("windows")]
        static void Main(string[] args)
        {
            Console.SetBufferSize(8192, 8192);

            Task.Run(() => {
                WaitingForTheEnd();
            });

            LoadBc2Cfg();
          
            if (!ExecutionConn())
            {
                Console.WriteLine("警告:数据库连接失败."); Console.ReadKey(); return;
            }
           
            Net.StartSvr(18321, true);

            Net.StartSvr(18326, false);

            Net.StartSvr(18390, true );

            Net.StartSvr(18395, false );

            Thread.Sleep(-1);
        }

        /// <summary>载入配置</summary>
        static void LoadBc2Cfg()
        {
            // 配置文件
            CfgPath = $@"{CurrentPath}\Bc2Cfg.ini";
            // 服务器IP
            SvrIp = ReadINI("Config", "SvrIp", "127.0.0.1", CfgPath);
            // 解锁武器
            UnlockRank = Convert.ToBoolean(ReadINI("Config", "UnlockRank", "true", CfgPath));
            // 允许注册
            AllowRegister = Convert.ToBoolean(ReadINI("Config", "AllowRegister", "true", CfgPath));
            // PunkBuster版本
            PunkBusterVersion = ReadINI("Config", "PunkBusterVersion", "v1.905 | A1382 C2.305", CfgPath);
            // ROMEPC版本
            BVersion = ReadINI("Config", "BVersion", "ROMEPC784592", CfgPath);
            //Bc2WebSvr
            WebSvr = ReadINI("Config", "WebSvr", string.Empty, CfgPath);
            //Bc2Web端口
            WebPort = Convert.ToInt32(ReadINI("Config", "WebPort", "18392", CfgPath));
            // MySql服务器地址
            string DbServer = ReadINI("Config", "DbServer", string.Empty, CfgPath);
            // MySql端口
            string DbPort = ReadINI("Config", "DbPort", string.Empty, CfgPath);
            // MySql用户名
            string DbUser = ReadINI("Config", "DbUser", string.Empty, CfgPath); ;
            // MySql密码
            string DbPassword = ReadINI("Config", "DbPassword", string.Empty, CfgPath);
            // MySql库名
            string DbName = ReadINI("Config", "DbName", string.Empty, CfgPath);
            // MySql连接字符串
            ConnStr = $"Server={DbServer};Port={DbPort};UserId={DbUser};Password={DbPassword};Database={DbName};Charset=utf8mb4;Pooling=true";
        }

        static void WaitingForTheEnd()
        {
            Console.Write($"----------------------------------------\r\n{DateTime.Now} ↓ Enter 'q' Exit ↓\r\n----------------------------------------\r\n");

            string q;

            while (!(q = Console.ReadLine()!.Trim().ToLower()).Equals("q"))
            {
                Console.Clear(); Console.Write($"---------------------------------------\r\n{DateTime.Now} ↓ Enter 'q' Exit ↓\r\n---------------------------------------\r\n\r\n");

                switch (q)
                {
                    case "1":
                        Console.WriteLine($"---------------当前在线服务器:{R34SvrInfo.Count}---当前在线人数:{R11CliInfo.Count}------------");
                        break;

                    case "2":

                        int SvrCount = 0;

                        if (R34SvrInfo.IsEmpty)
                        {
                            Console.WriteLine("R34SvrInfo无数据...");

                        }
                        else
                        {
                            foreach (var v in R34SvrInfo)
                            {
                                Console.WriteLine($"---------------------------------------服务器[{++SvrCount}]=>PID={v.Value.RequestPId}");

                                foreach (var kv in v.Value.Cgde)
                                {
                                    Console.WriteLine($"{kv.Key}={kv.Value}");
                                }

                                Console.WriteLine("---------------------------------------");

                                foreach (var kv in v.Value.Ugde)
                                {
                                    Console.WriteLine($"{kv.Key}={kv.Value}");
                                }
                            }
                        }
                        break;

                    case "3":
                        if (R11CliInfo.IsEmpty)
                        {
                            Console.WriteLine("R11CliInfo无数据...");
                        }
                        else
                        {
                            foreach (var v in R11CliInfo)
                            {
                                Console.WriteLine("---------------------------");
                                Console.WriteLine($"[{v.Key} = {v.Value.OIDA_18390}] => {v.Value.PId} => {v.Value.UName} = {v.Value.PName} => 最近一次心跳:{v.Value.LastHeartbeatTime}");
                            }
                        }
                        break;

                    default: break;
                }
            }
            Environment.Exit(0);
        }
    }
}