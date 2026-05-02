using MediaInfoLib;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MediainfoProjectNg
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private readonly FileInfos _fileInfos;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private GridLength _rightPanelOriginalWidth;
        private readonly string _mediaInfoStatusString = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            _rightPanelOriginalWidth = RightPanelDef.Width;
            _fileInfos = (FileInfos)FindResource("FileInfos");
            _mainWindowViewModel = (MainWindowViewModel)FindResource("WindowViewModel");
            DataContext = _mainWindowViewModel;

            var v = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            _mainWindowViewModel.TitleString = $"mediainfo project ng {v}";

            MediaInfo? MI = null;
            try
            {
                MI = new MediaInfo();
                var version = MI.Option("Info_Version");
                if (version == "Unable to load MediaInfo library")
                {
                    _mediaInfoStatusString = "MediainfoLib unavailable.";
                    _mainWindowViewModel.TitleString += $" [{_mediaInfoStatusString}]";
                    MessageBox.Show("无法载入适用的 MediainfoLib，请检查！", "mediainfo project ng", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    _mediaInfoStatusString = $"MediainfoLib {version[15..]}";
                    _mainWindowViewModel.TitleString += $" [{_mediaInfoStatusString}]";
                    _mainWindowViewModel.StatusString = _mediaInfoStatusString;
                }
            }
            finally
            {
                MI?.Close();
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _fileInfos.Clear();
            _mainWindowViewModel.StatusString = "";
        }

        private void ToggleRightPanelButton_Click(object sender, RoutedEventArgs e)
        {
            if (RightPanel.Visibility == Visibility.Visible)
            {
                RightPanel.Visibility = Visibility.Collapsed;
                _rightPanelOriginalWidth = RightPanelDef.Width;
                RightPanelDef.Width = new GridLength(0);
                RightPanelDef.MinWidth = 0;
                PanelSplitter.Visibility = Visibility.Collapsed;
                ToggleRightPanelButton.Content = "显示右侧面板";
            }
            else
            {
                RightPanel.Visibility = Visibility.Visible;
                RightPanelDef.Width = _rightPanelOriginalWidth;
                RightPanelDef.MinWidth = 320;
                PanelSplitter.Visibility = Visibility.Visible;
                ToggleRightPanelButton.Content = "隐藏右侧面板";
            }
        }

        private async void CaptureWindowButton_Click(object sender, RoutedEventArgs e)
        {
            if (_fileInfos.Count == 0)
            {
                MessageBox.Show("列表中没有可截图的文件。", "mediainfo project ng", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = ".png",
                FileName = $"MPNG-{DateTime.Now:yyyyMMdd-HHmmss}.png",
                Filter = "PNG 图片 (*.png)|*.png",
                Title = "保存窗口截图"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            CaptureWindowButton.IsEnabled = false;
            _mainWindowViewModel.StatusString = "正在生成截图...";

            try
            {
                await SaveFullRowsScreenshotAsync(dialog.FileName);
                _mainWindowViewModel.StatusString = $"截图已保存: {dialog.FileName}";
            }
            catch (Exception ex)
            {
                _mainWindowViewModel.StatusString = "截图失败";
                MessageBox.Show($"截图失败：{ex.Message}", "mediainfo project ng", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CaptureWindowButton.IsEnabled = true;
            }
        }

        private async void DataGrid1_OnDrop(object sender, DragEventArgs e)
        {
            _mainWindowViewModel.StatusString = string.Empty;
            if (e.Data.GetData(DataFormats.FileDrop) is not string[] urls) return;
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

        private void DataGridRow_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGridRow) return;
            var row = (DataGridRow)sender;
            var q = (FileInfo)row.Item;
            var win = new TechnicalWindow(q);
            win.Show();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
