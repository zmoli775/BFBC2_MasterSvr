@echo off
cls
:: windows_x64生成
:: go env -w CGO_ENABLED=0
:: go env -w GOOS = windows
:: go env -w GOARCH = amd64

:: windows_x64服务端编译
go build -ldflags "-s -w" -o ./Bc2Web.exe main.go


