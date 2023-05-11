using Bc2BlazeSvr.Core;
using static Bc2BlazeSvr.Bc2.Bc2Pub;
using static Bc2BlazeSvr.Myfunc;
using Bc2BlazeSvr.Bc2.r34;

namespace Bc2BlazeSvr.Bc2.r11
{
    internal static class _18395
    {
        public static void Handle(Packet p)
        {             
            switch (p.CMD)
            {
                case "CONN": CONN(p); break;

                case "USER": USER(p); break;

                case "GLST": GLST(p); break;

                case "LLST": LLST(p); break;

                case "GDAT": GDAT(p); break;

                case "EGRS": EGRS(p); break;

                case "EGAM": EGAM(p); break;

                case "ECNL": ECNL(p); break;

                case "PING": PING(p); break;

                default: ShowUnknownPacket(p); break;
            }
        }
        private static void PING(Packet p) { return; }

        private static void EGRS(Packet p)
        {
            p.Add("TID", p.TID);
            p.Send("0x40000000");
        }

        private static void EGAM(Packet p)
        {
            string
                TICKET = GetTimeStamp10(),
                IP = p.SKT.RemoteIpAddress,
                GID = p.GetVal("GID"),
                PORT = p.GetVal("PORT"),
                PTYPE = p.GetVal("PTYPE"),
                R_INT_IP = p.GetVal("R-INT-IP"),
                R_INT_PORT = p.GetVal("R-INT-PORT");

                          
            // 获取R34服务器信息
            var s = GetServerInfo(GID);

            // 根据18395端口的SocketChilkatID查找用户信息
            var c = GetUserInfo(null, p.OID);

            if (s != null && c != null)
            {
                int PID, AP, MP;

                PID = ++s.RequestPId; c.RequestPId = PID; AP = Convert.ToInt32(s.Cgde["AP"]); MP = Convert.ToInt32(s.Cgde["MAX-PLAYERS"]);

                if (AP + 1 > MP)
                {
                    /*
                     ->N EGAM 0x40000000 {PORT=10000, R-INT-PORT=10000, R-INT-IP=10.0.0.1, PTYPE=P, LID=257, GID=147326(my服务器), TID=121}
                     <-N EGAM 0x71756575 {TID=121, QPOS=1, QLEN=2, LID=257, GID=147326(my服务器)}
                     <-A QENT {R-INT-PORT=10000, R-INT-IP=10.0.0.1, NAME=Violent%3aKiTtEn, PID=15, UID=940351400 (我的PID), LID=257, GID=147326} 	
                     <-A QLEN {QPOS=0, QLEN=1, LID=257, GID=147326(my服务器)}       
                     */

                    s.队列增加(out int qposqlen);

                    p.Add("QPOS", qposqlen);
                    p.Add("QLEN", qposqlen + 1);

                    p.Add("TID", p.TID);
                    p.Add("LID", LID);
                    p.Add("GID", GID);
                    p.Send("0x71756575");               // <-N EGAM 0x71756575

                    Packet QENT = new(s.Sc);
                    QENT.Add("R-INT-PORT", R_INT_PORT);
                    QENT.Add("R-INT-IP", R_INT_IP);
                    QENT.Add("NAME", c.PName);
                    QENT.Add("PID", c.RequestPId);      // R34请求进服计数++RequestPId
                    QENT.Add("UID", c.PId);             // 士兵角色ID
                    QENT.Add("LID", LID);
                    QENT.Add("GID", GID);
                    QENT.Send("0x40000000", "QENT");    // <-A QLEN 
                }
                else
                {
                    /*
                     ->N EGAM 0x40000000 {PORT=10000, R-INT-PORT=10000, R-INT-IP=10.0.0.1, PTYPE=P, LID=257, GID=147326(my服务器), TID=73}
                     <-N EGAM {TID=73, LID=257, GID=147326(my服务器)}
                     <-A EGEG {PL=pc, TICKET=-1115699262, PID=9, P=27001, HUID=224444376, INT-PORT=27001, EKEY=EMf5cdQzZ3hCLPP7BEeqHg%3d%3d, INT-IP=192.168.4.227, UGID=e132c9db-f2a2-4bf8-a95e-f496b790a211, I=119.6.97.156, LID=257, GID=147326(my服务器)}
                     <-A EGRQ {R-INT-PORT=10000, R-INT-IP=10.0.0.1, PORT=10000, NAME=Violent%3aKiTtEn, PTYPE=P, TICKET=-1115699262, PID=9, UID=940351400 (我的PID), IP=171.88.7.155, LID=257, GID=147326}	                                         
                     */
                    p.Add("TID", p.TID);
                    p.Add("LID", LID);
                    p.Add("GID", GID);
                    p.Send("0x40000000");

                    p.Add("PL", "pc");
                    p.Add("TICKET", TICKET);
                    p.Add("PID", PID);
                    p.Add("P", s.Cgde["PORT"]);
                    p.Add("HUID", HU);
                    p.Add("INT-PORT", s.Cgde["INT-PORT"]);
                    p.Add("EKEY", EKEY);
                    p.Add("INT-IP", s.Cgde["INT-IP"]);
                    p.Add("UGID", s.Cgde["UGID"]);
                    p.Add("I", s.Cgde["IP"]);
                    p.Add("LID", LID);
                    p.Add("GID", GID);
                    p.Send("0x40000000", "EGEG");

                    Packet EGRQ = new(s.Sc);
                    EGRQ.Add("R-INT-PORT", R_INT_PORT);
                    EGRQ.Add("R-INT-IP", R_INT_IP);
                    EGRQ.Add("PORT", PORT);
                    EGRQ.Add("NAME", c.PName);
                    EGRQ.Add("PTYPE", PTYPE);
                    EGRQ.Add("TICKET", TICKET);
                    EGRQ.Add("PID", c.RequestPId);    // R34请求进服计数++RequestPId
                    EGRQ.Add("UID", c.PId);           // 士兵角色ID
                    EGRQ.Add("IP", IP);
                    EGRQ.Add("LID", LID);
                    EGRQ.Add("GID", GID);
                    EGRQ.Send("0x40000000", "EGRQ");
                }            
            }
        }

