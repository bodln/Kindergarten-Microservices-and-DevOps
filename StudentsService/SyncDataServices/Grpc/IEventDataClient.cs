﻿namespace StudentsService.SyncDataServices.Grpc
{
    public interface IEventDataClient
    {
        Task<bool> Participate(string token, int studentId, int eventId);
    }
}
