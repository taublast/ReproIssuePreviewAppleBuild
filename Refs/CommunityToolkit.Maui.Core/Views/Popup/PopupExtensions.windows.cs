using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using CommunityToolkit.Maui.Core.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Primitives;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media;

namespace CommunityToolkit.Maui.Core.Views;

/// <summary>
/// Extension class where Helper methods for Popup lives.
/// </summary>
public static class PopupExtensions
{

	/// <summary>
	/// Method to update the <see cref="Maui.Core.IPopup.BackgroundColor"/>
	/// </summary>
	/// <param name="mauiPopup">An instance of <see cref="Popup"/>.</param>
	/// <param name="popup">An instance of <see cref="Maui.Core.IPopup"/>.</param>
	public static void SetBackgroundColor(this MauiPopup mauiPopup, IPopup popup)
	{
		if (mauiPopup.Overlay != null)
		{
			mauiPopup.Overlay.Background = new SolidColorBrush(popup.BackgroundColor.ToWindowsColor());
		}
	}

	/// <summary>
	/// Method to update the <see cref="Maui.Core.IPopup.Color"/> based on the <see cref="Maui.Core.IPopup.Color"/>.
	/// </summary>
	/// <param name="mauiPopup">An instance of <see cref="Popup"/>.</param>
	/// <param name="popup">An instance of <see cref="Maui.Core.IPopup"/>.</param>
	public static void SetColor(this MauiPopup mauiPopup, IPopup popup)
	{


		var color = popup.Color ?? Colors.Transparent;
		if (mauiPopup.PopupView.Child is Panel panel)
		{
			panel.Background = color.ToPlatform();
		}
	}

	/// <summary>
	/// Method to update the popup anchor based on the <see cref="Maui.Core.IPopup.Anchor"/>.
	/// </summary>
	/// <param name="mauiPopup">An instance of <see cref="Popup"/>.</param>
	/// <param name="popup">An instance of <see cref="IPopup"/>.</param>
	/// <param name="mauiContext">An instance of <see cref="IMauiContext"/>.</param>
	public static void SetAnchor(this MauiPopup mauiPopup, IPopup popup, IMauiContext? mauiContext)
	{
		ArgumentNullException.ThrowIfNull(mauiContext);
		mauiPopup.PopupView.PlacementTarget = popup.Anchor?.ToPlatform(mauiContext);
	}

	/// <summary>
	/// Method to update the popup size based on the <see cref="Maui.Core.IPopup.Size"/>.
	/// </summary>
	/// <param name="mauiPopup">An instance of <see cref="Popup"/>.</param>
	/// <param name="popup">An instance of <see cref="IPopup"/>.</param>
	/// <param name="mauiContext">An instance of <see cref="IMauiContext"/>.</param>
	public static void SetSize(this MauiPopup mauiPopup, IPopup popup, IMauiContext? mauiContext)
	{
		ArgumentNullException.ThrowIfNull(mauiContext);
		ArgumentNullException.ThrowIfNull(popup.Content);

		const double defaultBorderThickness = 0;
		const double defaultSize = 600;

		var currentSize = new Size { Width = defaultSize, Height = defaultSize / 2 };

		var popupParent = mauiContext.GetPlatformWindow();
		var fullBounds = popupParent.Bounds;

		var popupParentFrame = fullBounds;
		if (!popup.IgnoreSafeArea)
		{
			popupParentFrame = GetSafeArea(mauiContext);
		}

		if (popup.Size.IsZero)
		{
			if (double.IsNaN(popup.Content.Width) || (double.IsNaN(popup.Content.Height)))
			{
				currentSize = popup.Content.Measure(double.IsNaN(popup.Content.Width) ? popupParentFrame.Width : popup.Content.Width,
					double.IsNaN(popup.Content.Height) ? popupParentFrame.Height : popup.Content.Height);

				if (double.IsNaN(popup.Content.Width))
				{
					currentSize.Width = popup.HorizontalOptions == LayoutAlignment.Fill ? popupParentFrame.Width : currentSize.Width;
				}
				if (double.IsNaN(popup.Content.Height))
				{
					currentSize.Height = popup.VerticalOptions == LayoutAlignment.Fill ? popupParentFrame.Height : currentSize.Height;
				}
			}
			else
			{
				currentSize.Width = popup.Content.Width;
				currentSize.Height = popup.Content.Height;
			}
		}
		else
		{
			currentSize.Width = popup.Size.Width;
			currentSize.Height = popup.Size.Height;
		}

		currentSize.Width = Math.Min(currentSize.Width, popupParentFrame.Width);
		currentSize.Height = Math.Min(currentSize.Height, popupParentFrame.Height);

		mauiPopup.PopupView.Width = currentSize.Width;
		mauiPopup.PopupView.Height = currentSize.Height;
		mauiPopup.PopupView.MinWidth = mauiPopup.PopupView.MaxWidth = currentSize.Width + (defaultBorderThickness * 2);
		mauiPopup.PopupView.MinHeight = mauiPopup.PopupView.MaxHeight = currentSize.Height + (defaultBorderThickness * 2);

		if (mauiPopup.PopupView.Child is FrameworkElement control)
		{
			control.Width = mauiPopup.PopupView.Width;
			control.Height = mauiPopup.PopupView.Height;
		}
	}

