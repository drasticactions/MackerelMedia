using Microsoft.Extensions.Logging;

namespace MackerelMedia;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
#if MACCATALYST
		// Changes buttons to match iPad behavior.
		// This allows us to keep colors and styles.
		Microsoft.Maui.Handlers.ButtonHandler.Mapper.AppendToMapping("ButtonChange", (handler, view) =>
		{
			handler.PlatformView.PreferredBehavioralStyle = UIKit.UIBehavioralStyle.Pad;
			handler.PlatformView.Layer.CornerRadius = 5;
			handler.PlatformView.ClipsToBounds = true;
		});

		// Adds toolbar to window.
		Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping("WindowChange", (handler, view) =>
		{
			if (handler.PlatformView?.WindowScene?.Titlebar != null && view is Window win)
			{
				var toolbar = new AppKit.NSToolbar();
				toolbar.Delegate = new ToolbarDelegate(win);
				toolbar.DisplayMode = AppKit.NSToolbarDisplayMode.Icon;

				handler.PlatformView.WindowScene.Titlebar.Toolbar = toolbar;
				handler.PlatformView.WindowScene.Titlebar.ToolbarStyle = UIKit.UITitlebarToolbarStyle.Automatic;
				handler.PlatformView.WindowScene.Titlebar.Toolbar.Visible = true;
			}
		});
#endif
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureMauiHandlers(handlers =>
			{
#if MACCATALYST || IOS
                handlers.AddHandler<Editor, Controls.BlogHandler>();
#endif
			})
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
