using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using FFUClientCode;
using Windows.UI.Popups;
using Windows.Networking.BackgroundTransfer;
using System.Threading;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using ExceptionHelper;





// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Phone_Helper
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FFUSearch : Page
    {

        public string ffu { get; set; }
        public FFUSearch()
        {
            
            this.InitializeComponent();
            ProgRing.IsActive = false;
            ProgRing.Visibility = Visibility.Collapsed;
            ProgRing.IsEnabled = false;
            //Init ndtk and retrieve product info from DPP
            StorageFolder LocalFolder = ApplicationData.Current.LocalFolder;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;


            //SoReFetch Home
            SoReIntro.Text += "FFU Download Tool" + "\n" + "\n";
            SoReIntro.Text += "Search for FFUs for your Windows Phone!" + "\n";
            SoReIntro.Text += "\n\n" + "• Product Type is REQUIRED"
                            + "\n" + "• Product Code is required"
                            + "\n" + "• Download speeds may be slow-ish";
            
            SoReIntro.Text += "\n" + "• Emergency Files require Product Type only";
        
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void SearchClickBtn(object sender, RoutedEventArgs e)
        {
            string ProductType = ProdTypeInfo.Text;
            string ProductCode = ProdCodeInput.Text;

            if (ProductCode == "")
            {
                //SearchOutput.Text = "Product Type REQUIRED!";
                MessageDialog ProdTypeError = new MessageDialog("Product Code REQUIRED!");
                ProdTypeError.Commands.Add(new UICommand("Close"));
                await ProdTypeError.ShowAsync();
                return;

            }
            if (ProductType == "")
            {
                //SearchOutput.Text = "Product Type REQUIRED!";
                MessageDialog ProdTypeError = new MessageDialog("Product Type REQUIRED!");
                ProdTypeError.Commands.Add(new UICommand("Close"));
                await ProdTypeError.ShowAsync();
                return;

            }
            else
            {

                SearchHeader.Text = "Fill in the info below:";

                
                string OperatorCode = OperatorInfo.Text;
                string foundType = string.Empty;
                //           string enosw = string.Empty;


                SearchOutput.Text = "Searching for FFU.";
                ProgRing.IsEnabled = true;
                ProgRing.IsActive = true;
                ProgRing.Visibility = Visibility.Visible;

                
                    string ffu = await FFUClient.SearchFFU(ProductType, ProductCode, OperatorCode);
                    string[] emergency = await FFUClient.SearchEmergencyFiles(ProductType);
                    SearchOutput.Text = "Found Results: " + "\n" + "\n";
                    ProgRing.IsEnabled = false;
                    ProgRing.IsActive = false;
                    ProgRing.Visibility = Visibility.Collapsed;



                    SearchOutput.Text += "FFU: " + ffu + "\n" + "\n";

                
                //+ emergency;

                //            enosw = LumiaDownloadModel.SearchENOSW(o.ProductType, FirmwareRevision, Revision);

                // ffu = FFUClient.FoundProductType;
                //SearchOutput.Text += ffu; 
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DLButton_Click(object sender, RoutedEventArgs e)
        {
            string ProductType = ProdTypeInfo.Text;
            SearchHeader.Text = "Fill in the info below:";
            string ProductCode = ProdCodeInput.Text;
            if (ProductCode == "")
            {
                //SearchOutput.Text = "Product Type REQUIRED!";
                MessageDialog ProdTypeError = new MessageDialog("Product Code REQUIRED!");
                ProdTypeError.Commands.Add(new UICommand("Close"));
                await ProdTypeError.ShowAsync();
                return;

            }
            if (ProductType == "")
            {
                //SearchOutput.Text = "Product Type REQUIRED!";
                MessageDialog ProdTypeError = new MessageDialog("Product Type REQUIRED!");
                ProdTypeError.Commands.Add(new UICommand("Close"));
                await ProdTypeError.ShowAsync();
                return;
            }
            else
            {


                
                string OperatorCode = OperatorInfo.Text;
                string foundType = string.Empty;
                //           string enosw = string.Empty;

                SearchOutput.Text = "Searching for FFU, If found download will start!";
                ProgRing.IsEnabled = true;
                ProgRing.IsActive = true;
                ProgRing.Visibility = Visibility.Visible;
                try
                {
                    string ffu = await FFUClient.SearchFFU(ProductType, ProductCode, OperatorCode);

                    string filename = Path.GetFileName(new Uri(ffu).AbsolutePath);

                    SearchOutput.Text += "\n" + "\n" + "Filename: " + filename;

                    // Download
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
                        SearchOutput.Text = "No Folder Selected";
                        ProgRing.IsEnabled = false;
                        ProgRing.IsActive = false;
                        ProgRing.Visibility = Visibility.Collapsed;
                        return;
                    }
                    SearchOutput.Text += "\n" + "\n" + "FFU: " + ffu + "\n";

                    StorageFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);
                    Uri downloadUrl = new Uri(ffu);
                    downloadOperation = backgroundDownloader.CreateDownload(downloadUrl, file);
                    var total = (long)downloadOperation.Progress.TotalBytesToReceive;
                    var received = (long)downloadOperation.Progress.BytesReceived;
                    ProgRing.IsEnabled = false;
                    ProgRing.IsActive = false;
                    ProgRing.Visibility = Visibility.Collapsed;
                    string NotificationTitle = "Downloading FFU Started";
                    string NotificationContent = "Please Wait";
                    ShowNotification(NotificationTitle, NotificationContent);

                    Progress<DownloadOperation> progress = new Progress<DownloadOperation>(x => ProgressChanged(downloadOperation));
                    cancellationToken = new CancellationTokenSource();
                    await downloadOperation.StartAsync().AsTask(cancellationToken.Token, progress);
                }

                catch (System.Exception ex)
                {
                    if (ex.Message == "FFU not found")
                    {
                        SearchHeader.Text = "Fill in the info below:";
                        var ThrownException = new MessageDialog(ex.Message + "\n\n" + "Did you enter the Product Code?");
                        ThrownException.Commands.Add(new UICommand("Close"));
                        await ThrownException.ShowAsync();
                    }
                    if (ex.Message == "Object reference not set to an instance of an object.")
                    {
                        //ThrownExceptionError(ex);
                        var ThrownException = new MessageDialog("An Issue Occured. Download Aborted");
                        ThrownException.Commands.Add(new UICommand("Close"));
                        await ThrownException.ShowAsync();
                    }

                }
                //}
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="downloadOperation"></param>
        private async void ProgressChanged(DownloadOperation downloadOperation)
        {
            var total = (long)downloadOperation.Progress.TotalBytesToReceive;
            var received = (long)downloadOperation.Progress.BytesReceived;
            int progress = (int)(100 * (double)(received / (double)total));
            


            switch (downloadOperation.Progress.Status)
            {
                case BackgroundTransferStatus.Running:

                    {
                        SearchHeader.Text = $"Downloaded {received.ToFileSize()} of { total.ToFileSize()} - {progress}%";
                       
                        break;
                    }

                case BackgroundTransferStatus.Completed:
                    {
                        string NotificationTitleComplete = "Downloading FFU Completed";
                        string NotificationContentComplete = "File Downloaded Successfully";
                        ShowNotification(NotificationTitleComplete, NotificationContentComplete);
                        break;
                    }

                case BackgroundTransferStatus.PausedByApplication:
                    {
                        SearchOutput.Text = "Download paused.";
                        SearchHeader.Text = "Fill in the info below:";
                        break;
                    }
                case BackgroundTransferStatus.PausedCostedNetwork:
                    {
                        SearchOutput.Text = "Download paused because of metered connection.";
                        SearchHeader.Text = "Fill in the info below:";
                        break;
                    }
                case BackgroundTransferStatus.PausedNoNetwork:
                    {
                        //SearchOutput.Text = "No network detected. Please check your internet connection.";
                        SearchHeader.Text = "Fill in the info below:";
                        MessageDialog nonetwork = new MessageDialog("No network detected. Please check your internet connection.");
                        nonetwork.Commands.Add(new UICommand("Close"));
                        await nonetwork.ShowAsync();

                        break;
                    }
                case BackgroundTransferStatus.Error:
                    {
                        //SearchOutput.Text = "An error occured while downloading.";
                        SearchHeader.Text = "Fill in the info below:";
                        MessageDialog dlerror = new MessageDialog("An error occured while downloading.");
                        dlerror.Commands.Add(new UICommand("Close"));
                        await dlerror.ShowAsync();
                        break;
                    }
                case BackgroundTransferStatus.Canceled:
                    {
                        SearchHeader.Text = "Fill in the info below:";
                        SearchOutput.Text = "Download canceled.";
                        break;
                    }

            }

            if (progress >= 100)
            {
                downloadOperation = null;
            }
        }

       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
            public static string extractData(string text, string tag)
            {
            var pattern = $"{tag}:(?<{tag}>.*)";
            Match m = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (m.Success)
            {
                if (m.Groups != null && m.Groups.Count > 0)
                {
                    return m.Groups[tag].ToString();
                }
            }
            return "";
        }

       


        /// <summary>
        /// Emergency File search
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void Search2ClickBtn(object sender, RoutedEventArgs e)
        {

            SearchHeader2.Text = "Fill in the info below:";
            string ProductType = ProdTypeInfo2.Text;
           // string ProductCode = ProdCodeInput2.Text;
            //string OperatorCode = OperatorInfo2.Text;
            string foundType = string.Empty;
            
            if (ProductType == "")
            {
                //SearchOutput.Text = "Product Type REQUIRED!";
                MessageDialog ProdTypeError = new MessageDialog("Product Type REQUIRED!");
                ProdTypeError.Commands.Add(new UICommand("Close"));
                await ProdTypeError.ShowAsync();
                return;
            }
            else
            {
                SearchOutput2.Text = "Searching for Emergency Files.";
                ProgRing.IsEnabled = true;
                ProgRing.IsActive = true;
                ProgRing.Visibility = Visibility.Visible;
                try
                {
                    //string ffu = await FFUClient.SearchFFU(ProductType, ProductCode, OperatorCode);
                    string[] emergency = await FFUClient.SearchEmergencyFiles(ProductType);

                    //var text = JsonConvert.SerializeObject(emergency);

                    SearchOutput2.Text = "Found Results: " + "\n" + "\n";
                    string filename1 = Path.GetFileName(new Uri(emergency[0]).AbsolutePath);
                    string filename2 = Path.GetFileName(new Uri(emergency[1]).AbsolutePath);
                    ProgRing.IsEnabled = false;
                    ProgRing.IsActive = false;
                    ProgRing.Visibility = Visibility.Collapsed;
                    SearchOutput2.Text += "Filename: " + "\n" + filename1 + "\n" + filename2 + "\n" + "\n";
                    SearchOutput2.Text += "Emergency: " + "\n" + emergency[0] + "\n" + emergency[1];

                }

                catch (System.Exception ex)
                {

                    Exceptions.ThrownExceptionError(ex);
                }
                //+ emergency;

                //            enosw = LumiaDownloadModel.SearchENOSW(o.ProductType, FirmwareRevision, Revision);

                // ffu = FFUClient.FoundProductType;
                //SearchOutput.Text += ffu; 
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DLButton2_Click(object sender, RoutedEventArgs e)
        {
            SearchHeader2.Text = "Fill in the info below:";
            string ProductType = ProdTypeInfo2.Text;
            //string ProductCode = ProdCodeInput2.Text;
            //string OperatorCode = OperatorInfo2.Text;
            if (ProductType == "")
            {
                //SearchOutput.Text = "Product Type REQUIRED!";
                MessageDialog ProdTypeError = new MessageDialog("Product Type REQUIRED!");
                ProdTypeError.Commands.Add(new UICommand("Close"));
                await ProdTypeError.ShowAsync();
                return;
            }
            else
            {
                string foundType = string.Empty;
                //           string enosw = string.Empty;

                SearchOutput2.Text = "Searching for Emergency Files, If found download will start!";

                try
                {
                    string[] emergency = await FFUClient.SearchEmergencyFiles(ProductType);
                    string filename1 = Path.GetFileName(new Uri(emergency[0]).AbsolutePath);
                    string filename2 = Path.GetFileName(new Uri(emergency[1]).AbsolutePath);
                    SearchOutput2.Text += "\n" + "\n" + "Filename: " + "\n" + filename1 + "\n" + filename2;

                    //

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
                        SearchOutput2.Text = "";
                        ProgRing.IsEnabled = false;
                        ProgRing.IsActive = false;
                        ProgRing.Visibility = Visibility.Collapsed;
                        return;
                    }
                    SearchOutput2.Text += "\n" + "\n" + "Emergency: " + "\n" + emergency[0] + "\n" + "\n" + emergency[1];

                    StorageFile file = await folder.CreateFileAsync(filename1, CreationCollisionOption.GenerateUniqueName);
                    Uri downloadUrl = new Uri(emergency[0]);
                    downloadOperation = backgroundDownloader.CreateDownload(downloadUrl, file);
                    Progress<DownloadOperation> progress = new Progress<DownloadOperation>(x => ProgressChanged(downloadOperation));
                    cancellationToken = new CancellationTokenSource();
                    await downloadOperation.StartAsync().AsTask(cancellationToken.Token, progress);

                    DownloadOperation downloadOperation2;
                    CancellationTokenSource cancellationToken2;
                    BackgroundDownloader backgroundDownloader2 = new BackgroundDownloader();

                    StorageFile file2 = await folder.CreateFileAsync(filename2, CreationCollisionOption.GenerateUniqueName);
                    Uri downloadUrl2 = new Uri(emergency[1]);
                    downloadOperation2 = backgroundDownloader2.CreateDownload(downloadUrl2, file2);
                    Progress<DownloadOperation> progress2 = new Progress<DownloadOperation>(x => ProgressChanged(downloadOperation2));
                    cancellationToken2 = new CancellationTokenSource();
                    await downloadOperation2.StartAsync().AsTask(cancellationToken2.Token, progress2);


                }

                catch (System.Exception ex)
                {
                    if (ex.Message == "Object reference not set to an instance of an object.")
                    {
                        if (ProductType == string.Empty)
                        {
                            //ThrownExceptionError(ex);
                            var ThrownException = new MessageDialog("Make sure you entered the Product Type");
                            ThrownException.Commands.Add(new UICommand("Close"));
                            await ThrownException.ShowAsync();
                        }
                        else
                        {
                            var ThrownException = new MessageDialog("File Browser Closed. Download Aborted");
                            ThrownException.Commands.Add(new UICommand("Close"));
                            await ThrownException.ShowAsync();
                        }
                    }
                    else
                    {
                        SearchOutput2.Text = ex.ToString();
                        Exceptions.ThrownExceptionError(ex);
                    }
                }
            }
        }
       /// <summary>
       /// Notification system for download
       /// </summary>
       /// <param name="title"></param>
       /// <param name="stringContent"></param>
       /// <param name="expireTime"></param>
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
    }
}

