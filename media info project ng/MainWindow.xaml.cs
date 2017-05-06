using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using MediaInfoLib;

namespace media_info_project_ng
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileInfoModel _fileInfoModel = new FileInfoModel();

        public MainWindow()
        {
            InitializeComponent();
            DataGrid1.DataContext = _fileInfoModel;
            StatusBlock.DataContext = _fileInfoModel;
            //            var MI = new MediaInfo();
            //            var toDisplay = MI.Option("Info_Version");
            //            if (toDisplay.Length == 0)
            //            {
            //                toDisplay = "MediaInfo.Dll: this version of the DLL is not compatible";
            //            }
            //            StatusBlock.Text = toDisplay;
#if DEBUG
            Button1.IsEnabled = true;
#else
            Button1.IsEnabled = false;
#endif
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            var sw = new Stopwatch();
            var toDisplay = string.Empty;

            sw.Start();

            _fileInfoModel.AddItems(new List<string>
            {
                @"D:\Hanasaku\[Liuyun&VCB-S]HanaSaku Iroha[18][Hi10p_1080p][BDRip][x264_flac_ac3].mkv",
                @"D:\Videos\[HorribleSubs] Zero kara Hajimeru Mahou no Sho - 04 [1080p].mkv",
                @"D:\LLSS67\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [Ma10p_1080p]\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [10][Ma10p_1080p][x265_flac].mkv",
                @"D:\LLSS67\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [Ma10p_1080p]\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [11][Ma10p_1080p][x265_flac].mkv",
                @"D:\LLSS67\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [Ma10p_1080p]\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [12][Ma10p_1080p][x265_flac].mkv",
                @"D:\LLSS67\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [Ma10p_1080p]\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [13][Ma10p_1080p][x265_flac].mkv",
                @"D:\LLSS\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [Ma10p_1080p]\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [01v2][Ma10p_1080p][x265_flac].mkv",
                @"D:\LLSS\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [Ma10p_1080p]\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [02][Ma10p_1080p][x265_flac].mkv",
                @"D:\LLSS\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [Ma10p_1080p]\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [03][Ma10p_1080p][x265_flac].mkv",
                @"D:\LLSS\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [Ma10p_1080p]\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [04][Ma10p_1080p][x265_flac].mkv",
                @"D:\LLSS\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [Ma10p_1080p]\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [05][Ma10p_1080p][x265_flac].mkv",
                @"D:\LLSS\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [Ma10p_1080p]\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [06][Ma10p_1080p][x265_flac].mkv",
                @"D:\LLSS\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [Ma10p_1080p]\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [07][Ma10p_1080p][x265_flac].mkv",
                @"D:\LLSS\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [Ma10p_1080p]\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [08][Ma10p_1080p][x265_flac].mkv",
                @"D:\LLSS\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [Ma10p_1080p]\[Nyamazing&VCB-Studio] LoveLive! Sunshine!! [09][Ma10p_1080p][x265_flac].mkv",
            });

            sw.Stop();

            toDisplay += $"Elapsed {sw.ElapsedMilliseconds}ms\r\n";

            toDisplay += $"Count: {_fileInfoModel.ItemsCount}\r\n";

            TxtBox.Text = toDisplay;
//            var binding = new Binding {Source = $"列表中已有 {_fileInfoModel.ItemsCount} 个项目"};
//            StatusBlock.SetBinding(TextBlock.TextProperty, binding);
        }


        private void DataGrid1_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TxtBox.Text = DataGrid1.SelectedIndex == -1
                ? ""
                : _fileInfoModel.FileInfos[DataGrid1.SelectedIndex].Summary;
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            _fileInfoModel.Clear();
            TxtBox.Text = "";
        }

        private void DataGrid1_OnDrop(object sender, DragEventArgs e)
        {
            TxtBox.Text = string.Empty;
            var sw = new Stopwatch();
            var paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (paths == null) return;
            sw.Start();
            foreach (var path in paths)
            {
                switch (File.GetAttributes(path))
                {
                    case FileAttributes.Archive:
                    {
                        TxtBox.Text += Utils.LoadFile(path, ref _fileInfoModel);
                        break;
                    }
                    case FileAttributes.Directory:
                        TxtBox.Text += Utils.LoadDirectory(path, ref _fileInfoModel);
                        break;
                }
            }
            sw.Stop();
            TxtBox.Text += $"\r\nTotal time cost: {sw.ElapsedMilliseconds}ms\r\n";
        }

        private void DataGrid1_OnDragEnter(object sender, DragEventArgs e)
        {
            // TODO: It doesn't work!!!! 
            // Edited: May work, but no effect on the mouse cursor
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.All : DragDropEffects.None;
        }

        private void DataGrid1_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is ScrollViewer)
            {
                ((DataGrid) sender).UnselectAll();
            }
        }

        private void DataGrid1_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete) return;
            _fileInfoModel.RemoveItem(DataGrid1.SelectedIndex);
        }
    }
}