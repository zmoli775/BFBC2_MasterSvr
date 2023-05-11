using Bc2BlazeSvr.Core;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static Bc2BlazeSvr.Bc2.Bc2Pub;
using static Bc2BlazeSvr.Myfunc;

namespace Bc2BlazeSvr.Bc2.r11
{
    internal static class _18390
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
            else if (p.CMD.Equals("xmsg"))
            {
                switch (p.TXN)
                {
                    case "ModifySettings": ModifySettings(p); break;

                    case "GetMessages": GetMessages(p); break;

                    case "SendMessage": SendMessage(p); break;

                    default: ShowUnknownPacket(p); break;
                }
            }
            else if (p.CMD.Equals("acct"))
            {
                switch (p.TXN)
                {
                    case "NuLogin": NuLogin(p); break;                     // 帐户登录

                    case "NuGetPersonas": NuGetPersonas(p); break;

                    case "NuLoginPersona": NuLoginPersona(p); break;

                    case "NuGetEntitlements": NuGetEntitlements(p); break;

                    case "GetLockerURL": GetLockerURL(p); break;

                    case "GetTelemetryToken": GetTelemetryToken(p); break;

                    case "NuEntitleUser": NuEntitleUser(p); break;         // 赎回代码                     

                    case "NuSearchOwners": NuSearchOwners(p); break;       // 查找好友

                    case "GetCountryList": GetCountryList(p); break;       // 注册(获取国家列表)

                    case "NuGetTos": NuGetTos(p); break;                   // 显示协议

                    case "NuAddAccount": NuAddAccount(p); break;           // 注册帐号

                    case "NuAddPersona": NuAddPersona(p); break;           // 注册士兵角色

                    case "NuDisablePersona": NuDisablePersona(p); break;   // 删除士兵角色

                    case "NuLookupUserInfo": NuLookupUserInfo(p); break;

                    default: ShowUnknownPacket(p); break;
                }
            }
            else if (p.CMD.Equals("asso"))
            {
                switch (p.TXN)
                {
                    case "GetAssociations": GetAssociations(p); break;

                    default: ShowUnknownPacket(p); break;
                }
            }
            else if (p.CMD.Equals("pres"))
            {
                switch (p.TXN)
                {
                    case "PresenceSubscribe": PresenceSubscribe(p); break;

                    case "SetPresenceStatus": SetPresenceStatus(p); break;

                    default: ShowUnknownPacket(p); break;
                }
            }