        private static void GDAT(Packet p)
        {
            if (p.HasKey("GID") && p.HasKey("LID") && p.HasKey("TID"))
            {
                /*                 
                 ->N GDAT 0x40000000 {LID=257, GID=147326(my服务器), TID=128}
                 <-A QLEN {QPOS=0, QLEN=1, LID=257, GID=147326(my服务器)}
                 <-N GDAT {JP=0, HN=bfbc2.server.p, B-U-level=Levels/MP_002, B-U-sguid=497023417, N="#3 BC2 HaoYue %3dWolves%3d", B-U-Provider=3C557757FDB7058D403797BC9D96ECFE62E88062A8F721CD, I=119.6.97.156, J=O, HU=224444376, B-U-Time="T%3a200.00 S%3a 9.78 L%3a 0.00 F%3a 300 B%3a 0", V=2.0, B-U-gamemode=SQRUSH, P=27001, B-U-Hardcore=0, B-U-hash=B739D947-9368-CBDD-4B35-29F10C40E10A, B-U-Softcore=1, B-numObservers=0, B-U-gameMod=BC2, TYPE=G, LID=257, B-version=ROMEPC784592, 
                 B-U-QueueLength=0, B-U-region=Asia, B-U-HasPassword=0, QP=1, MP=8, GID=147326(my服务器), B-U-public=1, B-U-EA=0, B-U-Punkbuster=1, PL=PC, B-U-elo=1642, B-maxObservers=0, PW=0, TID=128, AP=8}
                 */
                string gid = p.GetVal("GID");

                _r34? s; if ((s = GetServerInfo(gid)) != null)
                {
                    if (s.QPOSQLEN > 0)
                    {
                        p.Add("QPOS", s.Cgde["B-U-QueueLength"]);      // 排在第几位
                        p.Add("QLEN", s.QPOSQLEN);                     // 排队长度

                        p.Add("LID", LID);
                        p.Add("GID", gid);
                        p.Send("0x40000000", "QLEN");
                    }

                    GDAT(p, s.Cgde, 0);

                    GDET(p, s.Ugde, out List<string> pdats);

                    if (pdats.Count > 0)
                    {
                        PDAT(p, pdats, gid);
                    }
                }
            }
            else
            {
                p.Add("TID", p.TID);
                p.Send("0x40000000", "GDAT");
                p.Send("PING");
            }
        }

        private static void ECNL(Packet p)
        {
            /*
              ->O ECNL 0x40000000 {LID=257, GID=147326(my服务器), TID=123}
              <-O ECNL {TID=123, LID=257, GID=147326(my服务器)}           
             */
            string GID = p.GetVal("GID");

            try
            {
                _r34? s = null; if ((s = GetServerInfo(GID)) != null)
                {
                    if (s.QPOSQLEN > 0)
                    {
                        // R11客户端主动取消排队进服(查找用户信息为了找到RequestPId(进服请求Id))
                        _r11? c; if ((c = GetUserInfo(null, p.OID)) != null)
                        {
                            s.队列减少();
                            /*
                             <-A QLVT {PID=15, LID=257, GID=147326}
                             */
                            Packet QLVT = new(s.Sc);
                            QLVT.Add("PID", c.RequestPId); // R34请求进服计数++RequestPId
                            QLVT.Add("LID", LID);
                            QLVT.Add("GID", GID);
                            QLVT.Send("0x40000000", "QLVT");
                        }
                    }
                }
            }
            finally
            {
                p.Add("TID", p.TID);
                p.Add("LID", LID);
                p.Add("GID", GID);
                p.Send("0x40000000"); //p.Send("0x6d697363");
            }
        }

