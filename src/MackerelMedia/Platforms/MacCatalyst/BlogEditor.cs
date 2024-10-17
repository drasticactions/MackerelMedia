using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;

namespace MackerelMedia.Controls;

public class BlogEditor : MauiTextView, IUIEditMenuInteractionDelegate
{
}

public class BlogHandler : EditorHandler
{
    protected override MauiTextView CreatePlatformView()
    {
        var platformEditor = new BlogEditor();

#if !MACCATALYST
			var accessoryView = new MauiDoneAccessoryView();
			accessoryView.SetDataContext(this);
			accessoryView.SetDoneClicked(OnDoneClicked);
			platformEditor.InputAccessoryView = accessoryView;
#endif

        return platformEditor;
    }
}