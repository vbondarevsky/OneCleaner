using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace OneCleaner.Platform
{
    class InstalledVersionUI : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private string _name;
        private string _version;
        private bool _isChecked;
        private State _state;
        private string _uninstallStr;
        private int _index;
        private string _size;

        public InstalledVersionUI(string name, string version, bool isChecked, State state, string uninstallString, string size)
        {
            this._name = name;
            this._version = version;
            this._isChecked = isChecked;
            this._state = state;
            this._uninstallStr = uninstallString;
            this._size = size;
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        public string Version
        {
            get { return _version; }
            set
            {
                if (_version != value)
                {
                    _version = value;
                    OnPropertyChanged("Version");
                }
            }
        }
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged("IsChecked");
                }
            }
        }
        public State State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged("State");
                }
            }
        }

        public int Index
        {
            get { return _index; }
            set
            {
                if (_index != value)
                {
                    _index = value;
                    OnPropertyChanged("Index");
                }
            }
        }

        public string Size
        {
            get { return _size; }
            set
            {
                if (_size != value)
                {
                    _size = value;
                    OnPropertyChanged("Size");
                }
            }
        }

        protected virtual void OnPropertyChanged(string propChanged)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propChanged));
        }

        public Task<bool> Uninstall()
        {
            return Task.Run(() =>
            {
                try
                {
                    object[] args = new object[0];

#if DEBUG
                    var result = 0;
                    System.Threading.Thread.Sleep(5000);
#else
                    //var result = mngObject.InvokeMethod("Uninstall", args);
                    
                    if(uninstallStr == null)
                    {
                        return false;
                    }

                    var ProcessInfo = new System.Diagnostics.ProcessStartInfo("msiexec.exe", String.Format("/X{0} /quiet",uninstallStr));
                    ProcessInfo.CreateNoWindow = true;
                    ProcessInfo.UseShellExecute = true;

                    var Process = System.Diagnostics.Process.Start(ProcessInfo);
                    Process.WaitForExit();
                    var result = Process.ExitCode;
                    Process.Close();

#endif

                    if (result == 0)
                    {
                        _state = State.UnInstalled;
                        _isChecked = false;
                        OnPropertyChanged("State");
                        OnPropertyChanged("IsChecked");
                        return true;
                    }
                    else
                        return false;

                }
                catch (Exception)
                {
                    return false;
                }
            });
        }
    }

    enum State
    {
        Installed,
        MarkedForUninstall,
        UnInstalled,
        Uninstalling
    }

    [ValueConversion(typeof(State), typeof(FontWeights))]
    public class StateToFontWeightsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((State)value == State.Uninstalling)
                return FontWeights.Bold;
            else
                return FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("This method should never be called");
        }
    }
}
