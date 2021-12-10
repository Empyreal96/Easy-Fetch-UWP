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
using VideoLibrary;
using Windows.Storage.Pickers;
using Windows.Storage;
using ExceptionHelper;
using Windows.UI.Popups;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Networking.BackgroundTransfer;
using System.Threading;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;





// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Phone_Helper
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Downloaders : Page
    {

        // Youtube
        public string link { get; set; }
        public string Title { get; set; }
        public string Extension { get; set; }
        public string Length { get; set; }
        public string AudioBitrate { get; set; }
        public string AudioFormats { get; set; }
        public string VideoFormat { get; set; }
        public string VideoRes { get; set; }
        public string FPS { get; set; }
        public string VideoID { get; set; }

        DownloadOperation downloadOperation;
        CancellationTokenSource cancellationToken;

        BackgroundDownloader backgroundDownloader = new BackgroundDownloader();

        public static string UserFileName { get; set; }
        // Start
        public Downloaders()
        {
            this.InitializeComponent();

            

            /*YTHelpText.Text = "- No HD Quality Videos are able to download yet.\n" +
                                @"- Make sure URL starts with 'https:\\www.'" + "\n" +
                                "- Progress on downloads will be improved in the future"; */
            ProgRing.IsEnabled = false;
            ProgRing.IsActive = false;
            ProgRing.Visibility = Visibility.Collapsed;
            YTDownloadBtn.Visibility = Visibility.Collapsed;
            YTSearchOutput.Visibility = Visibility.Collapsed;
            YTThumbnailBorder.Visibility = Visibility.Collapsed;
            YTDownloadMP3Btn.Visibility = Visibility.Collapsed;
            contentFrame.Visibility = Visibility.Collapsed;
            contentFrame.IsEnabled = false;
            ButtonCancel.Visibility = Visibility.Collapsed;
            ButtonPauseResume.Visibility = Visibility.Collapsed;
            ProgressBarDownload.Visibility = Visibility.Collapsed;
        }









        /// <summary>
        /// Youtube Downloader Page
        /// 
        /// 
        /// 
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void YTSearchBtn_Click(object sender, RoutedEventArgs e)
        {

            ProgressText.Text = "Searching for Link";
            if (DownloaderYTHeader.Text == "")
            {
                var ThrownException = new MessageDialog("Enter a valid YouTube URL!");
                ThrownException.Commands.Add(new UICommand("Close"));
                await ThrownException.ShowAsync();
            }
            ProgRing.IsEnabled = true;
            ProgRing.IsActive = true;
            ProgRing.Visibility = Visibility.Visible;
            link = DownloaderYTHeader.Text;

            ///
            /// Need to fix
            try
            {
                if (link.Contains("http:"))
                {
                    string fixedlink = link.Replace("http:", "https:");
                    link = fixedlink;
                }
                if (link.Contains("m."))
                {
                    string fixedlink = link.Replace("m.", "www.");
                    link = fixedlink;
                }
            } catch (Exception ex)
            {
                Exceptions.ThrownExceptionError(ex);
            }


            //await CheckVideo(link);
            var youTube = YouTube.Default; // starting point for YouTube actions
            var Justvideo = youTube.GetVideo(link); // gets a Video object with info about the video
            var videoInfos = youTube.GetAllVideosAsync(link).GetAwaiter().GetResult();
            ProgressText.Text = "";
            var video = videoInfos.First(i => i.IsAdaptive); //videoInfos.First(i => i.Resolution == videoInfos.Max(j => j.Resolution));
            var minBitrate = videoInfos.First(i => i.AudioBitrate == videoInfos.Min(j => j.AudioBitrate));
            string URIs = video.Uri;
            var adaptive = videoInfos.First(i => i.IsAdaptive);
            Title = video.Title;
            Extension = video.FileExtension;
            //TimeSpan durationinseconds = TimeSpan.FromMilliseconds(video.ContentLength);
            long LengthIn = (long)video.ContentLength;
            Length = LengthIn.ToFileSize();
            AudioBitrate = video.AudioBitrate.ToString();
            AudioFormats = video.AudioFormat.ToString();
            VideoFormat = video.Format.ToString();
            FPS = video.Fps.ToString();
            VideoRes = video.Resolution.ToString();

            VideoID = link.Replace("https://www.youtube.com/watch?v=", "");
            await GetThumbnail();
            YTDownloadMP3Btn.Visibility = Visibility.Visible;
            YTThumbnailBorder.Visibility = Visibility.Visible;
            YTSearchOutput.Visibility = Visibility.Visible;
            YTSearchOutput.Text = $"Video Information:\n\n" +
                                    $"File Name: {Title}\n" +
                                    $"File Extension: {Extension}\n" +
                                    $"File Size: {Length}\n" +
                                    $"Audio Bitrate: {AudioBitrate}\n" +
                                    $"Audio Format: {AudioFormats}\n" +
                                    $"Video Format: {VideoFormat}\n" +
                                    $"Video Resolution: {VideoRes}\n" +
                                    $"Video FPS: {FPS}\n\n" +
                                    $"URIs: {URIs}";
            YTDownloadBtn.Visibility = Visibility.Visible;





            ProgRing.IsEnabled = false;
            ProgRing.IsActive = false;
            ProgRing.Visibility = Visibility.Collapsed;


        }
        private async Task GetThumbnail()
        {
            string url = $"http://img.youtube.com/vi/{VideoID}/0.jpg";

            var rass = RandomAccessStreamReference.CreateFromUri(new Uri(url));
            using (IRandomAccessStream stream = await rass.OpenReadAsync())
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.SetSource(stream);
                YTThumbnail.Source = bitmapImage;
            }

        }

        private async void YTDownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            ProgRing.IsEnabled = true;
            ProgRing.IsActive = true;
            ProgRing.Visibility = Visibility.Visible;
            await SaveVideoToDisk(link);

        }

        public async Task SaveVideoToDisk(string DLlink)
        {
            ProgRing.IsEnabled = true;
            ProgRing.IsActive = true;
            ProgRing.Visibility = Visibility.Visible;
            try
            {
                ProgressText.Text = "Downloading, Please Wait";
                var youTube = YouTube.Default; // starting point for YouTube actions
                //var video = youTube.GetVideo(DLlink); // gets a Video object with info about the video
                var videoInfos = youTube.GetAllVideosAsync(link).GetAwaiter().GetResult();
                var video = videoInfos.First(i => i.IsAdaptive); //videoInfos.First(i => i.Resolution == videoInfos.Max(j => j.Resolution));
                FolderPicker folderPicker = new FolderPicker();
                folderPicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
                folderPicker.FileTypeFilter.Add("*");
                StorageFolder folder = await folderPicker.PickSingleFolderAsync();
                StorageFile file = await folder.CreateFileAsync(video.FullName, CreationCollisionOption.GenerateUniqueName);
                await FileIO.WriteBytesAsync(file, await video.GetBytesAsync());
                ProgressText.Text = "";
                //File.WriteAllBytes(folder.Path + file, await video.GetBytesAsync());
                await DownloadComplete(Title);
                ProgRing.IsEnabled = false;
                ProgRing.IsActive = false;
                ProgRing.Visibility = Visibility.Collapsed;
            } catch (Exception ex)
            {
                await DownloadFailed(Title);
                Exceptions.ThrownExceptionError(ex);
                ProgRing.IsEnabled = false;
                ProgRing.IsActive = false;
                ProgRing.Visibility = Visibility.Collapsed;
            }
        }

        private async void YTDownloadMP3Btn_Click(object sender, RoutedEventArgs e)
        {
            ProgRing.IsEnabled = true;
            ProgRing.IsActive = true;
            ProgRing.Visibility = Visibility.Visible;
            await SaveAudioToDisk(link);

        }

        public async Task SaveAudioToDisk(string DLlink)
        {
            ProgRing.IsEnabled = true;
            ProgRing.IsActive = true;
            ProgRing.Visibility = Visibility.Visible;
            try
            {
                ProgressText.Text = "Downloading, Please Wait";
                var youTube = YouTube.Default; // starting point for YouTube actions
                var audio = await YouTube.Default.GetAllVideosAsync(DLlink);
                var audios = audio.Where(_ => _.AudioFormat == AudioFormat.Aac && _.AdaptiveKind == AdaptiveKind.Audio).ToList();
                var mpAudio = audios.FirstOrDefault(x => x.AudioBitrate > 0);
                var video = youTube.GetVideo(DLlink); // gets a Video object with info about the video
                //var videoInfos = youTube.GetAllVideosAsync(link).GetAwaiter().GetResult();
                //var video = videoInfos.First(i => i.IsAdaptive); //videoInfos.First(i => i.Resolution == videoInfos.Max(j => j.Resolution));
                FolderPicker MP3folderPicker = new FolderPicker();
                MP3folderPicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
                MP3folderPicker.FileTypeFilter.Add("*");
                StorageFolder mp3folder = await MP3folderPicker.PickSingleFolderAsync();
                if (video.FullName.Contains(".mp4"))
                {
                    string fixextension = video.FullName.Replace(".mp4", ".mp3");
                    Title = fixextension;
                }
                StorageFile mp3file = await mp3folder.CreateFileAsync(Title, CreationCollisionOption.GenerateUniqueName);

                await FileIO.WriteBytesAsync(mp3file, await mpAudio.GetBytesAsync());
                ProgressText.Text = "";
                //File.WriteAllBytes(folder.Path + file, await video.GetBytesAsync());
                await DownloadComplete(Title);
                ProgRing.IsEnabled = false;
                ProgRing.IsActive = false;
                ProgRing.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                await DownloadFailed(Title);
                Exceptions.ThrownExceptionError(ex);
                ProgRing.IsEnabled = false;
                ProgRing.IsActive = false;
                ProgRing.Visibility = Visibility.Collapsed;
            }
        }


        ////
        ///
        /// NOTIFICATINS FOR DOWNLOADS - Likely temporary
        ///
        ///
        ///
        ///
        ///
        ////

        public async Task DownloadComplete(string fileName)
        {
            var DownloadComplete = new MessageDialog($"{fileName} downloaded successfully");
            DownloadComplete.Commands.Add(new UICommand("Close"));
            await DownloadComplete.ShowAsync();
        }

        public async Task DownloadFailed(string fileName)
        {
            var DownloadFailed = new MessageDialog($"{fileName} Failed to Download, Try Again or submit an Issue if the problem continues.");
            DownloadFailed.Commands.Add(new UICommand("Close"));
            await DownloadFailed.ShowAsync();
        }


        /// <summary>
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// </summary>
        /// <param name="DLLink"></param>
        public async void TestFFMPEG()
        {

        }





















        private async void FileDownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            ButtonCancel.Visibility = Visibility.Visible;
            ButtonPauseResume.Visibility = Visibility.Visible;
            ProgressBarDownload.Visibility = Visibility.Visible;

            string link = DownloaderFileHeader.Text;
            string fileNamefetched = Path.GetFileName(link);

            try
            {
                TextBox fileNameUser = new TextBox();
                fileNameUser.PlaceholderText = "Enter a File Name";
                fileNameUser.Text = fileNamefetched;
                fileNameUser.Height = 45;
                ContentDialog dialog = new ContentDialog();
                dialog.Content = fileNameUser;
                dialog.IsSecondaryButtonEnabled = true;
                dialog.PrimaryButtonText = "Confirm";
                dialog.SecondaryButtonText = "Cancel";

                if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    if (fileNameUser.Text == "")
                    {
                        fileNameUser.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                    }
                    else
                    {
                        fileNameUser.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Green);
                        UserFileName = fileNameUser.Text;
                    }
                }


            } catch (Exception ex)
            {
                Exceptions.ThrownExceptionError(ex);
            }
             



            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
            folderPicker.ViewMode = PickerViewMode.Thumbnail;
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                StorageFile file = await folder.CreateFileAsync(UserFileName, CreationCollisionOption.GenerateUniqueName);
                downloadOperation = backgroundDownloader.CreateDownload(new Uri(link), file);
                Progress<DownloadOperation> progress = new Progress<DownloadOperation>(progressChanged);
                cancellationToken = new CancellationTokenSource();
                ButtonDownload.IsEnabled = false;
                ButtonCancel.IsEnabled = true;
                ButtonPauseResume.IsEnabled = true;
                try
                {
                    TextBlockStatus.Text = "Initializing...";
                    await downloadOperation.StartAsync().AsTask(cancellationToken.Token, progress);
                }
                catch (TaskCanceledException)
                {
                    TextBlockStatus.Text = "Download canceled.";
                    await downloadOperation.ResultFile.DeleteAsync();
                    ButtonPauseResume.Content = "Resume";
                    ButtonCancel.IsEnabled = false;
                    ButtonCancel.IsEnabled = false;
                    ButtonPauseResume.IsEnabled = false;
                    ButtonDownload.IsEnabled = true;
                    downloadOperation = null;
                }
            }




            /*            DownloadOperation downloadOperation;
                        CancellationTokenSource cancellationToken;
                        BackgroundDownloader backgroundDownloader = new BackgroundDownloader();
                        FolderPicker file = new FolderPicker();
                        file.SuggestedStartLocation = PickerLocationId.Downloads;
                        file.FileTypeFilter.Add("*.*");
                        StorageFolder storageFolder = await file.PickSingleFolderAsync();

                        if (storageFolder == null)
                        {

                            ProgRing.IsEnabled = false;
                            ProgRing.IsActive = false;
                            ProgRing.Visibility = Visibility.Collapsed;
                            return;
                        }




                        if (link == "")
                        {
                            Exception ex = new Exception();
                            string urlbox = "File URL";
                            await Exceptions.URLNameBlank(urlbox, ex);
                        }
                        string filename = "";
                        StorageFile fileFetched = await storageFolder.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);
                        Uri downloadUrl = new Uri(link);
                        downloadOperation = backgroundDownloader.CreateDownload(downloadUrl, fileFetched);
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
            */
        }


        private void progressChanged(DownloadOperation downloadOperation)
        {
            int progress = (int)(100 * ((double)downloadOperation.Progress.BytesReceived / (double)downloadOperation.Progress.TotalBytesToReceive));
            TextBlockProgress.Text = String.Format("{0} of {1} kb. downloaded - {2}% complete.", downloadOperation.Progress.BytesReceived / 1024, downloadOperation.Progress.TotalBytesToReceive / 1024, progress);
            ProgressBarDownload.Value = progress;
            switch (downloadOperation.Progress.Status)
            {
                case BackgroundTransferStatus.Running:
                    {
                        TextBlockStatus.Text = "Downloading...";
                        ButtonPauseResume.Content = "Pause";
                        break;
                    }
                case BackgroundTransferStatus.PausedByApplication:
                    {
                        TextBlockStatus.Text = "Download paused.";
                        ButtonPauseResume.Content = "Resume";
                        break;
                    }
                case BackgroundTransferStatus.PausedCostedNetwork:
                    {
                        TextBlockStatus.Text = "Download paused because of metered connection.";
                        ButtonPauseResume.Content = "Resume";
                        break;
                    }
                case BackgroundTransferStatus.PausedNoNetwork:
                    {
                        TextBlockStatus.Text = "No network detected. Please check your internet connection.";
                        break;
                    }
                case BackgroundTransferStatus.Error:
                    {
                        TextBlockStatus.Text = "An error occured while downloading.";
                        break;
                    }
            }
            if (progress >= 100)
            {
                TextBlockStatus.Text = "Download complete.";
                ButtonCancel.IsEnabled = false;
                ButtonPauseResume.IsEnabled = false;
                ButtonDownload.IsEnabled = true;
                downloadOperation = null;
            }
        }

        private void ButtonPauseResume_Click(object sender, RoutedEventArgs e)
        {
            if (ButtonPauseResume.Content.ToString() == "Pause")
            {
                try
                {
                    downloadOperation.Pause();
                }
                catch (InvalidOperationException)
                {

                }
            }
            else
            {
                try
                {
                    downloadOperation.Resume();
                }
                catch (InvalidOperationException)
                {
                }
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            cancellationToken.Cancel();
            cancellationToken.Dispose();

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
        }
    }

