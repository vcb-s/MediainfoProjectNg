using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MediainfoProjectNg
{
    internal class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }

    class MainWindowViewModel : BaseViewModel
    {
        private string _statusString = string.Empty;
        public string StatusString
        {
            get => _statusString;
            set
            {
                _statusString = value;
                Notify(nameof(StatusString));
            }
        }

        private string _titleString = string.Empty;
        public string TitleString
        {
            get => _titleString;
            set
            {
                _titleString = value;
                Notify(nameof(TitleString));
            }
        }
    }

    public class FileInfos : ObservableCollection<FileInfo>
    {
        public FileInfos()
        {
        }

        public FileInfos(IEnumerable<string> urls)
        {
            foreach (var url in urls)
            {
                Add(new FileInfo(url));
            }
        }

        public void AddItem(string url)
        {
            Add(new FileInfo(url));
        }

        public void AddItem(FileInfo info)
        {
            Add(info);
        }

        public void AddItems(IEnumerable<string> urls)
        {
            foreach (var url in urls)
            {
                Add(new FileInfo(url));
            }
        }

        public void AddItems(IEnumerable<FileInfo> infos)
        {
            foreach (var info in infos)
            {
                Add(info);
            }
        }
    }
}
