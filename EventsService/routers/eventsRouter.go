package routers

import (
    "EventsService/controllers"
    "EventsService/middleware"
    "github.com/gin-gonic/gin"
)

func EventsRouter() *gin.Engine {
    router := gin.Default()

    router.Use(middleware.JWTMiddleware())

    eventsGroup := router.Group(EventsBaseRoute)
    {
        eventsGroup.POST("/Arrange", middleware.RequireRoles("Nanny"), eventsController.ArrangeEvent)
        eventsGroup.DELETE("/Cancel/:id", middleware.RequireRoles("Nanny"), eventsController.CancelEvent)
        eventsGroup.GET("/GetAll", middleware.RequireRoles("Admin"), eventsController.GetAllEvents)
        eventsGroup.POST("/:eventId/Invite/:studentId", middleware.RequireRoles("Nanny"), eventsController.InviteStudent)
    }

    return router
}
