﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Uno.Toolkit.Samples.Entities;
using Uno.Toolkit.Samples.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;

#if IS_WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
#endif

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Uno.Toolkit.Samples.Content.Controls
{
    [SamplePage(SampleCategory.Controls, "TabBar", DataType = typeof(TabBarViewModel))]
    public sealed partial class TabBarSamplePage : Page
    {
        public TabBarSamplePage()
        {
            this.InitializeComponent();
        }
    }

	public class TabBarViewModel : ViewModelBase
	{
        public int Tab1Count { get => GetProperty<int>(); set => SetProperty(value); }
        public int Tab2Count { get => GetProperty<int>(); set => SetProperty(value); }
        public int Tab3Count { get => GetProperty<int>(); set => SetProperty(value); }
        public List<string> Items { get => GetProperty<List<string>>(); set => SetProperty(value); }

        public ICommand Tab1CountCommand => new Command(_ => Tab1Count++);
        public ICommand Tab2CountCommand => new Command(_ => Tab2Count++);
        public ICommand Tab3CountCommand => new Command(_ => Tab3Count++);

		public TabBarViewModel()
		{
            Items = new List<string> { "Tab 1", "Tab 2", "Tab 3" };
		}
	}
}