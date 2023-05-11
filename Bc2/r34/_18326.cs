using Bc2BlazeSvr.Core;
using System.Text;
using static Bc2BlazeSvr.Bc2.Bc2Pub;
using static Bc2BlazeSvr.Myfunc;

namespace Bc2BlazeSvr.Bc2.r34
{
    internal static class _18326
    {
        public static void Handle(Packet p)
        {
            switch (p.CMD)
            {
                case "CONN": CONN(p); break;

                case "USER": USER(p); break;

                case "CGAM": CGAM(p); break;

                case "UBRA": UBRA(p); break;

                case "UGAM": UGAM(p); break;

                case "UGDE": UGDE(p); break;

                case "EGRS": EGRS(p); break;

                //case "EGRQ": EGRQ(p); break;

                case "ECNL": ECNL(p); break;

                case "PLVT": PLVT(p); break;

                case "PENT": PENT(p); break;

                case "&lgr": lgr(p); break;

                case "PING": PING(p); break;

                case "UQUE": UQUE(p); break;

                case "DQEG": DQEG(p); break;

                case "QLVT": QLVT(p); break;

                default: ShowUnknownPacket(p); break;
            }
        }
        private static void lgr(Packet p)
        { 
            return; 
        }

        private static void QLVT(Packet p)
        {
            p.Add("TID", p.TID);
            p.Send("0x40000000");
        }

        private static void UQUE(Packet p)
        {
            /*
             ->O UQUE 0x40000000 {LID=257, GID=147326, QUEUE=20, TID=314} 
             <-O UQUE {TID=314} 
             */
            p.Add("TID", p.TID);
            p.Send("0x40000000");
        }

        private static void DQEG(Packet p)
        {
            /*
             ->N DQEG 0x40000000 {LID=257, GID=147326, PID=20, TID=315} 
             <-N DQEG {TID=315} 
             */
            p.Add("TID", p.TID);
            p.Send("0x40000000");
        }

        private static void PING(Packet p)
        {
            return;
        }

        private static void PLVT(Packet p)
        {
            /*
             ->O PLVT 0x40000000 {LID=257, GID=147326, PID=10, TID=311} 	
             <-A KICK {PID=10, LID=257, GID=147326} 
             <-O PLVT {TID=311} 
             */
            string PID = p.GetVal("PID"), GID = p.GetVal("GID");
            p.Add("PID", PID);
            p.Add("LID", LID);
            p.Add("GID", GID);
            p.Send("0x40000000", "KICK");

            p.Add("TID", p.TID);
            p.Send("0x40000000"); 
        }

        private static void PENT(Packet p)
        {
            string GID = p.GetVal("PID");

            _r34? s;

            if ((s = GetServerInfo(GID)) != null)
            {
                s.队列减少();
            }

            /*
             ->N PENT 0x40000000 {LID=257, GID=97130, PID=2, TID=21}  
             <-N PENT {TID=21, PID=2} 
             */
            p.Add("TID", p.TID);
            p.Add("PID", p.GetVal("PID"));
            p.Send("0x40000000");
        }

        private static void EGRS(Packet p)
        {
            /*
             ->N EGRS 0x40000000 {LID=257, GID=65807, ALLOWED=1, PID=1, TID=6}
             <-N EGRS {TID=6}
             */
            p.Add("TID", p.TID);
            p.Send("0x40000000");
        }

