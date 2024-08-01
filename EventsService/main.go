package main

import (
	"EventsService/routers"
	"log"
	"net/http"

	"github.com/joho/godotenv"
	"github.com/gin-gonic/gin"
)

func main() {
	err := godotenv.Load()
	if err != nil {
		log.Fatal("Error loading .env file")
	}else{
		log.Println("Successfully loaded .env file")
	}

	// Create a new router
	router := routers.EventsRouter()

	gin.SetMode(gin.ReleaseMode)

	// Define the port for the server
	port := ":8080" // You can change this to any port you prefer

	// Start the server
	log.Printf("Starting server on port %s", port)
	if err := http.ListenAndServe(port, router); err != nil {
		log.Fatalf("Could not start server: %v", err)
	}
}