        private static void GLST(Packet p)
        {
            int count = int.Parse(p.GetVal("COUNT"));
            int favonly = int.Parse(p.GetVal("FILTER-FAV-ONLY"));
            int serverCount = R34SvrInfo.Count;
            string favgameuid = p.GetVal("FAV-GAME-UID");
            bool HaveGID = p.HasKey("GID");

            if (HaveGID || favgameuid.Equals("frostbite")) // 搜索扫尾
            {
                /*
                 ->N GLST 0x40000000 {LID=257, TYPE=G, FILTER-FAV-ONLY=0, FILTER-NOT-FULL=0, FILTER-NOT-PRIVATE=0, FILTER-NOT-CLOSED=0, FILTER-MIN-SIZE=0, FILTER-ATTR-U-gameMod=BC2, FAV-PLAYER=, FAV-GAME=, GID=146824, COUNT=2000, FAV-PLAYER-UID=, FAV-GAME-UID=, TID=5}
                 <-N GLST {TID=5, LOBBY-NUM-GAMES=63, NUM-GAMES=0, LID=257, LOBBY-MAX-GAMES=10000}
                 */
                p.Add("TID", p.TID);
                p.Add("LOBBY-NUM-GAMES", serverCount);
                p.Add("NUM-GAMES", 0);
                p.Add("LID", LID);
                p.Add("LOBBY-MAX-GAMES", 10000);
                p.Send("0x40000000");
            }
            else if (0 == favonly && count == 2000 && string.IsNullOrEmpty(favgameuid)) // 大厅搜索
            {
                p.Add("TID", p.TID);
                p.Add("LID", LID);
                p.Add("LOBBY-MAX-GAMES", 10000);


                if (serverCount <= 0) // 大厅搜索-无服务器
                {
                    p.Add("LOBBY-NUM-GAMES", serverCount);
                    p.Add("NUM-GAMES", 0);
                    p.Send("0x40000000");
                }
                else  // 大厅搜索-存在服务器
                {
                    p.Add("LOBBY-NUM-GAMES", serverCount);
                    p.Add("NUM-GAMES", serverCount);
                    p.Send("0x40000000");

                    // 有服务器则发送服务器GDAT

                    foreach (var kv in R34SvrInfo)
                    {
                        GDAT(p, kv.Value.Cgde, 0xAA);
                    }
                }
            }
            else if (1 == favonly && count == -1) // 喜好|历史搜索
            {
                p.Add("TID", p.TID);
                p.Add("LID", LID);
                p.Add("LOBBY-MAX-GAMES", 10000);

                if (string.IsNullOrEmpty(favgameuid))
                {
                    p.Add("LOBBY-NUM-GAMES", serverCount);
                    p.Add("NUM-GAMES", 0);
                    p.Send("0x40000000");
                }
                else
                {
                    string[] guids = favgameuid.Split(';', StringSplitOptions.RemoveEmptyEntries);

                    int guidslen = guids.Length;

                    List<Dictionary<string, object>> gdats = new();

                    if (guidslen > 0)
                    {
                        for (int i = 0; i < guidslen; i++)
                        {
                            foreach (var kv in R34SvrInfo)
                            {
                                if ((string)kv.Value.Cgde["UGID"] == guids[i])
                                {
                                    gdats.Add(kv.Value.Cgde);
                                    break;
                                }
                            }
                        }
                    }

                    if (guidslen <= 0 || gdats.Count <= 0)
                    {
                        p.Add("LOBBY-NUM-GAMES", 0);
                        p.Add("NUM-GAMES", 0);
                        p.Send("0x40000000");
                    }
                    else
                    {
                        p.Add("LOBBY-NUM-GAMES", serverCount);
                        p.Add("NUM-GAMES", gdats.Count);
                        p.Send("0x40000000");
                        foreach (var kv in gdats)
                        {
                            GDAT(p, kv, 0xAB);
                        }
                    }
                }
            }
            else
            {
                ShowUnknownPacket(p);
            }
        }

