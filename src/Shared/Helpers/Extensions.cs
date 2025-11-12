using System;

namespace WorkerService.Shared.Helpers;

public static class Extensions
{
    public static string ToLogString(this Exception ex)
    {
        return $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
    }
}
