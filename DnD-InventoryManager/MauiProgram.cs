using DnD_InventoryManager.Facades;
using DnD_InventoryManager.Mappers;
using DnD_InventoryManager.Services;
using DnD_InventoryManager.ViewModels;
using DnD_InventoryManager.Views;
using Microsoft.Extensions.Logging;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace DnD_InventoryManager;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseBarcodeReader()
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

		builder.Services.AddTransient<CharacterEditPage>();
		builder.Services.AddTransient<CharacterEditViewModel>();

		builder.Services.AddTransient<CharacterDetailPage>();
		builder.Services.AddTransient<CharacterDetailViewModel>();
		
		builder.Services.AddTransient<ItemDetailPage>();
		builder.Services.AddTransient<ItemDetailViewModel>();
		
		builder.Services.AddTransient<ItemEditPage>();
		builder.Services.AddTransient<ItemEditViewModel>();

		builder.Services.AddTransient<DiceRollerPage>();
		builder.Services.AddTransient<DiceRollerViewModel>();

		builder.Services.AddSingleton<CharacterMapper>();
		builder.Services.AddSingleton<ItemMapper>();
		
		builder.Services.AddSingleton<CharacterFacade>();
		builder.Services.AddSingleton<ItemFacade>();

		builder.Services.AddSingleton<DatabaseService>();
		
		builder.Services.AddSingleton<NfcService>();

		builder.Services.AddSingleton<ApiService>();
		
		builder.Services.AddSingleton<QrService>();
		
		builder.Services.AddTransient<QrCodeDisplayPage>();
		builder.Services.AddTransient<QrCodeDisplayViewModel>();
		builder.Services.AddTransient<QrScanPage>();
		builder.Services.AddTransient<QrScanViewModel>();


		
		return builder.Build();
	}
}
