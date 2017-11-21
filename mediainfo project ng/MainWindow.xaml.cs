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
        private static MainWindowViewModel _mainWindowViewModel;
        public MainWindow()
        {
            InitializeComponent();
            _fileInfos = (FileInfos) FindResource("FileInfos");
            _mainWindowViewModel = (MainWindowViewModel) FindResource("WindowViewModel");
            DataContext = _mainWindowViewModel;
            
            var MI = new MediaInfo();
            var version = MI.Option("Info_Version");
            if (version == "Unable to load MediaInfo library")
            {
                _mainWindowViewModel.TitleString = "mediainfo project ng [Mediainfo: Unavailable]";
                MessageBox.Show("无法载入适用的 mediainfo，请检查！", "mediainfo project ng",MessageBoxButton.OK,MessageBoxImage.Error);
            }
            else
            {
                _mainWindowViewModel.TitleString = $"mediainfo project ng [Mediainfo: {version.Substring(15)}]";
                _mainWindowViewModel.StatusString = $"Mediainfo DLL {version.Substring(15)} at your service.";
            }

            var v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
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
            _mainWindowViewModel.StatusString = Empty;

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

            _mainWindowViewModel.StatusString += $"Elapsed {sw.ElapsedMilliseconds}ms ";
            _mainWindowViewModel.StatusString += $"Count: {_fileInfos.Count - before}";
#endif
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            _fileInfos.Clear();
            _mainWindowViewModel.StatusString = "";
        }

        private void DataGrid1_OnDrop(object sender, DragEventArgs e)
        {
            _mainWindowViewModel.StatusString = Empty;
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
            _mainWindowViewModel.StatusString += $"Total time cost: {sw.ElapsedMilliseconds}ms";
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
            // TODO: Find a refined way (About synchronize)
            if (e.Key != Key.Delete) return;
            _fileInfos.Remove((FileInfo) DataGrid1.SelectedItem);
        }
    }
}