using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TNet;

namespace LandMass_Manager
{
    /// <summary>
    /// Lógica interna para ServerConfig.xaml
    /// </summary>
    public partial class ServerConfig : MetroWindow
    {
        private bool _UPnP;
        private TNet.UPnP GetUPnP = new TNet.UPnP();
        public ServerConfig()
        {
            InitializeComponent();
            {
                Loaded += ServerConfig_Loaded;
                Closing += ServerConfig_Closing;
            }
            Culture.ChangeCulture();
        }

        private void ServerConfig_Loaded(Object sender, RoutedEventArgs e)
        {
            GetPropsInResources();
            {
                AutoFillPort();
            }
        }

        void GetPropsInResources()
        {
            UPnP.IsChecked = Properties.Settings.Default.UPnP;
            udpPort.Text = Properties.Settings.Default.UDPPort.ToString();
        }


        private void ServerConfig_Closing(Object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            {
                this.Hide();
            }
        }

        private void socketPort_TextChanged(Object sender, TextChangedEventArgs e)
        {

        }

        private void name_TextChanged(Object sender, TextChangedEventArgs e)
        {

        }

        private void UPnP_Checked(Object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.UPnP = true;
            Properties.Settings.Default.Save();
        }

        private void UPnP_Unchecked(Object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.UPnP = false;
            Properties.Settings.Default.Save();
        }

        private void udpPort_KeyDown(Object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {
                    Properties.Settings.Default.UDPPort = int.Parse(((TextBox)sender).Text);
                    if (Properties.Settings.Default.UDPPort != MainWindow._portServerLobby)
                    {
                        Properties.Settings.Default.Save();
                        {
                            UnFocus();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Exclusive Port", "SERVER", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch
            {
                MessageBox.Show("Its Crazy????", "SERVER", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void UnFocus()
        {
            Keyboard.ClearFocus();
        }
        async void AutoFillPort()
        {
            while (true)
            {
                if (MainWindow.AllServers[0].isActive)
                {
                    groupServers.IsEnabled = true;
                }
                else { groupServers.IsEnabled = false; }
                //Server 1
                portTCP1.Text = (MainWindow.TCPPort() + 3).ToString();
                portUDP1.Text = (MainWindow.UDPPort() + 4).ToString();
                //Server 2
                portTCP2.Text = (MainWindow.TCPPort() + 3 * 2).ToString();
                portUDP2.Text = (MainWindow.UDPPort() + 4 * 2).ToString();
                //Server 3
                portTCP3.Text = (MainWindow.TCPPort() + 3 * 3).ToString();
                portUDP3.Text = (MainWindow.UDPPort() + 4 * 3).ToString();
                await Task.Delay(3000);
            }
        }
        private void UnBan_Click(Object sender, RoutedEventArgs e)
        {
            string[] FindHWID = File.ReadAllLines(MainWindow.Path);
            for (int i = 0; i < FindHWID.Length; i++)
            {
                if (Regex.IsMatch(FindHWID[i], $@"\b{textunban.Text}\b"))
                {
                    FindHWID[i] = null;
                    {
                        File.WriteAllLines(MainWindow.Path, FindHWID.Where(x => !string.IsNullOrEmpty(x) || !string.IsNullOrWhiteSpace(x)));
                        {
                            MainWindow.singleton.UnBan(textunban.Text);
                            break;
                        }
                    }
                }
            }
        }

        private void OpenList_Click(Object sender, RoutedEventArgs e)
        {
            Process.Start(MainWindow.Path);
        }

        private void textunban_PreviewTextInput(Object sender, TextCompositionEventArgs e)
        {
            //e.Handled = !IsTextAllowed(e.Text);
        }
        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void udpPort_TextChanged(Object sender, TextChangedEventArgs e)
        {
            /////////////////
        }
        void OpenPortUPnP(int _tcp, int _udp)
        {
            GetUPnP.OpenTCP(_tcp, OnOpenPort);
            GetUPnP.WaitForThreads();
            GetUPnP.OpenUDP(_udp, OnOpenPort);
            GetUPnP.WaitForThreads();
        }

        private void OnOpenPort(UPnP up, Int32 port, ProtocolType protocol, Boolean success)
        {
            if (success)
            {
                if (protocol == ProtocolType.Udp)
                {
                    Console.WriteLine($"{up.status} openned port on host {port} using protocol {protocol}");
                }
            }
            else
            {
                Console.WriteLine($"{up.status} port on host {port} using protocol {protocol}");
            }
        }
        int TCPPort;
        int UDPPort;
        void ResetPositionServer()
        {
            MainWindow.OnServerActive -= 1;
        }
        private void Start_Server1_Click(Object sender, RoutedEventArgs e)
        {
            _UPnP = Properties.Settings.Default.UPnP;
            //////////////////////////////////////////////
            TCPPort = int.Parse(portTCP1.Text);
            UDPPort = int.Parse(portUDP1.Text);
            {
                if (_UPnP)
                {
                    OpenPortUPnP(TCPPort, UDPPort);
                }
            }
            ////////////////////////////////
            if (!MainWindow.AllServers[1].isActive)
            {
                MainWindow.singleton.InitializeServer(MainWindow.AllServers[1], TCPPort, UDPPort, false, (Button)sender);
                return;
            }
            //
            GetUPnP.Close();
            GetUPnP.WaitForThreads();
            {
                ResetPositionServer();
                MainWindow.AllServers[1].Stop();
                {
                    ((Button)sender).Content = "Start";
                    ((Button)sender).Background = Brushes.White;
                }
            }
        }

        private void Start_Server2_Click(Object sender, RoutedEventArgs e)
        {
            _UPnP = Properties.Settings.Default.UPnP;
            //////////////////////////////////////////////
            TCPPort = int.Parse(portTCP2.Text);
            UDPPort = int.Parse(portUDP2.Text);
            {
                if (_UPnP)
                {
                    OpenPortUPnP(TCPPort, UDPPort);
                }
            }
            ////////////////////////////////
            if (!MainWindow.AllServers[2].isActive)
            {
                MainWindow.singleton.InitializeServer(MainWindow.AllServers[2], TCPPort, UDPPort, false, (Button)sender);
                return;
            }
            //
            GetUPnP.Close();
            GetUPnP.WaitForThreads();
            {
                ResetPositionServer();
                MainWindow.AllServers[2].Stop();
                {
                    ((Button)sender).Content = "Start";
                    ((Button)sender).Background = Brushes.White;
                }
            }
        }
    }
}
