using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Views;
using Microsoft.Maui.Platform;

namespace CommunityToolkit.Maui.Views;

/// <summary>
/// Extension methods for <see cref="Popup"/>.
/// </summary>
public static partial class PopupExtensions
{
	static void PlatformShowPopup(Popup popup, IMauiContext mauiContext)
	{
		var window = mauiContext.GetPlatformWindow().GetWindow();

		if (window?.Content is not Page parent)
		{
			throw new InvalidOperationException("Window Content cannot be null");
		}

		if (window.Content is Element element)
		{
			element.AddLogicalChild(popup);
			var platform = popup.ToHandler(mauiContext);

			if (platform.PlatformView is MauiPopup native)
			{
				var root = element.Handler?.PlatformView as Microsoft.UI.Xaml.Controls.ContentControl;
				if (root != null)
				{
					if (root.Content is Microsoft.UI.Xaml.UIElement uiElement)
					{
						native.XamlRoot = uiElement.XamlRoot;
					}
				}
			}

			platform?.Invoke(nameof(IPopup.OnOpened));
		}
	}

	static Task<object?> PlatformShowPopupAsync(Popup popup, IMauiContext mauiContext, CancellationToken token)
	{
		PlatformShowPopup(popup, mauiContext);
		return popup.Result.WaitAsync(token);
	}
}