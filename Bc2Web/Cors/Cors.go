package cors

import (
	"github.com/gin-gonic/gin"
	"net/http"
	"time"
)

/* 全局跨域 */
func Cors() gin.HandlerFunc {
	return func(c *gin.Context) {
		method := c.Request.Method
		origin := c.Request.Header.Get("Origin")
		if len(origin) > 0 {
			c.Header("Access-Control-Allow-Origin", origin)
			c.Header("Access-Control-Allow-Credentials", "true")
			c.Header("Access-Control-Allow-Methods", "POST, GET, OPTIONS, PUT, DELETE, UPDATE")
			c.Header("Access-Control-Allow-Headers", "Authorization, Content-Length, Content-Type, X-CSRF-Token, Token,session")
			c.Header("Access-Control-Expose-Headers", "*")
			c.Header("Access-Control-Max-Age", string(12*time.Hour))
		}
		if method == "OPTIONS" {
			c.AbortWithStatus(http.StatusNoContent)
		}
		c.Next()
	}
}
