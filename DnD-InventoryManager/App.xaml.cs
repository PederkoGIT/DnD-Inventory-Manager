using DnD_InventoryManager.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DnD_InventoryManager;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;
    public App(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
    
    protected override void OnStart()
    {
        base.OnStart();

        var databaseService = _serviceProvider.GetRequiredService<DatabaseService>();
        databaseService.Init();
    }
}