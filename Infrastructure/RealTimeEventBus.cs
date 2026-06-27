using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CareSphere.Infrastructure
{
    public class RealTimeEventBus
    {
        // Dictionary to hold event subscribers based on topic.
        // The value is a ConcurrentBag of delegates (Func<string, Task> where string is a payload).
        private readonly ConcurrentDictionary<string, ConcurrentBag<Func<string, Task>>> _subscribers = new();

        /// <summary>
        /// Subscribes to a specific topic.
        /// </summary>
        /// <param name="topic">The event topic (e.g., "QueueUpdated")</param>
        /// <param name="handler">The async handler to invoke</param>
        public void Subscribe(string topic, Func<string, Task> handler)
        {
            var handlers = _subscribers.GetOrAdd(topic, _ => new ConcurrentBag<Func<string, Task>>());
            handlers.Add(handler);
        }

        /// <summary>
        /// Publishes an event to all subscribers of a topic.
        /// </summary>
        /// <param name="topic">The event topic</param>
        /// <param name="payload">Optional data payload (e.g., DoctorId or PatientId)</param>
        public async Task PublishAsync(string topic, string payload = "")
        {
            if (_subscribers.TryGetValue(topic, out var handlers))
            {
                var tasks = new List<Task>();
                foreach (var handler in handlers)
                {
                    // Fire and forget, but catch exceptions to avoid crashing the publisher
                    tasks.Add(SafeInvoke(handler, payload));
                }
                
                // Do not await tasks to block the publisher, let them run in parallel
                _ = Task.WhenAll(tasks);
            }
            await Task.CompletedTask;
        }

        private async Task SafeInvoke(Func<string, Task> handler, string payload)
        {
            try
            {
                await handler(payload);
            }
            catch (Exception ex)
            {
                // In a production app, log this exception
                Console.WriteLine($"RealTimeEventBus error invoking handler: {ex.Message}");
            }
        }
    }
}
