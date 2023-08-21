﻿using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Uno.Toolkit.UI;

public class ShadowCollection : ObservableCollection<Shadow>
{
	public bool HasInnerShadow() => this.Any(s => s.IsInner);

	public string ToKey(double width, double height, Windows.UI.Color? contentBackground)
		=> string.Format(CultureInfo.InvariantCulture, $"w{width},h{height}") +
		(contentBackground.HasValue ? $",cb{contentBackground.Value}:" : ":") +
		string.Join("/", this.Select(x => x.ToKey()));
}