        /// <summary>
        /// 服务器列表包
        /// flag=0xAA(大厅搜索)
        /// flag=0xAB(喜好|历史搜索)
        /// </summary>  
        private static void GDAT(Packet p, Dictionary<string, object> s, int flag)
        {
            p.Add("JP", 0);
            p.Add("HN", HN);
            p.Add("B-U-level", s["B-U-level"]);
            p.Add("B-U-sguid", s["B-U-sguid"]);
            p.Add("N", s["NAME"]);
            p.Add("B-U-Provider", s["B-U-Provider"]);
            p.Add("I", s["IP"]);
            p.Add("J", s["JOIN"]);
            p.Add("HU", HU);

            p.Add("B-U-Time", s["B-U-Time"]);
            p.Add("V", ProtocolVersion);
            p.Add("B-U-gamemode", s["B-U-gamemode"]);
            p.Add("P", s["PORT"]);
            p.Add("B-U-Hardcore", s["B-U-Hardcore"]);
            p.Add("B-U-hash", s["B-U-hash"]);
            p.Add("B-U-Softcore", s["B-U-Softcore"]);
            p.Add("B-numObservers", s["B-numObservers"]);
            p.Add("B-U-gameMod", "BC2");
            p.Add("TYPE", "G");
            p.Add("LID", LID);
            p.Add("B-version", s["B-version"]);

            p.Add("B-U-QueueLength", s["B-U-QueueLength"]);
            p.Add("B-U-region", s["B-U-region"]);
            p.Add("B-U-HasPassword", s["B-U-HasPassword"]);
            p.Add("QP", s["QP"]);
            p.Add("MP", s["MAX-PLAYERS"]);
            p.Add("GID", s["GID"]);
            p.Add("B-U-public", s["B-U-public"]);
            p.Add("B-U-EA", s["B-U-EA"]);
            p.Add("B-U-Punkbuster", s["B-U-Punkbuster"]);
            p.Add("PL", ClientPlatform);
            p.Add("B-U-elo", s["B-U-elo"]);
            p.Add("B-maxObservers", s["B-maxObservers"]);
            p.Add("PW", 0);
            p.Add("TID", p.TID);
            p.Add("AP", s["AP"]);

            // 0xAA(大厅搜索)
            if (flag == 0xAA)
            {
                p.Add("F", 0);
                p.Add("NF", 0);
            }
            // 0xAB(喜好|历史搜索)
            else if (flag == 0xAB)
            {
                p.Add("F", 1);
                p.Add("NF", 0);
            }
            if ((string)s["B-U-Punkbuster"] == "1")
            {
                p.Add("B-U-PunkBusterVersion", s["B-U-PunkBusterVersion"]);
            }
            p.Send("0x40000000", "GDAT");
        }

        private static void PDAT(Packet p, List<string> pdats, string GID)
        {
            /*
             <-U PDAT {NAME=X555,             TID=74, PID=1,(进入R34服务器请求Id) UID=2006763559,             LID=257, GID=147326(R34服务器GID)}
             <-U PDAT {NAME=fesiver,          TID=74, PID=8,(进入R34服务器请求Id) UID=1871813928,             LID=257, GID=147326(R34服务器GID)}
             <-U PDAT {NAME=Violent%3aKiTtEn, TID=74, PID=9,(进入R34服务器请求Id) UID=940351400(士兵角色PID), LID=257, GID=147326(R34服务器GID)}
             */
            string[] split; string pname;

            foreach (string item in pdats)
            {
                split = item.Split('|', StringSplitOptions.RemoveEmptyEntries);

                if (split.Length > 0 && split.Length == 5)
                {
                    pname = split[0];

                    var user = GetUserInfo(null, 0, 0, pname);

                    if (user != null)
                    {
                        p.Add("NAME", pname);          // 士兵角色名
                        p.Add("PID", user.RequestPId); // 此处PID是进入R34服务器时的请求Id
                        p.Add("UID", user.PId);        // 此处UID非帐户Id真实传送的是士兵角色Id(EA很迷啊)
                        p.Add("TID", p.TID);
                        p.Add("LID", LID);
                        p.Add("GID", GID);
                        p.Send("0x40000000", "PDAT");    
                    }
                }
            }
        }