            else if (p.CMD.Equals("recp"))
            {
                switch (p.TXN)
                {
                    case "GetRecord": GetRecord(p); break;

                    case "GetRecordAsMap": GetRecordAsMap(p); break;

                    case "AddRecord": case "UpdateRecord": AddRecordUpdateRecord(p); break; // 新增|更新狗牌战绩(已完成:2023-04-14 17:24:04)

                    default: ShowUnknownPacket(p); break;
                }
            }
            else if (p.CMD.Equals("rank"))
            {
                switch (p.TXN)
                {
                    case "GetStats": GetStats(p); break;

                    case "GetRankedStats": GetRankedStats(p); break;

                    case "GetRankedStatsForOwners": GetRankedStatsForOwners(p); break;

                    case "GetTopNAndStats": GetTopNAndStats(p); break;

                    case "UpdateStats": UpdateStats(p); break;

                    default: rank(p); break;
                }
            }
            else if (p.CMD.Equals("pnow"))
            {
                switch (p.TXN)
                {
                    case "Start": Start(p); break;
                }
            }
            else
            {
                ShowUnknownPacket(p);
            }
        }

        private static void NuLookupUserInfo(Packet p)
        {
            p.Add("TXN", "NuLookupUserInfo");
            p.Add("userInfo.[]", 0);
            p.Send();
        }

        private static void SendMessage(Packet p)
        {
            SendMsg(p, "MessageRecipientsToInvalid", 22006);
        }

        /// <summary>新增|更新狗牌战绩</summary>
        private static void AddRecordUpdateRecord(Packet p)
        {
            int len = Convert.ToInt32(p.GetVal("values.[]"));
            int key; string val;
            try
            {
                if (p.HasKey("remove.[]") && !p.GetVal("remove.[]").Equals("0"))
                {
                    // 出现特殊情况`需排查
                    ShowUnknownPacket(p);
                }
                if (p.HasKey("recordName") && p.GetVal("recordName").Equals("dogtags"))
                {
                    var c = GetUserInfoFor18390(p.OID);

                    if (c != null)
                    {
                        for (int i = 0; i < len; i++)
                        {
                            key = Convert.ToInt32(p.GetVal($"values.{i}.key")); val = p.GetVal($"values.{i}.value");

                            // 先查询有无狗牌战绩`有则更新`无则插入
                            if (ExecuteScalar($"SELECT `val` FROM `dogtags` WHERE `pid` = '{c.PId}' AND `key` = '{key}' LIMIT 0,1") == null)
                            {
                                // 插入
                                _ = ExecuteNonQuery($"INSERT INTO `dogtags` (`pid`, `key`, `val`) VALUES ({c.PId}, {key}, '{val}')");
                            }
                            else
                            {   // 更新
                                _ = ExecuteNonQuery($"UPDATE `dogtags` SET `val` = '{val}' WHERE `pid` = {c.PId} AND `key` = {key}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"18390->AddRecordUpdateRecord出错catchException:\n{ex}");
            }
            finally
            {
                p.Add("TXN", p.TXN);
                p.Send();
            }
        }
        private static void Ping(Packet p)
        {
            p.Add("TXN", "MemCheck");
            p.Add("memcheck.[]", 0);
            p.Add("type", 0);
            p.Add("salt", GetTimeStamp10());
            p.Send("0x80000000");
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

        private static void NuDisablePersona(Packet p)
        {
            p.Add("TXN", "NuDisablePersona");
            p.Send();
        }

        private static void NuAddPersona(Packet p)
        {
            string pname = p.GetVal("name");

            if (!Regex.IsMatch(pname, @"^[\w+$]{4,15}"))
            {
                SendMsg(p, "NuAddPersona", 21);
            }
            else
            {
                object uId = ExecuteScalar($"SELECT UId FROM `users` WHERE LOWER(`PName`) = '{pname.ToLower()}'");

                if (uId == null)
                {
                    var c = GetUserInfoFor18390(p.OID);
                    if (c != null)
                    {
                        if (c.PId == 0)
                        {
                            if (!(ExecuteNonQuery($"INSERT INTO `ranks` (`pid`, `rank`) VALUES ({c.UId}, @rank)", new MySqlParameter[] { new MySqlParameter("@rank", DefaultRank) }) > 0))
                            {
                                p.SKT.Close(0); Console.WriteLine("警告：创建默认Rank失败[强制断开连接]"); return;
                            }
                        }

                        string pKey = $"W1-G{MD5Salt(c.UName)}HAAAKDw.";
                        int pId = c.UId;
                        c.PId = pId;
                        c.PName = pname;
                        c.PKey = pKey;

                        if (ExecuteNonQuery($"UPDATE `users` SET `PId` = {pId}, `PName` = '{pname}', `PKey` = '{pKey}' WHERE `UId` = {pId}") > 0)
                        {
                            p.Add("TXN", p.TXN);
                            p.Add("lkey", pKey);
                            p.Add("userId", pId);
                            p.Add("profileId", pId);
                            p.Send();
                        }
                        else
                        {
                            p.Add("TXN", p.TXN);
                            p.Send();
                        }
                    }
                }
                else
                {
                    SendMsg(p, "NuAddPersona", 160);
                }
            }
        }

        /// <summary>注册帐号</summary>
        private static void NuAddAccount(Packet p)
        {
            string name = p.GetVal("nuid").ToLower().Trim();
            string pwd = p.GetVal("password");
            string regtime = GetDateTimeNow();
            try
            {
                if (Regex.IsMatch(name, @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$") && name.Length >= 6 && name.Length <= 64)
                {
                    object r = ExecuteScalar($"SELECT UId FROM users WHERE LOWER(UName) = '{name}' LIMIT 0, 1");
                    if (r == null)
                    {
                        pwd = MD5Salt(pwd);

                        if (ExecuteNonQuery($"INSERT INTO `users` (`UName`, `UKey`, `PId`, `RegTime`, `Ip`) VALUES ('{name}', '{pwd}', 0, '{regtime}', '{p.SKT.RemoteIpAddress}')") > 0)
                        {
                            p.Add("TXN", "NuAddAccount");
                            p.Send();
                        }
                        else
                        {
                            SendMsg(p, "FailedToCreateAccount", 99);
                        }
                    }
                    else
                    {
                        SendMsg(p, "ThatAccountNameIsAlreadyTaken", 160);
                    }
                }
                else
                {
                    SendMsg(p, "TheRequiredParametersForThisCallAreMissingOrInvalid", 21);
                }
            }
            catch (Exception)
            {
                SendMsg(p, "FailedToCreateAccount", 99);
            }
        }

        private static void NuGetTos(Packet p)
        {
            p.Add("TXN", "NuGetTos");
            p.Add("tos", FilterString(@$"BFBC2 Emu MasterSvr by:zmoli775@qq.com"));
            p.Add("version", "20426_17.20426_17");
            p.Send();
        }

        private static void GetCountryList(Packet p)
        {
            if (AllowRegister)
            {
                p.Add("TXN", "GetCountryList");
                p.Add("countryList.[]", 1);
                p.Add("countryList.0.ISOCode", "CN");
                p.Add("countryList.0.description", $"\"{FilterString($"0x77.CN->Version:{Bc2Pub.Version}")}\"");
                p.Add("countryList.0.allowEmailsDefaultValue", 1);
                p.Add("countryList.0.registrationAgeLimit", 0);
                p.Add("countryList.0.parentalControlAgeLimit", 0);
            }
            else
            {
                p.Add("TXN", "GetCountryList");
                p.Add("countryList.[]", 0);
            }
            p.Send();
        }
        private static void Start(Packet p)
        {
            SendMsg(p, "NotFound", 5000);
        }

        private static void GetTopNAndStats(Packet p)
        {
            p.Add("TXN", "GetTopNAndStats");
            p.Add("rankedStats.[]", 0);
            p.Send();
        }
        private static void NuSearchOwners(Packet p)
        {
            SendMsg(p, "NotFound", 104);
        }

        private static void GetRankedStatsForOwners(Packet p)
        {
            SendMsg(p, "NotFound", 5000);
        }


        /// <summary>激活码处理</summary>
        private static void NuEntitleUser(Packet p)
        {
            SendMsg(p, "ActCodeError", 181);
        }

        ///<summary>rank</summary>
        private static void rank(Packet p)
        {
            if (p.HasKey("size") && p.HasKey("data"))
            {
                int size = int.Parse(p.GetVal("size")); string data = p.GetVal("data");

                var c = GetUserInfoFor18390(p.OID);

                if (c != null)
                {
                    if (data.StartsWith("VFhO"))
                    {
                        c.Temp = string.Empty;
                    }

                    bool End = data.EndsWith("%3d"); if (End) { data = data.Replace("%3d", "="); }

                    c.Temp += data;

                    if (c.Temp.Length == size || End)
                    {
                        c.Temp = string.Empty;

                        object rankJson = ExecuteScalar($"SELECT `rank` FROM `ranks` WHERE pid = {c.PId}");

                        Dictionary<string, object>? dic = null;

                        if (rankJson != null)
                        {
                            dic = JSONStr2Dic(Convert.ToString(rankJson));
                        }

                        if (dic == null)
                        {
                            c.Temp = string.Empty;
                            Console.WriteLine("警告:18390->rank->登录失败");
                            p.SKT.Close(0);
                        }

                        int len = IdxKeyA.Length; string key, text = DateTime.Now.ToString("yyyyMMdd"); StringBuilder rank = new();

                        rank.Append($"TXN=GetStats\nstats.[]={len}\n");

                        for (int i = 0; i < len; i++)
                        {
                            key = IdxKeyA[i];

                            rank.Append($"stats.{i}.key={key}\n");

                            if (key.Equals("level") || key.Equals("elo")) { rank.Append($"stats.{i}.text={text}\n"); }

                            if (key.Equals("webvet"))
                            {
                                rank.Append($"stats.{i}.value=1\n");
                            }
                            else
                            {
                                rank.Append($"stats.{i}.value={dic![key]}\n");
                            }
                        }

                        p.Send("rank", "rank", rank); return;
                    }
                    else if (c.Temp.Length > size)
                    {
                        c.Temp = string.Empty;
                        p.Add("TXN", "GetStats"); p.Add("stats.[]", 0);
                        p.Send(); return;
                    }
                }
                else
                {
                    Console.WriteLine($"没找到用户信息->rank");
                    ShowPacket(p);
                    p.SKT.Close(0);
                }
            }
            else
            {
                ShowUnknownPacket(p);
            }
        }

        private static void GetRankedStats(Packet p)
        {
            p.Add("TXN", "GetRankedStats");
            p.Add("stats.[]", 0);
            p.Send();
        }

        private static void GetMessages(Packet p)
        {
            /*             
             ->N req xmsg 0xc0000013 {TXN=GetMessages, attachmentTypes.[]=1, attachmentTypes.0=text/plain, box=inbox, chunkSize=0}
             <-N res xmsg 0x80000013 {TXN=GetMessages, messages.[]=0}
             */
            p.Add("TXN", "GetMessages");
            p.Add("messages.[]", 0);
            p.Send();
        }

        private static void ModifySettings(Packet p)
        {
            /*
             ->N req xmsg 0xc000000e {TXN=ModifySettings, retrieveAttachmentTypes.[]=0, retrieveAttachmentTypes.0=text/plain, notifyMessages=1}
             <-N res xmsg 0x8000000e {TXN=ModifySettings}
             */
            p.Add("TXN", p.TXN);
            p.Send();
        }

        ///<summary>GetTelemetryToken</summary>
        private static void GetTelemetryToken(Packet p)
        {
            /*
             ->N req acct 0xc000000b {TXN=GetTelemetryToken}
             <-N res acct 0x8000000b {
                     telemetryToken=MTU5LjE1My4yMzUuMjYsOTk0Nixlbk1PLF7wlZTv8avrubWRkunE4ueA45Xq6/aN27S37tThh6/NmrXYkIGDhoyYsODAgYOGjJiw2JCBg4aMmLDgwIHzho6ZteKYs7OGy6LBjoXq1OjUl8KMiZqkhsuYtvDAuYPHjJi45rCp5q3Tpwo%3d, 
                     enabled=CA,MX,PR,US,VI, 
                     TXN=GetTelemetryToken, 
                     filters=, 
                     disabled=AD,AF,AG,AI,AL,AM,AN,AO,AQ,AR,AS,AW,AX,AZ,BA,BB,BD,BF,BH,BI,BJ,BM,BN,BO,BR,BS,BT,BV,BW,BY,BZ,CC,CD,CF,CG,CI,CK,CL,CM,CN,CO,CR,CU,CV,CX,DJ,DM,DO,DZ,EC,EG,EH,ER,ET,FJ,FK,FM,FO,GA,GD,GE,GF,GG,GH,GI,GL,GM,GN,GP,GQ,GS,GT,GU,GW,GY,HM,HN,HT,ID,IL,IM,IN,IO,IQ,IR,IS,JE,JM,JO,KE,KG,KH,KI,KM,KN,KP,KR,KW,KY,KZ,LA,LB,LC,LI,LK,LR,LS,LY,MA,MC,MD,ME,MG,MH,ML,MM,MN,MO,MP,MQ,MR,MS,MU,MV,MW,MY,MZ,NA,NC,NE,NF,NG,NI,NP,NR,NU,OM,PA,PE,PF,PG,PH,PK,PM,PN,PS,PW,PY,QA,RE,RS,RW,SA,SB,SC,SD,SG,SH,SJ,SL,SM,SN,SO,SR,ST,SV,SY,SZ,TC,TD,TF,TG,TH,TJ,TK,TL,TM,TN,TO,TT,TV,TZ,UA,UG,UM,UY,UZ,VA,VC,VE,VG,VN,VU,WF,WS,YE,YT,ZM,ZW,ZZ}            
             p.Add("telemetryToken", ReplaceStr(Base64Enc($"{SvrIp},18326,enUS,^Zmoli775")));
             */
            p.Add("TXN", "GetTelemetryToken");
            p.Add("telemetryToken", string.Empty);
            p.Add("filters", string.Empty);
            p.Add("enabled", string.Empty);
            p.Add("disabled", string.Empty);
            p.Send();
        }

        ///<summary>SetPresenceStatus</summary>
        private static void SetPresenceStatus(Packet p)
        {
            /*
             ->O req pres 0xc0000019 {TXN=SetPresenceStatus, status.show=game, status.attributes.{joinable}=true}
             ->O req pres 0xc000001a {TXN=SetPresenceStatus, status.show=game, status.status="1:32:1:22:#3 BC2 HaoYue %3dWolves%3d:Levels/MP_004", status.attributes.{joinable}=true}
             ->O req pres 0xc000001b {TXN=SetPresenceStatus, status.show=chat}
             <-O res pres 0x80000019 {TXN=SetPresenceStatus}
             <-O res pres 0x8000001a {TXN=SetPresenceStatus}
             <-O res pres 0x8000001b {TXN=SetPresenceStatus}          
             */
            p.Add("TXN", "SetPresenceStatus");
            p.Send();
        }

        private static void GetStats(Packet p)
        {
            /*
             ->N req rank 0xc0000014 {TXN=GetStats, periodId=0, periodPast=0, keys.0=rank, keys.1=score, keys.2=time, keys.[]=3}
             <-N res rank 0x80000014 {
             TXN=GetStats,
             stats.[]=3,
             stats.0.key=rank,  stats.0.value=50.0
             stats.1.key=score, stats.1.value=8000, 
             stats.2.key=time,  stats.2.value=201427.674
             }
            */

            var u = GetUserInfoFor18390(p.OID);
            if (u != null)
            {
                string sql = $"SELECT JSON_EXTRACT( `rank`, '$.rank' ) `rank`, JSON_EXTRACT( `rank`, '$.score' ) `score`, JSON_EXTRACT( `rank`, '$.time' ) `time` FROM `ranks` WHERE pid = {u.PId}";
                string[] kyes = { "rank", "score", "time" };
                p.Add("TXN", p.TXN);
                p.Add("stats.[]", 3);
                if (ExecuteReader(sql, out DataTable? dt))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        p.Add($"stats.{i}.key", kyes[i]);
                        p.Add($"stats.{i}.value", dt!.Rows[0][kyes[i]]);
                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        p.Add($"stats.{i}.key", kyes[i]);
                        p.Add($"stats.{i}.value", 0);
                    }
                }
                p.Send();
            }
        }
        private static void GetRecordAsMap(Packet p)
        {
            /*
             ->N req recp 0xc0000012 {TXN=GetRecordAsMap, recordName=dogtags}
             <-N res recp 0x80000012 {localizedMessage="Record not found", errorContainer.[]=0, TXN=GetRecordAsMap, errorCode=5000}             
             */
            if (p.HasKey("recordName") && p.GetVal("recordName").Equals("dogtags"))
            {
                var c = GetUserInfoFor18390(p.OID);
                if (c != null && ExecuteReader($"SELECT `pid`, `key`, `val` FROM `dogtags` WHERE `pid` = {c.PId}", out DataTable? dt))
                {
                    int count = dt!.Rows.Count;
                    StringBuilder sb = new();
                    sb.Append($"TXN=GetRecordAsMap\n");
                    sb.Append($"values.{{}}={count}\n");
                    sb.Append($"TTL=0\n");
                    sb.Append($"state=1\n");
                    string lastModified = DateTime.Now.ToString("yyyy-MM-dd HH'%3a'mm'%3a'ss.0");
                    sb.Append($"lastModified=\"{lastModified}\"\n");
                    DataRow dr; for (int i = 0; i < count; i++)
                    {
                        dr = dt.Rows[i];
                        sb.Append($"values.{{{dr["key"]}}}={dr["val"]}\n");
                    }
                    p.Send("rank", "recp", sb);
                    return;
                }
                else
                {
                    SendMsg(p, "NotFound", 5000);
                }
            }
        }

        private static void GetRecord(Packet p)
        {
            /*
             ->N req recp 0xc0000019 {TXN=GetRecord, recordName=clan}
             <-N res recp 0x80000019 {localizedMessage="Record not found", errorContainer.[]=0, TXN=GetRecord, errorCode=5000}
             */
            SendMsg(p, "NotFound", 5000);
        }

        private static void GetLockerURL(Packet p)
        {
            var user = GetUserInfoFor18390(p.OID);
            if (user != null)
            {
                p.Add("TXN", "GetLockerURL");
                p.Add("URL", $"http%3a//{WebSvr}:{WebPort}/pid/{user.PId}/fileupload/locker.jsp");        
                p.Send();
            }
            else
            {
                p.Add("TXN", "GetLockerURL");
                p.Add("URL", $"http%3a//{WebSvr}:{WebPort}/pid/0/fileupload/locker.jsp");
                p.Send();
            }
        }

        private static void NuGetEntitlements(Packet p)
        {
            string groupName = p.GetVal("groupName");

            var c = GetUserInfoFor18390(p.OID);

            if (c != null)
            {
                int userId = c.UId;

                if (groupName.Equals("BFBC2PC"))
                {
                    string[] tags;

                    if (UnlockRank)
                    {
                        tags = new string[] { "BFBC2%3aPC%3aVIETNAM_PDLC", "BFBC2%3aPC%3aALLKIT", "BFBC2%3aPC%3aLimitedEdition", "BFBC2%3aPC%3aMAXALLKIT" };
                    }
                    else
                    {
                        tags = new string[] { "BFBC2%3aPC%3aVIETNAM_PDLC", "BFBC2%3aPC%3aALLKIT", "BFBC2%3aPC%3aLimitedEdition" };
                    }

                    int len = tags.Length; for (int i = 0; i < len; i++)
                    {
                        p.Add($"entitlements.{i}.entitlementTag", tags[i]);
                        p.Add($"entitlements.{i}.userId", userId);
                        p.Add($"entitlements.{i}.groupName", "BFBC2PC");
                        p.Add($"entitlements.{i}.terminationDate", string.Empty);
                        p.Add($"entitlements.{i}.statusReasonCode", string.Empty);
                        p.Add($"entitlements.{i}.productId", "DR%3a156691300");
                        p.Add($"entitlements.{i}.grantDate", "2010-06-16T7%3a30Z");
                        p.Add($"entitlements.{i}.status", "ACTIVE");
                        p.Add($"entitlements.{i}.entitlementId", 0);
                        p.Add($"entitlements.{i}.version", 0);
                    }
                    p.Add("TXN", "NuGetEntitlements");
                    p.Add("entitlements.[]", len);
                    p.Send();
                }
                else if (groupName.Equals("AddsVetRank"))
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
                else // Bug修复NuGetEntitlements包错误
                {
                    p.Add("TXN", "NuGetEntitlements");
                    p.Add("entitlements.[]", 0);
                    p.Send();
                }            
            }
        }

        //
        private static void PresenceSubscribe(Packet p)
        {
            /*
             ->N req pres 0xc000000a {TXN=PresenceSubscribe, requests.[]=1, requests.0.userId=941955336}
             <-N res pres 0x8000000a {
             TXN=PresenceSubscribe,
             responses.[]=1,
             responses.0.outcome=0,  
             responses.0.owner.type=0, 
             responses.0.owner.id=941955336, 
             responses.0.owner.name=AsukaLangely~
             }
             */
            string userId = p.GetVal("requests.0.userId");
            p.Add("TXN", "PresenceSubscribe");
            p.Add("responses.[]", 1);
            p.Add("responses.0.outcome", 0);
            p.Add("responses.0.owner.type", 0);
            p.Add("responses.0.owner.id", userId);
            p.Add("responses.0.owner.name", "Zmoli775");
            p.Send();
        }

        private static void GetAssociations(Packet p)
        {
            /*             
             ->N req asso 0xc0000006 {TXN=GetAssociations, domainPartition.domain=eagames, domainPartition.subDomain=BFBC2, domainPartition.key=, type=PlasmaMute, owner.id=2015698787, owner.type=1}
             ->N req asso 0xc0000007 {TXN=GetAssociations, domainPartition.domain=eagames, domainPartition.subDomain=BFBC2, domainPartition.key=, type=PlasmaBlock, owner.id=2015698787, owner.type=1}
             ->N req asso 0xc0000008 {TXN=GetAssociations, domainPartition.domain=eagames, domainPartition.subDomain=BFBC2, domainPartition.key=, type=PlasmaFriends, owner.id=2015698787, owner.type=1}
             ->N req asso 0xc0000009 {TXN=GetAssociations, domainPartition.domain=eagames, domainPartition.subDomain=BFBC2, domainPartition.key=, type=PlasmaRecentPlayers, owner.id=2015698787, owner.type=1}
             +++++++++++++++++++++++
             <-N res asso 0x80000006 {domainPartition.domain=eagames, domainPartition.subDomain=BFBC2, TXN=GetAssociations, owner.id=2015698787, owner.type=1, maxListSize=20,  members.[]=0, owner.name=zmoli775|x555, type=PlasmaMute}
             <-N res asso 0x80000007 {domainPartition.domain=eagames, domainPartition.subDomain=BFBC2, TXN=GetAssociations, owner.id=2015698787, owner.type=1, maxListSize=20,  members.[]=0, owner.name=zmoli775|x555, type=PlasmaBlock}
             <-N res asso 0x80000008 {domainPartition.domain=eagames, domainPartition.subDomain=BFBC2, TXN=GetAssociations, owner.id=2015698787, owner.type=1, maxListSize=20,  members.[]=0, owner.name=zmoli775|x555, type=PlasmaFriends}
             <-N res asso 0x80000009 {domainPartition.domain=eagames, domainPartition.subDomain=BFBC2, TXN=GetAssociations, owner.id=2015698787, owner.type=1, maxListSize=100, members.[]=0,                           type=PlasmaRecentPlayers}             
             */

            string ownerid = p.GetVal("owner.id"), type = p.GetVal("type");
            var c = GetUserInfoFor18390(p.OID);
            if (c != null)
            {
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
                        p.Add("owner.name", c.PName);
                        p.Add("maxListSize", 20);
                        p.Add("members.[]", 0);
                        break;
                    case "PlasmaBlock":
                        p.Add("owner.name", c.PName);
                        p.Add("maxListSize", 20);
                        p.Add("members.[]", 0);
                        break;
                    case "PlasmaFriends":
                        p.Add("owner.name", c.PName);
                        p.Add("maxListSize", 20);
                        p.Add("members.[]", 0);
                        break;
                    case "PlasmaRecentPlayers":
                        p.Add("maxListSize", 100);
                        p.Add("members.[]", 0);
                        break;
                    default:
                        ShowUnknownPacket(p);
                        break;
                }
                p.Send();
            }
        }

        private static void NuLoginPersona(Packet p)
        {
            /*
             ->N req acct 0xc0000005 {TXN=NuLoginPersona, name=Violent:KiTtEn}
             <-N res acct 0x80000005 {lkey=W1-GXWZPPNrLDJEACki6HAAAKDw., profileId=940351400, TXN=NuLoginPersona, userId=940351400}
             */
            var c = GetUserInfoFor18390(p.OID);
            if (c != null)
            {
                p.Add("TXN", p.TXN);
                p.Add("lkey", c.PKey);
                p.Add("userId", c.PId);
                p.Add("profileId", c.PId);
                p.Send();
            }
        }

        private static void Hello(Packet p)
        {
            /*
             ->N req fsys 0xc0000001 {TXN=Hello, clientString=bfbc2-pc, sku=PC, locale=en_US, clientPlatform=PC, clientVersion=1.0, SDKVersion=5.1.2.0.0, protocolVersion=2.0, fragmentSize=8096, clientType=}
             <-N res fsys 0x80000001 {domainPartition.domain=eagames, messengerIp=messaging.ea.com, messengerPort=13505, domainPartition.subDomain=BFBC2, TXN=Hello, activityTimeoutSecs=0, curTime="Apr-06-2023 11%3a54%3a22 UTC", theaterIp=bfbc2-pc.theater.ea.com, theaterPort=18395}
             <-A res fsys 0x80000000 {TXN=MemCheck, memcheck.[]=0, type=0, salt=1624497440}
             ->R res fsys 0x80000000 {TXN=MemCheck, result=}
             */
            p.Add("domainPartition.domain", "eagames");
            p.Add("messengerIp", SvrIp);
            p.Add("messengerPort", msgp_Port);
            p.Add("domainPartition.subDomain", "BFBC2");
            p.Add("TXN", "Hello");
            p.Add("activityTimeoutSecs", 0);
            p.Add("curTime", $"\"{GetDateTimeUtc()}\"");
            p.Add("theaterIp", SvrIp);
            p.Add("theaterPort", r11t_Port);
            p.Send();
            //+++++++++++++++++++++++
            p.Add("TXN", "MemCheck");
            p.Add("memcheck.[]", 0);
            p.Add("type", 0);
            p.Add("salt", DateTime.Now.ToString("16MMssffff"));
            p.Send("0x80000000");
        }

        ///<summary>帐户登录</summary>
        private static void NuLogin(Packet p)
        {
            /* 
             ShowPacket(p);          
             ->N req acct 0xc0000002 {TXN=NuLogin, returnEncryptedInfo=0, encryptedInfo=Ciyvab0tregdVsBtboIpeChe4G6uzC1v5_-SIxmvSLJMmNcJtClaO2wWusEzW6V01FWIwT4DMHe9ucVfUPI8N1lwEP4oesW8Bue1wUJ4dNyAuBXtX7DbmMoZ8epWimjXXan3iQFHP88rOg136lfdfw.., macAddr=$000000000000}
             <-N res acct 0x80000002 {nuid=cc20945@qq.com, lkey=W1-GXWZv-NeWvlzKCk76HAAAKDw., profileId=1000106927699, TXN=NuLogin, userId=1000106927699}
             (Ciyvab0tregdVsBtboIpeChe4G6uzC1v5_-SIxmvSL{变量}..)  
            */
            string  mac = p.GetVal("macAddr");

            string uName = string.Empty, uKey = string.Empty, encryptedInfo;

            if (p.HasKey("nuid"))
            {
                uName = p.GetVal("nuid").ToLower().Trim();
            }

            if (p.HasKey("password"))
            {
                uKey = p.GetVal("password");
                uKey = MD5Salt(uKey);
            }

            if (p.HasKey("encryptedInfo"))
            {
                encryptedInfo = p.GetVal("encryptedInfo").Replace("..", string.Empty).Replace("Ciyvab0tregdVsBtboIpeChe4G6uzC1v5_-SIxmvSL", string.Empty).Replace("%3d", "=");
                encryptedInfo = AesDecStr(encryptedInfo);
                if (!string.IsNullOrEmpty(encryptedInfo))
                {            
                    var fg = encryptedInfo.Split((char)0x0B, StringSplitOptions.RemoveEmptyEntries);
                    if (fg.Length == 2)
                    {
                        uName = fg[0];
                        uKey = fg[1];
                    }
                }
            }

            if (!string.IsNullOrEmpty(uName)&& !string.IsNullOrEmpty(uKey) && !string.IsNullOrEmpty(mac) && ExecuteReader($"SELECT `UId`, `PId`, `PName`, `PKey` FROM `users` WHERE `UName` = '{uName}' AND `UKey` = '{uKey}' LIMIT 0,1", out DataTable? dt))
            {
                int uId = Convert.ToInt32(dt!.Rows[0]["UId"]);

                // 主动清理死链接
                R11ClearInfoByLastHeartbeatTime();

                // 帐号重复登录判断(PId有可能为零所以这里传UId)
                if (GetUserInfo(null, 0, 0, null, uId) != null)
                {
                    SendMsg(p, "AccountRepeatLogin", 251); Console.WriteLine($"帐号:{uName}IP:{p.SKT.RemoteIpAddress}:{p.SKT.RemotePort}重复登录..."); return;
                }

                string rKey = $"W1-U{uKey}HAAAKDw.";

                string? pKey, pName;

                var r11 = new _r11
                {
                    OIDA_18390 = p.OID,
                    ULogin = true,
                    UId = uId,
                    UName = uName,
                    UKey = rKey
                };

                int pId = Convert.ToInt32(dt!.Rows[0]["PId"]);

                if (pId > 0)
                {
                    pName = Convert.ToString(dt!.Rows[0]["PName"]);
                    pKey = Convert.ToString(dt!.Rows[0]["PKey"]);
                    if (!string.IsNullOrEmpty(pKey) && !string.IsNullOrEmpty(pName))
                    {
                        r11.PName = pName;
                        r11.PKey = pKey;
                        r11.PId = pId;
                    }
                }

                if (R11CliInfo.TryAdd(p.OID, r11))
                {
                    _ = ExecuteNonQuery($"UPDATE `users` SET `InTime` = '{GetDateTimeNow()}', `Mac` = '{mac}', `Ip` = '{p.SKT.RemoteIpAddress}' WHERE `UId` = {uId}");
                    p.Add("TXN", "NuLogin");
                    p.Add("nuid", uName);
                    p.Add("lkey", rKey);
                    p.Add("profileId", uId);
                    p.Add("userId", uId);
                    if (p.GetVal("returnEncryptedInfo").Equals("1")&& !p.HasKey("encryptedInfo"))
                    {
                        string encryptedLoginInfo = $"Ciyvab0tregdVsBtboIpeChe4G6uzC1v5_-SIxmvSL{FilterString(AesEncStr($"{uName}{(char)0x0B}{uKey}"))}..";
                        p.Add("encryptedLoginInfo", encryptedLoginInfo);
                    }
                    p.Send();
                }
            }
            else
            {
                SendMsg(p, "NuLogin", 122);
            }
        }
        private static void NuGetPersonas(Packet p)
        {
            /*
             ->N req acct 0xc0000003 {TXN=NuGetPersonas, namespace=}
             <-N res acct 0x80000003 {personas.[]=1, personas.0=X555, TXN=NuGetPersonas}
             */
            var c = GetUserInfoFor18390(p.OID);
            if (c != null)
            {
                p.Add("TXN", p.TXN);
                if (c.PId > 0)
                {
                    p.Add("personas.[]", 1); p.Add("personas.0", c.PName);
                }
                else
                {
                    p.Add("TXN", p.TXN); p.Add("personas.[]", 0);
                }
                p.Send();
            }
        }

        private static void MemCheck(Packet p)
        {
            /*
             <-A res fsys 0x80000000 {TXN=MemCheck, memcheck.[]=0, type=0, salt=1624497440}
             ->R res fsys 0x80000000 {TXN=MemCheck, result=}
             */
            return;
        }

        ///<summary>被动心跳</summary>  
        private static void GetPingSites(Packet p)
        {
            var user = GetUserInfoFor18390(p.OID);
            user?.UpdateHeartbeatTime();
            /*
             TXN=GetPingSites, 
             pingSite.[]=4,
             minPingSitesToPing=0,
             +++++++++++++++++++++++
             pingSite.0.name=nrt,
             pingSite.0.addr=109.200.220.1,
             pingSite.0.type=0,
             +++++++++++++++++++++++
             pingSite.1.name=gva,
             pingSite.1.addr=159.153.72.181,
             pingSite.1.type=0, 
             +++++++++++++++++++++++
             pingSite.2.name=sjc,
             pingSite.2.addr=159.153.70.181,
             pingSite.2.type=0,
             +++++++++++++++++++++++
             pingSite.3.name=iad,
             pingSite.3.addr=159.153.93.213,
             pingSite.3.type=0
             */
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

        private static void Goodbye(Packet p)
        {
            var c = GetUserInfoFor18390(p.OID);
            if (c != null)
            {
                _ = ExecuteNonQuery($"UPDATE `users` SET `OutTime` = '{GetDateTimeNow()}' WHERE `UId` = {c.PId}");
            }
            R11AndR34ClearInfo(p.OID);
        }
    }
}
