using UIKit;
using Uno.Toolkit.Samples;

namespace Uno.Toolkit.WinUI.Samples
{
	public class EntryPoint
{
		// This is the main entry point of the application.
		static void Main(string[] args)
		{
			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
			UIApplication.Main(args, null, typeof(App));
		}
	}
}
