package main

import (
	"EventsService/Routers"
	"log"
	"net/http"
)

func main() {
	// Create a new router
	router := routers.EventsRouter()

	// Define the port for the server
	port := ":8080" // You can change this to any port you prefer

	// Start the server
	log.Printf("Starting server on port %s", port)
	if err := http.ListenAndServe(port, router); err != nil {
		log.Fatalf("Could not start server: %v", err)
	}
}
