﻿using HandyControl.Controls;
using HandySub.Assets;
using HandySub.Models;
using HtmlAgilityPack;
using ModernWpf.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net;
using System.Windows.Navigation;
using Page = ModernWpf.Controls.Page;
using HandySub.Assets.Strings;
using System.Windows;
using Downloader;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.ComponentModel;
using HandyControl.Tools;
using HandyControl.Data;
using HandyControl.Tools.Extension;
using static HandySub.Assets.Helper;
namespace HandySub.Views
{
    /// <summary>
    /// Interaction logic for ESubtitleDownload.xaml
    /// </summary>
    public partial class ESubtitleDownload : Page
    {
        ObservableCollection<DownloadModel> DataList = new ObservableCollection<DownloadModel>();
        private string location = string.Empty;
        private string subtitleUrl = string.Empty;
        public ESubtitleDownload()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var link = e?.Parameter()?.ToString();
            if (!string.IsNullOrEmpty(link))
                subtitleUrl = link;

            if (!string.IsNullOrEmpty(subtitleUrl))
            {
                GetSubtitle();
            }
        }

        private async void GetSubtitle()
        {
            try
            {
                tgBlock.IsChecked = false;

                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(subtitleUrl);

                var items = doc.DocumentNode.SelectNodes("//a[@class='Download']");
                if (items == null)
                {
                    Growl.ErrorGlobal(Lang.ResourceManager.GetString("SubNotFound"));
                }
                else
                {
                    DataList?.Clear();
                    foreach (var node in items)
                    {
                        var displayName = node.SelectSingleNode(".//span[last()]").InnerText;
                        var downloadLink = node.Attributes["href"].Value;
                        if (!displayName.Contains("جهت حمایت از ما کلیک کنید"))
                        {
                            var item = new DownloadModel
                            { 
                                DisplayName = displayName, 
                                DownloadLink = downloadLink
                            };
                            DataList.Add(item);
                        }
                    }

                    listView.ItemsSource = DataList;
                }

                tgBlock.IsChecked = true;
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            catch (ArgumentNullException)
            {
            }
            catch (NullReferenceException)
            {
            }
            catch (WebException ex)
            {
                Growl.ErrorGlobal(Lang.ResourceManager.GetString("ServerNotFound") + "\n" + ex.Message);
            }
            catch (HttpRequestException hx)
            {
                Growl.ErrorGlobal(Lang.ResourceManager.GetString("ServerNotFound") + "\n" + hx.Message);
            }
            finally
            {
                tgBlock.IsChecked = true;
            }
        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetSubtitle();
        }
        private async void btnDownload_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                tgBlock.IsChecked = false;
                prgStatus.Value = 0;
                prgStatus.IsIndeterminate = false;
                var btnDownload = sender as Button;
                var link = btnDownload.Tag.ToString();
                if (!string.IsNullOrEmpty(link))
                {
                    // if luanched from ContextMenu set location next to the movie file
                    if (!string.IsNullOrEmpty(App.WindowsContextMenuArgument[0]))
                        location = App.WindowsContextMenuArgument[1];
                    else // get location from config
                        location = Settings.StoreLocation;

                    if (!Settings.IsIDMEnabled)
                    {
                        var downloader = new DownloadService();
                        downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                        downloader.DownloadFileCompleted += Downloader_DownloadFileCompleted;
                        await downloader.DownloadFileTaskAsync(link, new DirectoryInfo(location));
                    }
                    else
                    {
                        tgBlock.IsChecked = true;
                        OpenLinkWithIDM(link, IDMNotFound);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Growl.ErrorGlobal(Lang.ResourceManager.GetString("AdminError"));
            }
            catch (NotSupportedException)
            {
            }
            catch (ArgumentException)
            {
            }
            finally
            {
                tgBlock.IsChecked = true;
            }
        }

        private void IDMNotFound()
        {
            Growl.WarningGlobal(Lang.ResourceManager.GetString("IDMNot"));
        }

        private void Downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            DispatcherHelper.RunOnMainThread(() =>
            {
                tgBlock.IsChecked = true;
                prgStatus.Value = 0;
                prgStatus.IsIndeterminate = true;
                txtStatus.Text = string.Empty;
                if (Settings.IsShowNotification)
                {
                    var downlaodedFileName = ((DownloadPackage)e.UserState).FileName;

                    Growl.ClearGlobal();
                    Growl.AskGlobal(new GrowlInfo
                    {
                        CancelStr = Lang.ResourceManager.GetString("Cancel"),
                        ConfirmStr = Lang.ResourceManager.GetString("OpenFolder"),
                        Message = Lang.ResourceManager.GetString("FileDownloaded").Format(Path.GetFileNameWithoutExtension(downlaodedFileName)),
                        ActionBeforeClose = b =>
                        {
                            if (!b) return true;

                            Process.Start("explorer.exe", "/select, \"" + downlaodedFileName + "\"");
                            return true;
                        }
                    });
                }
            });
        }

        private void Downloader_DownloadProgressChanged(object sender, Downloader.DownloadProgressChangedEventArgs e)
        {
            DispatcherHelper.RunOnMainThread(() => {
                prgStatus.Value = e.ProgressPercentage;
                txtStatus.Text = $"{(int)e.ProgressPercentage}%";
            });
        }
    }
}
