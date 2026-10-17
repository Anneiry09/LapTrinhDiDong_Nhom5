using System;

namespace AnFit.Api.DTOs.Requests
{
    public class SyncHealthDataRequest
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }
}