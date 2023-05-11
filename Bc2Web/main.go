package main

import (
	cors "Bc2Web/Cors"
	"flag"
	"fmt"
	"github.com/gin-gonic/gin"
	"net/http"
	"strconv"
)

var webport *int

func main() {

	webport = flag.Int("webport", 18392, "Web服务器工作端口")
	// 执行解析
	flag.Parse()

	fmt.Printf("Bc2Web辅助系统工作端口:%d\n", *webport)

	fmt.Printf(`
Bc2Web辅助系统启动命令->
Bc2Web.exe - webport 端口号
`)

	// 切换为生产模式
	gin.SetMode(gin.ReleaseMode)
	// 创建引擎对象
	e := gin.Default()
	// 设置全局跨域访问
	e.Use(cors.Cors())
	// 设置静态文件
	e.StaticFile("./favicon.ico", "./wwwroot/static/favicon.ico")

	e.StaticFile("./easo/editorial/BF/2010/BFBC2/config/PC/game.xml", "./wwwroot/static/game")

	e.StaticFile("./easo/editorial/BF/2010/BFBC2/config/PC/version", "./wwwroot/static/version")
	// 设置静态文件夹
	e.StaticFS("./static", http.Dir("./wwwroot/static"))
	//
	e.Handle("GET", "/", index)

	e.Handle("GET", "/pid/:pid/fileupload/locker.jsp", locker)

	// 设置监听Ip:端口
	if er1 := e.Run(fmt.Sprintf(":%d", *webport)); er1 != nil {
		fmt.Println(er1.Error())
	}
}

func index(c *gin.Context) {
	c.String(http.StatusOK, c.ClientIP())
}

func locker(c *gin.Context) {
	pidstr := c.Param("pid")
	cmd := c.Query("cmd")
	game := c.Query("game")
	site := c.Query("site")
	pname := c.Query("pers")
	pid, err := strconv.Atoi(pidstr)
	if err != nil {
		return
	}
	c.Header("Content-Type", "text/xml")
	if pid == 0 || site != "easo" || cmd != "dir" || game != "/eagames/BFBC2" {
		c.String(http.StatusOK, `<LOCKER error="2"/>`)
	} else {
		c.String(http.StatusOK, fmt.Sprintf(`<?xml version="1.0" encoding="UTF-8"?>
<LOCKER error="0" game="/eagames/bfbc2" maxBytes="2867200" maxFiles="10" numBytes="0" numFiles="0" ownr="%d" pers="%s"/>`, pid, pname))
	}
}