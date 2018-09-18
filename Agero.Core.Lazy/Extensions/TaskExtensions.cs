using Agero.Core.Checker;
using System.Threading.Tasks;

namespace Agero.Core.Lazy.Extensions
{
    /// <summary>Task extensions</summary>
    public static class TaskExtensions
    {
        /// <summary>Checks whether task completed and has "Canceled" or "Faulted" state</summary>
        public static bool IsCompletedWithError(this Task task)
        {
            Check.ArgumentIsNull(task, "task");

            if (!task.IsCompleted)
                return false;

            return task.IsCanceled || task.IsFaulted;
        }
    }
}
