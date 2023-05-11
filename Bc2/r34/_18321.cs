using Bc2BlazeSvr.Core;
using System.Collections;
using static Bc2BlazeSvr.Bc2.Bc2Pub;
using static Bc2BlazeSvr.Myfunc;
using StringBuilder = System.Text.StringBuilder;

namespace Bc2BlazeSvr.Bc2.r34
{
    internal static class _18321
    {
        public static void Handle(Packet p)
        {        
            if (p.CMD.Equals("fsys"))
            {
                switch (p.TXN)
                {
                    case "Hello": Hello(p); break;

                    case "MemCheck": MemCheck(p); break;

                    case "Goodbye": Goodbye(p); break;

                    case "GetPingSites": GetPingSites(p); break;

                    case "Ping": Ping(p); break;

                    default: ShowUnknownPacket(p); break;
                }
            }
            else if (p.CMD.Equals("acct"))
            {
                switch (p.TXN)
                {
                    case "NuLogin": NuLogin(p); break;

                    case "NuGetPersonas": NuGetPersonas(p); break;

                    case "NuLoginPersona": NuLoginPersona(p); break;

                    case "NuGetEntitlements": NuGetEntitlements(p); break;

                    case "NuLookupUserInfo": NuLookupUserInfo(p); break;

                    case "NuGrantEntitlement": NuGrantEntitlement(p); break;

                    default: ShowUnknownPacket(p); break;
                }
            }
            else if (p.CMD.Equals("asso"))
            {
                switch (p.TXN)
                {
                    case "GetAssociations": GetAssociations(p); break;

                    case "AddAssociations": AddAssociations(p); break;
                    
                    default: ShowUnknownPacket(p); break;
                }
            }
            else if (p.CMD.Equals("pres"))
            {
                switch (p.TXN)
                {
                    case "SetPresenceStatus": SetPresenceStatus(p); break;

                    default: ShowUnknownPacket(p); break;
                }
            }
            else if (p.CMD.Equals("rank"))
            {
                switch (p.TXN)
                {
                    case "UpdateStats":  UpdateStats(p); break;

                    default: rank(p); break;
                }
            }
            else if (p.CMD.Equals("fltr"))
            {
                switch (p.TXN)
                {
                    case "FilterProfanity": FilterProfanity(p); break;

                    default: rank(p); break;
                }
            }
            else
            {
                ShowUnknownPacket(p);
            }
        }
        
        private static void Ping(Packet p) 
        {
            return;
        }

        private static void FilterProfanity(Packet p)
        {
            p.Add("TXN", "FilterProfanity");
            p.Add("strings.[]", 1);
            p.Add("realtime", 1);
            p.Add("subChar", "*");
            p.Add("strings.0.status", 0);
            p.Add("strings.0.filtered", p.GetVal("strings.0"));
            p.Send();
        }

        private static void UpdateStats(Packet p)
        {
            try
            {
                int pid = Convert.ToInt32(p.GetVal("u.0.o"));
                object rankJson = ExecuteScalar($"SELECT `rank` FROM `ranks` WHERE pid = {pid}");
                if (rankJson != null)
                {
                    Dictionary<string, object>? dic = JSONStr2Dic(Convert.ToString(rankJson));
                    Dictionary<string, object> ret = new();
                    if (dic != null && dic.Count > 0)
                    {
                        int len = Convert.ToInt32(p.GetVal("u.0.s.[]")); int ut, pt, rank; string k; double v, NewTotalScore = 0;
                        for (int i = 0; i < len; i++)
                        {
                            ut = Convert.ToInt32(p.GetVal($"u.0.s.{i}.ut")); k = p.GetVal($"u.0.s.{i}.k"); v = Convert.ToDouble(p.GetVal($"u.0.s.{i}.v")); pt = Convert.ToInt32(p.GetVal($"u.0.s.{i}.pt"));

                            if (k.Equals("score")) { continue; }

                            if (k.Equals("sc_assault") || k.Equals("sc_demo") || k.Equals("sc_support") || k.Equals("sc_recon") || k.Equals("sc_vehicle") || k.Equals("sc_award"))
                            {
                                NewTotalScore += Convert.ToDouble(v);
                            }

                            if (dic.ContainsKey(k) && ut == 3)
                            {
                                v += Convert.ToDouble(dic[k]);
                            }
                            ret.Add(k, v);
                        }
                        NewTotalScore += Convert.ToDouble(dic["score"]);
                        ret.Add("score", NewTotalScore);
                        UpdateRanks(ret, pid);
                    }                  
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"警告:18321->UpdateStats方法出错:\n{ex.Message}");
            }
            finally
            {
                p.Add("TXN", "UpdateStats");
                p.Send();
            }
        }
        private static void NuGrantEntitlement(Packet p)
        {
            p.Add("TXN", "NuGetEntitlements");
            p.Send();
        }

