namespace Infrastructure.BackgroundTasks

{
    /// <summary>
    /// Defines a contract for background reminder or maintenance tasks.
    /// Each task must implement its own execution logic inside ExecuteAsync.
    /// </summary>
    public interface IReminderTask
    {
        /// <summary>
        /// Executes the task logic using the provided service.
        /// </summary>
        /// <param name="services">Scoped service provider for resolving dependencies.</param>
        Task ExecuteAsync(IServiceProvider services);
    }
}