using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using StringBuilder = System.Text.StringBuilder;
using FileAccess = System.IO.FileAccess;
using System.Runtime.InteropServices;
using System.Data;
using MySql.Data.MySqlClient;

namespace Bc2BlazeSvr
{
    internal static class Myfunc
    {
        public static void xlog(string str, string? path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = "log.txt";
            }
            using FileStream fs = new(path, FileMode.Append, FileAccess.Write);
            byte[] data = Encoding.UTF8.GetBytes(str);
            fs.Write(data, 0, data.Length);
        }

        public static int BytesToInt(this byte[] data)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(data);
            return BitConverter.ToUInt16(data, 0);
        }

        public static byte[] IntToBytes(int number)
        {
            byte[] bytes = new byte[4]; bytes[3] = (byte)number; bytes[2] = (byte)((number >> 8) & 0xFF); bytes[1] = (byte)((number >> 16) & 0xFF); bytes[0] = (byte)((number >> 24) & 0xFF); return bytes;
        }

        public static byte[] IntToBytes(uint number)
        {
            byte[] bytes = new byte[4]; bytes[3] = (byte)number; bytes[2] = (byte)((number >> 8) & 0xFF); bytes[1] = (byte)((number >> 16) & 0xFF); bytes[0] = (byte)((number >> 24) & 0xFF); return bytes;
        }

        /// <summary>键值对转字符串</summary>
        public static string DicToStr(Dictionary<string, object> dic)
        {
            int end, len = dic.Count; if (null == dic || len <= 0) { return string.Empty; }
            end = len - 1; StringBuilder sb = new(); for (int i = 0; i < len; i++)
            {
                KeyValuePair<string, object> kv = dic.ElementAt(i);
                if (i != end) { sb.Append($"{kv.Key}={kv.Value}\n"); }
                else { sb.Append($"{kv.Key}={kv.Value}\n\0"); }
            }
            return sb.ToString();
        }

        /// <summary>格式化接受字符串</summary> 
        public static string FmtRxs(string p)
        {
            return p.Replace("\n", ",");
        }

        public static string FmtTxs(string p)
        {
            return p.Replace("\n", ",").TrimEnd(new char[] { ',', '\0' });
        }
        /// <summary>
        /// 字符串转键值对
        /// </summary>
        public static Dictionary<string, string> StrToDic(string msg)
        {
            Dictionary<string, string> dic = new(); string[] a, kv; string k, v;

            a = msg.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            if (a.Length > 0)
            {
                foreach (var item in a)
                {
                    kv = item.Split('=', StringSplitOptions.None);

                    if (kv.Length == 2)
                    {
                        k = kv[0]; v = @kv[1];

                        if (!string.IsNullOrEmpty(k))
                        {
                            if (!dic.TryAdd(k, v))
                            {
                                Console.WriteLine($"---------------------------------------\n错误->MsgToDic.TryAdd()添加键值对失败:\n{k}={v}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"---------------------------------------\n错误->MsgToDic.Split键值对空值:\nIsNullOrEmpty(Key)");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"---------------------------------------\n错误->MsgToDic.Split分割异常:\n{msg}->{string.Join(",", kv)}");
                    }
                }
                return dic;
            }
            else { return dic; }
        }

        /// <summary>写数据包</summary><param name="CMD">包命令</param><param name="SEQ">包序号</param><param name="DATA">包内容</param><returns>返回(待发送)完整组包</returns>
        public static byte[] WritePacket(string cmd, uint seq, byte[] data)
        {
            using MemoryStream ms = new();
            using (BinaryWriter bw = new(ms))
            {
                bw.Write(Encoding.ASCII.GetBytes(cmd)); // 写包命令(4字节)               
                bw.Write(IntToBytes(seq));              // 写包序号(4字节)              
                bw.Write(IntToBytes(12 + data.Length)); // 写包长度(4字节)             
                bw.Write(data);                         // 写内容        
            }
            return ms.ToArray();
        }

        /// <summary>字节数组转Hex字符串</summary><param name="p">传需要处理的字节数组</param><param name="hex">格式化为:0xFF</param><returns>返回(16进制)Hex字符串</returns>
        public static string Bytes2Hexstr(byte[] p, bool hex = false)
        {
            if (p.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                if (!hex)
                {
                    foreach (var v in p) { sb.Append($"{v:X2} "); }
                    return sb.ToString().TrimEnd(' ');
                }
                else
                {
                    foreach (var v in p) { sb.Append($"0x{v:X2}, "); }
                    return sb.ToString().TrimEnd(new char[] { ',', ' ' });
                }
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// RankDic赋值
        /// </summary>  
        public static string DicToJSONStr(Dictionary<string, object> dic)
        {
            if (dic != null && dic.Count > 0)
            {
                return JsonConvert.SerializeObject(dic);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>字符串是否包含关键字</summary><param name="str">待检测的字符串</param><param name="keyword">string[]关键字</param><returns>如果包含返回true、不包含返回false</returns>
        public static bool ContainsKeyWord(this string str, params string[] keyword)
        {
            bool ret = false;
            foreach (var v in keyword)
            {
                if (string.IsNullOrEmpty(str))
                { return false; }
                ret = str.Contains(v);
                if (ret) break;
            }
            return ret;
        }

        /// <summary>获取时间戳位长13(北京时间)</summary>
        public static string GetTimeStamp13()
        {
            return Convert.ToString((DateTime.Now.Ticks - 621356256000000000) / 10000);    // 除10000时间戳为13位、除10000000时间戳为10位   
        }

        /// <summary>获取时间戳位长10(北京时间)</summary>
        public static string GetTimeStamp10()
        {
            return Convert.ToString((DateTime.Now.Ticks - 621356256000000000) / 10000000); // 除10000时间戳为13位、除10000000时间戳为10位   
        }


        /// <summary>GetDateTimeNow(2023-04-12 14:31:05)</summary>     
        public static string GetDateTimeNow()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static CultureInfo cultureInfo = new("en-us");

        /// <summary>获取DateTimeUTC(格式:"Apr-06-2023 11%3a54%3a22 UTC")</summary>
        public static string GetDateTimeUtc()
        {
            // UTC时间格式"Apr-06-2023 11%3a54%3a22 UTC"
            return DateTime.Now.ToString(@"MMM-dd-yyyy HH\%3amm\%3ass UTC", cultureInfo);
        }

        /// <summary>Base64加密</summary>
        public static string Base64Enc(string srt)
        {
            try
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(srt));
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>Base64解密</summary>
        public static string Base64Dec(string str)
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(str));
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>MD5Salt加密</summary>    
        public static string MD5Salt(string str)
        {
            try
            {
                if (string.IsNullOrEmpty(str))
                {
                    return string.Empty;
                }
                MD5 md5 = MD5.Create();
                string salt = "o0k^6/=]2`o0k3RC]4C/z2Z5kMLD`0iJ514S%Eq:cx&+?:2_@y-t/u";
                salt = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes($"{str}{salt}")), 4, 8);
                return salt.Replace("-", string.Empty).ToLower();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>生成指定长度随机字符串</summary>
        public static string GetRandomStr(int length)
        {
            Random rd = new();
            string cr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghizklmnopqrstuvwxyz0123456789", sr = string.Empty;
            for (int i = 0; i < length; i++)
            {
                sr += cr[rd.Next(cr.Length)];
            }
            return sr;
        }

        /// <summary>键值对转JSON字符串</summary>
        public static string Dic2JSONStr(Dictionary<string, object> dic)
        {
            if (dic != null && dic.Count > 0)
            {
                return JsonConvert.SerializeObject(dic);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>JSON字符串转键值对</summary>
        public static Dictionary<string, object>? JSONStr2Dic(string? str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(str);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Exe当前路径
        /// </summary>
        public static string CurrentPath = Environment.CurrentDirectory;

        /// <summary>读INI方法</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "GetPrivateProfileString")]
        public static extern void GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);
        /// <summary>读取INI</summary>
        public static string ReadINI(string lpAppName, string lpKeyName, string lpDefault, string lpFileName)
        {
            int nSize = 256;
            StringBuilder lpReturnedString = new(nSize);
            GetPrivateProfileString(lpAppName, lpKeyName, lpDefault, lpReturnedString, nSize, lpFileName);
            return lpReturnedString.ToString();
        }

        /// <summary>写INI方法</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "WritePrivateProfileString")]
        public static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        /// <summary>写INI</summary>
        public static bool WriteINI(string lpAppName, string lpKeyName, string lpDefault, string lpFileName)
        {
            return WritePrivateProfileString(lpAppName, lpKeyName, lpDefault, lpFileName);
        }

        /// <summary>检查MySql连接(连接正常返回true反之false)</summary>
        public static bool ExecutionConn()
        {
            using MySqlConnection Conn = new(ConnStr);
            try
            {
                if (Conn.State != ConnectionState.Open) Conn.Open(); return true;
            }
            catch (MySqlException)
            {
                return false;
            }
        }

        /// <summary>MySql连接字符串</summary>
        public static string? ConnStr;
        public static bool ExecuteReader(string sql, out DataTable? dt)
        {
            using MySqlConnection Conn = new(ConnStr);
            if (Conn.State != ConnectionState.Open) Conn.Open();
            using MySqlCommand cmd = new(sql, Conn);
            MySqlDataReader r = cmd.ExecuteReader();
            if (r.HasRows)
            {
                dt = new(); for (int i = 0; i < r.FieldCount; i++)
                {
                    dt.Columns.Add(new DataColumn(r.GetName(i)));
                }
                int effectRows = 0; while (r.Read())
                {
                    DataRow dr = dt.NewRow(); for (int i = 0; i < r.FieldCount; i++)
                    {
                        if (!r.IsDBNull(i))
                        {
                            dr[i] = r[i].ToString();
                        }
                    }
                    dt.Rows.Add(dr); effectRows++;
                }
                return true;
            }
            else
            {
                dt = null; return false;
            }
        }

        public static int ExecuteNonQuery(string sql, MySqlParameter[] pars)
        {
            using MySqlConnection Conn = new(ConnStr);
            if (Conn.State != ConnectionState.Open) Conn.Open();
            using MySqlCommand cmd = new(sql, Conn);
            if (pars != null)
            {
                foreach (MySqlParameter p in pars)
                {
                    cmd.Parameters.AddWithValue(p.ParameterName, p.Value);
                }
            }
            return cmd.ExecuteNonQuery();
        }


        public static int ExecuteNonQuery(string sql)
        {
            using MySqlConnection Conn = new(ConnStr);
            if (Conn.State != ConnectionState.Open) Conn.Open();
            using MySqlCommand cmd = new(sql, Conn);
            return cmd.ExecuteNonQuery();
        }

        public static object ExecuteScalar(string sql)
        {
            using MySqlConnection Conn = new(ConnStr);
            if (Conn.State != ConnectionState.Open) Conn.Open();
            using MySqlCommand cmd = new(sql, Conn);
            return cmd.ExecuteScalar();
        }

        /// <summary>AES加密字符串</summary>
        public static string AesEncStr(string str)
        {
            try
            {
                using Aes aes = Aes.Create(); aes.Key = AesKey; aes.IV = AesIV;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using MemoryStream msEncrypt = new();
                using CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write);
                using (StreamWriter swEncrypt = new(csEncrypt))
                {
                    swEncrypt.Write(str);
                }
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        /// <summary>密钥&向量</summary>
        private static readonly
            byte[] AesKey = Encoding.UTF8.GetBytes("P9WUAlzX6g$fieCc2Pxttp8fHi5ddNXw"),
            AesIV = Encoding.UTF8.GetBytes("WevFYqaNvp&eVgfM");

        /// <summary>AES解密字符串</summary>
        public static string AesDecStr(string str)
        {
            try
            {
                byte[] base64bytes = Convert.FromBase64String(str);
                using Aes aes = Aes.Create(); aes.Key = AesKey; aes.IV = AesIV;
                ICryptoTransform ct = aes.CreateDecryptor(aes.Key, aes.IV);
                using MemoryStream ms = new(base64bytes);
                using CryptoStream cs = new(ms, ct, CryptoStreamMode.Read);
                using StreamReader sr = new(cs);
                return sr.ReadToEnd();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}