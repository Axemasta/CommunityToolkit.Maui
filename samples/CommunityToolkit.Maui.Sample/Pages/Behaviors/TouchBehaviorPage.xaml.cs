using System.Diagnostics;
using CommunityToolkit.Maui.Sample.ViewModels.Behaviors;
namespace CommunityToolkit.Maui.Sample.Pages.Behaviors;

public partial class TouchBehaviorPage : BasePage<TouchBehaviorViewModel>
{
	public TouchBehaviorPage(TouchBehaviorViewModel viewModel)
		: base(viewModel)
	{
		InitializeComponent();
		
		if (Application.Current is not null)
		{
			Application.Current.RequestedThemeChanged += (sender, args) =>
			{
				Debug.WriteLine($"TouchBehaviorPage - App Theme changed: {args.RequestedTheme}");
			};
		}
	}
}