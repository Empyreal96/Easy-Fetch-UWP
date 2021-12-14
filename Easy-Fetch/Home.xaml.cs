using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Security.Cryptography.Certificates;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static Phone_Helper.MainPage;
using Windows.System;
using Octokit;
using System.Threading;
using ExceptionHelper;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Phone_Helper
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Home : Windows.UI.Xaml.Controls.Page
    {
        public static string CurrentBuildVersion = "1.13.16-prerelease";
        public static string PreviousBuildVersion = "1.13.14-prerelease";
        public static string UploadedFileName = "Easy-Fetch_1.13.17.0_Debug_Test.zip";
        public StorageFolder folder { get; set; }
        public static string UpdateURL { get; set; }
        DownloadOperation downloadOperation;
        CancellationTokenSource cancellationToken;

        BackgroundDownloader backgroundDownloader = new BackgroundDownloader();
        public Home()
        {
            this.InitializeComponent();
            DLUpdate.IsEnabled = true;
            DLUpdate.Visibility = Visibility.Visible;
            //CheckForUpdate();

            HomePage.Text = $"[Version: {CurrentBuildVersion}]\n"; 
            HomePage.Text += "This is in ongoing development, suggestions and feedback are welcome. Features and UI not final" + "\n\n";
            HomePage.Text += "A simple tool to help users:" + "\n"
                           + "• Use Windows Device Portal to Install Apps etc." + "\n" 
                           + "• Search for FFU Files and Download" + "\n"
                           + "• Download Update Cabs for W10M" + "\n"
                           + "• Search and Download Appx files from MS Store" + "\n"
                           + "• Download Files and Youtube Videos" + "\n"
                           + "• Extract Archives easily";

        }

        private async void CheckForUpdate()
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue("Easy-Fetch-UWP"));
            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("Empyreal96", "Easy-Fetch-UWP");
            var latestRelease = releases[0];
            if (latestRelease.Assets != null && latestRelease.Assets.Count > 0)
            {


                var UpdateAvailable = new MessageDialog("An update is available");
                UpdateAvailable.Commands.Add(new UICommand("Close"));
                await UpdateAvailable.ShowAsync();
                DLUpdate.IsEnabled = true;
            }
        }

       

        private async void DLUpdate_Click(object sender, RoutedEventArgs e)
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue("Easy-Fetch-UWP"));
            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("Empyreal96", "Easy-Fetch-UWP");
            var latestRelease = releases[0];
            if (latestRelease.Assets != null && latestRelease.Assets.Count > 0)
            {
                if (latestRelease.TagName == CurrentBuildVersion)
                {
                    UpdateOut.Text = "No Updates Found! Check back later.";
                }
                //Test Function
               // if(latestRelease.TagName == PreviousBuildVersion)
               // {
               //     UpdateOut.Text = "You are on an unreleased build";
               // }

                else
                {
                    var updateURL = latestRelease.Assets[0].BrowserDownloadUrl;
                    UpdateURL = $"https://github.com/Empyreal96/Easy-Fetch-UWP/releases/download/{latestRelease.TagName}/{UploadedFileName}";
                   // string updateURL = $"https://github.com/Empyreal96/Easy-Fetch-UWP/releases/download/1.13.16-prerelease/Easy-Fetch_1.13.16.0_Debug_Test.zip";
                    UpdateOut.Text = $"Latest Build: {latestRelease.TagName}\n";
                    UpdateOut.Text += $"Current Build: {CurrentBuildVersion}\n";
                    UpdateOut.Text += $"{UpdateURL}\n";
                    try
                    {

                    
                    FolderPicker folderPicker = new FolderPicker();
                    folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
                    folderPicker.ViewMode = PickerViewMode.Thumbnail;
                    folderPicker.FileTypeFilter.Add("*");
                    folder = await folderPicker.PickSingleFolderAsync();
                    if (folder == null)
                    {
                        return;
                    }
                    StorageFile file = await folder.CreateFileAsync($"{UploadedFileName}", CreationCollisionOption.GenerateUniqueName);

                    downloadOperation = backgroundDownloader.CreateDownload(new Uri(UpdateURL), file);
                    
                    Progress<DownloadOperation> progress = new Progress<DownloadOperation>(progressChanged);
                    cancellationToken = new CancellationTokenSource();
                        await downloadOperation.StartAsync().AsTask(cancellationToken.Token, progress);

                    } catch (Exception ex)
                    {
                        Exceptions.ThrownExceptionError(ex);
                    }
                }
               // DLUpdate.IsEnabled = true;
            } 
        }
        private void progressChanged(DownloadOperation downloadOperation)
        {
            int progress = (int)(100 * ((double)downloadOperation.Progress.BytesReceived / (double)downloadOperation.Progress.TotalBytesToReceive));
            //TextBlockProgress.Text = String.Format("{0} of {1} kb. downloaded - {2}% complete.", downloadOperation.Progress.BytesReceived / 1024, downloadOperation.Progress.TotalBytesToReceive / 1024, progress);
            ProgressBarDownload.Value = progress;
            switch (downloadOperation.Progress.Status)
            {
                case BackgroundTransferStatus.Running:
                    {
                        UpdateOut.Text = $"Downloading from {UpdateURL}";
                        //ButtonPauseResume.Content = "Pause";
                        break;
                    }
                case BackgroundTransferStatus.PausedByApplication:
                    {
                        UpdateOut.Text = "Download paused.";
                        //ButtonPauseResume.Content = "Resume";
                        break;
                    }
                case BackgroundTransferStatus.PausedCostedNetwork:
                    {
                        UpdateOut.Text = "Download paused because of metered connection.";
                        //ButtonPauseResume.Content = "Resume";
                        break;
                    }
                case BackgroundTransferStatus.PausedNoNetwork:
                    {
                        UpdateOut.Text = "No network detected. Please check your internet connection.";
                        break;
                    }
                case BackgroundTransferStatus.Error:
                    {
                        UpdateOut.Text = "An error occured while downloading.";
                        break;
                    }
            }
            if (progress >= 100)
            {
                UpdateOut.Text = $"Download complete. Update downloaded to {folder.Path}\\{UploadedFileName}";
               // ButtonCancel.IsEnabled = false;
                //ButtonPauseResume.IsEnabled = false;
                //ButtonDownload.IsEnabled = true;
                downloadOperation = null;
            }
        }
    }
    
}
