# Microservices Architecture

This repository contains three microservices:

1. **StudentsService** - .NET 8
2. **NanniesService** - .NET 8
3. **EventsService** - Go

## Communication

- **NanniesService and StudentsService**:
  - Communicate via RabbitMQ.
  - Endpoint: `api/Nanny/{studentId:int}/Grade/{grade:int}`.

- **StudentsService and EventsService**:
  - Communicate via gRPC.
  - Endpoint: `api/Student/{studentId:int}/Participate/{eventId}`.

## Deployment and Infrastructure

- **Kubernetes**: All services are deployed on Kubernetes.
- **Databases**:
  - `StudentsService` and `NanniesService` share a MySQL server database.
  - `EventsService` uses MongoDB.
- **Authentication**: JWT Bearer authentication is implemented across all services.
- **API Gateway**: Services are connected through an API gateway managed by Ingress NGINX at `http://acme.com`.

This setup ensures secure and efficient communication between services, leveraging modern technologies and best practices in microservices architecture.
