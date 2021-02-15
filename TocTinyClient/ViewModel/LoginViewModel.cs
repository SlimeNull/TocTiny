using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TocTiny.Client.ViewModel
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string acceptButtonContent = "Connect";
        private string nickname = Environment.UserName;
        private string port = "2020";
        private string iPAddress = "chonet.top";

        public string Nickname
        {
            get => nickname;
            set
            {
                nickname = value;
                OnPropertyChanged("Nickname");
            }
        }
        public string IPAddress
        {
            get => iPAddress;
            set
            {
                iPAddress = value;
                OnPropertyChanged("IPAddress");
            }
        }
        public string Port {
            get => port;
            set {
                port = value;
                OnPropertyChanged("Port");
            }
        }
        public string AcceptButtonContent
        {
            get => acceptButtonContent;
            set
            {
                acceptButtonContent = value;
                OnPropertyChanged("AcceptButtonContent");
            }
        }
        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
