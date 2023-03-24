﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uno.Toolkit.RuntimeTests.Extensions;
using Uno.Toolkit.RuntimeTests.Helpers;
using Uno.Toolkit.UI;
using Uno.UI.RuntimeTests;
using Windows.UI;
#if IS_WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
#endif
using AutoLayoutControl = Uno.Toolkit.UI.AutoLayout;

namespace Uno.Toolkit.RuntimeTests.Tests;

[TestClass]
[RunsOnUIThread]
internal class AutoLayoutTest
{
	[TestMethod]
	[RequiresFullWindow]
	[DataRow(Orientation.Vertical, 10, 130, 250)]
	[DataRow(Orientation.Horizontal, 10, 70, 130)]
	public async Task When_SpaceBetween_with_spacing(Orientation orientation, double expectedResult1, double expectedResult2, double expectedResult3)
	{
		var SUT = new AutoLayout()
		{
			Orientation = orientation,
			Justify = AutoLayoutJustify.SpaceBetween,
			Padding = new Thickness(10),
			Spacing = 100,
			Width = 190,
			Height = 360,
			Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0))
		};

		var border1 = new Border() { Width = 50, Height = 100, Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255)) };
		var border2 = new Border() { Width = 50, Height = 100, Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255)) };
		var border3 = new Border() { Width = 50, Height = 100, Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255)) };

		AutoLayout.SetCounterAlignment(border1, AutoLayoutAlignment.Start);
		AutoLayout.SetCounterAlignment(border2, AutoLayoutAlignment.Start);
		AutoLayout.SetCounterAlignment(border3, AutoLayoutAlignment.Start);

		SUT.Children.Add(border1);
		SUT.Children.Add(border2);
		SUT.Children.Add(border3);

		await UnitTestUIContentHelperEx.SetContentAndWait(SUT);

		var layoutRect0Actual = LayoutInformation.GetLayoutSlot(SUT.Children[0] as FrameworkElement);
		var layoutRect1Actual = LayoutInformation.GetLayoutSlot(SUT.Children[1] as FrameworkElement);
		var layoutRect2Actual = LayoutInformation.GetLayoutSlot(SUT.Children[2] as FrameworkElement);

		if (orientation is Orientation.Vertical)
		{
			Assert.AreEqual(expectedResult1, layoutRect0Actual.Y);
			Assert.AreEqual(expectedResult2, layoutRect1Actual.Y);
			Assert.AreEqual(expectedResult3, layoutRect2Actual.Y);
		}
		else
		{
			Assert.AreEqual(expectedResult1, layoutRect0Actual.X);
			Assert.AreEqual(expectedResult2, layoutRect1Actual.X);
			Assert.AreEqual(expectedResult3, layoutRect2Actual.X);
		}
	}

	[TestMethod]
	[RequiresFullWindow]
	[DataRow(true, Orientation.Vertical, VerticalAlignment.Bottom, HorizontalAlignment.Left, new[] { 10, 10, 10, 10 }, new[] { 108, 0, 0, 10 }, 10, 298, 110, 12, 185)]
	[DataRow(true, Orientation.Vertical, VerticalAlignment.Top, HorizontalAlignment.Left, new[] { 10, 10, 10, 10 }, new[] { 108, 10, 0, 0 }, 10, 12, 110, 12, 185)]
	[DataRow(true, Orientation.Vertical, VerticalAlignment.Top, HorizontalAlignment.Left, new[] { 10, 10, 10, 10 }, new[] { 108, 10, 0, 0 }, -30, 12, 110, 12, 165)]
	[DataRow(true, Orientation.Horizontal, VerticalAlignment.Top, HorizontalAlignment.Left, new[] { 10, 10, 10, 10 }, new[] { 10, 10, 0, 0 }, 10, 12, 12, 12, 105)]
	[DataRow(true, Orientation.Horizontal, VerticalAlignment.Top, HorizontalAlignment.Left, new[] { 10, 10, 10, 10 }, new[] { 10, 10, 0, 0 }, 10, 12, 12, 12, 105)]
	[DataRow(true, Orientation.Horizontal, VerticalAlignment.Top, HorizontalAlignment.Left, new[] { 10, 10, 10, 10 }, new[] { 10, 10, 0, 0 }, -30, 12, 12, 12, 85)]
	[DataRow(false, Orientation.Vertical, VerticalAlignment.Top, HorizontalAlignment.Left, new[] { 10, 10, 10, 10 }, new[] { 108, 10, 0, 0 }, 10, 12, 110, 138, 248)]
	[DataRow(false, Orientation.Horizontal, VerticalAlignment.Top, HorizontalAlignment.Left, new[] { 10, 10, 10, 10 }, new[] { 108, 10, 0, 0 }, 10, 12, 110, 78, 138)]
	[DataRow(false, Orientation.Vertical, VerticalAlignment.Top, HorizontalAlignment.Left, new[] { 10, 10, 10, 10 }, new[] { 108, 10, 0, 0 }, -20, 12, 110, 168, 248)]
	[DataRow(false, Orientation.Horizontal, VerticalAlignment.Top, HorizontalAlignment.Left, new[] { 10, 10, 10, 10 }, new[] { 108, 10, 0, 0 }, -20, 12, 110, 108, 138)]
	public async Task When_AbsolutePosition_WithPadding(bool isStretch, Orientation orientation, VerticalAlignment vAlign, HorizontalAlignment hAlign, int[] padding, int[] margin, int spacing, double expectedY, double expectedX, double rec1expected, double rec2expected)
	{
		var SUT = new AutoLayout()
		{
			Orientation = orientation,
			PrimaryAxisAlignment = AutoLayoutAlignment.End,
			BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
			BorderThickness = new Thickness(2),
			Padding = new Thickness(padding[0], padding[1], padding[2], padding[3]),
			Spacing = spacing,
			Width = 200,
			Height = 360,
		};
		var border1 = new Border()
		{
			//Height = 100,
			Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255)),
		};

		var border2 = new Border()
		{
			//Height = 100,
			Background = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)),
		};

		var border3 = new Border()
		{
			Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
			Height = 50,
			Width = 50,
			Margin = new Thickness(margin[0], margin[1], margin[2], margin[3]),
			VerticalAlignment = vAlign,
			HorizontalAlignment = hAlign,
		};

		AutoLayout.SetIsIndependentLayout(border3, true);

		if (isStretch)
		{
			AutoLayout.SetPrimaryAlignment(border1, AutoLayoutPrimaryAlignment.Stretch);
			AutoLayout.SetPrimaryAlignment(border2, AutoLayoutPrimaryAlignment.Stretch);
		}
		else
		{
			if (orientation is Orientation.Vertical)
			{
				border1!.Height = 100;
				border2!.Height = 100;
			}
			else
			{
				border1!.Width = 50;
				border2!.Width = 50;
			}
		}

		SUT.Children.Add(border1);
		SUT.Children.Add(border2);
		SUT.Children.Add(border3);

		await UnitTestUIContentHelperEx.SetContentAndWait(SUT);

		var border1Transform = border1.TransformToVisual(SUT).TransformPoint(new Windows.Foundation.Point(0, 0));
		var border2Transform = border2.TransformToVisual(SUT).TransformPoint(new Windows.Foundation.Point(0, 0));
		var border3Transform = border3.TransformToVisual(SUT).TransformPoint(new Windows.Foundation.Point(0, 0));

		if (orientation is Orientation.Vertical)
		{
			Assert.AreEqual(border1Transform!.Y, rec1expected); ;
			Assert.AreEqual(border2Transform!.Y, rec2expected);
		}
		else
		{
			Assert.AreEqual(border1Transform!.X, rec1expected);
			Assert.AreEqual(border2Transform!.X, rec2expected);
		}

		Assert.AreEqual(border3Transform!.Y, expectedY);
		Assert.AreEqual(border3Transform!.X, expectedX);
	}

	[TestMethod]
	[RequiresFullWindow]
	[DataRow(false, Orientation.Vertical, AutoLayoutAlignment.Start, AutoLayoutAlignment.Center, 100, 25)]
	[DataRow(false, Orientation.Vertical, AutoLayoutAlignment.Center, AutoLayoutAlignment.Center, 25, 25)]
	[DataRow(false, Orientation.Vertical, AutoLayoutAlignment.Start, AutoLayoutAlignment.Start, 100, 100)]
	[DataRow(false, Orientation.Vertical, AutoLayoutAlignment.Center, AutoLayoutAlignment.Start, 25, 100)]
	[DataRow(false, Orientation.Horizontal, AutoLayoutAlignment.Center, AutoLayoutAlignment.Start, 100, 25)]
	[DataRow(false, Orientation.Horizontal, AutoLayoutAlignment.Center, AutoLayoutAlignment.Center, 25, 25)]
	[DataRow(false, Orientation.Horizontal, AutoLayoutAlignment.Start, AutoLayoutAlignment.Start, 100, 100)]
	[DataRow(false, Orientation.Horizontal, AutoLayoutAlignment.Start, AutoLayoutAlignment.Center, 25, 100)]
	[DataRow(true, Orientation.Vertical, AutoLayoutAlignment.Start, AutoLayoutAlignment.Center, 100, 25)]
	[DataRow(true, Orientation.Vertical, AutoLayoutAlignment.Center, AutoLayoutAlignment.Center, 100, 25)]
	[DataRow(true, Orientation.Vertical, AutoLayoutAlignment.End, AutoLayoutAlignment.Center, 100, 25)]
	[DataRow(true, Orientation.Vertical, AutoLayoutAlignment.Start, AutoLayoutAlignment.Start, 100, 100)]
	[DataRow(true, Orientation.Vertical, AutoLayoutAlignment.Center, AutoLayoutAlignment.Start, 100, 100)]
	[DataRow(true, Orientation.Vertical, AutoLayoutAlignment.End, AutoLayoutAlignment.Start, 100, 100)]
	[DataRow(true, Orientation.Horizontal, AutoLayoutAlignment.Start, AutoLayoutAlignment.Center, 25, 100)]
	[DataRow(true, Orientation.Horizontal, AutoLayoutAlignment.Center, AutoLayoutAlignment.Center, 25, 100)]
	[DataRow(true, Orientation.Horizontal, AutoLayoutAlignment.End, AutoLayoutAlignment.Center, 25, 100)]
	[DataRow(true, Orientation.Horizontal, AutoLayoutAlignment.Start, AutoLayoutAlignment.Start, 100, 100)]
	[DataRow(true, Orientation.Horizontal, AutoLayoutAlignment.Center, AutoLayoutAlignment.Start, 100, 100)]
	[DataRow(true, Orientation.Horizontal, AutoLayoutAlignment.End, AutoLayoutAlignment.Start, 100, 100)]

	// Issue with TransformToVisual not having the same result in iOS and Android and WinIU uno issue #11774
	//https://github.com/unoplatform/uno/issues/11774
