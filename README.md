# ->【Battlefield:Bad Company 2】 Emu Server
#### SourceCode：CheckOut “Master” branch
### 源码：请查看“Master”分支
#### If you have a better fix, please push it to me and I will merge it.
---
```run.bat
@echo off
@mode con cols=40 lines=8
cls
start Frost.Game.Main_Win32_Final_R34.exe -serverInstancePath "#1/" -feslhost 1.1.1.1:18321 -theaterhost 1.1.1.1:18326
```
---
```Win32Game.cfg
-server -dedicated -mapPack2Enabled 1 -displayAsserts 0 -displayErrors 0 -crashDumpAsserts 1 -heartBeatInterval 20000 -runMultipleGame -serverAdminLogs 0
```
