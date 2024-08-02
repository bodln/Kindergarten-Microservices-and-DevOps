package middleware

import (
    "github.com/dgrijalva/jwt-go"
    "github.com/gin-gonic/gin"
    "net/http"
    "os"
    "strings"
)

type CustomClaims struct {
    Roles []string `json:"roles"`
    jwt.StandardClaims
}

func JWTMiddleware() gin.HandlerFunc {
    return func(c *gin.Context) {
        tokenString := c.GetHeader("Authorization")

        if tokenString == "" {
            c.JSON(http.StatusUnauthorized, gin.H{"error": "Missing Authorization Header"})
            c.Abort()
            return
        }

        // Remove "Bearer " from the start of the token string
        tokenString = tokenString[len("Bearer "):]

        token, err := jwt.ParseWithClaims(tokenString, &CustomClaims{}, func(token *jwt.Token) (interface{}, error) {
            // Check the token signing method
            if _, ok := token.Method.(*jwt.SigningMethodHMAC); !ok {
                return nil, http.ErrNotSupported
            }
            // Return the secret key for validating the token
            return []byte(os.Getenv("JWT_KEY")), nil
        })

        if err != nil || !token.Valid {
            c.JSON(http.StatusUnauthorized, gin.H{"error": "Invalid or expired token"})
            c.Abort()
            return
        }

        claims, ok := token.Claims.(*CustomClaims)
        if !ok {
            c.JSON(http.StatusUnauthorized, gin.H{"error": "Invalid claims"})
            c.Abort()
            return
        }

        // Validate issuer and audience
        tokenClaims := token.Claims.(*jwt.StandardClaims)
        if tokenClaims.Issuer != os.Getenv("JWT_ISSUER") {
            c.JSON(http.StatusUnauthorized, gin.H{"error": "Invalid issuer"})
            c.Abort()
            return
        }
        
        validAudience := false
        audiences := strings.Split(os.Getenv("JWT_AUDIENCES"), ",")
        for _, aud := range audiences {
            if tokenClaims.Audience == strings.TrimSpace(aud) {
                validAudience = true
                break
            }
        }
        
        if !validAudience {
            c.JSON(http.StatusUnauthorized, gin.H{"error": "Invalid audience"})
            c.Abort()
            return
        }

        // Attach roles to the context
        c.Set("roles", claims.Roles)

        c.Next()
    }
}
