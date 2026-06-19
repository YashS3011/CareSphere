using Microsoft.AspNetCore.Authorization;

namespace CareSphere.Authorization
{
    /// <summary>
    /// Authorization requirement for queue access.
    /// Allows users with Queue_View, Queue_Manage, or Encounters_View permissions to access the queue.
    /// </summary>
    public class QueueAccessRequirement : IAuthorizationRequirement
    {
    }
}
