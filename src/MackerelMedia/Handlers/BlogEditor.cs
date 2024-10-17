#if MACCATALYST || IOS
using DA.UI.Tools;
using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using MobileCoreServices;
using PhotosUI;
using UIKit;

namespace MackerelMedia.Controls;

public class BlogEditor : MauiTextView, IUIEditMenuInteractionDelegate
{
    public override UIMenu? GetEditMenu(UITextRange textRange, UIMenuElement[] suggestedActions)
    {
        UIMenu menu;

        if (!textRange.IsEmpty)
        {
            // Check if selected text has attribute

            var attributedString = new NSMutableAttributedString(this.AttributedText);

            var selectedRange = ConvertUITextRangeToNSRange(textRange);

            var attributes = attributedString.GetAttributes(selectedRange.Location, out var range) ?? new NSDictionary();

            if (attributes.ContainsKey(UIStringAttributeKey.Link))
            {
                menu = UIMenu.Create("Insert", new UIMenuElement[]
                {
                    UIAction.Create("Remove Link", null, "removeLink", handler: (x) => this.RemoveUrlsAsync(x).FireAndForgetSafeAsync())
                });
            }
            else
            {
                menu = UIMenu.Create("Insert", new UIMenuElement[]
                {
                    UIAction.Create("Url", null, "urlInsert", handler: (x) => this.InsertUrlAsync(x).FireAndForgetSafeAsync())
                });
            }
        }
        else
        {
            menu = UIMenu.Create("Insert", new UIMenuElement[]
            {
                UIAction.Create("Image", null, "imageInsert", handler: (x) => this.InsertImageAsync(x).FireAndForgetSafeAsync())
            });
        }
        

        suggestedActions = suggestedActions.Prepend(menu).ToArray();
        return UIMenu.Create(suggestedActions);
    }

    private Task RemoveUrlsAsync(UIAction x)
    {
        var selectedRange = this.SelectedTextRange;
        if (selectedRange is null)
        {
            return Task.CompletedTask;
        }
        var attributedString = new NSMutableAttributedString(this.AttributedText);
        attributedString.RemoveAttribute(UIStringAttributeKey.Link, ConvertUITextRangeToNSRange(selectedRange));
        this.AttributedText = attributedString;

        return Task.CompletedTask;
    }

    private async Task InsertUrlAsync(UIAction x)
    {
        UIAlertController alert = UIAlertController.Create("Insert URL", "Please enter the URL", UIAlertControllerStyle.Alert);
        alert.AddTextField((textField) =>
        {
            textField.Placeholder = "URL";
        });

        alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, (action) =>
        {
            var url = alert.TextFields[0].Text;
            // Verify the URL is valid
            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                return;
            }

            // Remove existing links in the selected range if they exist.
            var selectedRange = this.SelectedTextRange;
            if (selectedRange is null)
            {
                return;
            }

            var attributedString = new NSMutableAttributedString(this.AttributedText);
            attributedString.AddAttribute(UIStringAttributeKey.Link, new NSUrl(url), ConvertUITextRangeToNSRange(selectedRange));
            this.AttributedText = attributedString;
        }));

        alert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

        var viewController = FindViewController(this);
        if (viewController is null)
        {
            return;
        }

        await viewController.PresentViewControllerAsync(alert, true);
    }

    private async Task InsertImageAsync(UIAction x)
    {
        // Get a photo from the photo library
        var photoPicker = new PHPickerViewController(new PHPickerConfiguration
        {
            Filter = PHPickerFilter.ImagesFilter,
            SelectionLimit = 1
        });

        photoPicker.Delegate = new PickerDelegate(this.InsertImageWithUIImage);

        var viewController = FindViewController(this);
        if (viewController is null)
        {
            return;
        }

        await viewController.PresentViewControllerAsync(photoPicker, true);
    }

    NSRange ConvertUITextRangeToNSRange(UITextRange textRange)
    {
        UITextPosition beginning = this.BeginningOfDocument;
        int location = (int)this.GetOffsetFromPosition(beginning, textRange.Start);
        int length = (int)this.GetOffsetFromPosition(textRange.Start, textRange.End);
        return new NSRange(location, length);
    }

    private UIViewController? FindViewController(UIView view)
    {
        UIResponder nextResponder = view.NextResponder;

        if (nextResponder is UIViewController controller)
        {
            return controller;
        }
        else if (nextResponder is UIView nextView)
        {
            return FindViewController(nextView);
        }
        else
        {
            return null;
        }
    }

    private void InsertImageWithUIImage(UIImage image)
    {
        var attachment = new NSTextAttachment();
        attachment.Image = image;
        attachment.Bounds = new CoreGraphics.CGRect(0, 0, 100, 100);
        var attributedString = new NSMutableAttributedString(this.AttributedText);
        attributedString.Append(new NSAttributedString("\n"));
        attributedString.Append(NSAttributedString.FromAttachment(attachment));
        this.AttributedText = attributedString;
    }

    private class PickerDelegate : PHPickerViewControllerDelegate
    {
        private Action<UIImage> action;

        public PickerDelegate(Action<UIImage> action)
        {
            this.action = action;
        }

        public override async void DidFinishPicking(PHPickerViewController picker, PHPickerResult[] results)
        {
            picker.DismissViewController(true, null);

            foreach (var result in results)
            {
                if (result.ItemProvider.CanLoadObject(typeof(UIImage)))
                {
                    var image = await result.ItemProvider.LoadObjectAsync<UIImage>();
                    BeginInvokeOnMainThread(() => action.Invoke(image));
                    await Task.Delay(1000);
                }
            }
        }
    }
}

public class BlogHandler : EditorHandler
{
    protected override MauiTextView CreatePlatformView()
    {
        var platformEditor = new BlogEditor();

        return platformEditor;
    }
}
#endif