using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using StoreLib;
using System.Threading.Tasks;
using Windows.Web;
using StoreLib.Models;
using StoreLib.Services;
using Windows.Storage.Pickers;
using Windows.Web.Http.Headers;
using Windows.Web.Http;
using System.Text.RegularExpressions;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.Foundation.Metadata;
using ExceptionHelper;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Phone_Helper
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StoreFetch : Page
    {



        public StoreFetch()
        {

            this.InitializeComponent();
            MobileRadio.IsChecked = true;
            LoadingRing.IsEnabled = false;
            LoadingRing.IsActive = false;
            LoadingRing.Visibility = Visibility.Collapsed;


        }
        
        public static string packageItem { get; set; }
        public static string FetchedFileName { get; set; }
        //public static string IsMobile { get; set; }
        public static string fetchedName { get; set; }
        //public HttpContentDispositionHeaderValue ContentDisposition { get; set; }



        private async void StoreSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            
            await SearchStoreInit();

        }

        public async Task SearchStoreInit()
        {
            string query = StoreSearchText.Text.ToString();
            try
            {
                
                OutputText.Text = "Searching for " + query;
                LoadingRing.IsEnabled = true;
                LoadingRing.IsActive = true;
                LoadingRing.Visibility = Visibility.Visible;
                if (MobileRadio.IsChecked == true)
                {

                    try
                    {
                        StoreGrid.Children.Clear();
                        StoreOptions storeOptions = new StoreOptions(query, StoreLib.Models.DeviceFamily.Mobile);
                        

                        var packages = await StoreHelper.findPackages(storeOptions);

                        StoreHelper.StoreItemClick = async (sender, args) =>
                        {
                            var packageItem = (PackageInfo)sender;
                            //Do what you want with the package

                            string content = packageItem.fileName + "\n" + "Size: " + packageItem.fileSize.ToFileSize() + "\n\n" + packageItem.fileURL.ToString();
                            string title = "Confirm Download";
                            var downloadCommand = new UICommand("Download", cmd => { InitDownload(packageItem); });
                            var cancelCommand = new UICommand("Cancel", cmd => { return; });

                            var dialog = new MessageDialog(content, title);
                            dialog.Options = MessageDialogOptions.None;
                            dialog.Commands.Add(cancelCommand);
                            dialog.Commands.Add(downloadCommand);

                            dialog.DefaultCommandIndex = 0;
                            dialog.CancelCommandIndex = 0;
                            
                            

                            if (cancelCommand != null)
                            {
                                // Devices with a hardware back button
                                // use the hardware button for Cancel.
                                // for other devices, show a third option

                                var t_hardwareBackButton = "Windows.Phone.UI.Input.HardwareButtons";

                                if (ApiInformation.IsTypePresent(t_hardwareBackButton))
                                {
                                    // disable the default Cancel command index
                                    // so that dialog.ShowAsync() returns null
                                    // in that case

                                    dialog.CancelCommandIndex = UInt32.MaxValue;
                                }
                                else
                                {
                                    dialog.Commands.Add(cancelCommand);
                                    dialog.CancelCommandIndex = (uint)dialog.Commands.Count - 1;
                                }
                            }

                            var command = await dialog.ShowAsync();

                            if (command == null && cancelCommand != null)
                            {
                                // back button was pressed
                                // invoke the UICommand

                                cancelCommand.Invoked(cancelCommand);
                            }

                            else
                            {
                                // handle cancel command
                                return;
                            } 
                           

                        };



                        StoreHelper.AppendResultToGrid(packages, StoreGrid);
                        LoadingRing.IsEnabled = false;
                        LoadingRing.IsActive = false;
                        LoadingRing.Visibility = Visibility.Collapsed;
                        filterResults("arm", true);
                        OutputText.Text = "Results found for " + query;

                        /*foreach(var pItem in packages){
                            var packageURL = pItem.fileURL;
                            var packageLogo = pItem.fileLogo;
                            OutputText.Text += "\n" + "Filename: " + pItem.fileName.ToString() + "\n\n";
                            OutputText.Text += "File URL: " + pItem.fileURL.ToString() + "\n\n";
                            //If you want more options use: 'packageItem' it will contain more details about the package 
                        }*/
                       
                    } 
                    
                    catch (Exception ex)
                    {
                        Exceptions.ThrownExceptionError(ex);
                    }
                    
                }
                else if (DesktopRadio.IsChecked == true)
                {
                    try
                    {
                        StoreGrid.Children.Clear();
                        StoreOptions storeOptions = new StoreOptions(query, StoreLib.Models.DeviceFamily.Desktop);
                        //IncludeFrameworks = true;

                        var packages = await StoreHelper.findPackages(storeOptions);

                        StoreHelper.StoreItemClick += async (sender, args) =>
                        {
                            var packageItem = (PackageInfo)sender;
                            //Do what you want with the package
                            string content = "File: " + packageItem.fileName + "\n" + "Size: " + packageItem.fileSize.ToFileSize() + "\n\n" + packageItem.fileURL.ToString();
                            string title = "Confirm Download";
                            var downloadCommand = new UICommand("Download", cmd => { InitDownload(packageItem); });
                            var cancelCommand = new UICommand("Cancel", cmd => { return; });

                            var dialog = new MessageDialog(content, title);
                            dialog.Options = MessageDialogOptions.None;
                            dialog.Commands.Add(cancelCommand);
                            dialog.Commands.Add(downloadCommand);

                            dialog.DefaultCommandIndex = 0;
                            dialog.CancelCommandIndex = 0;



                            if (cancelCommand != null)
                            {
                                // Devices with a hardware back button
                                // use the hardware button for Cancel.
                                // for other devices, show a third option

                                var t_hardwareBackButton = "Windows.Phone.UI.Input.HardwareButtons";

                                if (ApiInformation.IsTypePresent(t_hardwareBackButton))
                                {
                                    // disable the default Cancel command index
                                    // so that dialog.ShowAsync() returns null
                                    // in that case

                                    dialog.CancelCommandIndex = UInt32.MaxValue;
                                }
                                else
                                {
                                    dialog.Commands.Add(cancelCommand);
                                    dialog.CancelCommandIndex = (uint)dialog.Commands.Count - 1;
                                }
                            }

                            var command = await dialog.ShowAsync();

                            if (command == null && cancelCommand != null)
                            {
                                // back button was pressed
                                // invoke the UICommand

                                cancelCommand.Invoked(cancelCommand);
                            }

                            else
                            {
                                // handle cancel command
                                return;
                            }

                        };
                        StoreHelper.AppendResultToGrid(packages, StoreGrid);
                        
                        LoadingRing.IsEnabled = false;
                        LoadingRing.IsActive = false;
                        LoadingRing.Visibility = Visibility.Collapsed;
                        filterResults("all", true);
                        /*foreach (var pItem in packages)
                        {
                            var packageURL = pItem.fileURL;
                            var packageLogo = pItem.fileLogo;
                            OutputText.Text += pItem.fileName.ToString() + "\n\n";
                            OutputText.Text += pItem.fileURL.ToString() + "\n\n";
                        }*/

                    }
                    catch (Exception ex)
                    {
                        Exceptions.ThrownExceptionError(ex);
                    }
                }
                
                
            }
            catch (Exception ex)
            {

                Exceptions.ThrownExceptionError(ex);

            }
        }

        private async void InitDownload(object sender)
        {
            try
            {
                var packageItem = (PackageInfo)sender;
                DownloadOperation downloadOperation;
                CancellationTokenSource cancellationToken;
                BackgroundDownloader backgroundDownloader = new BackgroundDownloader();
                FolderPicker folderPicker = new FolderPicker();
                folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
                //folderPicker.ViewMode = PickerViewMode.List;
                folderPicker.FileTypeFilter.Add("*");
                StorageFolder folder = await folderPicker.PickSingleFolderAsync();
                if (folder == null)
                {
                    return;
                }
                StorageFile file = await folder.CreateFileAsync(packageItem.fileName, CreationCollisionOption.GenerateUniqueName);
                Uri downloadUrl = new Uri(packageItem.fileURL.ToString());
                downloadOperation = backgroundDownloader.CreateDownload(downloadUrl, file);
                fetchedName = packageItem.fileName;
                var total = (long)downloadOperation.Progress.TotalBytesToReceive;
                var received = (long)downloadOperation.Progress.BytesReceived;
                string NotificationTitle = "Download Started";
                string NotificationContent = "Downloading " + packageItem.fileName + ", Please Wait";
                ShowNotification(NotificationTitle, NotificationContent);

                Progress<DownloadOperation> progress = new Progress<DownloadOperation>(x => ProgressChanged(downloadOperation));
                cancellationToken = new CancellationTokenSource();
                await downloadOperation.StartAsync().AsTask(cancellationToken.Token, progress);
            } catch (Exception ex)
            {
                Exceptions.ThrownExceptionError(ex);
                return;
            }
        }

        private void filterResults(string arch, bool includeFrameworks)
        {
            StoreGrid.Children.Clear();
            var storeFilters = new StoreFilter(arch, includeFrameworks);
            StoreHelper.FilterResults.Invoke(null, storeFilters);
        }

        
        public async void ProgressChanged(DownloadOperation downloadOperation)
        {
            var total = (long)downloadOperation.Progress.TotalBytesToReceive;
            var received = (long)downloadOperation.Progress.BytesReceived;
            int progress = (int)(100 * (double)(received / (double)total));



            switch (downloadOperation.Progress.Status)
            {
                case BackgroundTransferStatus.Running:

                    {
                        OutputText.Text = $"Downloaded {received.ToFileSize()} of { total.ToFileSize()} - {progress}%";
                        if (received.ToFileSize() == total.ToFileSize())
                        {
                            OutputText.Text = "Download Complete";
                            string NotificationTitleComplete = "Success";
                            string NotificationContentComplete = "Downloading " + fetchedName + " Completed";
                            ShowNotification(NotificationTitleComplete, NotificationContentComplete);
                        }
                        break;
                    }

                case BackgroundTransferStatus.Completed:
                    {
                        
                        OutputText.Text = "Download Completed!";
                        break;
                    }

                case BackgroundTransferStatus.PausedByApplication:
                    {
                        OutputText.Text = "Download paused.";
                        break;
                    }
                case BackgroundTransferStatus.PausedCostedNetwork:
                    {
                        OutputText.Text = "Download paused because of metered connection.";
                        break;
                    }
                case BackgroundTransferStatus.PausedNoNetwork:
                    {
                        MessageDialog nonetwork = new MessageDialog("No network detected. Please check your internet connection.");
                        nonetwork.Commands.Add(new UICommand("Close"));
                        await nonetwork.ShowAsync();

                        break;
                    }
                case BackgroundTransferStatus.Error:
                    {
                        MessageDialog dlerror = new MessageDialog("An error occured while downloading.");
                        dlerror.Commands.Add(new UICommand("Close"));
                        await dlerror.ShowAsync();
                        break;
                    }
                case BackgroundTransferStatus.Canceled:
                    {
                        OutputText.Text = "Download canceled.";
                        break;
                        
                    }

            }

            if (progress >= 100)
            {
                downloadOperation = null;
            }
            
        }

        public static void ShowNotification(string title, string stringContent, int expireTime = 15)
        {
            try
            {
                ToastNotifier ToastNotifier = ToastNotificationManager.CreateToastNotifier();
                Windows.Data.Xml.Dom.XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
                Windows.Data.Xml.Dom.XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");
                toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode(title));
                toastNodeList.Item(1).AppendChild(toastXml.CreateTextNode(stringContent));
                IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
                Windows.Data.Xml.Dom.XmlElement audio = toastXml.CreateElement("audio");
                audio.SetAttribute("src", "ms-winsoundevent:Notification.SMS");

                ToastNotification toast = new ToastNotification(toastXml);
                if (expireTime > 0)
                {
                    toast.ExpirationTime = DateTime.Now.AddSeconds(expireTime);
                }
                ToastNotifier.Show(toast);
            }
            catch (System.Exception ex)
            {

            }
        }

        //Get file name using HTTP Response
        public  async Task<string> getFileNameWithResponse(string link)
        {
            try
            {
                var FileName = "File.Appx";
                var cancellationTokenSource = new CancellationTokenSource();
                Uri fileLink = new Uri(link);
                var _client = new Windows.Web.Http.HttpClient();
                var response = await _client.GetAsync(fileLink, Windows.Web.Http.HttpCompletionOption.ResponseHeadersRead).AsTask(cancellationTokenSource.Token);
                if (response.IsSuccessStatusCode)
                {
                    FileName = response.Content.Headers?.ContentDisposition?.FileName;
                    FetchedFileName = response.Content.Headers?.ContentDisposition?.FileName;
                }
                return FileName;
            }
            catch (Exception ex)
            {
                Exceptions.ThrownExceptionError(ex);
                return null;
            }
        }

        
    }
}
