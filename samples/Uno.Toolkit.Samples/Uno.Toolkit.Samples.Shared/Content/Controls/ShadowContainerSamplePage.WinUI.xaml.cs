﻿#if IS_WINUI
using Uno.Toolkit.Samples.Entities;
using Uno.Toolkit.UI;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Toolkit.Samples.Entities.Data;

namespace Uno.Toolkit.Samples.Content.Controls
{
	[SamplePage(SampleCategory.Controls, nameof(ShadowContainer), Description = "Add many colored shadows to your controls.", DataType = typeof(TestCollections))]
	public sealed partial class ShadowContainerSamplePage : Page
	{
		public ShadowContainerSamplePage()
		{
			this.InitializeComponent();
		}
	}
}
#endif
