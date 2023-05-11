namespace Bc2BlazeSvr.Bc2.r11
{
    internal class _r11
    {
        /// <summary>连接18390端口的SocketChilkatID</summary>
        public int OIDA_18390 { get; set; } = 0;
        /// <summary>连接18395端口的SocketChilkatID</summary>
        public int OIDB_18395 { get; set; } = 0;
        /// <summary>EGAM请求进入R34服务器++PID</summary>
        public int RequestPId { get; set; } = 0;
        /// <summary>1960RANK传输Base64加密字符串临时存放</summary>
        public string? Temp { get; set; } = string.Empty;
        /// <summary>帐户内码(数据库自增ID)</summary>
        public int UId { get; set; } = 0;
        /// <summary>帐户名(登录名)</summary>
        public string UName { get; set; } = string.Empty;
        /// <summary>帐户特征码=16位MD5(帐户密码)</summary>
        public string UKey { get; set; } = string.Empty;
        /// <summary>帐户已登录安全校验</summary>
        public bool ULogin { get; set; } = false;
        /// <summary>士兵角色ID</summary>
        public int PId { get; set; } = 0;
        /// <summary>士兵角色名</summary>
        public string PName { get; set; } = string.Empty;
        /// <summary>士兵角色特征码=16位MD5(UName)</summary>
        public string PKey { get; set; } = string.Empty;
        /// <summary>士兵角色已登录安全校验</summary>
        public bool PLogin { get; set; } = false;
        /// <summary>最后一次心跳时间</summary>
        public DateTime LastHeartbeatTime { get; private set; }
        /// <summary>更新心跳时间</summary>
        public void UpdateHeartbeatTime()
        {
            LastHeartbeatTime = DateTime.Now;
        }     
    }
}
