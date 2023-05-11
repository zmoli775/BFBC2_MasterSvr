using Chilkat;
using System;
using System.Text;
using SocketChilkat = Chilkat.Socket;
using Task = System.Threading.Tasks.Task;
using TaskChilkat = Chilkat.Task;
using AbortCheck = Chilkat.Socket.AbortCheck;
using Bc2BlazeSvr.Bc2;
using Bc2BlazeSvr.Bc2.r34;
using Bc2BlazeSvr.Bc2.r11;
using static Bc2BlazeSvr.Bc2.Bc2Pub;
using System.Net.Sockets;

namespace Bc2BlazeSvr.Core
{
    internal class Net
    {
        private const int BufferSize = 8192;
        /// <summary>
        /// 启动服务器
        /// </summary>
        /// <param name="port">端口</param>
        /// <param name="ssl">启用Tls</param>
        /// <param name="ht">大于0启动心跳</param>
        internal static void StartSvr(int port, bool ssl)
        {
            SocketChilkat svr = new()
            {
                MaxReadIdleMs = 10 * 1000,
                MaxSendIdleMs = 10 * 1000,
                // 设置接收|发送Packet大小
                SendPacketSize = BufferSize,
                ReceivePacketSize = BufferSize
            };

            if (!svr.UnlockComponent("42wFAK.CB11127_DQEq7AMk5E0U"))
            {
                Console.WriteLine($"UnlockComponent解锁失败:\n{svr.LastErrorXml}"); return;
            }
            if (ssl)
            {
                Cert cert = new(); // 加载证书       

                if (!cert.LoadPfxFile(@"Keys\zmoli775.pfx", "zmoli775") || !svr.InitSslServer(cert))
                {
                    Console.WriteLine($"LoadPfxFile或InitSslServer失败."); return;
                }

            }
            if (!svr.BindAndListen(port, 10)) // 设置监听端口(排队数值50)
            {
                Console.WriteLine($"监听失败{svr.LocalIpAddress}:{port}"); return;
            }

         
            Task.Run(() =>
            {
                AcceptConnectionAsync(svr);
            });
        }

        private static void AcceptConnectionAsync(SocketChilkat svr)
        {
            Console.WriteLine($"监听成功->{svr.LocalIpAddress}:{svr.LocalPort}");

            TaskChilkat ac; SocketChilkat c;

            while (true)
            {
                ac = svr.AcceptNextConnectionAsync(15 * 1000);

                if (ac != null && ac.Run() && ac.Wait(0) && ac.Finished)
                {   
                    c = new SocketChilkat();

                    if (c.LoadTaskResult(ac))
                    {
                        Task.Run(() =>
                        {
                            c.SendPacketSize = BufferSize;
                            c.ReceivePacketSize = BufferSize;                         
                            var r = new Rec(c);
                            r.ReceiveData();
                        });
                    }
                }
            }
        }
    }
    public class Rec
    {
        public Rec(SocketChilkat p)
        {
            sc = p;
        }
        private SocketChilkat sc { get; set; }

        public void ReceiveData()
        {
            Online(sc);

            TaskChilkat rc; byte[] rd; int rl;

            while (sc.IsConnected)
            {
                rc = sc.ReceiveBytesAsync();

                if (rc != null && rc.Run() && rc.Wait(0) && rc.Finished)
                {
                    rd = rc.GetResultBytes(); rl = rd.Length;

                    if (rl <= 0)
                    {
                        break;
                    }
                    else if (rl >= 12)
                    {
                        var p = new Packet(sc);
                     
                        if (!p.UnPack(rd, rl)) // 解包(解包正确返回true)
                        {
                            Console.WriteLine($"---------------------------------------\n警告->解包失败:\n触发端口:{sc.LocalPort}\n包长度:{rl}\n包内容:{Encoding.UTF8.GetString(rd)}\n包HEXS:{Myfunc.Bytes2Hexstr(rd, false)}");
                            sc.Close(0);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"警告:有异常包进入...\n{Myfunc.Bytes2Hexstr(rd)}");
                    }
                }
            }
            if (sc.IsConnected) sc.Close(0);

            Offline(sc.ObjectId);
        }

        /// <summary>
        /// 客户端上线
        /// </summary>
        private static void Online(SocketChilkat c)
        {

            int port = c.LocalPort; string cli;

            if (Bc2Pub.r34p_Port == port || Bc2Pub.r34t_Port == port)
            {
                cli = $"R34端[{c.ObjectId}]";
            }
            else if (Bc2Pub.r11p_Port == port || Bc2Pub.r11t_Port == port)
            {
                cli = $"R11端[{c.ObjectId}]";
            }
            else
            {
                cli = $"未知端[{c.ObjectId}]";
            }

            Console.WriteLine($"{c.RemoteIpAddress}:{c.RemotePort}->{cli}连接->{c.LocalPort}");
        }
        /// <summary>
        /// 客户端下线
        /// </summary>
        public static void Offline(int c)
        {
            R11AndR34ClearInfo(c);
        }
    }
}