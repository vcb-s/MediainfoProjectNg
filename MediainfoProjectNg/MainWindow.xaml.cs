using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MediaInfoLib;

namespace MediainfoProjectNg
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private readonly FileInfos _fileInfos;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private System.Windows.GridLength _rightPanelOriginalWidth;
        public MainWindow()
        {
            InitializeComponent();
            _rightPanelOriginalWidth = RightPanelDef.Width;
            _fileInfos = (FileInfos) FindResource("FileInfos");
            _mainWindowViewModel = (MainWindowViewModel) FindResource("WindowViewModel");
            DataContext = _mainWindowViewModel;

            var v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            _mainWindowViewModel.TitleString = $"mediainfo project ng {v}";

            MediaInfo? MI = null;
            try
            {
                MI = new MediaInfo();
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
                ToggleRightPanelButton.Content = "显示右侧面板";
            }
            else
            {
                RightPanel.Visibility = Visibility.Visible;
                RightPanelDef.Width = _rightPanelOriginalWidth;
                RightPanelDef.MinWidth = 320;
                ToggleRightPanelButton.Content = "隐藏右侧面板";
            }
        }

        private async void DataGrid1_OnDrop(object sender, DragEventArgs e)
        {
            _mainWindowViewModel.StatusString = string.Empty;
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

        private void DataGridRow_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is DataGridRow)) return;
            var row = (DataGridRow) sender;
            var q = (FileInfo) row.Item;
            var win = new TechnicalWindow(q);
            win.Show();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
