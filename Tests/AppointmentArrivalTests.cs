using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CareSphere.Data;
using CareSphere.Infrastructure;
using CareSphere.Models;
using CareSphere.Modules.Appointments.Services;
using Xunit;

namespace CareSphere.Tests
{
    public class DummyQueueService : CareSphere.Modules.Clinical.Services.IQueueService
    {
        public Task<DoctorQueueEntry> AddToQueueAsync(DoctorQueueEntry entry) => Task.FromResult(entry);
        public Task<List<DoctorQueueEntry>> GetQueueForDoctorAsync(Guid doctorId) => Task.FromResult(new List<DoctorQueueEntry>());
        public Task UpdateStatusAsync(Guid entryId, string newStatus) => Task.CompletedTask;
        public Task<int> CalculateEtaAsync(Guid doctorId) => Task.FromResult(0);
    }

    public class AppointmentArrivalTests
    {
        [Fact]
        public async Task MarkArrivedAsync_ShouldUpdateStatusAndWriteOutboxMessage()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("CareSphere_AppointmentArrivalTests_DB_" + Guid.NewGuid())
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var tenantId = Guid.NewGuid();
            var patientId = Guid.NewGuid();
            var doctorId = Guid.NewGuid();
            var appointmentId = Guid.NewGuid();

            using (var context = new ApplicationDbContext(options, new BypassTenantContext(tenantId)))
            {
                // Seed Doctor
                var doctor = new Doctor
                {
                    Id = doctorId,
                    FirstName = "John",
                    LastName = "Smith",
                    Specialization = "Cardiology",
                    RegistrationNumber = "REG-123",
                    TenantId = tenantId,
                    CreatedAt = DateTime.UtcNow
                };

                // Seed Patient
                var patient = new Patient
                {
                    Id = patientId,
                    Mrn = "MRN-APP-01",
                    FirstName = "Alice",
                    LastName = "Doe",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Gender = "Female",
                    TenantId = tenantId,
                    CreatedAt = DateTime.UtcNow
                };

                // Seed Appointment
                var appointment = new Appointment
                {
                    Id = appointmentId,
                    PatientId = patientId,
                    DoctorId = doctorId,
                    TenantId = tenantId,
                    SlotStart = DateTime.UtcNow.AddHours(1),
                    SlotEnd = DateTime.UtcNow.AddHours(2),
                    AppointmentType = "OPD",
                    Status = "Scheduled",
                    BookedByUserId = "test-user",
                    CreatedAt = DateTime.UtcNow
                };

                context.Doctors.Add(doctor);
                context.Patients.Add(patient);
                context.Appointments.Add(appointment);
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options, new BypassTenantContext(tenantId)))
            {
                var dummyQueue = new DummyQueueService();
                var eventBus = new RealTimeEventBus();
                var appointmentService = new AppointmentService(context, dummyQueue, eventBus);

                // Act
                await appointmentService.MarkArrivedAsync(appointmentId, tenantId);

                // Assert: Verify Status is updated
                var updatedAppt = await context.Appointments.FindAsync(appointmentId);
                Assert.NotNull(updatedAppt);
                Assert.Equal("Arrived", updatedAppt.Status);

                // Assert: Verify ServiceBusOutbox entry is created
                var outboxItem = await context.ServiceBusOutboxes
                    .FirstOrDefaultAsync(o => o.MessageType == "PatientArrived" && o.TenantId == tenantId);
                Assert.NotNull(outboxItem);
                Assert.Equal("Pending", outboxItem.Status);
                Assert.Contains(appointmentId.ToString(), outboxItem.Payload);
                Assert.Contains(patientId.ToString(), outboxItem.Payload);
                Assert.Contains(doctorId.ToString(), outboxItem.Payload);
            }
        }
    }
}
