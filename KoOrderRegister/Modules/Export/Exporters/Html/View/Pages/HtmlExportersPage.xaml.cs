
using KoOrderRegister.Modules.Export.Html.ViewModel;

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
}