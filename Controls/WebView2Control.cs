using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace AvaloniauiWebView2.Controls
{
    public class WebView2Control : NativeControlHost
    {
        private CoreWebView2Controller? _controller;
        private CoreWebView2? _webView;

        public static readonly StyledProperty<string?> SourceProperty =
            AvaloniaProperty.Register<WebView2Control, string?>(
                nameof(Source));

        public string? Source
        {
            get => GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly StyledProperty<IDictionary<string, string>?> HeadersProperty =
            AvaloniaProperty.Register<WebView2Control, IDictionary<string, string>?>(
                nameof(Headers));

        public IDictionary<string, string>? Headers
        {
            get => GetValue(HeadersProperty);
            set => SetValue(HeadersProperty, value);
        }

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
        {
            var placeholder = base.CreateNativeControlCore(parent);
            _ = InitWebView2Async(placeholder);
            return placeholder;
        }

        private async Task InitWebView2Async(IPlatformHandle parent)
        {
            try
            {
                var options = new CoreWebView2EnvironmentOptions
                {
                    AdditionalBrowserArguments =
                        "--disable-features=msSmartScreenProtection " +
                        "--disable-sync --no-first-run"
                };

                var env = await CoreWebView2Environment.CreateAsync(
                    browserExecutableFolder: null,
                    userDataFolder: "WebView2Data",
                    options: options);

                _controller = await env.CreateCoreWebView2ControllerAsync(parent.Handle);
                _controller.Bounds = new Rectangle(
                    0, 0,
                    (int)Bounds.Width,
                    (int)Bounds.Height);

                _webView = _controller.CoreWebView2;

                ConfigurePrivacy(_webView);
                ConfigureRequestHeaders(_webView);
                BindSource();

                if (!string.IsNullOrEmpty(Source))
                    _webView.Navigate(Source);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebView2 init failed: {ex}");
            }
        }

        private void BindSource()
        {
            this.GetObservable(SourceProperty)
                .Subscribe(url =>
                {
                    if (!string.IsNullOrWhiteSpace(url))
                        _webView?.Navigate(url);
                });
        }

        private void ConfigureRequestHeaders(CoreWebView2 webView)
        {
            webView.AddWebResourceRequestedFilter(
                "*",
                CoreWebView2WebResourceContext.All);

            webView.WebResourceRequested += (_, e) =>
            {
                if (Headers == null)
                    return;

                foreach (var header in Headers)
                {
                    e.Request.Headers.SetHeader(header.Key, header.Value);
                }
            };
        }

        private static void ConfigurePrivacy(CoreWebView2 webView)
        {
            var settings = webView.Settings;
            settings.IsReputationCheckingRequired = false;
            settings.AreDevToolsEnabled = true;
            settings.IsScriptEnabled = true;
            settings.IsStatusBarEnabled = false;
            settings.IsZoomControlEnabled = true;
        }

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);

            if (_controller != null)
            {
                _controller.Bounds = new Rectangle(
                    0, 0,
                    (int)e.NewSize.Width,
                    (int)e.NewSize.Height);
            }
        }

        protected override void DestroyNativeControlCore(IPlatformHandle control)
        {
            _controller?.Close();
            _controller = null;
            _webView = null;
            base.DestroyNativeControlCore(control);
        }
    }
}
