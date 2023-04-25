﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uno.Disposables;
using Uno.Toolkit.RuntimeTests.Extensions;
using Uno.Toolkit.RuntimeTests.Helpers;
using Uno.Toolkit.RuntimeTests.Tests.TestPages;
using Uno.Toolkit.UI;
using Uno.UI.RuntimeTests;
using Windows.System;

#if __IOS__
using UIKit;
#endif

#if IS_WINUI
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;
#else
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
#endif


namespace Uno.Toolkit.RuntimeTests.Tests
{
	[TestClass]
	[RunsOnUIThread]
	internal partial class NavigationBarTests
	{
		[TestMethod]
		[DataRow(MainCommandMode.Back, DisplayName = nameof(MainCommandMode.Back))]
		[DataRow(MainCommandMode.Action, DisplayName = nameof(MainCommandMode.Action))]
		public async Task MainCommand_In_Popup_Without_Page(MainCommandMode mainCommandMode)
		{
			var shouldGoBack = mainCommandMode == MainCommandMode.Back;
			var navigationBar = new NavigationBar { Content = "Title", MainCommandMode = mainCommandMode };
			var popup = new Popup { Width = 100, Height = 100, HorizontalOffset = 100, VerticalOffset = 100, Child = new StackPanel { Children = { navigationBar } } };
			var content = new StackPanel { Children = { popup } };

			try
			{
				await UnitTestUIContentHelperEx.SetContentAndWait(content);

				popup.IsOpen = true;

				await UnitTestsUIContentHelper.WaitForIdle();
				await UnitTestsUIContentHelper.WaitForLoaded(popup);

				Assert.IsTrue(navigationBar.TryPerformMainCommand() == shouldGoBack, "Unexpected result from TryPerformMainCommand");

				await UnitTestsUIContentHelper.WaitForIdle();

				Assert.IsTrue(popup.IsOpen == !shouldGoBack, "Popup is in an incorrect state");
			}
			finally
			{
				popup.IsOpen = false;
			}
		}

		[TestMethod]
		[DataRow(MainCommandMode.Back, DisplayName = nameof(MainCommandMode.Back))]
		[DataRow(MainCommandMode.Action, DisplayName = nameof(MainCommandMode.Action))]
		public async Task MainCommand_In_Popup_With_Page(MainCommandMode mainCommandMode)
		{
			NavigationBar? firstPageNavBar = null;
			var shouldGoBack = mainCommandMode == MainCommandMode.Back;
			var popup = new Popup { Width = 100, Height = 100, HorizontalOffset = 100, VerticalOffset = 100 };
			var content = new Border { Width = 100, Height = 100, Child = popup };
			var frame = new Frame() { Width = 400, Height = 400 };

			popup.Child = frame;
			try
			{
				await UnitTestUIContentHelperEx.SetContentAndWait(content);

				popup.IsOpen = true;

				await UnitTestsUIContentHelper.WaitForIdle();

				frame.Navigate(typeof(NavBarFirstPage));

				await UnitTestsUIContentHelper.WaitForIdle();

				var firstPage = frame.Content as NavBarFirstPage;
				if (firstPage?.FindChild<NavigationBar>() is { } firstNavBar)
				{
					firstPageNavBar = firstNavBar;
					firstNavBar.MainCommandMode = mainCommandMode;
				}

				await UnitTestsUIContentHelper.WaitForLoaded(firstPageNavBar!);

				Assert.IsTrue(firstPageNavBar!.TryPerformMainCommand() == shouldGoBack, "Unexpected result from TryPerformMainCommand");

				await UnitTestsUIContentHelper.WaitForIdle();

				Assert.IsTrue(popup.IsOpen == !shouldGoBack, "Popup is in an incorrect state");
			}
			finally
			{
				popup.IsOpen = false;
			}
		}