        private static void NuLookupUserInfo(Packet p)
        {
            /*
             ->N(发往验证端) req acct 0xc000000a {TXN=NuLookupUserInfo, userInfo.[]=1, userInfo.0.userName=zmoli775|x555}
             <-N(验证端回复) res acct 0x8000000a {userInfo.0.namespace=battlefield, userInfo.0.masterUserId=1000106927699, userInfo.0.userId=2015698787, TXN=NuLookupUserInfo, userInfo.0.userName=zmoli775|x555, userInfo.[]=1}
             */

            int userInfo = Convert.ToInt32(p.GetVal("userInfo.[]")); string userName = p.GetVal("userInfo.0.userName");

            if (userInfo == 1)
            {
                var user = GetUserInfo(null, 0, 0, userName);

                if (user != null)
                {
                    p.Add("TXN", "NuLookupUserInfo");
                    p.Add("userInfo.[]", userInfo);
                    p.Add("userInfo.0.userId", user.PId);
                    p.Add("userInfo.0.masterUserId", user.UId);
                    p.Add("userInfo.0.userName", userName);
                    p.Add("userInfo.0.namespace", "battlefield");
                    p.Send();
                }
                else
                {
                    ShowPacket(p);
                    Console.WriteLine($"18321=>NuLookupUserInfo：userInfo.0.userName:{userName}未找到...");
                }
            }
            else
            {
                ShowPacket(p);
                Console.WriteLine("18321=>NuLookupUserInfo：userInfo.[]不等于1");
            }
        }

        private static void rank(Packet p)
        {
            if (p.HasKey("size") && p.HasKey("data"))
            {
                int size = int.Parse(p.GetVal("size")); string data = p.GetVal("data");

                if (data.StartsWith("VFhO"))
                {
                    p.SKT.UserData = string.Empty;
                }

                bool End = data.EndsWith("%3d"); if (End) { data = data.Replace("%3d", "="); }

                p.SKT.UserData += data;
       
                if (p.SKT.UserData.Length == size || End)
                {
                    var rkst = Base64Dec(p.SKT.UserData[..92]); p.SKT.UserData = string.Empty;

                    int pid = 0; var dicTmp = StrToDic(rkst);

                    if (dicTmp != null)
                    {
                        pid = Convert.ToInt32(dicTmp["owner"]);
                    }

                    if (!(pid>0))
                    {
                        Console.WriteLine("警告:18321->rank->传输Rank失败");
                        return;
                    }

                    object rankJson = ExecuteScalar($"SELECT `rank` FROM `ranks` WHERE pid = {pid}");

                    Dictionary<string, object>? dic = null;

                    if (rankJson != null)
                    {
                        dic = JSONStr2Dic(Convert.ToString(rankJson));
                    }

                    if (dic == null)
                    {           
                        Console.WriteLine("警告:18321->rank->登录失败");
                        return;
                    }

                    StringBuilder rank = new(); string key, text = DateTime.Now.ToString("yyyyMMdd");

                    int len = IdxKeyB.Length;

                    rank.Append($"TXN=GetStats\nstats.[]={len}\n");

                    for (int i = 0; i < len; i++)
                    {
                        key = IdxKeyB[i];
                        rank.Append($"stats.{i}.key={key}\n");
                        rank.Append($"stats.{i}.value={dic![key]}\n");
                        if (key.Equals("elo") || key.Equals("level"))
                        {                       
                            rank.Append($"stats.{i}.text={text}\n");
                        }   
                    }

                    p.Send("rank", "rank", rank);
                }
                else if (p.SKT.UserData.Length > size)
                {
                    p.SKT.UserData = string.Empty;
                    p.Add("TXN", "GetStats"); p.Add("stats.[]", 0);
                    p.Send(); return;
                }
            }
            else
            {
                ShowUnknownPacket(p);
            }
        }

