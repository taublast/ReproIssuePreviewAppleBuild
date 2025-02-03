using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Maui.Core.Extensions;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace CommunityToolkit.Maui.Core.Views;

/// <summary>
/// The native implementation of Popup control.
/// </summary>
/// <remarks>
/// Constructor of <see cref="MauiPopup"/>.
/// </remarks>
/// <param name="mauiContext">An instance of <see cref="IMauiContext"/>.</param>
/// <exception cref="ArgumentNullException">If <paramref name="mauiContext"/> is null an exception will be thrown. </exception>
public class MauiPopup(IMauiContext mauiContext) : UIViewController
{

	internal bool CanBeDismissedByTappingInternal;
	readonly IMauiContext mauiContext = mauiContext ?? throw new ArgumentNullException(nameof(mauiContext));
	UIView? overlay;

	/// <summary>
	/// The native fullscreen overlay
	/// </summary>
	public UIView? Overlay
	{
		get
		{
			return overlay;
		}
	}

	/// <summary>
	/// An instance of the <see cref="PageHandler"/> that holds the <see cref="IPopup.Content"/>.
	/// </summary>
	public PageHandler? Control { get; private set; }

	/// <summary>
	/// An instance of the <see cref="IPopup"/>.
	/// </summary>
	public IPopup? VirtualView { get; private set; }

	internal UIViewController? ViewController { get; private set; }

	/// <summary>
	/// Method to update the Popup's size.
	/// </summary>
	/// <param name="size"></param>
	public void SetElementSize(Size size) =>
		Control?.ContainerView?.SizeThatFits(size);

	/// <inheritdoc/>
	public override void ViewDidLayoutSubviews()
	{
		base.ViewDidLayoutSubviews();

		_ = View ?? throw new InvalidOperationException($"{nameof(View)} cannot be null.");
		_ = View.Superview ?? throw new InvalidOperationException($"{nameof(View.Superview)} cannot be null.");

		View.Superview.Layer.CornerRadius = 0.0f;
		View.Superview.Layer.MasksToBounds = false;

		SetElementSize(new Size(View.Bounds.Width, View.Bounds.Height));

		if (VirtualView is not null)
		{
			this.SetSize(VirtualView);
			this.SetLayout(VirtualView);
		}
	}

	/// <inheritdoc/>
	public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
	{
		coordinator.AnimateAlongsideTransition(_ =>
		{
			// Before screen rotate
		}, _ =>
		{
			// After screen rotate
			if (VirtualView is not null)
			{
				this.SetSize(VirtualView);
				this.SetLayout(VirtualView);
			}
		});

		if (View is not null)
		{
			View.Bounds = new CGRect(0, 0, PreferredContentSize.Width, PreferredContentSize.Height);
		}

		base.ViewWillTransitionToSize(toSize, coordinator);
	}

	/// <summary>
	/// Method to initialize the native implementation.
	/// </summary>
	/// <param name="element">An instance of <see cref="IPopup"/>.</param>
	[MemberNotNull(nameof(VirtualView), nameof(ViewController))]
	public void SetElement(IPopup element)
	{
#if MACCATALYST
		if (element.Parent?.Handler is not PageHandler)
		{
			throw new InvalidOperationException($"The {nameof(element.Parent)} must be of type {typeof(PageHandler)}.");
		}
#endif

		VirtualView = element;
		ModalPresentationStyle = UIModalPresentationStyle.OverFullScreen;
		ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve;

		_ = View ?? throw new InvalidOperationException($"{nameof(View)} cannot be null.");
		_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} cannot be null.");

#if MACCATALYST
		var pageHandler = VirtualView.Parent.Handler as PageHandler;
		var rootViewController = pageHandler?.ViewController ?? WindowStateManager.Default.GetCurrentUIViewController() ?? throw new InvalidOperationException($"{nameof(PageHandler.ViewController)} cannot be null.");
#else
		var rootViewController = WindowStateManager.Default.GetCurrentUIViewController() ?? throw new InvalidOperationException($"{nameof(PageHandler.ViewController)} cannot be null.");
#endif

		ViewController ??= rootViewController;
	}

	/// <summary>
	/// Method to clean up the resources of the <see cref="MauiPopup"/>.
	/// </summary>
	public void CleanUp()
	{
		if (VirtualView is null)
		{
			return;
		}

		VirtualView = null;

		if (PresentationController is UIPopoverPresentationController presentationController)
		{
			presentationController.Delegate = null;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="func"></param>
	/// <param name="virtualView"></param>
	/// <returns></returns>
	[MemberNotNull(nameof(Control), nameof(ViewController))]
	public void CreateControl(Func<IPopup, PageHandler> func, in IPopup virtualView)
	{
		Control = func(virtualView);

		_ = View ?? throw new InvalidOperationException($"{nameof(View)} cannot be null.");
		SetView(View, Control);

		_ = ViewController ?? throw new InvalidOperationException($"{nameof(ViewController)} cannot be null.");

		AddToCurrentPageViewController(ViewController);

		if (virtualView.Handler != null)
		{
			overlay = new UIView(this.View.Bounds)
			{
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
				BackgroundColor = virtualView.BackgroundColor.ToPlatform()
			};

			if (virtualView.CanBeDismissedByTappingOutsideOfPopup)
			{
				var tapGesture = new UITapGestureRecognizer(tapEvent =>
				{
					if (CanBeDismissedByTappingInternal)
					{
						DismissViewController(true, null);
						_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} cannot be null.");
						VirtualView.Handler?.Invoke(nameof(IPopup.OnDismissedByTappingOutsideOfPopup));
					}
				})
				{
					CancelsTouchesInView = false
				};
				overlay.AddGestureRecognizer(tapGesture);
			}

			this.View.InsertSubview(overlay, 0);
		}

		this.SetSize(virtualView);
		this.SetLayout(virtualView);
	}

	void SetView(UIView view, IPlatformViewHandler control)
	{
		view.AddSubview(control.ViewController?.View ?? throw new InvalidOperationException($"{nameof(control.ViewController.View)} cannot be null."));
		view.Bounds = new CGRect(0, 0, PreferredContentSize.Width, PreferredContentSize.Height);
		AddChildViewController(control.ViewController);

		view.SafeTopAnchor().ConstraintEqualTo(control.ViewController.View.SafeTopAnchor()).Active = true;
		view.SafeBottomAnchor().ConstraintEqualTo(control.ViewController.View.SafeBottomAnchor()).Active = true;
		view.SafeLeadingAnchor().ConstraintEqualTo(control.ViewController.View.SafeLeadingAnchor()).Active = true;
		view.SafeTrailingAnchor().ConstraintEqualTo(control.ViewController.View.SafeTrailingAnchor()).Active = true;

		if (VirtualView is not null)
		{
			this.SetColor(VirtualView);
		}
	}

	void AddToCurrentPageViewController(UIViewController viewController)
	{
		viewController.PresentViewController(this, true, null);
	}

}