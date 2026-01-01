using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace AvaloniauiWebView2.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string url = "https://browserleaks.com/ip";

        public IDictionary<string, string> Headers { get; } =
        new Dictionary<string, string>
        {
            ["user-agent"] = "Mozilla/5.0 (iPhone; CPU iPhone OS 18_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/18.5 Mobile/15E148 Safari/604.1",
            ["referer"] = "https://www.google.com/",
        };
    }
}
