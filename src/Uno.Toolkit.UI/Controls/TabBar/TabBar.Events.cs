﻿using System;
using System.Collections.Generic;
using System.Text;
using Windows.Foundation;

namespace Uno.Toolkit.UI
{
    partial class TabBar
    {
        public event TypedEventHandler<TabBar, TabBarSelectionChangedEventArgs>? SelectionChanged;
    }
}