#if !(__IOS__ || __ANDROID__)
	[DataRow(false, Orientation.Vertical, AutoLayoutAlignment.Start, AutoLayoutAlignment.End, 100, -50)]
	[DataRow(false, Orientation.Vertical, AutoLayoutAlignment.Center, AutoLayoutAlignment.End, 25, -50)]
	[DataRow(false, Orientation.Vertical, AutoLayoutAlignment.End, AutoLayoutAlignment.End, -50, -50)]
	[DataRow(false, Orientation.Vertical, AutoLayoutAlignment.End, AutoLayoutAlignment.Center, -50, 25)]
	[DataRow(false, Orientation.Vertical, AutoLayoutAlignment.End, AutoLayoutAlignment.Start, -50, 100)]
	[DataRow(false, Orientation.Horizontal, AutoLayoutAlignment.Center, AutoLayoutAlignment.End, -50, 25)]
	[DataRow(false, Orientation.Horizontal, AutoLayoutAlignment.End, AutoLayoutAlignment.Start, 100, -50)]
	[DataRow(false, Orientation.Horizontal, AutoLayoutAlignment.End, AutoLayoutAlignment.Center, 25, -50)]
	[DataRow(false, Orientation.Horizontal, AutoLayoutAlignment.End, AutoLayoutAlignment.End, -50, -50)]
	[DataRow(false, Orientation.Horizontal, AutoLayoutAlignment.Start, AutoLayoutAlignment.End, -50, 100)]
	[DataRow(true, Orientation.Vertical, AutoLayoutAlignment.Start, AutoLayoutAlignment.End, 100, -50)]
	[DataRow(true, Orientation.Vertical, AutoLayoutAlignment.Center, AutoLayoutAlignment.End, 100, -50)]
	[DataRow(true, Orientation.Vertical, AutoLayoutAlignment.End, AutoLayoutAlignment.End, 100, -50)]
	[DataRow(true, Orientation.Horizontal, AutoLayoutAlignment.Start, AutoLayoutAlignment.End, -50, 100)]
	[DataRow(true, Orientation.Horizontal, AutoLayoutAlignment.Center, AutoLayoutAlignment.End, -50, 100)]
	[DataRow(true, Orientation.Horizontal, AutoLayoutAlignment.End, AutoLayoutAlignment.End, -50, 100)]
