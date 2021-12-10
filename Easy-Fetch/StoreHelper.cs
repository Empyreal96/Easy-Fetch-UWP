using StoreLib.Models;
using StoreLib.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Phone_Helper
{
    public static class StoreHelper
    {
        public static void ShowError(Exception ex)
        {

        }
        static DisplayCatalogHandler dcathandler = new DisplayCatalogHandler(DCatEndpoint.Production, new Locale(Market.US, Lang.en, true));
        public static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public static bool isStoreLoading = false;
        public static async Task<List<PackageInfo>> findPackages(StoreOptions storeOptions)
        {
            isStoreLoading = true;
            var query = storeOptions.Query;
            var queryIsPackageID = storeOptions.QueryIsPackageID;
            var DevFamily = storeOptions.DeviceFamily;
            List<PackageInfo> packagesData = new List<PackageInfo>();
            try
            {
                cancellationTokenSource.Cancel();
            }
            catch (Exception ex)
            {

            }
            cancellationTokenSource = new CancellationTokenSource();

            try
            {
                DCatSearch search = null;
                if (!queryIsPackageID)
                {
                    search = await dcathandler.SearchDCATAsync(query, DevFamily);
                }
                if (queryIsPackageID || search?.TotalResultCount > 0)
                {
                    DisplayCatalogHandler displayCatalog = new DisplayCatalogHandler(DCatEndpoint.Production, new Locale(Market.US, Lang.en, true));

                    var packageID = "";
                    if (queryIsPackageID)
                    {
                        packageID = query.Trim();
                    }
                    else
                    {
                        packageID = search.Results[0].Products[0].ProductId;
                    }
                    await displayCatalog.QueryDCATAsync(packageID);

                    string xml = await FE3Handler.SyncUpdatesAsync(displayCatalog.ProductListing.Product.DisplaySkuAvailabilities[0].Sku.Properties.FulfillmentData.WuCategoryId, cancellationTokenSource.Token);
                    IList<string> RevisionIds = new List<string>();
                    IList<string> UpdateIDs = new List<string>();
                    IList<PackageItem> Packages = new List<PackageItem>();
                    //Helpers.Logger(xml);
                    FE3Handler.ProcessUpdateIDs(xml, out RevisionIds, out UpdateIDs, out Packages);
                    IList<FE3Handler.UrlItem> FileUris = await FE3Handler.GetFileUrlsAsync(UpdateIDs, RevisionIds);

                    string[] acceptedExts = new string[] { ".appx", ".appxbundle", ".msix", ".msixbundle" };
                    for (int i = 0; i < FileUris.Count; i++)
                    {
                        var fileuri = FileUris[i];
                        var packageItem = Packages[i];

                        if (cancellationTokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                        string FIDigest = fileuri.digest;

                        var FileUri = fileuri.url;

                        var fileInfo = new HTTPFileInfo(packageItem.PackageMoniker, 0);
                        if (packageItem.PackageMoniker == null || packageItem.PackageMoniker.Trim().Length == 0)
                        {
                            fileInfo = new HTTPFileInfo(packageItem.UpdateID, 0);
                        }
                        //Below disabled due some network delays
                        /*if (packageItem.IsAppxFramework && IncludeFrameworks)
                        {
                            try
                            {
                                fileInfo = await getFileNameWithResponse(fileuri.url, packageItem);
                            }
                            catch (Exception ex)
                            {

                            }
                        }*/

                        var fileName = fileInfo.fileName;
                        var tempFileName = fileName;
                        try
                        {

                        }
                        catch (Exception ex)
                        {

                        }
                        //Add only the accepted files
                        //if ((!IncludeFrameworks && !packageItem.IsAppxFramework) || IncludeFrameworks)
                        {
                            //if (arch.ToLower().Equals("all") || fileName.ToLower().Contains(arch) || fileName.ToLower().Contains("neutral"))
                            {
                                if (packageItem.PackageType.ToLower().Equals("appx"))
                                {
                                    if (packageItem.IsAppxBundle)
                                    {
                                        fileName = $"{fileName}.appxbundle";
                                        fileInfo.fileName = fileName;
                                    }
                                    else
                                    {
                                        fileName = $"{fileName}.appx";
                                        fileInfo.fileName = fileName;
                                    }
                                }
                                else if (packageItem.PackageType.ToLower().Equals("uap"))
                                {
                                    if (packageItem.IsAppxBundle)
                                    {
                                        fileName = $"{fileName}.appxbundle";
                                        fileInfo.fileName = fileName;
                                    }
                                    else
                                    {
                                        fileName = $"{fileName}.appx";
                                        fileInfo.fileName = fileName;
                                    }
                                }
                                else
                                if (packageItem.PackageType.ToLower().Equals("msix"))
                                {
                                    if (packageItem.IsAppxBundle)
                                    {
                                        fileName = $"{fileName}.msixbundle";
                                        fileInfo.fileName = fileName;
                                    }
                                    else
                                    {
                                        fileName = $"{fileName}.msix";
                                        fileInfo.fileName = fileName;
                                    }
                                }
                                else
                                if (packageItem.PackageType.ToLower().Equals("xap"))
                                {
                                    fileName = $"{fileName}.xaps";
                                    fileInfo.fileName = fileName;
                                }
                                else if (packageItem.IsAppxBundle)
                                {
                                    fileName = $"{fileName}.appxbundle";
                                    fileInfo.fileName = fileName;
                                }

                                try
                                {
                                    var fileTestInfo = GetFileInfo(xml, FIDigest, fileName);
                                    if (fileTestInfo.fileSize > 0)
                                    {
                                        var fileExactExt = Path.GetExtension(fileTestInfo.fileName);
                                        if (fileExactExt != null && fileExactExt.Length > 0)
                                        {
                                            fileName = $"{tempFileName}{fileExactExt}";
                                            fileInfo.fileName = fileName;
                                        }
                                        fileInfo.fileSize = fileTestInfo.fileSize;
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                                /*if (fileInfo.fileSize > 0)
                                {
                                    fileName = $"[{fileInfo.fileSize.ToFileSize()}] {fileName}";
                                }*/

                                Uri logo = null;
                                if (!packageItem.IsAppxFramework)
                                {
                                    try
                                    {
                                        logo = StoreLib.Utilities.ImageHelpers.GetImageUri(ImagePurpose.Logo, displayCatalog.ProductListing);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                                if (storeOptions.WUTDownloadAction)
                                {
                                    FileUri = new Uri($"wut::{FileUri}#force--{fileName}");
                                }
                                PackageInfo packageInfo = new PackageInfo(fileInfo, packageItem, FileUri, logo);
                                packagesData.Add(packageInfo);
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException oex)
            {

            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
            isStoreLoading = false;
            return packagesData;
        }

        //static StorageItemThumbnail appxIcon = null;
        public static EventHandler StoreItemClick;
        public static EventHandler SaveAsHTMLClick;
        public static EventHandler FilterResults;
        public static EventHandler DownloadAllClick;
        public static ListView listView;
        public static void AppendResultToGrid(List<PackageInfo> packageInfos, Grid grid, string arch = "all", bool includeFrameworks = true)
        {
            try
            {
                ScrollViewer scrollViewer = new ScrollViewer();
                scrollViewer.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
                StackPanel linksContainer = new StackPanel();
                listView = new ListView();
                List<string> urls = new List<string>();
                var sortedPackages = packageInfos.OrderBy(item => item.PackageItem.IsAppxFramework);
                List<PackageInfo> packagesHTML = new List<PackageInfo>();
                foreach (var pItem in sortedPackages)
                {
                    if ((!includeFrameworks && !pItem.PackageItem.IsAppxFramework) || includeFrameworks)
                    {
                        if (arch.ToLower().Equals("all") || pItem.fileName.ToLower().Contains(arch.ToLower()) || pItem.fileName.ToLower().Contains("neutral"))
                        {
                            ListViewItem listViewItem = new ListViewItem();
                            listViewItem.Tag = pItem;

                            var fileName = pItem.fileName;
                            var fileUri = pItem.fileURL;

                            if (pItem.PackageItem.IsAppxFramework)
                            {
                                urls.Add(pItem.fileURL.ToString());
                            }
                            packagesHTML.Add(pItem);
                            StackPanel stackPanelItem = new StackPanel();
                            stackPanelItem.Orientation = Orientation.Horizontal;
                            TextBlock textBlock = new TextBlock();
                            Windows.UI.Xaml.Controls.Image image = new Windows.UI.Xaml.Controls.Image();

                            textBlock.Text = fileName;
                            textBlock.VerticalAlignment = VerticalAlignment.Center;
                            textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
                            int left = 3;
                            int top = 3;
                            int right = 15;
                            int bottom = 3;

                            BitmapImage imageSource = new BitmapImage();
                            var fileExt = ".appx";
                            try
                            {
                                fileExt = Path.GetExtension(pItem.fileName);
                            }
                            catch (Exception ex)
                            {

                            }
                            //If package is framework we revert the icon to package icon
                            if (pItem.PackageItem.IsAppxFramework || pItem.fileLogo == null)
                            {
                                imageSource = new BitmapImage(new Uri("ms-appx:///Assets/appx.png"));
                            }
                            else
                            {
                                imageSource = new BitmapImage(pItem.fileLogo);
                            }
                            image.Source = imageSource;
                            image.VerticalAlignment = VerticalAlignment.Center;
                            image.Margin = new Thickness(left, top, right, bottom);
                            image.Width = 64;
                            image.Height = 64;

                            stackPanelItem.Children.Add(image);
                            StackPanel fileInfo = new StackPanel();
                            fileInfo.VerticalAlignment = VerticalAlignment.Center;
                            fileInfo.Children.Add(textBlock);
                            TextBlock fileType = new TextBlock();
                            TextBlock fileSize = new TextBlock();
                            StackPanel SizeContainer = new StackPanel();
                            SizeContainer.Orientation = Orientation.Horizontal;

                            fileType.Text = fileExt.Replace(".", "").ToUpper();
                            fileType.FontWeight = FontWeights.Bold;
                            fileType.Foreground = new SolidColorBrush(Colors.DodgerBlue);
                            fileType.FontSize = 14;

                            SizeContainer.Children.Add(fileType);
                            if (pItem.fileSize > 0)
                            {
                                fileSize.Text = $"({pItem.fileSize.ToFileSize()})";
                                fileSize.FontWeight = FontWeights.Bold;
                                fileSize.Foreground = new SolidColorBrush(Colors.Orange);
                                fileSize.FontSize = 14;
                                fileSize.Margin = new Thickness(5, 0, 0, 0);
                                SizeContainer.Children.Add(fileSize);
                            }

                            fileInfo.Children.Add(SizeContainer);
                            stackPanelItem.Children.Add(fileInfo);
                            listViewItem.Content = stackPanelItem;
                            listViewItem.BorderThickness = new Thickness(0, 0, 0, 1);

                            listViewItem.BorderBrush = (App.Current.Resources["SystemControlBackgroundBaseLowBrush"] as SolidColorBrush);
                            listView.Items.Add(listViewItem);
                        }
                    }
                }
                listView.IsItemClickEnabled = true;
                try
                {
                    listView.ItemClick -= ListView_ItemClick;
                }
                catch (Exception ex)
                {

                }
                listView.ItemClick += ListView_ItemClick;
                FilterResults = async (sender, args) =>
                {
                    var filterOptions = (StoreFilter)args;
                    AppendResultToGrid(packageInfos, grid, filterOptions.StoreArc, filterOptions.IncludeFrameworks);
                };
                if ((SaveAsHTMLClick != null && packagesHTML.Count > 0) || (DownloadAllClick != null && urls.Count > 0))
                {
                    CommandBar commandBar = new CommandBar();
                    if (SaveAsHTMLClick != null && packagesHTML.Count > 0)
                    {
                        AppBarButton saveAsHTML = new AppBarButton();
                        saveAsHTML.Label = "HTML";
                        saveAsHTML.Icon = new SymbolIcon(Symbol.Save);
                        saveAsHTML.Click += (sender, args) =>
                        {
                            if (SaveAsHTMLClick != null)
                            {
                                SaveAsHTMLClick.Invoke(packagesHTML, EventArgs.Empty);
                            }
                        };
                        commandBar.PrimaryCommands.Add(saveAsHTML);
                    }
                    if (DownloadAllClick != null && urls.Count > 0)
                    {
                        AppBarButton downloadAll = new AppBarButton();

                        downloadAll.Label = "Frameworks";
                        downloadAll.Icon = new SymbolIcon(Symbol.Download);
                        downloadAll.Click += (sender, args) =>
                        {
                            if (DownloadAllClick != null)
                            {
                                DownloadAllClick.Invoke(urls, EventArgs.Empty);
                            }
                        };
                        commandBar.PrimaryCommands.Add(downloadAll);
                    }
                    commandBar.Background = (App.Current.Resources["ApplicationPageBackgroundThemeBrush"] as SolidColorBrush);
                    linksContainer.Children.Add(commandBar);
                }
                linksContainer.Children.Add(listView);

                scrollViewer.Content = linksContainer;
                grid.Children.Add(scrollViewer);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }


        private static void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var clickedItem = (StackPanel)e.ClickedItem;
                if (StoreItemClick != null)
                {
                    StoreItemClick.Invoke(((ListViewItem)clickedItem.Parent).Tag, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static ScrollViewer GetResultAsScrollViewer(List<PackageInfo> packageInfos)
        {
            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalAlignment = HorizontalAlignment.Stretch;
            StackPanel linksContainer = new StackPanel();
            try
            {
                foreach (var pItem in packageInfos)
                {
                    HyperlinkButton hyperlinkButton = new HyperlinkButton();
                    var fileName = pItem.fileName;
                    var fileUri = pItem.fileURL;
                    hyperlinkButton.Content = fileName;
                    hyperlinkButton.NavigateUri = fileUri;
                    linksContainer.Children.Add(hyperlinkButton);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
            scrollViewer.Content = linksContainer;
            return scrollViewer;
        }

        public static void CancelRequest()
        {
            cancellationTokenSource.Cancel();
        }

        public static HTTPFileInfo GetFileInfo(string xml, string digest, string name = "test.appx")
        {
            HTTPFileInfo hTTPFileInfo = new HTTPFileInfo(name, 0);
            try
            {
                var pattern = $"<File FileName=\"(?<name>[\\w\\s_\\d.\\-()!@#$%^&_+=';]+)\" Digest=\"{digest}\" DigestAlgorithm=\"SHA1\" Size=\"(?<size>\\d+)\"";
                Match m = Regex.Match(xml, pattern, RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    if (m.Groups != null && m.Groups.Count > 0)
                    {
                        var fileName = m.Groups["name"].Value;
                        long fileSize = 0;
                        try
                        {
                            fileSize = long.Parse(m.Groups["size"].Value);
                        }
                        catch (Exception ex)
                        {

                        }
                        hTTPFileInfo = new HTTPFileInfo(fileName, fileSize);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return hTTPFileInfo;
        }
        //Get file name using HTTP Response
        public static async Task<HTTPFileInfo> getFileNameWithResponse(Uri fileLink, PackageItem packageItem = null)
        {
            HTTPFileInfo hTTPFileInfo = new HTTPFileInfo("test.appx", 0);
            if (packageItem != null)
            {
                hTTPFileInfo = new HTTPFileInfo(packageItem.PackageMoniker, 0);
            }
            var _client = new Windows.Web.Http.HttpClient();
            var response = await _client.GetAsync(fileLink, Windows.Web.Http.HttpCompletionOption.ResponseHeadersRead).AsTask(cancellationTokenSource.Token);
            if (response.IsSuccessStatusCode)
            {
                var FileName = response.Content.Headers?.ContentDisposition?.FileName;
                long FileSize = 0;
                try
                {
                    FileSize = (long)response.Content.Headers.ContentLength.GetValueOrDefault();
                }
                catch (Exception e)
                {
                    FileSize = 0;
                }
                hTTPFileInfo = new HTTPFileInfo(FileName, FileSize);
            }
            return hTTPFileInfo;
        }
    }
    public class HTTPFileInfo
    {
        public string fileName;
        public long fileSize;
        public HTTPFileInfo(string name, long size)
        {
            fileName = name;
            fileSize = size;
        }
    }
    public class PackageInfo
    {
        public string fileName;
        public long fileSize;
        public Uri fileURL;
        public Uri fileLogo;
        public PackageItem PackageItem;
        public PackageInfo(HTTPFileInfo hTTPFileInfo, PackageItem packageItem, Uri url, Uri logo = null)
        {
            fileName = hTTPFileInfo.fileName;
            fileSize = hTTPFileInfo.fileSize;
            fileURL = url;
            fileLogo = logo;
            PackageItem = packageItem;
        }
    }
    public class StoreOptions
    {
        public string Query;
        public DeviceFamily DeviceFamily = DeviceFamily.Desktop;
        public bool QueryIsPackageID = false;
        public bool WUTDownloadAction = false;
        public StoreOptions(string query, DeviceFamily deviceFamily)
        {
            Query = query;
            DeviceFamily = deviceFamily;
        }
        public StoreOptions(string query, DeviceFamily deviceFamily, bool queryIsPackageID)
        {
            Query = query;
            DeviceFamily = deviceFamily;
            QueryIsPackageID = queryIsPackageID;
        }
    }
    public class StoreFilter : EventArgs
    {
        public string StoreArc = "all";
        public bool IncludeFrameworks = false;
        public StoreFilter(string arc, bool frameworks)
        {
            StoreArc = arc.ToLower();
            IncludeFrameworks = frameworks;
        }
    }
}