	/// <summary>
	///  Method to update the popup layout.
	/// </summary>
	/// <param name="mauiPopup">An instance of <see cref="Popup"/>.</param>
	/// <param name="popup">An instance of <see cref="IPopup"/>.</param>
	/// <param name="mauiContext">An instance of <see cref="IMauiContext"/>.</param>
	public static void SetLayout(this MauiPopup mauiPopup, IPopup popup, IMauiContext? mauiContext)
	{
		ArgumentNullException.ThrowIfNull(mauiContext);
		ArgumentNullException.ThrowIfNull(popup.Content);

		var popupParent = mauiContext.GetPlatformWindow();
		var fullBounds = popupParent.Bounds;

		var popupParentFrame = fullBounds;
		if (!popup.IgnoreSafeArea)
		{
			popupParentFrame = GetSafeArea(mauiContext);
		}

		var contentSize = popup.Content.ToPlatform(mauiContext).DesiredSize;

		var isFlowDirectionRightToLeft = popup.Content?.FlowDirection == Microsoft.Maui.FlowDirection.RightToLeft;
		var horizontalOptionsPositiveNegativeMultiplier = isFlowDirectionRightToLeft ? -1 : 1;

		var verticalOptions = popup.VerticalOptions;
		var horizontalOptions = popup.HorizontalOptions;
		if (popup.Anchor is not null)
		{
			mauiPopup.PopupView.DesiredPlacement = PopupPlacementMode.Top;
		}
		else if (IsTopLeft(verticalOptions, horizontalOptions))
		{
			mauiPopup.PopupView.DesiredPlacement = PopupPlacementMode.TopEdgeAlignedLeft;
			mauiPopup.PopupView.HorizontalOffset = (popupParentFrame.Width - popupParentFrame.Width * horizontalOptionsPositiveNegativeMultiplier) / 2;
			mauiPopup.PopupView.VerticalOffset = popupParentFrame.Top;
		}
		else if (IsTop(verticalOptions, horizontalOptions))
		{
			mauiPopup.PopupView.DesiredPlacement = PopupPlacementMode.Top;
			mauiPopup.PopupView.HorizontalOffset = (popupParentFrame.Width - contentSize.Width * horizontalOptionsPositiveNegativeMultiplier) / 2;
			mauiPopup.PopupView.VerticalOffset = popupParentFrame.Top;
		}
		else if (IsTopRight(verticalOptions, horizontalOptions))
		{
			mauiPopup.PopupView.DesiredPlacement = PopupPlacementMode.TopEdgeAlignedRight;
			mauiPopup.PopupView.HorizontalOffset = (popupParentFrame.Width + popupParentFrame.Width * horizontalOptionsPositiveNegativeMultiplier) / 2 - contentSize.Width * horizontalOptionsPositiveNegativeMultiplier;
			mauiPopup.PopupView.VerticalOffset = popupParentFrame.Top;
		}
		else if (IsRight(verticalOptions, horizontalOptions))
		{
			mauiPopup.PopupView.DesiredPlacement = PopupPlacementMode.Right;
			mauiPopup.PopupView.HorizontalOffset = (popupParentFrame.Width + popupParentFrame.Width * horizontalOptionsPositiveNegativeMultiplier) / 2 - contentSize.Width * horizontalOptionsPositiveNegativeMultiplier;
			mauiPopup.PopupView.VerticalOffset =  (popupParentFrame.Top + popupParentFrame.Height - contentSize.Height) / 2;
		}
		else if (IsBottomRight(verticalOptions, horizontalOptions))
		{
			mauiPopup.PopupView.DesiredPlacement = PopupPlacementMode.BottomEdgeAlignedRight;
			mauiPopup.PopupView.HorizontalOffset = (popupParentFrame.Width + popupParentFrame.Width * horizontalOptionsPositiveNegativeMultiplier) / 2 - contentSize.Width * horizontalOptionsPositiveNegativeMultiplier;
			mauiPopup.PopupView.VerticalOffset = popupParentFrame.Top + popupParentFrame.Height - contentSize.Height;
		}
		else if (IsBottom(verticalOptions, horizontalOptions))
		{
			mauiPopup.PopupView.DesiredPlacement = PopupPlacementMode.Bottom;
			mauiPopup.PopupView.HorizontalOffset = (popupParentFrame.Width - contentSize.Width * horizontalOptionsPositiveNegativeMultiplier) / 2;
			mauiPopup.PopupView.VerticalOffset = popupParentFrame.Top + popupParentFrame.Height - contentSize.Height;
		}
		else if (IsBottomLeft(verticalOptions, horizontalOptions))
		{
			mauiPopup.PopupView.DesiredPlacement = PopupPlacementMode.BottomEdgeAlignedLeft;
			mauiPopup.PopupView.HorizontalOffset = (popupParentFrame.Width - popupParentFrame.Width * horizontalOptionsPositiveNegativeMultiplier) / 2;
			mauiPopup.PopupView.VerticalOffset = popupParentFrame.Top + popupParentFrame.Height - contentSize.Height;
		}
		else if (IsLeft(verticalOptions, horizontalOptions))
		{
			mauiPopup.PopupView.DesiredPlacement = PopupPlacementMode.Left;
			mauiPopup.PopupView.HorizontalOffset = (popupParentFrame.Width - popupParentFrame.Width * horizontalOptionsPositiveNegativeMultiplier) / 2;
			mauiPopup.PopupView.VerticalOffset = (popupParentFrame.Top + popupParentFrame.Height - contentSize.Height) / 2;
		}
		else if (IsCenter(verticalOptions, horizontalOptions))
		{
			mauiPopup.PopupView.DesiredPlacement = PopupPlacementMode.Auto;
			mauiPopup.PopupView.HorizontalOffset = (popupParentFrame.Width - contentSize.Width * horizontalOptionsPositiveNegativeMultiplier) / 2;
			mauiPopup.PopupView.VerticalOffset = (popupParentFrame.Top + popupParentFrame.Height - contentSize.Height) / 2;
		}
		else if (IsFillLeft(verticalOptions, horizontalOptions))
		{
			mauiPopup.PopupView.DesiredPlacement = PopupPlacementMode.Auto;
			mauiPopup.PopupView.HorizontalOffset = (popupParentFrame.Width - popupParentFrame.Width * horizontalOptionsPositiveNegativeMultiplier) / 2;
			mauiPopup.PopupView.VerticalOffset = (popupParentFrame.Top + popupParentFrame.Height - contentSize.Height) / 2;
		}
		else if (IsFillCenter(verticalOptions, horizontalOptions))
		{
			mauiPopup.PopupView.DesiredPlacement = PopupPlacementMode.Auto;
			mauiPopup.PopupView.HorizontalOffset = (popupParentFrame.Width - contentSize.Width * horizontalOptionsPositiveNegativeMultiplier) / 2;
			mauiPopup.PopupView.VerticalOffset = (popupParentFrame.Top + popupParentFrame.Height - contentSize.Height) / 2;
		}
		else if (IsFillRight(verticalOptions, horizontalOptions))
		{
			mauiPopup.PopupView.DesiredPlacement = PopupPlacementMode.Auto;
			mauiPopup.PopupView.HorizontalOffset = (popupParentFrame.Width + popupParentFrame.Width * horizontalOptionsPositiveNegativeMultiplier) / 2 - contentSize.Width * horizontalOptionsPositiveNegativeMultiplier;
			mauiPopup.PopupView.VerticalOffset = (popupParentFrame.Top + popupParentFrame.Height - contentSize.Height) / 2;
		}
		else if (IsTopFill(verticalOptions, horizontalOptions))
		{
			mauiPopup.PopupView.DesiredPlacement = PopupPlacementMode.Auto;
			mauiPopup.PopupView.HorizontalOffset = (popupParentFrame.Width - contentSize.Width * horizontalOptionsPositiveNegativeMultiplier) / 2;
			mauiPopup.PopupView.VerticalOffset = popupParentFrame.Top;
		}
		else if (IsCenterFill(verticalOptions, horizontalOptions))
		{
			mauiPopup.PopupView.DesiredPlacement = PopupPlacementMode.Auto;
			mauiPopup.PopupView.HorizontalOffset = (popupParentFrame.Width - contentSize.Width * horizontalOptionsPositiveNegativeMultiplier) / 2;
			mauiPopup.PopupView.VerticalOffset = (popupParentFrame.Top + popupParentFrame.Height - contentSize.Height) / 2;
		}
		else if (IsBottomFill(verticalOptions, horizontalOptions))
		{
			mauiPopup.PopupView.DesiredPlacement = PopupPlacementMode.Auto;
			mauiPopup.PopupView.HorizontalOffset = (popupParentFrame.Width - contentSize.Width * horizontalOptionsPositiveNegativeMultiplier) / 2;
			mauiPopup.PopupView.VerticalOffset = popupParentFrame.Top + popupParentFrame.Height - contentSize.Height;
		}
		else if (IsFill(verticalOptions, horizontalOptions))
		{
			mauiPopup.PopupView.DesiredPlacement = PopupPlacementMode.Auto;
			mauiPopup.PopupView.HorizontalOffset = (popupParentFrame.Width - contentSize.Width * horizontalOptionsPositiveNegativeMultiplier) / 2;
			mauiPopup.PopupView.VerticalOffset = (popupParentFrame.Top + popupParentFrame.Height - contentSize.Height) / 2;
		}
		else
		{
			mauiPopup.PopupView.DesiredPlacement = PopupPlacementMode.Auto;
			mauiPopup.PopupView.HorizontalOffset = (popupParentFrame.Width - contentSize.Width * horizontalOptionsPositiveNegativeMultiplier) / 2;
			mauiPopup.PopupView.VerticalOffset = (popupParentFrame.Top + popupParentFrame.Height - contentSize.Height) / 2;
		}

		static bool IsTopLeft(LayoutAlignment verticalOptions, LayoutAlignment horizontalOptions) => verticalOptions == LayoutAlignment.Start && horizontalOptions == LayoutAlignment.Start;
		static bool IsTop(LayoutAlignment verticalOptions, LayoutAlignment horizontalOptions) => verticalOptions == LayoutAlignment.Start && horizontalOptions == LayoutAlignment.Center;
		static bool IsTopRight(LayoutAlignment verticalOptions, LayoutAlignment horizontalOptions) => verticalOptions == LayoutAlignment.Start && horizontalOptions == LayoutAlignment.End;
		static bool IsRight(LayoutAlignment verticalOptions, LayoutAlignment horizontalOptions) => verticalOptions == LayoutAlignment.Center && horizontalOptions == LayoutAlignment.End;
		static bool IsBottomRight(LayoutAlignment verticalOptions, LayoutAlignment horizontalOptions) => verticalOptions == LayoutAlignment.End && horizontalOptions == LayoutAlignment.End;
		static bool IsBottom(LayoutAlignment verticalOptions, LayoutAlignment horizontalOptions) => verticalOptions == LayoutAlignment.End && horizontalOptions == LayoutAlignment.Center;
		static bool IsBottomLeft(LayoutAlignment verticalOptions, LayoutAlignment horizontalOptions) => verticalOptions == LayoutAlignment.End && horizontalOptions == LayoutAlignment.Start;
		static bool IsLeft(LayoutAlignment verticalOptions, LayoutAlignment horizontalOptions) => verticalOptions == LayoutAlignment.Center && horizontalOptions == LayoutAlignment.Start;
		static bool IsCenter(LayoutAlignment verticalOptions, LayoutAlignment horizontalOptions) => verticalOptions == LayoutAlignment.Center && horizontalOptions == LayoutAlignment.Center;
		static bool IsFillLeft(LayoutAlignment verticalOptions, LayoutAlignment horizontalOptions) => verticalOptions == LayoutAlignment.Fill && horizontalOptions == LayoutAlignment.Start;
		static bool IsFillCenter(LayoutAlignment verticalOptions, LayoutAlignment horizontalOptions) => verticalOptions == LayoutAlignment.Fill && horizontalOptions == LayoutAlignment.Center;
		static bool IsFillRight(LayoutAlignment verticalOptions, LayoutAlignment horizontalOptions) => verticalOptions == LayoutAlignment.Fill && horizontalOptions == LayoutAlignment.End;
		static bool IsTopFill(LayoutAlignment verticalOptions, LayoutAlignment horizontalOptions) => verticalOptions == LayoutAlignment.Start && horizontalOptions == LayoutAlignment.Fill;
		static bool IsCenterFill(LayoutAlignment verticalOptions, LayoutAlignment horizontalOptions) => verticalOptions == LayoutAlignment.Center && horizontalOptions == LayoutAlignment.Fill;
		static bool IsBottomFill(LayoutAlignment verticalOptions, LayoutAlignment horizontalOptions) => verticalOptions == LayoutAlignment.End && horizontalOptions == LayoutAlignment.Fill;
		static bool IsFill(LayoutAlignment verticalOptions, LayoutAlignment horizontalOptions) => verticalOptions == LayoutAlignment.Fill && horizontalOptions == LayoutAlignment.Fill;
	}

	/// <summary>
	/// Returns offsets taken by stem UI
	/// </summary>
	/// <param name="mauiContext"></param>
	/// <returns></returns>
	static Windows.Foundation.Rect GetSafeArea(IMauiContext mauiContext)
	{
		var platformWindow = mauiContext.GetPlatformWindow();
		var topOffset = 0;
		if (platformWindow.AppWindow.TitleBar != null)
		{
			var scale = platformWindow.AppWindow.Size.Width / platformWindow.Bounds.Width;
			topOffset = (int)(platformWindow.AppWindow.TitleBar.Height / scale);
		}
		return new (platformWindow.Bounds.X, topOffset, platformWindow.Bounds.Width, platformWindow.Bounds.Height - topOffset);
	}


}