        /// <summary>AddAssociations</summary>
        private static void AddAssociations(Packet p)
        {
            /*
             ->N req asso 0xc0000009 {TXN=AddAssociations, domainPartition.domain=eagames, domainPartition.subDomain=BFBC2, domainPartition.key=, type=PlasmaRecentPlayers, listFullBehavior=RollLeastRecentlyModified, addRequests.[]=1, addRequests.0.owner.id=224444376, addRequests.0.owner.type=1, addRequests.0.member.id=2015698787, addRequests.0.member.type=1, addRequests.0.mutual=0}
             <-O res asso 0x80000009 {result.[]=0, domainPartition.domain=eagames, domainPartition.subDomain=BFBC2, maxListSize=100, TXN=AddAssociations, type=PlasmaRecentPlayers}
             */
            p.Add("result.[]", 0);
            p.Add("domainPartition.domain", "eagames");
            p.Add("domainPartition.subDomain", "BFBC2");
            p.Add("maxListSize", 100);
            p.Add("TXN", "AddAssociations");
            p.Add("type", "PlasmaRecentPlayers");
            p.Send();
        }
        /// <summary>士兵角色赋权</summary>
        private static void NuGetEntitlements(Packet p)
        {
            string userId = p.GetVal("masterUserId");

            if (p.HasKey("projectId")&& p.GetVal("projectId").Equals("136844"))
            {
                string[] tags;

                if (UnlockRank)
                {
                    tags = new string[] { 
                        "ONLINE_ACCESS",                // 在线访问权(必须有`不然被T出服务器)
                        "BFBC2%3aPC%3aVIETNAM_ACCESS",  // 越南访问权(同上)
                        "BFBC2%3aPC%3aALLKIT",          // 迷彩包（非必须）
                        "BFBC2%3aPC%3aLimitedEdition",  // 数字豪华版（非必须+黄金色枪）
                        "BFBC2%3aPC%3aMAXALLKIT"        // 1级解锁80%武器（非必须）
                    };
                }
                else
                {
                    tags = new string[] { "ONLINE_ACCESS", "BFBC2%3aPC%3aVIETNAM_ACCESS" };
                }

                int len = tags.Length; for (int i = 0; i < len; i++)
                {
                    p.Add($"entitlements.{i}.entitlementTag", tags[i]);
                    p.Add($"entitlements.{i}.userId", userId);
                    p.Add($"entitlements.{i}.groupName", "BFBC2PC");
                    p.Add($"entitlements.{i}.terminationDate", string.Empty);
                    p.Add($"entitlements.{i}.statusReasonCode", string.Empty);
                    p.Add($"entitlements.{i}.productId", "DR%3a156691300");                                             
                    p.Add($"entitlements.{i}.grantDate", "2011-02-10T9%3a7Z");
                    p.Add($"entitlements.{i}.status", "ACTIVE");
                    p.Add($"entitlements.{i}.entitlementId", 0);
                    p.Add($"entitlements.{i}.version", 0);
                }
                p.Add("TXN", "NuGetEntitlements");
                p.Add("entitlements.[]", len);
                p.Send();
            }
            else if (p.HasKey("groupName") && p.GetVal("groupName").Equals("AddsVetRank"))
            {
                string[] tags = { "BF1942%3aPC%3aADDSVETRANK" };
                int len = tags.Length; for (int i = 0; i < len; i++)
                {
                    p.Add($"entitlements.{i}.entitlementTag", tags[i]);
                    p.Add($"entitlements.{i}.userId", userId);
                    p.Add($"entitlements.{i}.groupName", "AddsVetRank");
                    p.Add($"entitlements.{i}.terminationDate", string.Empty);
                    p.Add($"entitlements.{i}.statusReasonCode", string.Empty);
                    p.Add($"entitlements.{i}.productId", "OFB-EAST%3a56186");
                    p.Add($"entitlements.{i}.grantDate", "2010-06-16T7%3a30Z");
                    p.Add($"entitlements.{i}.entitlementId", 0);
                    p.Add($"entitlements.{i}.status", "ACTIVE");
                    p.Add($"entitlements.{i}.version", 0);
                }
                p.Add("TXN", "NuGetEntitlements");
                p.Add("entitlements.[]", len);
                p.Send();
            }
            else
            {
                p.Add("TXN", "NuGetEntitlements");
                p.Add("entitlements.[]", 0);
                p.Send();
            }
        }

