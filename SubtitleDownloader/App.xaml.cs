﻿using HandyControl.Data;
using HandyControl.Tools;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.IO;
using System.Net;
using System.Windows;

namespace SubtitleDownloader
{
    public partial class App
    {
        public static string WindowsContextMenuArgument = string.Empty;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            GlobalData.Init();

            //init Appcenter Crash Reporter
            AppCenter.Start("3770b372-60d5-49a1-8340-36a13ae5fb71",
                   typeof(Analytics), typeof(Crashes));
            AppCenter.Start("3770b372-60d5-49a1-8340-36a13ae5fb71",
                               typeof(Analytics), typeof(Crashes));

            //set Lang
            ConfigHelper.Instance.SetLang(GlobalData.Config.UILang);

            //set Skin
            if (GlobalData.Config.Skin != SkinType.Default)
            {
                UpdateSkin(GlobalData.Config.Skin);
            }

            //get ContextMenu Argument
            if (e.Args.Length > 0)
            {
                WindowsContextMenuArgument = Path.GetFileNameWithoutExtension(e.Args[0]);
            }
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }
        internal void UpdateSkin(SkinType skin)
        {
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/HandyControl;component/Themes/Skin{skin.ToString()}.xaml")
            });
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/HandyControl;component/Themes/Theme.xaml")
            });
        }
    }
}
