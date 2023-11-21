#if HAS_UNO
#define UNO14502_WORKAROUND // https://github.com/unoplatform/uno/issues/14502
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;

namespace Uno.Toolkit.UI.Helpers;

internal interface IResponsiveCallback
{
	void OnSizeChanged(Size size, ResponsiveLayout layout);
}

public partial class ResponsiveLayout : DependencyObject
{
	#region DependencyProperty: Narrowest

	public static DependencyProperty NarrowestProperty { get; } = DependencyProperty.Register(
		nameof(Narrowest),
		typeof(double),
		typeof(ResponsiveLayout),
		new PropertyMetadata(0d));

	public double Narrowest
	{
		get => (double)GetValue(NarrowestProperty);
		set => SetValue(NarrowestProperty, value);
	}

	#endregion
	#region DependencyProperty: Narrow

	public static DependencyProperty NarrowProperty { get; } = DependencyProperty.Register(
		nameof(Narrow),
		typeof(double),
		typeof(ResponsiveLayout),
		new PropertyMetadata(0d));

	public double Narrow
	{
		get => (double)GetValue(NarrowProperty);
		set => SetValue(NarrowProperty, value);
	}

	#endregion
	#region DependencyProperty: Normal

	public static DependencyProperty NormalProperty { get; } = DependencyProperty.Register(
		nameof(Normal),
		typeof(double),
		typeof(ResponsiveLayout),
		new PropertyMetadata(0d));

	public double Normal
	{
		get => (double)GetValue(NormalProperty);
		set => SetValue(NormalProperty, value);
	}

	#endregion
	#region DependencyProperty: Wide

	public static DependencyProperty WideProperty { get; } = DependencyProperty.Register(
		nameof(Wide),
		typeof(double),
		typeof(ResponsiveLayout),
		new PropertyMetadata(0d));

	public double Wide
	{
		get => (double)GetValue(WideProperty);
		set => SetValue(WideProperty, value);
	}

	#endregion
	#region DependencyProperty: Widest

	public static DependencyProperty WidestProperty { get; } = DependencyProperty.Register(
		nameof(Widest),
		typeof(double),
		typeof(ResponsiveLayout),
		new PropertyMetadata(0d));

	public double Widest
	{
		get => (double)GetValue(WidestProperty);
		set => SetValue(WidestProperty, value);
	}

	#endregion

	public static ResponsiveLayout Create(double narrowest, double narrow, double normal, double wide, double widest) => new()
	{
		Narrowest = narrowest,
		Narrow = narrow,
		Normal = normal,
		Wide = wide,
		Widest = widest,
	};
}

internal class ResponsiveHelper
{
	private static readonly Lazy<ResponsiveHelper> _instance = new Lazy<ResponsiveHelper>(() => new ResponsiveHelper());
	private readonly List<WeakReference> _callbacks = new();
#if UNO14502_WORKAROUND
	private List<IResponsiveCallback> _hardCallbackReferences = new();
#endif

	public ResponsiveLayout Layout { get; private set; } = ResponsiveLayout.Create(150, 300, 600, 800, 1080);
	public Size WindowSize { get; private set; } = Size.Empty;

	public static ResponsiveHelper GetForCurrentView() => _instance.Value;

	private ResponsiveHelper() { }

	public void HookupEvent(Window window)
	{
		WindowSize = new Size(window.Bounds.Width, window.Bounds.Height);

		window.SizeChanged += OnWindowSizeChanged;
	}

	private void OnWindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
	{
		WindowSize = e.Size;

		// clean up collected references
		_callbacks.RemoveAll(reference => !reference.IsAlive);

		foreach (var reference in _callbacks.ToArray())
		{
			if (reference.IsAlive && reference.Target is IResponsiveCallback callback)
			{
#if UNO14502_WORKAROUND
				// note: In ResponsiveExtensionsSamplePage, if we are using SamplePageLayout with the template,
				// it seems to keep the controls (_weakTarget) alive, even if we navigate out and back(new page).
				// However, if we remove the SamplePageLayout, and adds the template as children instead,
				// they will be properly collected.

				// we are use a hard reference to keep the markup-extension alive.
				// we need to check if its reference target is still alive. if it is not then it should be removed.
				if (callback is ResponsiveExtension { _weakTarget: { IsAlive: false } })
				{
					_hardCallbackReferences.Remove(callback);
					_callbacks.Remove(reference);

					continue;
				}
#endif
				callback.OnSizeChanged(WindowSize, Layout);
			}
		}
	}

	internal void Register(IResponsiveCallback host)
	{
#if UNO14502_WORKAROUND
		// the workaround is only needed for ResponsiveExtension(MarkupExtension)
		if (host is ResponsiveExtension)
		{
			_hardCallbackReferences.Add(host);
		}
#endif

		var wr = new WeakReference(host);
		_callbacks.Add(wr);
	}
}