        private static void SetPresenceStatus(Packet p)
        {
            /*
             ->O req pres 0xc000000a {TXN=SetPresenceStatus, status.show=disc}
             <-O res pres 0x8000000a {TXN=SetPresenceStatus}
             */
            p.Add("TXN", p.TXN);
            p.Send();
        }

        ///<summary>被动心跳</summary>  
        private static void GetPingSites(Packet p)
        {          
            string[] sites = { "nrt", "gva", "sjc", "iad" };
            p.Add("TXN", "GetPingSites");
            p.Add("minPingSitesToPing", 0);
            p.Add("pingSite.[]", 4);
            for (int i = 0; i < 4; i++)
            {
                p.Add($"pingSite.{i}.name", sites[i]);
                p.Add($"pingSite.{i}.addr", SvrIp);
                p.Add($"pingSite.{i}.type", 0);
            }
            p.Send();
            p.Send("Ping");
        }

        private static void GetAssociations(Packet p)
        {
            int ownerid = Convert.ToInt32(p.GetVal("owner.id"));
            string type = p.GetVal("type");
            /*
             ->N req asso 0xc0000005 {TXN=GetAssociations, domainPartition.domain=eagames, domainPartition.subDomain=BFBC2, domainPartition.key=, owner.id=224444376, owner.type=1, type=PlasmaMute}
             ->N req asso 0xc0000006 {TXN=GetAssociations, domainPartition.domain=eagames, domainPartition.subDomain=BFBC2, domainPartition.key=, owner.id=224444376, owner.type=1, type=PlasmaBlock}
             ->N req asso 0xc0000007 {TXN=GetAssociations, domainPartition.domain=eagames, domainPartition.subDomain=BFBC2, domainPartition.key=, owner.id=224444376, owner.type=1, type=PlasmaFriends}
             ->N req asso 0xc0000008 {TXN=GetAssociations, domainPartition.domain=eagames, domainPartition.subDomain=BFBC2, domainPartition.key=, owner.id=224444376, owner.type=1, type=PlasmaRecentPlayers}
             */
            p.Add("domainPartition.domain", "eagames");
            p.Add("domainPartition.subDomain", "BFBC2");
            p.Add("TXN", "GetAssociations");
            p.Add("owner.id", ownerid);
            p.Add("owner.type", 1);
            p.Add("type", type);
            // +++++++++++++++++++++++
            switch (type)
            {
                case "PlasmaMute":
                    p.Add("owner.name", HN);
                    p.Add("maxListSize", 20);
                    p.Add("members.[]", 0);
                    p.Send();
                    break;
                case "PlasmaBlock":
                    p.Add("owner.name", HN);
                    p.Add("maxListSize", 20);
                    p.Add("members.[]", 0);
                    p.Send();
                    break;
                case "PlasmaFriends":
                    p.Add("owner.name", HN);
                    p.Add("maxListSize", 20);
                    p.Add("members.[]", 0);
                    p.Send();
                    break;
                case "PlasmaRecentPlayers":
                    p.Add("maxListSize", 100);
                    p.Add("members.[]", 0);
                    p.Send();
                    break;
                case "dogtags":
                    /*
                     ->N req asso 0xc000000d {TXN=GetAssociations, domainPartition.domain=eagames, domainPartition.subDomain=BFBC2, domainPartition.key=, type=dogtags, owner.id=2015698787, owner.type=1}
                     <-N res asso 0x8000000d {domainPartition.domain=eagames, domainPartition.subDomain=BFBC2, maxListSize=20, TXN=GetAssociations, members.[]=0, owner.name=zmoli775|x555, type=dogtags, owner.id=2015698787, owner.type=1}
                     */
                    var user = GetUserInfo(null, 0, ownerid);
                    if (user != null)
                    {
                        p.Add("owner.name", user.PName);
                        p.Add("maxListSize", 20);
                        p.Add("members.[]", 0);
                        p.Send();
                    }
                    else
                    {
                        ShowPacket(p);
                        Console.WriteLine("GetAssociations方法dogtags标签错误");
                    }
                    break; 
             
                default:
                    ShowUnknownPacket(p);
                    break;
            }           
        }

