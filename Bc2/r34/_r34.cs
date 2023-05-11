using SocketChilkat = Chilkat.Socket;
using static Bc2BlazeSvr.Bc2.Bc2Pub;

namespace Bc2BlazeSvr.Bc2.r34
{
    internal class _r34
    {
        public _r34(int gid, string ugid, SocketChilkat sc)
        {
            Sc = sc.CloneSocket();

            GID = gid;
            UGID = ugid;

            Cgde["IP"] = sc.RemoteIpAddress;
            Cgde["GID"] = gid;
            Cgde["UGID"] = ugid;

            Ugde["GID"] = gid;
            Ugde["UGID"] = ugid;
        }
        public SocketChilkat Sc { get; private set; }
        public int GID { get; private set; } = 0;
        public string UGID { get; private set; } = string.Empty;
        public int RequestPId { get; set; } = 0;

        public int QPOSQLEN { get; private set; } = 0;

        public void 队列增加(out int qposqlen)
        {
            ++QPOSQLEN;
            Cgde["QP"] = QPOSQLEN;
            qposqlen = QPOSQLEN; ;
        }

        public void 队列减少()
        {
            if (QPOSQLEN > 0)
            {
                --QPOSQLEN;
                Cgde["QP"] = QPOSQLEN;
            }
        }

        ///<summary>CGAM/UGAM</summary>
        public Dictionary<string, object> Cgde { get; set; } = new()
        { 
            { "GID", 0 },
            { "NAME", string.Empty },
            { "IP", string.Empty },
            { "PORT", 0 },
            { "B-U-level", string.Empty },
            { "INT-IP", string.Empty },
            { "INT-PORT", 0 },
            { "B-version", BVersion },
            { "B-U-gameMod", "BC2" },
            { "B-U-gamemode", string.Empty },
            { "B-U-hash", string.Empty },
            { "B-U-public", 1 },
            { "B-U-region", string.Empty },
            { "B-U-sguid", 0 },
            { "JOIN", "O" },         
            // ------------------
            { "RESERVE-HOST", 0 },
            { "HTTYPE", "A" },
            { "TYPE", "G" },
            { "QLEN", 100 },
            { "DISABLE-AUTO-DEQUEUE", 1 },
            { "HXFR", 0 },
            { "MAX-PLAYERS", 0 },
            { "UGID", string.Empty },
            { "SECRET", string.Empty },
            { "B-U-Hardcore", 0 },
            { "B-U-HasPassword", 0 },
            { "B-U-Punkbuster", 0 },
            { "AP", 0 },
            { "RT", string.Empty },
            { "LID", 257 },
            { "B-U-EA", 0 },
            { "B-U-Provider", string.Empty },
            { "B-U-QueueLength", 0 },
            { "QP", 0 },
            { "B-U-Softcore", "1" },
            { "B-U-Time", string.Empty },
            { "B-U-elo", 1000 },
            { "B-maxObservers", 0 },
            { "B-numObservers", 0 },
            { "B-U-PunkBusterVersion",$"\"{PunkBusterVersion}\""}
        };


        ///<summary>UGDE</summary>
        public Dictionary<string, object> Ugde { get; set; } = new()
        {                       
            { "GID", 0 },
            { "LID", 257 },
            { "UGID", 0 }, 
            { "D-AutoBalance", 1 },            
            { "D-BannerUrl", string.Empty },
            { "D-BannerUrl0", string.Empty },
            { "D-BannerUrl1", string.Empty },
            { "D-Crosshair", 1 },
            { "D-FriendlyFire", string.Empty },
            { "D-KillCam", 1 },
            { "D-Minimap", 1 },
            { "D-ThreeDSpotting", 1 },
            { "D-MinimapSpotting", 1 },
            { "D-ServerDescription0", string.Empty },
            { "D-ServerDescription1", string.Empty },
            { "D-ServerDescription2", string.Empty },
            { "D-ServerDescription3", string.Empty },
            { "D-ServerDescriptionCount", 0 },
            { "D-ThirdPersonVehicleCameras", 1 },
            { "D-pdat00", "|0|0|0|0" },
            { "D-pdat01", "|0|0|0|0" },
            { "D-pdat02", "|0|0|0|0" },
            { "D-pdat03", "|0|0|0|0" },
            { "D-pdat04", "|0|0|0|0" },
            { "D-pdat05", "|0|0|0|0" },
            { "D-pdat06", "|0|0|0|0" },
            { "D-pdat07", "|0|0|0|0" },
            { "D-pdat08", "|0|0|0|0" },
            { "D-pdat09", "|0|0|0|0" },
            { "D-pdat10", "|0|0|0|0" },
            { "D-pdat11", "|0|0|0|0" },
            { "D-pdat12", "|0|0|0|0" },
            { "D-pdat13", "|0|0|0|0" },
            { "D-pdat14", "|0|0|0|0" },
            { "D-pdat15", "|0|0|0|0" },
            { "D-pdat16", "|0|0|0|0" },
            { "D-pdat17", "|0|0|0|0" },
            { "D-pdat18", "|0|0|0|0" },
            { "D-pdat19", "|0|0|0|0" },
            { "D-pdat20", "|0|0|0|0" },
            { "D-pdat21", "|0|0|0|0" },
            { "D-pdat22", "|0|0|0|0" },
            { "D-pdat23", "|0|0|0|0" },
            { "D-pdat24", "|0|0|0|0" },
            { "D-pdat25", "|0|0|0|0" },
            { "D-pdat26", "|0|0|0|0" },
            { "D-pdat27", "|0|0|0|0" },
            { "D-pdat28", "|0|0|0|0" },
            { "D-pdat29", "|0|0|0|0" },
            { "D-pdat30", "|0|0|0|0" },
            { "D-pdat31", "|0|0|0|0" }
        };     
    }
}
