using Bc2BlazeSvr.Bc2;
using System.Text;
using SocketChilkat = Chilkat.Socket;
using StringBuilder = System.Text.StringBuilder;
using static Bc2BlazeSvr.Bc2.Bc2Pub;

namespace Bc2BlazeSvr.Core
{
    public class Packet
    {
        public Packet(SocketChilkat p )
        {
            SKT = p;
            PORT = p.LocalPort;
            OID = p.ObjectId;
        }

        /// <summary>SocketChilkat</summary>
        public SocketChilkat SKT { get; private set; }
        public string TXN { get; private set; } = string.Empty;
        public string CMD { get; private set; } = string.Empty;
        public uint SEQ { get; private set; }
        public int PORT { get; private set; }
        public int TID { get; private set; }

        /// <summary>SocketChilkat.ObjectId</summary>
        public int OID { get; private set; }

        ///<summary>接收数据键值对</summary>
        public Dictionary<string, string> RXD { get; private set; } = new ();

        ///<summary>发送数据键值对</summary>
        public Dictionary<string, object> TXD { get; private set; } = new();
        public string? TXS { get; private set; }
        public string RXS { get; private set; } = string.Empty;

        private readonly int HeaderSize = 12;
        public bool UnPack(byte[] pbs, int Totallength)
        {
            byte[] _len = new byte[4], // 数据(包)长度
                   _cmd = new byte[4], // 数据(包)命令
                   _seq = new byte[4], // 数据(包)序号
                   _data;              // 数据(包)内容
            
            string cmd, msg;

            int len, ren, offset = 0;


            do
            {
                // 取数据(包)命令bytes
                Array.Copy(pbs, offset, _cmd, 0, 4);
                // 取数据(包)序号bytes
                Array.Copy(pbs, offset + 4, _seq, 0, 4);
                // 取数据(包)长度bytes
                Array.Copy(pbs, offset + 8, _len, 0, 4);

                // 得到数据(包)长度
                len = Myfunc.BytesToInt(_len);
                // 数据(包)长度合法校验
                if (len < 12 || len >Totallength|| len > fragmentSize)
                {
                    Console.WriteLine($"---------------------------------------\n警告->读取数据(包)长度不正确:\n{len}"); return false;
                }        
                
                // 得到数据(包)命令
                cmd = Encoding.ASCII.GetString(_cmd);
                // 数据(包)命令合法校验
                if (string.IsNullOrEmpty(cmd) || !Cmds.Contains(cmd))
                {
                    Console.WriteLine($"---------------------------------------\n警告->非法或错误数据(包)命令:\n{cmd}"); return false;
                }

                CMD = cmd;    
                
                SEQ = (uint)Myfunc.BytesToInt(_seq);

                ren = len - HeaderSize;
                
                _data = new byte[ren];

                Array.Copy(pbs, offset + HeaderSize, _data, 0, ren);

                msg = Encoding.UTF8.GetString(_data).TrimEnd(new char[] { '\n', '\0' });

                /*
                 接收源数据
                 msg = "TXN=Hello\nclientString=bfbc2-pc\nsku=PC\nlocale=en_US\nclientPlatform=PC\nclientVersion=2.0\nSDKVersion=5.1.2.0.0\nprotocolVersion=2.0\nfragmentSize=8096\nclientType=server\n\0"                                  
                 处理后数据
                 msg = "TXN=Hello\nclientString=bfbc2-pc\nsku=PC\nlocale=en_US\nclientPlatform=PC\nclientVersion=2.0\nSDKVersion=5.1.2.0.0\nprotocolVersion=2.0\nfragmentSize=8096\nclientType=server"                 
                 */

                if (!string.IsNullOrEmpty(msg))
                {
                    RXD = Myfunc.StrToDic(msg);

                    RXS = Myfunc.FmtRxs(msg);

                    if (HasKey("TID"))
                    {
                        TID = Convert.ToInt32(GetVal("TID"));
                    }

                    if (HasKey("TXN"))
                    {
                        TXN = GetVal("TXN");
                    }
                }

                // 打印接收到的信息
                // Console.WriteLine($"{Rxs}\n----------------------------------------");

                if (len == Totallength)
                {
                    // 此处包解析完整(调用程序处理业务)
                    BusinessProcessing(this); return true;
                }
                else if ((offset += len) == Totallength)
                {
                    // 此处包解析完整(调用程序处理业务)
                    BusinessProcessing(this); return true;
                }
                else if (offset > Totallength)
                {
                    Console.WriteLine($"错误包(累加偏移量:{offset})大于包总长度..."); return false;
                }
                else
                {
                    // 此处包解析完整(调用程序处理业务)
                    BusinessProcessing(this);
                }
            } while (true);
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        public void Add(string k, object v)
        {
            TXD.TryAdd(k, v);
        }

        private byte[] PacketToBytes(string cmd, uint type)
        {
            string txd = Myfunc.DicToStr(TXD);
            TXS = Myfunc.FmtRxs(txd);
            TXD.Clear();
            return Myfunc.WritePacket(cmd, type, Encoding.UTF8.GetBytes(txd));
        }
        
        private byte[] PacketToBytes(string cmd, uint type, StringBuilder sb)
        {        
            return Myfunc.WritePacket(cmd, type, Encoding.UTF8.GetBytes(sb.ToString()));
        }
     
        public void Send(string? seqflag = null, string? cmd = null, StringBuilder? psb = null)
        {
            if (SKT == null) return;
            
            switch (seqflag)
            {
                case "0x40000000":
                     SKT.SendBytes(PacketToBytes(cmd ?? CMD, 0));
                    break;

                case "Ping":
                    SKT.SendBytes(Myfunc.WritePacket("fsys", 0, Ping));
                    break;

                case "PING":
                    SKT.SendBytes(Myfunc.WritePacket("PING", 0, Pend));
                    break;

                case "0xf0000000":
                    SKT.SendBytes(PacketToBytes(cmd ?? CMD, (SEQ & 0xfffffff) | 0xf0000000));
                    break;

                case "0x80000000":
                    SKT.SendBytes(PacketToBytes(cmd ?? CMD, 0x80000000));
                    break;

                // CGAM上线认证ugid|secret验证不正确发送拦截包
                case "0x62736563":
                    SKT.SendBytes(PacketToBytes(cmd ?? CMD, 0x62736563));
                    break;

                // CGAM上线认证ugid|secret发送拦截包后,ECNL需响应0x6e726f6d包
                case "0x6e726f6d":
                    SKT.SendBytes(PacketToBytes(cmd ?? CMD, 0x6e726f6d));
                    break;

                // R34服务器满员排队包
                case "0x71756575":
                    SKT.SendBytes(PacketToBytes(cmd ?? CMD, 0x71756575));
                    break;

                // R34服务器PLVT包(PID=1时触发)
                case "0x6d697363":
                    SKT.SendBytes(PacketToBytes(cmd ?? CMD, 0x6d697363));
                    break;

                case "rank":
                    {
                        string odat = psb!.ToString().TrimEnd('\n');                                                    // 原数据
                        int olen = odat.Length;                                                                         // 原数据长度
                        string edat = Myfunc.Base64Enc(odat);                                                           // 加密数据=Base64Enc(原数据)
                        int elen = edat.Length;                                                                         // 加密数据长度
                        edat.Replace("=", "%3d");                                                                       // 过滤字符
                        string tdat;                                                                                    // 分段数据临时承载
                        for (int i = 0; i < (Convert.ToDouble(elen) / fragmentSize); i++)
                        {
                            psb.Clear();                                                                                // 清理psb将加密数据分段截取装载到tdat                      
                            tdat = edat.Substring(i * fragmentSize, Math.Min(fragmentSize, elen - (i * fragmentSize)));
                            psb.Append($"data={tdat}\ndecodedSize={olen}\nsize={elen}\n\0");
                            SKT.SendBytes(PacketToBytes(cmd ?? CMD, (SEQ & 0xfffffff) | 0xf0000000, psb));
                        }
                    }
                    break;

                default:
                     SKT.SendBytes(PacketToBytes(cmd ?? CMD, SEQ & 0xfffffff | 0x80000000));
                    break;
            }
        }

        /// <summary>
        /// 获取对应Key值
        /// </summary>
        public string GetVal(string skey)
        {
            if (RXD.TryGetValue(skey, out var sval))
            { return sval; }
            else
            { return string.Empty; }
        }
        ///<summary>是否包含Key</summary>
        public bool HasKey(string key)
        {
            return RXD.ContainsKey(key);
        } 

      
    }
}
