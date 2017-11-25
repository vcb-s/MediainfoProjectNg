﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MediaInfoLib;
using static System.String;

namespace mediainfo_project_ng
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private readonly FileInfos _fileInfos;
        private static MainWindowViewModel _mainWindowViewModel;
        public MainWindow()
        {
            InitializeComponent();
            _fileInfos = (FileInfos) FindResource("FileInfos");
            _mainWindowViewModel = (MainWindowViewModel) FindResource("WindowViewModel");
            DataContext = _mainWindowViewModel;

            MediaInfo MI = null;
            try
            {
                MI = new MediaInfo();
                var v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                _mainWindowViewModel.TitleString = $"mediainfo project ng {v}";
                var version = MI.Option("Info_Version");
                if (version == "Unable to load MediaInfo library")
                {
                    _mainWindowViewModel.TitleString += " [Mediainfo: Unavailable]";
                    MessageBox.Show("无法载入适用的 mediainfo，请检查！", "mediainfo project ng", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    _mainWindowViewModel.TitleString += $" [Mediainfo: {version.Substring(15)}]";
                    _mainWindowViewModel.StatusString = $"Mediainfo DLL {version.Substring(15)} at your service.";
                }
            }
            finally
            {
                MI?.Close();
            }
#if DEBUG
            Button1.IsEnabled = true;
#else
            Button1.IsEnabled = false;
#endif
        }

        private async void Button1_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            _mainWindowViewModel.StatusString = Empty;

            // TODO: Make it in config file
            var fileList = new List<string>
            {
                @"C:\Users\Mark\Desktop\doc_2017-11-21_12-42-55.mp4",
                @"C:\Users\Mark\Desktop\K-ON!_06mp4.mp4"
            };

            var ret = await Utils.Load(fileList.ToArray());
            _fileInfos.AddItems(ret.Item1);

            _mainWindowViewModel.StatusString += $"Elapsed {ret.Item2}ms ";
            _mainWindowViewModel.StatusString += $"Count: {fileList.Count}";
#endif
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            _fileInfos.Clear();
            _mainWindowViewModel.StatusString = "";
        }

        private async void DataGrid1_OnDrop(object sender, DragEventArgs e)
        {
            _mainWindowViewModel.StatusString = Empty;
            if (!(e.Data.GetData(DataFormats.FileDrop) is string[] urls)) return;
            var oldList = _fileInfos.Select(info => info.GeneralInfo.FullPath).ToList();
            var ret = await Utils.Load(urls, url => oldList.Contains(url), url => _mainWindowViewModel.StatusString = Path.GetFileName(url));
            _fileInfos.AddItems(ret.info);
            _mainWindowViewModel.StatusString = $"Total time cost: {ret.duration}ms";
        }

        private void DataGrid1_OnDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.All : DragDropEffects.None;
        }

        private void DataGrid1_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is ScrollViewer)
            {
                DataGrid1.UnselectAll();
            }
        }

        private void DataGrid1_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // TODO: Find a refined way
            if (e.Key != Key.Delete) return;
            var selectedItems = DataGrid1.SelectedItems.Cast<FileInfo>().ToList();
            foreach (var item in selectedItems)
            {
                _fileInfos.Remove(item);
            }
        }
    }
}
