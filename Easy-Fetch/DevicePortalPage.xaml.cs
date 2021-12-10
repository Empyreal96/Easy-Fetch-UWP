using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
//using Microsoft.Tools.WindowsDevicePortal;
using System.Threading.Tasks;
using Windows.System;
using Microsoft.Tools.WindowsDevicePortal;
using System.Runtime.CompilerServices;
using Windows.UI.Popups;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;

namespace Phone_Helper
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DevicePortalPage : Page
    {
        public DevicePortalPage()
        {
            this.InitializeComponent();


            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                devportalweb.NavigationStarting += Devportalweb_NavigationStarting; ;
                devportalweb.LoadCompleted += Devportalweb_LoadCompleted;
                devportalweb.ScriptNotify += Devportalweb_ScriptNotify;
                devportalweb.NavigationFailed += Devportalweb_NavigationFailed;
                devportalweb.Settings.IsJavaScriptEnabled = true;

                Navigate();
            }
            catch (System.Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Devportalweb_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            try
            {
                LoadingPage.Visibility = Visibility.Visible;
            }
            catch(System.Exception ex)
            {

            }
        }

        private void Navigate()
        {
            try
            {
                var fullUrl = urlBox.Text;
                if (!fullUrl.StartsWith("https"))
                {
                    fullUrl = $"https://{fullUrl}";
                }
                urlBox.Text = fullUrl;
                Uri uri = new Uri(fullUrl);
                devportalweb.Navigate(uri);
            }
            catch(System.Exception ex)
            {
                ShowError(ex);
            }
        }

        private async void Devportalweb_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            try
            {
                LoadingPage.Visibility = Visibility.Collapsed;
                if (e.WebErrorStatus.ToString().ToLower().Contains("certificate"))
                {
                    var portal = new DevicePortal(new DefaultDevicePortalConnection(e.Uri.ToString(), "", ""));
                    var certificate = await portal.GetRootDeviceCertificateAsync(true);
                    portal.SetManualCertificate(certificate);
                    devportalweb.Navigate(e.Uri);
                }
                else
                {
                    ShowError(new System.Exception(e.WebErrorStatus.ToString()));
                }
            }catch(System.Exception ex)
            {

            }
        }

        private async void Devportalweb_ScriptNotify(object sender, NotifyEventArgs e)
        {
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var messageDialog = new MessageDialog(e.Value);
                    await messageDialog.ShowAsync();
                });
            }
            catch(System.Exception ex)
            {

            }
        }

        private async void Devportalweb_LoadCompleted(object sender, NavigationEventArgs e)
        {
            try
            {
                LoadingPage.Visibility = Visibility.Collapsed;

                //Alert Dialog
                var windowAlertHandler = "window.alert = function(message){ window.external.notify(message); }";
                var output = await devportalweb.InvokeScriptAsync("eval", new string[] { windowAlertHandler });

                //Confirm Dialog
                //Still no solution to show confirm dialog
                //So By Default we will return 'true' to start the requested process
                var windowConfirmHandler = "window.confirm = function(message){ return true; }";
                output = await devportalweb.InvokeScriptAsync("eval", new string[] { windowConfirmHandler });
            }
            catch (System.Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Navigate();
            }
            catch(System.Exception ex)
            {
                ShowError(ex);
            }
        }



        public static async void ShowError(System.Exception e, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (e != null)
            {
                string ExtraData = "";
                if (memberName.Length > 0 && sourceLineNumber > 0)
                {
                    sourceFilePath = Path.GetFileName(sourceFilePath);
                    ExtraData = $"\nName: {memberName}\nLine: {sourceLineNumber}";
                }
                string DialogMessage = e.Message + ExtraData;
                var messageDialog = new MessageDialog(DialogMessage);
                await messageDialog.ShowAsync();
            }
        }
    }

}