        private static void ECNL(Packet p)
        {
            /*
             CGAM服务器上线认证ugid|secret验证不正确,发送拦截包.ECNL需响应0x6e726f6d包                
             ->O ECNL 0x40000000 {LID=0, GID=0, TID=4}
             <-O ECNL 0x6e726f6d {TID=4, LID=0, GID=0}
             */

            int gid = int.Parse(p.GetVal("GID"));

            if (gid == 0)
            {
                p.Add("TID", p.TID);
                p.Add("LID", 0);
                p.Add("GID", 0);
                p.Send("0x6e726f6d");
            }
            else
            {
                p.Add("TID", p.TID);
                p.Add("LID", LID);
                p.Add("GID", gid);
                p.Send("0x40000000");
            }
        }
        private static void CGAM(Packet p)
        {
            /*
             ->N CGAM 0x40000000 {LID=-1, RESERVE-HOST=0, NAME="#2 CNBFBC2 %3dWolves%3d", PORT=28000, HTTYPE=A, TYPE=G, QLEN=100, DISABLE-AUTO-DEQUEUE=1, HXFR=0, INT-PORT=28000, INT-IP=1.1.1.1, MAX-PLAYERS=32, B-maxObservers=0, B-numObservers=0[08:33:29 AM] , UGID=8872ba2a-13f2-4db4-8707-1f1b7c9d48f3, SECRET=j5Ty1+wldpioITYZPJexbVVBUquh4W3di/X8MLJtLmHhcU6l4Wsekde2sLf7q6x/CQ8Y74vM2+sND5yBkNlIQA%3d%3d, B-U-Hardcore=1, B-U-HasPassword=0, B-U-Punkbuster=1, B-version=ROMEPC784592, JOIN=O, RT=, TID=3}
             <-N CGAM {TID=3, MAX-PLAYERS=32, EKEY=V+K6ZofRNO8qGR+70CyxIQ%3d%3d, UGID=8872ba2a-13f2-4db4-8707-1f1b7c9d48f3, JOIN=O, LID=257, SECRET=j5Ty1+wldpioITYZPJexbVVBUquh4W3diX8MLJtLmHhcU6l4Wsekde2sLf7q6x/CQ8Y74vM2+sND5yBkNlIQA%3d%3d, J=O, GID=146479}
             */
            int players = int.Parse(p.GetVal("MAX-PLAYERS"));

            string ugid = p.GetVal("UGID");
            string secret = p.GetVal("SECRET");
            int gid = Convert.ToInt32(p.OID);

            // 服务器上线认证ugid|secret验证不正确发送拦截包(按需启用)
            if (string.IsNullOrEmpty(ugid))
            {
                Console.WriteLine($"->[{p.OID}]->{p.SKT.RemoteIpAddress}:{p.SKT.RemotePort}->UGID|SECRET验证失败发送0x62736563包...");
                p.Add("TID", p.TID);
                p.Send("0x62736563");
                return;
            }

            var s = new _r34(gid, ugid, p.SKT);

            string key; foreach (var kv in p.RXD)
            {
                if (kv.Key.Equals("TID")) continue;

                key = kv.Key;

                if (s.Cgde.ContainsKey(key))
                {
                    s.Cgde[key] = kv.Value;
                }
                else
                {
                    Console.WriteLine($"CGAM缺少元素:{key}={kv.Value}");
                }
            }

             R34SvrInfo.TryAdd(p.OID, s);

            // var gids1 = DicToJSONStr(s.Cgde);

            // Console.WriteLine(gids1);

            // var gids2 = DicToJSONStr(s.Ugde);

            // Console.WriteLine(gids2);
       
            // _= ExecuteNonQuery($"INSERT INTO `svrs` (`GID`, `GDAT`, `GDET`) VALUES ({p.OID}, '@', '{gids2}')");
          
            p.Add("TID", p.TID);
            p.Add("MAX-PLAYERS", players);
            p.Add("EKEY", EKEY);
            p.Add("UGID", ugid);
            p.Add("JOIN", "O");
            p.Add("LID", LID);
            p.Add("SECRET", secret);
            p.Add("J", "O");
            p.Add("GID", gid);
            p.Send("0x40000000");
        }

        private static void UGAM(Packet p)
        {
            /*
             ->R UGAM {
             LID=257, 
             GID=146479, 
             NAME="#2 CNBFBC2 %3dWolves%3d", 
             B-U-EA=0, 
             B-U-Provider=754C642ED7E1AFE1092E84C5B7C046922BF1931B82A18BA1, 
             B-U-PunkBusterVersion="v1.905 | A1382 C2.305", 
             B-U-QueueLength=0, 
             B-U-Softcore=1, 
             B-U-Time="T:0.02 S: 0.01 L: 0.00 F: 2 B: 0", 
             B-U-elo=1000, 
             B-U-gameMod=BC2, 
             B-U-gamemode=CONQUEST, 
             B-U-hash=55AB8364-C13F-13B1-FC10-1C03747A62B4, 
             B-U-level=Levels/MP_012, 
             B-U-public=1, 
             B-U-region=AC, 
             B-U-sguid=89415004, 
             JOIN=O, 
             TID=4
             }*/

            var s = GetServerInfo(p.OID);
            if (s != null)
            {
                string key; foreach (var kv in p.RXD)
                {
                    if (kv.Key.Equals("TID")) continue;
                    key = kv.Key;
                    if (s.Cgde.ContainsKey(key))
                    {
                        s.Cgde[key] = kv.Value;
                    }
                    else
                    {
                        Console.WriteLine($"UGAM缺少元素:{key}={kv.Value}");
                    }
                }
            }
            else
            {
                Console.WriteLine("R34SvrInfo中未能找到服务器信息..."); ShowPacket(p); p.SKT.Close(0); return;
            }
        }

