package main

import (
	"context"
	"fmt" // Add this import for fmt
	//"EventsService/controllers"
	"EventsService/routers"
	"EventsService/proto" // Update the import path to match your project structure
	"log"
	"net"
	"net/http"

	"github.com/joho/godotenv"
	"github.com/gin-gonic/gin"
	"google.golang.org/grpc"
)

type server struct {
	proto.UnimplementedAssessStudentServiceServer
}

func (s *server) AssessStudent(ctx context.Context, req *proto.Student) (*proto.AssessmentResponse, error) {
	approved := req.GetGrade() > 1

	if approved {
		studentID := req.GetStudentId()
		eventID := req.GetEventId()
		token := req.GetToken() 

		url := fmt.Sprintf("/api/Event/%d/Invite/%d", eventID, studentID)

		httpReq, err := http.NewRequest("POST", url, nil)
		if err != nil {
			log.Printf("Failed to create HTTP request: %v", err)
			return &proto.AssessmentResponse{Approved: false}, nil
		}

		httpReq.Header.Set("Authorization", "Bearer "+token)

		client := &http.Client{}
		resp, err := client.Do(httpReq)
		if err != nil {
			log.Printf("Failed to send HTTP request: %v", err)
			return &proto.AssessmentResponse{Approved: false}, nil
		}
		defer resp.Body.Close()

		if resp.StatusCode != http.StatusOK {
			log.Printf("Failed to invite student, received status code: %d", resp.StatusCode)
			return &proto.AssessmentResponse{Approved: false}, nil
		}

		log.Printf("Student with ID %d successfully invited to event with ID %d", studentID, eventID)
	}

	return &proto.AssessmentResponse{Approved: approved}, nil
}


func main() {
	err := godotenv.Load()
	if err != nil {
		log.Fatal("Error loading .env file")
	} else {
		log.Println("Successfully loaded .env file")
	}

	router := routers.EventsRouter()

	gin.SetMode(gin.ReleaseMode)

	port := ":8080"
	go func() {
		log.Printf("Starting HTTP server on port %s", port)
		if err := http.ListenAndServe(port, router); err != nil {
			log.Fatalf("Could not start HTTP server: %v", err)
		}
	}()

	// Start gRPC server
	grpcPort := ":666" // Change this port if necessary
	lis, err := net.Listen("tcp", grpcPort)
	if err != nil {
		log.Fatalf("Failed to listen on %v: %v", grpcPort, err)
	} else {
		log.Printf("Listening on gRPC port: %v", grpcPort)
	}

	grpcServer := grpc.NewServer()
	proto.RegisterAssessStudentServiceServer(grpcServer, &server{})

	log.Printf("Starting gRPC server on port %s", grpcPort)
	if err := grpcServer.Serve(lis); err != nil {
		log.Fatalf("Failed to serve gRPC server: %v", err)
	}
}
