using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.String;

namespace mediainfo_project_ng
{
    internal class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }

    class MainWindowViewModel : BaseViewModel
    {
        private string _statusString = Empty;
        public string StatusString
        {
            get => _statusString;
            set
            {
                _statusString = value;
                Notify(nameof(StatusString));
            }
        }

        private string _titleString = Empty;
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

        public void AddItems(IEnumerable<string> urls)
        {
            foreach (var url in urls)
            {
                Add(new FileInfo(url));
            }
        }
    }
}
