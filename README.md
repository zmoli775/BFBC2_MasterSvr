修改cfg.ini
SvrIp=x.x.x.x(Server`Wan`IP)
----------------------------------------------------------------------------------------------------------------
R34服务器端口：18321、18326(TCP)
start Frost.Game.Main_Win32_Final_R34.exe -serverInstancePath "#1/"  -feslhost 127.0.0.1:18321 -theaterhost 127.0.0.1:18326
----------------------------------------------------------------------------------------------------------------
R11客户端端口：19021、19026(TCP)
----------------------------------------------------------------------------------------------------------------
ServerOptions.ini
----------------------------------------------------------------------------------------------------------------
[Options]
Name="#3-BFBC2->WolvesServer<-27001"
Port=27001
NumGameClientSlots=32
BannerUrl="https://cdn.nlark.com/yuque/0/2021/jpeg/2946654/1632977815891-952ee151-06fe-444a-88c8-7a684fdb3e78.jpeg"
ServerDescription="la la la wa ha ha o ye ye go go go ..."
Region=Asia
Ranked=true
Punkbuster=false

RemoteAdminPort=127.0.0.1:27001
RemoteAdminPassword=admin27001
RevisionLevel = 8
RevisionKey = 7C0A303E-F4D2-985E-763D-E7C41B1E06A3
GameModID = BC2
----------------------------------------------------------------------------------------------------------------