		[TestMethod]
		[DataRow(MainCommandMode.Back, DisplayName = nameof(MainCommandMode.Back))]
		[DataRow(MainCommandMode.Action, DisplayName = nameof(MainCommandMode.Action))]
		public async Task MainCommand_In_Popup_With_Page_With_BackStack(MainCommandMode mainCommandMode)
		{
			NavigationBar? firstPageNavBar = null;
			NavigationBar? secondPageNavBar = null;

			var shouldGoBack = mainCommandMode == MainCommandMode.Back;
			var popup = new Popup { Width = 100, Height = 100, HorizontalOffset = 100, VerticalOffset = 100 };
			
			var content = new Border { Width = 100, Height = 100, Child = popup };
			var frame = new Frame() { Width = 400, Height = 400 };

			popup.Child = frame;

			try
			{
				await UnitTestUIContentHelperEx.SetContentAndWait(content);

				popup.IsOpen = true;

				await UnitTestsUIContentHelper.WaitForIdle();

				frame.Navigate(typeof(NavBarFirstPage));

				await UnitTestsUIContentHelper.WaitForIdle();

				var firstPage = frame.Content as NavBarFirstPage;
				if (firstPage?.FindChild<NavigationBar>() is { } firstNavBar)
				{
					firstPageNavBar = firstNavBar;
					firstNavBar.MainCommandMode = mainCommandMode;
				}

				frame.Navigate(typeof(NavBarSecondPage));

				await UnitTestsUIContentHelper.WaitForIdle();

				var secondPage = frame.Content as NavBarSecondPage;
				if (secondPage?.FindChild<NavigationBar>() is { } secondNavBar)
				{
					secondPageNavBar = secondNavBar;
					secondNavBar.MainCommandMode = mainCommandMode;
				}

				await UnitTestsUIContentHelper.WaitForLoaded(secondPageNavBar!);

				//Assert that the back was handled by the NavBar
				Assert.IsTrue(secondPageNavBar!.TryPerformMainCommand() == shouldGoBack, "Unexpected result from TryPerformMainCommand");

				if (mainCommandMode == MainCommandMode.Back)
				{
					await UnitTestsUIContentHelper.WaitForLoaded(firstPageNavBar!);
					Assert.IsTrue(frame.CurrentSourcePageType == typeof(NavBarFirstPage), "Expected to navigate back to NavBarFirstPage");
				}
				else
				{
					await UnitTestsUIContentHelper.WaitForIdle();
					Assert.IsTrue(frame.CurrentSourcePageType == typeof(NavBarSecondPage), "Expected to stay on NavBarSecondPage");
				}

				Assert.IsTrue(popup.IsOpen, "Expected Popup to remain open");

				// Now we try to GoBack again
				if (mainCommandMode == MainCommandMode.Back)
				{
					Assert.IsTrue(firstPageNavBar!.TryPerformMainCommand(), "Expected Back to be handled by NavigationBar");
				}
				else
				{
					Assert.IsFalse(secondPageNavBar!.TryPerformMainCommand(), "Expected Back to not be handled by NavigationBar");
				}
				await UnitTestsUIContentHelper.WaitForIdle();


				if (mainCommandMode == MainCommandMode.Back)
				{
					Assert.IsFalse(popup.IsOpen, "Expected Popup to be closed");
				}
				else
				{
					Assert.IsTrue(frame.CurrentSourcePageType == typeof(NavBarSecondPage), "Expected to stay on NavBarSecondPage");
				}

			}
			finally
			{
				popup.IsOpen = false;
			}
		}

#if __IOS__
		[TestMethod]
		public async Task Can_Set_MainCommand_Label_Or_Content()
		{
			Frame frame = new Frame() { Width = 400, Height = 400 };
			await UnitTestUIContentHelperEx.SetContentAndWait(frame);
			await UnitTestsUIContentHelper.WaitForIdle();

			// FirstPage
			var firstNavBar = await frame.NavigateAndGetNavBar<FirstPage>();
			Assert.IsNull(firstNavBar?.GetNativeNavBar()?.BackItem);

			// LabelTitlePage
			var labelTitleNavBar = await frame.NavigateAndGetNavBar<LabelTitlePage>();
			Assert.AreEqual("Label Title", labelTitleNavBar?.GetNativeNavBar()?.BackItem?.BackButtonTitle);

			// ContentTitlePage 
			var contentTitleNavBar = await frame.NavigateAndGetNavBar<ContentTitlePage>();
			Assert.AreEqual("Content Title", contentTitleNavBar?.GetNativeNavBar()?.BackItem?.BackButtonTitle);	
		}

		[TestMethod]
		public async Task MainCommand_Use_LeftBarButtonItem_When_No_BackStack()
		{
			NavigationBar? firstPageNavBar = null;
			NavigationBar? secondPageNavBar = null;

			var popup = new Popup { Width = 100, Height = 100, HorizontalOffset = 100, VerticalOffset = 100 };
			
			var content = new Border { Width = 100, Height = 100, Child = popup };
			var frame = new Frame() { Width = 400, Height = 400 };

			popup.Child = frame;

			try
			{
				await UnitTestUIContentHelperEx.SetContentAndWait(content);

				popup.IsOpen = true;

				await UnitTestsUIContentHelper.WaitForIdle();

				frame.Navigate(typeof(NavBarFirstPage));

				await UnitTestsUIContentHelper.WaitForIdle();

				var firstPage = frame.Content as NavBarFirstPage;
				firstPageNavBar = firstPage?.FindChild<NavigationBar>();

				var renderedNativeNavItem = firstPageNavBar.GetNativeNavItem();

				Assert.IsNotNull(renderedNativeNavItem?.LeftBarButtonItem);
				
				frame.Navigate(typeof(NavBarSecondPage));

				await UnitTestsUIContentHelper.WaitForIdle();

				var secondPage = frame.Content as NavBarSecondPage;
				secondPageNavBar = secondPage?.FindChild<NavigationBar>();

				renderedNativeNavItem = secondPageNavBar.GetNativeNavItem();

				Assert.IsNull(renderedNativeNavItem?.LeftBarButtonItem);
			}
			finally
			{
				popup.IsOpen = false;
			}
		}

