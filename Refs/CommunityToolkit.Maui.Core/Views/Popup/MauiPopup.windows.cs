using System.Diagnostics.CodeAnalysis;
using Windows.UI.ViewManagement;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Color = Windows.UI.Color;
using HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;
using CommunityToolkit.Maui.Core.Extensions;

namespace CommunityToolkit.Maui.Core.Views;

/// <summary>
/// The native implementation of Popup control.
/// </summary>
public partial class MauiPopup : Grid
{
	readonly IMauiContext mauiContext;
	bool attached;
	Grid? overlay;

	/// <summary>
	/// The native popup view.
	/// </summary>
	public Popup PopupView { get; protected set; }

	/// <summary>
	/// The native fullscreen overlay
	/// </summary>
	public Grid? Overlay 
	{
		get
		{
			return overlay;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public bool CanBeDismissedByTappingOutside { get; set; }


	partial class BackgroundDimmer : Microsoft.UI.Xaml.Controls.Grid
	{
		public BackgroundDimmer(Action actionTapped)
		{
			PointerPressed += (s, e) =>
			{
				actionTapped?.Invoke();
			};
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="mauiContext"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public MauiPopup(IMauiContext mauiContext)
	{

		this.mauiContext = mauiContext ?? throw new ArgumentNullException(nameof(mauiContext));

		HorizontalAlignment = HorizontalAlignment.Stretch;
		VerticalAlignment = VerticalAlignment.Stretch;

		PopupView = new Popup()
		{
			LightDismissOverlayMode = LightDismissOverlayMode.Off,
			IsLightDismissEnabled = false
		};

		Children.Add(PopupView);
	}

	/// <summary>
	/// Method to initialize the native implementation.
	/// </summary>
	/// <param name="element">An instance of <see cref="IPopup"/>.</param>
	public FrameworkElement? SetElement(IPopup? element)
	{
		if (element == null)
		{
			PopupView.IsOpen = false;
			PopupView.Closed -= OnClosed;

			var window = mauiContext.GetPlatformWindow();
			if (window.Content is Panel rootPanel)
			{
				rootPanel.Children.Remove(this);
			}

			VirtualView = null;

			if (Content is not null)
			{
				Content.SizeChanged -= OnSizeChanged;
				Content = null;
			}
			
			return null;
		}

		VirtualView = element;

		var backgroundColor = VirtualView.BackgroundColor.ToWindowsColor();
		overlay = new BackgroundDimmer(() =>
		{
			if (CanBeDismissedByTappingOutside)
			{
				VirtualView?.OnDismissedByTappingOutsideOfPopup();
			}
		})
		{
			Background = new SolidColorBrush(backgroundColor), 
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch
		};

		//todo animate overlay fade-in
		Children.Insert(0, overlay);

		if (TryCreateContent(VirtualView, out var mauiContent))
		{

			PopupView.Child = mauiContent;
			Content = mauiContent;
			mauiContent.SizeChanged += OnSizeChanged;
			PopupView.Closed += OnClosed;
		}

		return mauiContent;
	}

	/// <summary>
	/// Opens the popup and shows the dimmer.
	/// </summary>
	public void Show()
	{
		if (!attached)
		{
			var window = mauiContext.GetPlatformWindow();
			if (window.Content is Panel rootPanel)
			{
				attached = true;
				rootPanel.Children.Add(this);
			}
		}

		PopupView.XamlRoot = this.XamlRoot;
		PopupView.IsOpen = true;

		_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} cannot be null");
		VirtualView.OnOpened();
	}


	void OnClosed(object? sender, object e)
	{
		if (!PopupView.IsOpen && this.CanBeDismissedByTappingOutside && VirtualView is not null)
		{
			VirtualView.Handler?.Invoke(nameof(IPopup.OnDismissedByTappingOutsideOfPopup));
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public void OnSizeChanged(object? sender, object e)
	{
		if (VirtualView is not null)
		{
			PopupExtensions.SetSize(this, VirtualView, mauiContext);
			PopupExtensions.SetLayout(this, VirtualView, mauiContext);
		}
	}

	/// <summary>
	/// An instance of the <see cref="IPopup"/>.
	/// </summary>
	public IPopup? VirtualView { get; protected set; }

	/// <summary>
	/// 
	/// </summary>
	public FrameworkElement? Content { get; protected set; }

	bool TryCreateContent(in IPopup popup, [NotNullWhen(true)] out FrameworkElement? container)
	{
		container = null;

		if (popup.Content is null)
		{
			return false;
		}

		container = popup.Content.ToPlatform(mauiContext);
		//Children.Add(container);

		return true;
	}


}