        private static void Hello(Packet p)
        {
            // TXN=Hello,clientString=bfbc2-pc,sku=PC,locale=en_US,clientPlatform=PC,clientVersion=2.0,SDKVersion=5.1.2.0.0,protocolVersion=2.0,fragmentSize=8096,clientType=server

            p.Add("domainPartition.domain", "eagames");
            p.Add("messengerIp", SvrIp);
            p.Add("messengerPort", msgp_Port);
            p.Add("domainPartition.subDomain", "BFBC2");
            p.Add("TXN", "Hello");
            p.Add("activityTimeoutSecs",0);
            p.Add("curTime", $"\"{GetDateTimeUtc()}\"");
            p.Add("theaterIp", SvrIp);
            p.Add("theaterPort", r34t_Port);
            p.Send();
            p.Add("TXN", "MemCheck");
            p.Add("memcheck.[]", 0);
            p.Add("type", 0);
            p.Add("salt", DateTime.Now.ToString("16MMssffff"));
            p.Send("0x80000000");
        }

        private static void MemCheck(Packet p)
        {
            return;
        }
        private static void Goodbye(Packet p)
        {
            R11AndR34ClearInfo(p.OID);
        }

        private static void NuLogin(Packet p)
        {
            p.Add("nuid", "bfbc2.server.pc@ea.com");
            p.Add("lkey", $"W1-GXWZv-9TuSIGrCki6HAAAKDw.");
            p.Add("profileId", HU);
            p.Add("TXN", p.TXN);
            p.Add("userId", HU);
            p.Add("encryptedLoginInfo", "Ciyvab0tregdVsBtboIpeChe4G6uzC1v5_-SIxmvSLKGJoXN5De-_KFv5EaPnLehNvmxTCCQLUDbBiX8D0oBU1sMVx6FTK8MRDN7_s0O3bb-_nKrhGl82-1HuDI2EksPMGIAZ098zrV93xqMvkocKg..");
            p.Send();
        }

        private static void NuGetPersonas(Packet p)
        {
            p.Add("personas.[]", 1);
            p.Add("personas.0", HN);
            p.Add("TXN", "NuGetPersonas");
            p.Send();
        }
        private static void NuLoginPersona(Packet p)
        {
            p.Add("lkey", "W1-GXWZv-_4-ruduCki6HAAAKDw.");
            p.Add("profileId", HU);
            p.Add("userId", HU);
            p.Add("TXN", "NuLoginPersona");
            p.Send();
        }
    }
}
