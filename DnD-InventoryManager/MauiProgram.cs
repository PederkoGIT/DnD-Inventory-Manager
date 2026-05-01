using DnD_InventoryManager.Facades;
using DnD_InventoryManager.Mappers;
using DnD_InventoryManager.Services;
using DnD_InventoryManager.ViewModels;
using DnD_InventoryManager.Views;
using Microsoft.Extensions.Logging;

namespace DnD_InventoryManager;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Services.AddSingleton<MainViewModel>();
		builder.Services.AddSingleton<MainPage>();

		builder.Services.AddTransient<EditCharacterPage>();
		builder.Services.AddTransient<EditCharacterViewModel>();

		builder.Services.AddTransient<CharacterDetailPage>();
		builder.Services.AddTransient<CharacterDetailViewModel>();
		
		builder.Services.AddTransient<EditItemPage>();
		builder.Services.AddTransient<EditItemViewModel>();

		builder.Services.AddTransient<DiceRollerPage>();
		builder.Services.AddTransient<DiceRollerViewModel>();

		builder.Services.AddSingleton<CharacterMapper>();
		builder.Services.AddSingleton<ItemMapper>();
		
		builder.Services.AddSingleton<CharacterFacade>();
		builder.Services.AddSingleton<ItemFacade>();

		builder.Services.AddSingleton<DatabaseService>();
		
		builder.Services.AddSingleton<NfcService>();

		builder.Services.AddSingleton<ItemService>();
		
		return builder.Build();
	}
}
