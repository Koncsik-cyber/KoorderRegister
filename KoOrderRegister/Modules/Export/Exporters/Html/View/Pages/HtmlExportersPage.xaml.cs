
using KoOrderRegister.Modules.Export.Exporters.Html.View.ViewModel;
using KoOrderRegister.Utility;
using KORCore.Utility;

namespace KoOrderRegister.Modules.Export.Html.Pages;

public partial class HtmlExportersPage: ContentPage
{
	private ExportersViewModel _viewModel;
    public HtmlExportersPage(ExportersViewModel exportersViewModel)
	{
        _viewModel = exportersViewModel;
        BindingContext = _viewModel;
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        using (new LowPriorityTaskManager())
        {
            base.OnAppearing();
        }
    }
}