#endif

	public async Task When_Padding(bool isStretch, Orientation orientation, AutoLayoutAlignment primaryAxisAlignment, AutoLayoutAlignment counterAlignment, double rec1expected, double rec2expected)
	{
		var SUT = new AutoLayout()
		{
			Orientation = orientation,
			Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
			Padding = new Thickness(100),
			Width = 400,
			Height = 400,
			PrimaryAxisAlignment = primaryAxisAlignment,
		};
		var border1 = new Border()
		{
			Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255)),
			Width = 350,
			Height = 350,
		};

		if (isStretch)
		{
			AutoLayout.SetPrimaryAlignment(border1, AutoLayoutPrimaryAlignment.Stretch);
		}

		AutoLayout.SetCounterAlignment(border1, counterAlignment);


		SUT.Children.Add(border1);

		await UnitTestUIContentHelperEx.SetContentAndWait(SUT);

		var border1Transform = border1.TransformToVisual(SUT).TransformPoint(new Windows.Foundation.Point(0,0));

		Assert.AreEqual(rec1expected, border1Transform!.Y);
		Assert.AreEqual(rec2expected, border1Transform!.X);
	}

	[TestMethod]
	[RequiresFullWindow]
	[DataRow(Orientation.Vertical, 10, 340, 280)]
	[DataRow(Orientation.Horizontal, 10, 240, 100)]
	public async Task When_Space_between_With_AbsolutePosition(Orientation orientation, double expected1, double expected2, double expected3)
	{
		var SUT = new AutoLayout()
		{
			Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
			Padding = new Thickness(10),
			Justify = AutoLayoutJustify.SpaceBetween,
			Width = 300,
			Height = 400,
			Orientation = orientation,
		};

		var border1 = new Border()
		{
			Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255)),
			Width = SUT.Orientation == Orientation.Vertical ? 200 : 50,
			Height = SUT.Orientation == Orientation.Vertical ? 50 : 200,
		};

		var border2 = new Border()
		{
			Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255)),
			Width = SUT.Orientation == Orientation.Vertical ? 200 : 50,
			Height = SUT.Orientation == Orientation.Vertical ? 50 : 200,
		};

		var border3 = new Border()
		{
			Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255)),
			Width = SUT.Orientation == Orientation.Vertical ? 200 : 50,
			Height = SUT.Orientation == Orientation.Vertical ? 50 : 200,
		};

		var border4 = new Border()
		{
			Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255)),
			Width = SUT.Orientation == Orientation.Vertical ? 200 : 50,
			Height = SUT.Orientation == Orientation.Vertical ? 50 : 200,
			Margin = SUT.Orientation == Orientation.Vertical ? new Thickness(10, 280, 0, 0) : new Thickness(100, 10, 0, 0),
			VerticalAlignment = VerticalAlignment.Top,
			HorizontalAlignment = HorizontalAlignment.Left,
		};

		AutoLayout.SetIsIndependentLayout(border4, true);

		SUT.Children.Add(border1);
		SUT.Children.Add(border2);
		SUT.Children.Add(border3);
		SUT.Children.Add(border4);

		await UnitTestUIContentHelperEx.SetContentAndWait(SUT);


		var border1Transform = border1.TransformToVisual(SUT).TransformPoint(new Windows.Foundation.Point(0, 0));
		var border3Transform = border3.TransformToVisual(SUT).TransformPoint(new Windows.Foundation.Point(0, 0));
		var border4Transform = border4.TransformToVisual(SUT).TransformPoint(new Windows.Foundation.Point(0, 0));

		if (orientation == Orientation.Vertical)
		{
			Assert.AreEqual(expected1, border1Transform!.Y);
			Assert.AreEqual(expected2, border3Transform!.Y);
			Assert.AreEqual(expected3, border4Transform!.Y);
		}
		else
		{
			Assert.AreEqual(expected1, border1Transform!.X);
			Assert.AreEqual(expected2, border3Transform!.X);
			Assert.AreEqual(expected3, border4Transform!.X);
		}
	}
}
