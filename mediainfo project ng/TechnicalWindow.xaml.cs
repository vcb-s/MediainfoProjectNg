using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace mediainfo_project_ng
{
    class KeyValue
    {
        public string Key { get; }
        public string Value { get; }

        public KeyValue(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }

    class KeyChildren
    {
        public string Key { get; }
        public List<object> Children { get; } = new List<object>();

        public KeyChildren(string key)
        {
            Key = key;
        }
    }

    /// <summary>
    /// TechnicalWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TechnicalWindow : Window
    {
        public TechnicalWindow(FileInfo info)
        {
            InitializeComponent();
            var detail = GetTreeStructure(info.GeneralInfo.Filename, info);
            // TODO: Unify two checks
            var errorInfos = Utils.CheckFile(info);
            DataContext = new
            {
                ErrorInfos = errorInfos,
                Detail = detail
            };
        }

        private static KeyChildren GetTreeStructure(string name, object o)
        {
            var props = o.GetType().GetProperties();
            var d = new KeyChildren(name);
            foreach (var prop in props)
            {
                var value = prop.GetValue(o);

                switch (value)
                {
                    case GeneralInfo _:
                    case FileInfo _:
                    case AudioInfo _:
                    case ChapterInfo _:
                    case ProfileInfo _:
                        d.Children.Add(GetTreeStructure(prop.Name, value));
                        break;
                    case IList list:
                        var keyChildren = new KeyChildren(prop.Name);
                        for (var i = 0; i < list.Count; i++)
                        {
                            var item = list[i];
                            keyChildren.Children.Add(GetTreeStructure($"{item.GetType().Name}[{i}]", item));
                        }

                        d.Children.Add(keyChildren);
                        break;
                    default:
                        if (prop.Name == "Summary" && value is string sum)
                        {
                            d.Children.Add(new KeyChildren(prop.Name) {Children = {new KeyValue("", sum)}});
                        }
                        else if ((prop.Name == "Duration" || prop.Name == "Timespan") && value is int ms)
                        {
                            var ticks = (long) ms * 10000;
                            var ts = new TimeSpan(ticks);
                            d.Children.Add(new KeyValue(prop.Name, ts.ToString(@"hh\:mm\:ss\.fff")));
                        }
                        else
                        {
                            d.Children.Add(new KeyValue(prop.Name, value.ToString()));
                        }

                        break;
                }
            }

            return d;
        }

        private void MenuItemCopy_OnClick(object sender, RoutedEventArgs e)
        {
            var s = (MenuItem) e.OriginalSource;
            Clipboard.SetText(((KeyValue) s.DataContext).Value);
        }

        private void MenuItemKeyValuePairCopy_OnClick(object sender, RoutedEventArgs e)
        {
            var s = (MenuItem) e.OriginalSource;
            var keyValue = (KeyValue) s.DataContext;
            Clipboard.SetText($"{keyValue.Key}: {keyValue.Value}");
        }
    }
}