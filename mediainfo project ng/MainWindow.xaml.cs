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

namespace mediainfo_project_ng
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
#if DEBUG
            var sw = new Stopwatch();
            var toDisplay = string.Empty;

            // TODO: Make it in config file
            var fileList = new List<string>
            {
                @"C:\Users\Mark\Videos\[LoliHouse] Sakura Quest - 18 [WebRip 1920x1080 HEVC-yuv420p10 AAC].mkv",
                @"C:\Users\Mark\Videos\京吹S2\[VCB-Studio] Hibike! Euphonium 2 [Ma10p_1080p]\[VCB-Studio] Hibike! Euphonium 2 [03][Ma10p_1080p][x265_flac_2aac].mkv",
                @"C:\Users\Mark\Videos\[SweetSub&LoliHouse] Flip Flappers [WebRip 1920x1080 HEVC-yuv420p10 AAC]\[SweetSub&LoliHouse] Flip Flappers - 01 [WebRip 1920x1080 HEVC-yuv420p10 AAC].mkv",
            };
            var before = _fileInfoModel.ItemsCount;

            sw.Start();
            toDisplay = fileList.Aggregate(toDisplay, (current, path) => current + Utils.LoadFile(path, ref _fileInfoModel));
            sw.Stop();

            toDisplay += $"Elapsed {sw.ElapsedMilliseconds}ms\r\n";
            toDisplay += $"Count: {_fileInfoModel.ItemsCount - before}\r\n";

            TxtBox.Text = toDisplay;
            // var binding = new Binding {Source = $"列表中已有 {_fileInfoModel.ItemsCount} 个项目"};
            // StatusBlock.SetBinding(TextBlock.TextProperty, binding);
#endif
        }


        private void DataGrid1_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Fix the problem after sorting in View
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
            // TODO: Start a new thread to prevent UI freeze
            foreach (var path in paths)
            {
                if (File.Exists(path))
                    TxtBox.Text += Utils.LoadFile(path, ref _fileInfoModel);
                else if (Directory.Exists(path))
                    TxtBox.Text += Utils.LoadDirectory(path, ref _fileInfoModel);
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