		[TestMethod]
		public async Task NavigationBar_Does_Render()
		{
			var frame = new Frame { Width = 200, Height = 200 }; ;
			await UnitTestUIContentHelperEx.SetContentAndWait(frame);

			frame.Navigate(typeof(NavBarSimplePage));

			await UnitTestsUIContentHelper.WaitForIdle();

			AssertNavigationBar(frame);
		}

		[TestMethod]
		public async Task NavigationBar_Does_Render_Within_AutoLayout()
		{
			var frame = new Frame { Width = 200, Height = 200 };;
			await UnitTestUIContentHelperEx.SetContentAndWait(frame);

			frame.Navigate(typeof(NavBarAutoLayoutPage));

			await UnitTestsUIContentHelper.WaitForIdle();

			AssertNavigationBar(frame);
		}

		[TestMethod]
		public async Task NavigationBar_Renders_BackItem_Within_AutoLayout()
		{
			var frame = new Frame { Width = 600, Height = 200 };;
			await UnitTestUIContentHelperEx.SetContentAndWait(frame);

			var firstNavBar = await frame.NavigateAndGetNavBar<NavBarAutoLayoutPage>();

			await UnitTestsUIContentHelper.WaitForIdle();

			frame.Navigate(typeof(NavBarAutoLayoutPage2));
			await UnitTestsUIContentHelper.WaitForIdle();

			await UnitTestUIContentHelperEx.WaitFor(() => firstNavBar?.GetNativeNavItem()?.BackButtonTitle == "Hello");
		}

		private static void AssertNavigationBar(Frame frame)
		{
			var page = frame.Content as Page;
			var presenter = frame.FindChild<NativeFramePresenter>();
			var navBar = page?.FindChild<NavigationBar>();

			var renderedNativeNavItem = navBar.GetNativeNavItem();
			var renderedNativeNavBar = navBar.GetNativeNavBar();

			Assert.IsNotNull(presenter);
			Assert.IsFalse(presenter!.NavigationController.NavigationBarHidden);

			Assert.AreSame(renderedNativeNavItem, presenter.NavigationController.TopViewController.NavigationItem);
			Assert.AreSame(renderedNativeNavBar, presenter.NavigationController.NavigationBar);
		}

		

		private sealed partial class FirstPage : Page
		{
			public FirstPage()
			{
				Content = new NavigationBar
				{
					Content = "First Page"
				};
			}
		}

		private sealed partial class LabelTitlePage : Page
		{
			public LabelTitlePage()
			{
				Content = new NavigationBar
				{
					MainCommand = new AppBarButton
					{
						Label = "Label Title"
					}
				};
			}
		}
		private sealed partial class ContentTitlePage : Page
		{
			public ContentTitlePage()
			{
				Content = new NavigationBar
				{
					MainCommand = new AppBarButton
					{
						Content = "Content Title"
					}
				};
			}
		}

		private partial class NavBarTestPage : Page
		{
			protected static FrameworkElement? PageContent;
			public static IDisposable SetPageContent(FrameworkElement pageContent)
			{
				PageContent = pageContent;
				return Disposable.Create(() => PageContent = null);
			}

			public NavBarTestPage()
			{
				Content = PageContent;
			}
		}
#endif
	}

#if __IOS__
	public static class NavigationBarTestHelper
	{
		public static UINavigationBar? GetNativeNavBar(this NavigationBar? navBar) => navBar
			?.TryGetRenderer<NavigationBar, NavigationBarRenderer>()
			?.Native;

		public static UINavigationItem? GetNativeNavItem(this NavigationBar? navBar) => navBar
			?.TryGetRenderer<NavigationBar, NavigationBarNavigationItemRenderer>()
			?.Native;

		public static async Task<NavigationBar?> NavigateAndGetNavBar<TPage>(this Frame frame) where TPage : Page
		{
			frame.Navigate(typeof(TPage));
			await UnitTestsUIContentHelper.WaitForIdle();

			var page = frame.Content as TPage;
			await UnitTestsUIContentHelper.WaitForLoaded(page!);
			return page?.FindChild<NavigationBar>();
		}	
	}
#endif
}