        private static void UBRA(Packet p)
        {
            p.SKT.SendBytes(WritePacket("UBRA", 0, Encoding.ASCII.GetBytes($"TID={p.TID}\n\0")));

            if (p.GetVal("START").Equals("0"))
            {
                p.Send("PING");
            }
        }


        private static void UGDE(Packet p)
        {
            /*
             ->R UGDE {
             TID=4,
             GID=146479, 
             LID=257,                                
             D-BannerUrl0=https://wx3.sinaimg.cn/mw690/bca4c2b1ly1g3jlvssf5mj20e801sdgr.j, 
             D-BannerUrl1=pg, 
             D-KillCam=1, 
             D-Minimap=1, 
             D-Crosshair=1, 
             D-AutoBalance=1, 
             D-FriendlyFire=0.0000, 
             D-ThreeDSpotting=1,            
             D-MinimapSpotting=1, 
             D-ServerDescriptionCount=1, 
             D-ServerDescription0="BC2",             
             D-ThirdPersonVehicleCameras=1,           
             D-pdat00=|0|0|0|0, 
             D-pdat01=|0|0|0|0, 
             D-pdat02=|0|0|0|0, 
             D-pdat03=|0|0|0|0, 
             D-pdat04=|0|0|0|0, 
             D-pdat05=|0|0|0|0, 
             D-pdat06=|0|0|0|0, 
             D-pdat07=|0|0|0|0, 
             D-pdat08=|0|0|0|0, 
             D-pdat09=|0|0|0|0, 
             D-pdat10=|0|0|0|0, 
             D-pdat11=|0|0|0|0, 
             D-pdat12=|0|0|0|0, 
             D-pdat13=|0|0|0|0, 
             D-pdat14=|0|0|0|0, 
             D-pdat15=|0|0|0|0, 
             D-pdat16=|0|0|0|0, 
             D-pdat17=|0|0|0|0, 
             D-pdat18=|0|0|0|0, 
             D-pdat19=|0|0|0|0, 
             D-pdat20=|0|0|0|0, 
             D-pdat21=|0|0|0|0, 
             D-pdat22=|0|0|0|0, 
             D-pdat23=|0|0|0|0, 
             D-pdat24=|0|0|0|0, 
             D-pdat25=|0|0|0|0, 
             D-pdat26=|0|0|0|0, 
             D-pdat27=|0|0|0|0, 
             D-pdat28=|0|0|0|0, 
             D-pdat29=|0|0|0|0, 
             D-pdat30=|0|0|0|0, 
             D-pdat31=|0|0|0|0
             }*/

            var s = GetServerInfo(p.OID);
            if (s != null)
            {
                string key; int players = 0; foreach (var kv in p.RXD)
                {
                    if (kv.Key.Equals("TID")) { continue; }
                    
                    key = kv.Key;
                    
                    if (s.Ugde.ContainsKey(key))
                    {
                        if (kv.Key.StartsWith("D-pdat") && !kv.Value.Equals("|0|0|0|0"))
                        {
                            ++players;
                        }
                        s.Ugde[key] = kv.Value;
                    }
                    else
                    {
                        Console.WriteLine($"UGDE缺少元素:{key}={kv.Value}");
                    }
                }
                if (players > 0)
                {
                    s.Cgde["AP"] = players;
                }
                else
                {
                    s.Cgde["AP"] = 0;
                }
            }
            else
            {
                Console.WriteLine("R34SvrInfo中未能找到服务器信息..."); ShowPacket(p); p.SKT.Close(0); return;
            }
        }

        private static void USER(Packet p)
        {
            /*
             ->N USER 0x40000000 {CID=, MAC=$000000000000, SKU=PC, LKEY=W1-GXWZv-_4-ruduCki6HAAAKDw., NAME=, TID=2}
             <-N USER {NAME=bfbc2.server.p, TID=2}
             */
            p.Add("NAME", HN);
            p.Add("TID", p.TID);
            p.Send("0x40000000");
        }

        private static void CONN(Packet p)
        {
            /*
             ->N(发往验证端) CONN 0x40000000 {PROT=2, PROD=bfbc2-pc, VERS=2.0, PLAT=PC, LOCALE=en_US, SDKVERSION=5.1.2.0.0, TID=1}
             <-N(验证端回复) CONN {TIME=1680782193, TID=1, activityTimeoutSecs=240, PROT=2}             
             */
            p.Add("TID", p.TID);
            p.Add("TIME", GetTimeStamp10());
            p.Add("PROT", 2);
            p.Add("activityTimeoutSecs", 240);
            p.Send("0x40000000");
        }
    }
}
