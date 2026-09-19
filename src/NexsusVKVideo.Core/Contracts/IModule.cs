namespace NexsusVKVideo.Core.Contracts;

public interface IModule
{
    string Id { get; }

    IReadOnlyCollection<string> Routes { get; }

    void RegisterServices(IModuleServiceRegistry services);

    Task StartAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);
}

public interface IModuleServiceRegistry
{
    void RegisterSingleton<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService;
}
