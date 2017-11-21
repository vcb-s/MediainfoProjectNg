using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using MediaInfoLib;
using static System.String;

namespace mediainfo_project_ng
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileInfos _fileInfos;

        // TODO: Proper data binding for statusString.
        private string _statusString;

        public MainWindow()
        {
            InitializeComponent();
            _fileInfos = (FileInfos) FindResource("FileInfos");
            var MI = new MediaInfo();
            var toDisplay = MI.Option("Info_Version");
            if (toDisplay.Length == 0)
            {
                toDisplay = " [Mediainfo: Unavailable]";
            }
            else
            {
                toDisplay = $" [Mediainfo: {toDisplay.Substring(15)}]";
            }
            Window1.Title += toDisplay;
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
            _statusString = Empty;

            // TODO: Make it in config file
            var fileList = new List<string>
            {
                @"C:\Users\Mark\Desktop\doc_2017-11-21_12-42-55.mp4",
                @"C:\Users\Mark\Desktop\K-ON!_06mp4.mp4"
            };
            var before = _fileInfos.Count;

            sw.Start();
            foreach (var path in fileList) {
                Utils.LoadFile(path, ref _fileInfos);
            }
            sw.Stop();

            _statusString += $"Elapsed {sw.ElapsedMilliseconds}ms ";
            _statusString += $"Count: {_fileInfos.Count - before}";
#endif
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            _fileInfos.Clear();
            _statusString = "";
        }

        private void DataGrid1_OnDrop(object sender, DragEventArgs e)
        {
            _statusString = Empty;
            var sw = new Stopwatch();
            var paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (paths == null) return;
            sw.Start();
            // TODO: Start a new thread to prevent UI freeze
            foreach (var path in paths)
            {
                if (File.Exists(path))
                    Utils.LoadFile(path, ref _fileInfos);
                else if (Directory.Exists(path))
                    Utils.LoadDirectory(path, ref _fileInfos);
            }
            sw.Stop();
            _statusString += $"Total time cost: {sw.ElapsedMilliseconds}ms";
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
            // TODO: Find a refined way (About synchronize)
            if (e.Key != Key.Delete) return;
            _fileInfos.Remove((FileInfo) DataGrid1.SelectedItem);
        }
    }
}