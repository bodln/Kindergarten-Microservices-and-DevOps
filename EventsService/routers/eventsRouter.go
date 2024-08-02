package routers

import (
    "EventsService/controllers"
    "EventsService/middleware"
    "github.com/gin-gonic/gin"
)

func EventsRouter() *gin.Engine {
    router := gin.Default()

    // Apply JWT middleware to all routes
    router.Use(middleware.JWTMiddleware())

    eventsGroup := router.Group(EventsBaseRoute)
    {
        eventsGroup.POST("/Arrange", middleware.RequireRoles("Nanny"), eventsController.ArrangeEvent)
        eventsGroup.DELETE("/Cancel/:id", middleware.RequireRoles("Nanny"), eventsController.CancelEvent)
        eventsGroup.GET("/GetAll", middleware.RequireRoles("Nanny", "Admin"), eventsController.GetAllEvents)
        eventsGroup.POST("/:eventId/Invite/:studentId", middleware.RequireRoles("Nanny"), eventsController.InviteStudent)
    }

    return router
}
