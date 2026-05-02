using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MediainfoProjectNg
{
    public partial class MainWindow
    {
        private const int MaxBitmapDimension = 32767;
        private const double MaxMeasureWidth = 20000;
        private const double StatusBarMinWidth = 500;
        private const double StatusBarRightPadding = 4;
        private const double DefaultColumnWidth = 80;
        private const double DataGridWidthPadding = 4;

        private async Task SaveFullRowsScreenshotAsync(string filePath)
        {
            if (Content is not FrameworkElement root)
            {
                throw new InvalidOperationException("无法找到窗口内容。");
            }

            if (root.ActualWidth <= 0 || root.ActualHeight <= 0)
            {
                throw new InvalidOperationException("窗口尚未完成布局，无法截图。");
            }

            var (screenshotSurface, screenshotDataGrid) = BuildScreenshotSurface();
            await UpdateScreenshotLayoutAsync(screenshotSurface, MaxMeasureWidth);

            var screenshotWidth = Math.Ceiling(GetScreenshotSurfaceWidth(screenshotDataGrid));
            screenshotSurface.Width = screenshotWidth;
            screenshotDataGrid.Width = Math.Ceiling(GetDataGridContentWidth(screenshotDataGrid));
            await UpdateScreenshotLayoutAsync(screenshotSurface, screenshotWidth);

            var captureSize = new Size(screenshotWidth, Math.Ceiling(screenshotSurface.DesiredSize.Height));
            SaveVisualAsPng(screenshotSurface, captureSize, filePath);
        }

        private async Task UpdateScreenshotLayoutAsync(FrameworkElement surface, double width)
        {
            surface.Measure(new Size(width, double.PositiveInfinity));
            surface.Arrange(new Rect(0, 0, width, Math.Ceiling(surface.DesiredSize.Height)));
            surface.UpdateLayout();
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle);
        }

        private (FrameworkElement Surface, DataGrid DataGrid) BuildScreenshotSurface()
        {
            var statusBar = BuildScreenshotStatusBar();
            var dataGrid = BuildScreenshotDataGrid();
            DockPanel.SetDock(statusBar, Dock.Bottom);

            var root = new DockPanel
            {
                Background = SystemColors.WindowBrush,
                LastChildFill = true,
                Children = { statusBar, dataGrid }
            };

            return (root, dataGrid);
        }

        private StatusBar BuildScreenshotStatusBar()
        {
            var statusBar = new StatusBar();
            var statusBarItem = new StatusBarItem
            {
                Content = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width = GridLength.Auto }
                    },
                    Children =
                    {
                        BuildStatusBarText(_mediaInfoStatusString, 0),
                        BuildStatusBarText($"列表中共有 {_fileInfos.Count} 个文件", 2)
                    }
                },
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = new Thickness(0, 0, StatusBarRightPadding, 0)
            };
            BindingOperations.SetBinding(
                statusBarItem,
                WidthProperty,
                new Binding(nameof(ActualWidth)) { Source = statusBar });
            statusBar.Items.Add(statusBarItem);

            return statusBar;
        }

        private static TextBlock BuildStatusBarText(string text, int column)
        {
            var textBlock = new TextBlock { Text = text };
            Grid.SetColumn(textBlock, column);
            return textBlock;
        }

        private DataGrid BuildScreenshotDataGrid()
        {
            var dataGrid = new DataGrid
            {
                HorizontalGridLinesBrush = DataGrid1.HorizontalGridLinesBrush,
                VerticalGridLinesBrush = DataGrid1.VerticalGridLinesBrush,
                IsReadOnly = true,
                Margin = DataGrid1.Margin,
                HeadersVisibility = DataGrid1.HeadersVisibility,
                CanUserAddRows = false,
                AutoGenerateColumns = false,
                SelectionUnit = DataGridSelectionUnit.FullRow,
                SelectionMode = DataGridSelectionMode.Extended,
                ItemsSource = _fileInfos,
                EnableRowVirtualization = false,
                EnableColumnVirtualization = false,
                RowStyle = DataGrid1.RowStyle,
                CellStyle = DataGrid1.CellStyle
            };

            VirtualizingPanel.SetIsVirtualizing(dataGrid, false);
            ScrollViewer.SetCanContentScroll(dataGrid, false);
            ScrollViewer.SetVerticalScrollBarVisibility(dataGrid, ScrollBarVisibility.Disabled);
            ScrollViewer.SetHorizontalScrollBarVisibility(dataGrid, ScrollBarVisibility.Disabled);

            foreach (var column in GetScreenshotColumns())
            {
                dataGrid.Columns.Add(CloneColumn(column));
            }

            return dataGrid;
        }

        private DataGridColumn[] GetScreenshotColumns()
        {
            var columns = DataGrid1.Columns
                .Where(column => column.Visibility == Visibility.Visible)
                .OrderBy(column => column.DisplayIndex)
                .ToArray();
            var fullPathIndex = Array.FindIndex(columns, IsFullPathColumn);
            return fullPathIndex > 0 ? columns[..fullPathIndex] : columns;
        }

        private static bool IsFullPathColumn(DataGridColumn column)
        {
            return string.Equals(column.Header?.ToString(), "完整路径", StringComparison.Ordinal);
        }

        private static double GetScreenshotSurfaceWidth(DataGrid dataGrid)
        {
            var dataGridContentWidth = GetDataGridContentWidth(dataGrid);
            var dataGridWidth = dataGridContentWidth + dataGrid.Margin.Left + dataGrid.Margin.Right;
            return Math.Min(MaxMeasureWidth, Math.Max(StatusBarMinWidth, dataGridWidth));
        }

        private static double GetDataGridContentWidth(DataGrid dataGrid)
        {
            var columnsWidth = dataGrid.Columns
                .Where(column => column.Visibility == Visibility.Visible)
                .Sum(column => column.ActualWidth > 0 ? column.ActualWidth : Math.Max(column.MinWidth, DefaultColumnWidth));
            return Math.Ceiling(columnsWidth + DataGridWidthPadding);
        }

        private static DataGridColumn CloneColumn(DataGridColumn source)
        {
            DataGridColumn clone = source switch
            {
                DataGridTextColumn textColumn => new DataGridTextColumn
                {
                    Binding = CloneBindingBase(textColumn.Binding),
                    ElementStyle = textColumn.ElementStyle,
                    EditingElementStyle = textColumn.EditingElementStyle
                },
                _ => throw new NotSupportedException($"截图暂不支持列类型 {source.GetType().Name}。")
            };

            clone.Header = source.Header;
            clone.Width = DataGridLength.Auto;
            clone.MinWidth = source.MinWidth;
            clone.MaxWidth = source.MaxWidth;
            clone.CellStyle = source.CellStyle;
            clone.HeaderStyle = source.HeaderStyle;
            clone.Visibility = source.Visibility;
            clone.IsReadOnly = source.IsReadOnly;
            clone.SortMemberPath = source.SortMemberPath;
            clone.CanUserResize = source.CanUserResize;
            clone.CanUserSort = source.CanUserSort;
            clone.CanUserReorder = source.CanUserReorder;

            return clone;
        }

        private static BindingBase? CloneBindingBase(BindingBase? source)
        {
            if (source is null)
            {
                return null;
            }

            if (source is not Binding binding)
            {
                return source;
            }

            var clone = new Binding
            {
                Path = binding.Path,
                XPath = binding.XPath,
                Mode = binding.Mode,
                UpdateSourceTrigger = binding.UpdateSourceTrigger,
                Converter = binding.Converter,
                ConverterCulture = binding.ConverterCulture,
                ConverterParameter = binding.ConverterParameter,
                FallbackValue = binding.FallbackValue,
                TargetNullValue = binding.TargetNullValue,
                StringFormat = binding.StringFormat,
                BindsDirectlyToSource = binding.BindsDirectlyToSource,
                ValidatesOnDataErrors = binding.ValidatesOnDataErrors,
                ValidatesOnExceptions = binding.ValidatesOnExceptions,
                NotifyOnSourceUpdated = binding.NotifyOnSourceUpdated,
                NotifyOnTargetUpdated = binding.NotifyOnTargetUpdated,
                NotifyOnValidationError = binding.NotifyOnValidationError
            };

            if (binding.ElementName is not null)
            {
                clone.ElementName = binding.ElementName;
            }
            else if (binding.RelativeSource is not null)
            {
                clone.RelativeSource = binding.RelativeSource;
            }
            else if (binding.Source is not null)
            {
                clone.Source = binding.Source;
            }

            foreach (var validationRule in binding.ValidationRules)
            {
                clone.ValidationRules.Add(validationRule);
            }

            return clone;
        }

        private static void SaveVisualAsPng(Visual visual, Size size, string filePath)
        {
            var dpi = VisualTreeHelper.GetDpi(visual);
            var pixelWidth = (int)Math.Ceiling(size.Width * dpi.DpiScaleX);
            var pixelHeight = (int)Math.Ceiling(size.Height * dpi.DpiScaleY);

            if (pixelWidth <= 0 || pixelHeight <= 0)
            {
                throw new InvalidOperationException("截图尺寸无效。");
            }

            if (pixelWidth > MaxBitmapDimension || pixelHeight > MaxBitmapDimension)
            {
                throw new InvalidOperationException($"截图尺寸过大（{pixelWidth}x{pixelHeight}），超过当前位图上限。");
            }

            var renderTarget = new RenderTargetBitmap(
                pixelWidth,
                pixelHeight,
                96 * dpi.DpiScaleX,
                96 * dpi.DpiScaleY,
                PixelFormats.Pbgra32);

            renderTarget.Render(visual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTarget));

            using var stream = File.Create(filePath);
            encoder.Save(stream);
        }
    }
}
