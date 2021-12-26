using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Position.ViewModel
{

    public class MainViewModel : ViewModelBase
    {
        public RelayCommand AddNewPoint { get; private set; }
        public RelayCommand StartServer { get; private set; }

        public MainViewModel()
        {
            ServerStatus = "Server Close";
        }

        private string _newPointName;
        public string NewPointName
        {
            get
            {
                return _newPointName;
            }

            set
            {
                if (_newPointName == value) return;

                _newPointName = value;
                RaisePropertyChanged(() => NewPointName);
            }
        }

        private string _serverStatus;
        public string ServerStatus
        {
            get
            {
                return _serverStatus;
            }

            set
            {
                if (_serverStatus == value) return;

                _serverStatus = value;
                RaisePropertyChanged(() => ServerStatus);
            }
        }

        private string _selectedA;
        public string SelectedA
        {
            get
            {
                return _selectedA;
            }

            set
            {
                if (_selectedA == value) return;

                _selectedA = value;
                RaisePropertyChanged(() => SelectedA);
            }
        }

        private string _selectedB;
        public string SelectedB
        {
            get
            {
                return _selectedB;
            }

            set
            {
                if (_selectedB == value) return;

                _selectedB = value;
                RaisePropertyChanged(() => SelectedB);
            }
        }

        private string _selectedC;
        public string SelectedC
        {
            get
            {
                return _selectedC;
            }

            set
            {
                if (_selectedC == value) return;

                _selectedC = value;
                RaisePropertyChanged(() => SelectedC);
            }
        }

        private string _selectedD;
        public string SelectedD
        {
            get
            {
                return _selectedD;
            }

            set
            {
                if (_selectedD == value) return;

                _selectedD = value;
                RaisePropertyChanged(() => SelectedD);
            }
        }
    }
}