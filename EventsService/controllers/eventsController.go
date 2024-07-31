package eventsController

import (
	"EventsService/models" // Correct import path for your models package
	"github.com/gin-gonic/gin"
	"go.mongodb.org/mongo-driver/bson/primitive"
	"net/http"
	"time" // Import time package
)

// ArrangeEvent handles POST requests to arrange an event
func ArrangeEvent(c *gin.Context) {
	var newEvent models.Event // Use the Event type from the models package
	if err := c.BindJSON(&newEvent); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	// Simulate saving the event to MongoDB
	newEvent.ID = primitive.NewObjectID() // Generate a new ObjectID
	newEvent.CreatedAt = primitive.NewDateTimeFromTime(time.Now()) // Set the creation time

	c.JSON(http.StatusCreated, newEvent)
}

// CancelEvent handles DELETE requests to cancel an event
func CancelEvent(c *gin.Context) {
	id := c.Param("id")
	// Simulate event cancellation
	c.JSON(http.StatusOK, gin.H{"message": "Event with ID " + id + " has been cancelled"})
}

// GetAllEvents handles GET requests to fetch all events
func GetAllEvents(c *gin.Context) {
	// Example static data
	events := []models.Event{ // Use the Event type from the models package
		{ID: primitive.NewObjectID(), Name: "Event 1", Description: "Description 1", Date: "2024-07-31", Location: "Location 1", CreatedAt: primitive.NewDateTimeFromTime(time.Now()), StudentIDs: []int{1, 2}},
		{ID: primitive.NewObjectID(), Name: "Event 2", Description: "Description 2", Date: "2024-08-01", Location: "Location 2", CreatedAt: primitive.NewDateTimeFromTime(time.Now()), StudentIDs: []int{3, 4}},
	}

	c.JSON(http.StatusOK, events)
}

// InviteStudent handles POST requests to invite a student to an event
func InviteStudent(c *gin.Context) {
	eventId := c.Param("eventId")
	studentId := c.Param("studentId")
	// Simulate inviting a student to an event
	c.JSON(http.StatusOK, gin.H{
		"message": "Student with ID " + studentId + " has been invited to event with ID " + eventId,
	})
}
