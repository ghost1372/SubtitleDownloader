﻿using HandyControl.Controls;
using Prism.Commands;
using Prism.Mvvm;
using System.Reflection;

namespace SubtitleDownloader.ViewModels
{
    public class UpdaterViewModel : BindableBase
    {
        #region Property
        private string _version;
        public string Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }

        private string _createdAt;
        public string CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        private string _publishedAt;
        public string PublishedAt
        {
            get => _publishedAt;
            set => SetProperty(ref _publishedAt, value);
        }

        private string _downloadUrl;
        public string DownloadUrl
        {
            get => _downloadUrl;
            set => SetProperty(ref _downloadUrl, value);
        }

        private string _currentVersion;
        public string CurrentVersion
        {
            get => _currentVersion;
            set => SetProperty(ref _currentVersion, value);
        }

        private string _changeLog;
        public string ChangeLog
        {
            get => _changeLog;
            set => SetProperty(ref _changeLog, value);
        }
        #endregion

        public DelegateCommand CheckUpdateCommand { get; private set; }

        public UpdaterViewModel()
        {
            CheckUpdateCommand = new DelegateCommand(CheckforUpdate);
        }

        private void CheckforUpdate()
        {
            try
            {
                UpdateHelper.GithubReleaseModel ver = UpdateHelper.CheckForUpdateGithubRelease("ghost1372", "SubtitleDownloader");
                CreatedAt = ver.CreatedAt.ToString();
                PublishedAt = ver.PublishedAt.ToString();
                DownloadUrl = ver.Asset[0].browser_download_url;
                CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                Version = ver.TagName.Replace("v", "");
                ChangeLog = ver.Changelog;
                if (ver.IsExistNewVersion)
                {
                    Growl.Success("نسخه جدید پیدا شد!");
                }
                else
                {
                    Growl.Info("شما از آخرین نسخه استفاده میکنید");
                }
            }
            catch (System.Exception)
            {

                Growl.Error("هیچ نسخه ای پیدا نشد");
            }
        }
    }
}