        private static void GDET(Packet p, Dictionary<string, object> s, out List<string> pdats)
        {
            string bannerUrl = (string)s["D-BannerUrl"];
            p.Add("TID", p.TID);
            p.Add("GID", s["GID"]);
            p.Add("LID", LID);
            if (string.IsNullOrEmpty(bannerUrl)) { p.Add("D-BannerUrl0", s["D-BannerUrl0"]); p.Add("D-BannerUrl1", s["D-BannerUrl1"]); }
            else { p.Add("D-BannerUrl", s["D-BannerUrl"]); p.Add("D-BannerUrl0", s["D-BannerUrl0"]); }
            p.Add("UGID", s["UGID"]);
            p.Add("D-ServerDescriptionCount", 4);
            p.Add("D-ServerDescription0", s["D-ServerDescription0"]);
            p.Add("D-ServerDescription1", s["D-ServerDescription1"]);
            p.Add("D-ServerDescription2", s["D-ServerDescription2"]);
            p.Add("D-ServerDescription3", s["D-ServerDescription3"]);
            p.Add("D-Minimap", s["D-Minimap"]);
            p.Add("D-KillCam", s["D-KillCam"]);
            p.Add("D-Crosshair", s["D-Crosshair"]);
            p.Add("D-AutoBalance", s["D-AutoBalance"]);
            p.Add("D-FriendlyFire", s["D-FriendlyFire"]);
            p.Add("D-ThreeDSpotting", s["D-ThreeDSpotting"]);
            p.Add("D-MinimapSpotting", s["D-MinimapSpotting"]);
            p.Add("D-ThirdPersonVehicleCameras", s["D-ThirdPersonVehicleCameras"]);
            pdats = new List<string>(); string key, val; for (int i = 0; i < 32; i++)
            {
                key = $"D-pdat{i:D2}"; val = (string)s[key];
                p.Add(key, val);
                if (!"|0|0|0|0".Equals(val))
                {
                    pdats.Add(val);
                }
            }
            p.Send("0x40000000", "GDET");       
        }

        private static void LLST(Packet p)
        {
            /*
             ->N LLST 0x40000000 {FILTER-FAV-ONLY=0, FILTER-NOT-FULL=0, FILTER-NOT-PRIVATE=0, FILTER-NOT-CLOSED=0, FILTER-MIN-SIZE=0, FAV-PLAYER=, FAV-GAME=, FAV-PLAYER-UID=, FAV-GAME-UID=, TID=10}
             <-N LLST {TID=10, NUM-LOBBIES=1}
             */
            p.Add("TID", p.TID);
            p.Add("NUM-LOBBIES", 1);
            p.Send("0x40000000");
            /*
             <-U LDAT {PASSING=66, NAME=bfbc2PC01, LOCALE=en_US, TID=10, MAX-GAMES=10000, NUM-GAMES=66, FAVORITE-GAMES=0, FAVORITE-PLAYERS=0, LID=257}
             */
            int serverCount = R34SvrInfo.Count;
            p.Add("PASSING", serverCount);
            p.Add("NAME", "bfbc2PC01");
            p.Add("LOCALE", "en_US");
            p.Add("TID", p.TID);
            p.Add("MAX-GAMES", 10000);
            p.Add("NUM-GAMES", serverCount);
            p.Add("FAVORITE-GAMES", 0);
            p.Add("FAVORITE-PLAYERS", 0);
            p.Add("LID", LID);
            p.Send("0x40000000", "LDAT");
        }

        private static void USER(Packet p)
        {
            /*
             ->N USER 0x40000000 {CID=, MAC=$000000000000, SKU=PC, LKEY=W1-GXWZPPNrLDJEACki6HAAAKDw., NAME=, TID=2}
             <-N USER {NAME=Violent%3aKiTtEn, TID=2}
             */
            string lkey = p.GetVal("LKEY");

            var user = GetUserInfo(lkey);

            // 根据士兵角色Key查找信息`18395刚经过CONN连接到达此处验证连接合法性`所以把18395端口的SocketChilkatID赋值方便后期查询
            if (user != null)
            {
                user.OIDB_18395 = p.OID;
                p.Add("NAME", user.PName);
                p.Add("TID", p.TID);
                p.Send("0x40000000");
            }
            else
            {
                Console.WriteLine("18395_USER_未找到用户信息..."); p.SKT.Close(0);
            }
        }

        private static void CONN(Packet p)
        {
            /*
             ->N CONN 0x40000000 {PROT=2, PROD=bfbc2-pc, VERS=1.0, PLAT=PC, LOCALE=en_US, SDKVERSION=5.1.2.0.0, TID=1}
             <-N CONN {TIME=1680573900, TID=1, activityTimeoutSecs=240, PROT=2}   
             */
            p.Add("TID", p.TID);
            p.Add("TIME", GetTimeStamp10());
            p.Add("PROT", p.GetVal("PROT"));
            p.Add("activityTimeoutSecs", 240);
            p.Send("0x40000000");
        }
    }
}
