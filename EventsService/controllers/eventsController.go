package eventsController

import (
    "EventsService/models"
    "context"
    "fmt"
    "log"
    "net/http"
    "os"
    "strconv"
    "time"

    "github.com/gin-gonic/gin"
    "github.com/joho/godotenv"
    "go.mongodb.org/mongo-driver/bson"
    "go.mongodb.org/mongo-driver/bson/primitive"
    "go.mongodb.org/mongo-driver/mongo"
    "go.mongodb.org/mongo-driver/mongo/options"
)

var eventsCollection *mongo.Collection

func init() {
    err := godotenv.Load(".env")
    if err != nil {
        log.Fatalf("Error loading .env file")
    }

    connectionString := os.Getenv("MONGODB_CONNECTION_STRING")
	fmt.Println("Connection string: " + connectionString)
	// connectionString := "mongodb://host.docker.internal:27017" // this is so the docker container can access host db

    clientOptions := options.Client().ApplyURI(connectionString)

    client, err := mongo.Connect(context.TODO(), clientOptions)
    if err != nil {
        log.Fatal(err)
    }

    err = client.Ping(context.TODO(), nil)
    if err != nil {
        log.Fatal(err)
    }

    fmt.Println("Mongodb connection success")

    dbName := os.Getenv("DB_NAME")
    colName := "events"

    eventsCollection = client.Database(dbName).Collection(colName)

    fmt.Println("Collection instance is ready")
}

func ArrangeEvent(c *gin.Context) {
	var ctx, cancel = context.WithCancel(context.Background())
	defer cancel()

	var newEvent models.Event
	if err := c.BindJSON(&newEvent); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	newEvent.ID = primitive.NewObjectID()
	newEvent.CreatedAt = primitive.NewDateTimeFromTime(time.Now())

	_, err := eventsCollection.InsertOne(ctx, newEvent)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to create event"})
		return
	}

	c.JSON(http.StatusCreated, newEvent)
}

func CancelEvent(c *gin.Context) {
	var ctx, cancel = context.WithCancel(context.Background())
	defer cancel()

	id := c.Param("id")
	objID, err := primitive.ObjectIDFromHex(id)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid ID"})
		return
	}

	_, err = eventsCollection.DeleteOne(ctx, bson.M{"_id": objID})
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to cancel event"})
		return
	}

	c.JSON(http.StatusOK, gin.H{"message": "Event with ID " + id + " has been cancelled"})
}

func GetAllEvents(c *gin.Context) {
	var ctx, cancel = context.WithCancel(context.Background())
	defer cancel()

	cursor, err := eventsCollection.Find(ctx, bson.M{})
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to fetch events"})
		return
	}
	defer cursor.Close(ctx)

	var events []models.Event
	if err = cursor.All(ctx, &events); err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to parse events"})
		return
	}

	c.JSON(http.StatusOK, events)
}
func InviteStudent(c *gin.Context) {
	log.Printf("Attempting to invite student: %v", c)

	var ctx, cancel = context.WithCancel(context.Background())
	defer cancel()

	eventId := c.Param("eventId")
	studentId := c.Param("studentId")

	objID, err := primitive.ObjectIDFromHex(eventId)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid event ID"})
		return
	}

	studentID, err := strconv.Atoi(studentId)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid student ID"})
		return
	}

	update := bson.M{"$push": bson.M{"studentIds": studentID}}
	_, err = eventsCollection.UpdateOne(ctx, bson.M{"_id": objID}, update)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to invite student"})
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"message": "Student with ID " + studentId + " has been invited to event with ID " + eventId,
	})
}
