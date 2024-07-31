package routers

import (
	"EventsService/controllers"

	"github.com/gin-gonic/gin"
)

func EventsRouter() *gin.Engine {
	router := gin.Default()

	eventsGroup := router.Group(EventsBaseRoute)
	{
		eventsGroup.POST("/Arrange", eventsController.ArrangeEvent)
		eventsGroup.DELETE("/Cancel/:id", eventsController.CancelEvent)
		eventsGroup.GET("/GetAll", eventsController.GetAllEvents)
		eventsGroup.POST("/:eventId/Invite/:studentId", eventsController.InviteStudent)
	}

	return router
}
