package pub

import (
	"bytes"
	"encoding/json"
	"fmt"
	"github.com/gin-gonic/gin"
	"io"
	"net/http"
	"time"
)

/*
获取当前时间13位时间戳
*/
func GetTimestamp13() int64 {
	return time.Now().UnixNano() / 1e6
}

/*
获取当前时间10位时间戳
*/
func GetTimestamp() int64 {
	return time.Now().Unix()
}

/*
日志打印
*/
func Xlog(p interface{}) {
	fmt.Println(p)
}

/*
JSON格式响应前端
*/
func Resp(c *gin.Context, code int, msg string, data interface{}) {
	c.JSON(200, map[string]interface{}{
		"code": code,
		"msg":  msg,
		"data": data,
	})
}

/*
发送POST请求
@url：请求地址
@data：post请求提交的数据
@contentType："application/json"、"application/x-www-form-urlencoded"
@content：请求返回的内容
*/
func Post(url string, data interface{}, contentType string) string {
	client := &http.Client{Timeout: 3 * time.Second}
	jsonStr, _ := json.Marshal(data)
	resp, err := client.Post(url, contentType, bytes.NewBuffer(jsonStr))
	if err != nil {
		panic(err)
	}
	defer resp.Body.Close()
	result, _ := io.ReadAll(resp.Body)
	return string(result)
}

/*
发送GET请求
@url：请求地址
@response：请求返回的内容
*/
func Get(url string) string {
	client := &http.Client{Timeout: 3 * time.Second}
	resp, err := client.Get(url)
	if err != nil {
		return ""
	}
	defer resp.Body.Close()
	var buffer [512]byte
	result := bytes.NewBuffer(nil)
	for {
		n, er0 := resp.Body.Read(buffer[0:])
		result.Write(buffer[0:n])
		if er0 != nil && er0 == io.EOF {
			break
		} else if er0 != nil {
			return ""
		}
	}
	return result.String()
}
