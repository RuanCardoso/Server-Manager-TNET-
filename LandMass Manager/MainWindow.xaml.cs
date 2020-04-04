using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Threading;
using MahApps.Metro.Controls;
using MahApps.Metro;
using System.Windows.Threading;
using System.IO;
using System.Globalization;
using TNet;
using System.Net.Sockets;
using System.Net;
using System.Net.Http;

namespace LandMass_Manager
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        /////////////////////////////////////////////////////////
        public static MainWindow singleton;
        /////////////////////////////////////////////////////////
        private ServerConfig config = new ServerConfig();
        private static LobbyServer _lobbyServer = new UdpLobbyServer();
        public static string Path = string.Concat(Environment.CurrentDirectory, "\\Server\\Ban.ini");
        /////////////////////////////////////////////////////////
        public static GameServer Server = new GameServer()
        {
            lobbyLink = new LobbyServerLink(_lobbyServer)
        };
        public static GameServer ServerTwo = new GameServer()
        {
            lobbyLink = new LobbyServerLink(_lobbyServer)
        };
        public static GameServer ServerTree = new GameServer()
        {
            lobbyLink = new LobbyServerLink(_lobbyServer)
        };
        public static GameServer[] AllServers = { Server, ServerTwo, ServerTree};
        /////////////////////////////////////////////////////////
        private string HWID;
        private static int ServerActive = 0;
        public static int OnServerActive {
            get => ServerActive;
            set {
                ServerActive = value;
                OnServerChange();
            }
        }
        /////////////////////////////////////////////////////////
        private UPnP uPnP = new UPnP();
        bool UPnP;
        public static int _portServerLobby = 5128;
        ///
        int w, h, wX, hY;
        void FileCreate()
        {
            if (Directory.CreateDirectory(string.Concat(Environment.CurrentDirectory, "\\Server")).Exists)
            {
                if (!File.Exists(Path))
                {
                    try
                    {
                        File.Create(Path);
                    }
                    catch
                    {
                        MessageBox.Show("Access Violation", "SYSTEM", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        public MainWindow()
        {
            singleton = this;
            FileCreate();
            {
                w = Console.WindowWidth;
                h = Console.WindowHeight;
                wX = w / 2 + (30);
                hY = h / 2;
            }
            {
                var handle = GetConsoleWindow();
                Console.SetWindowSize(wX, hY);
                {
                    Console.SetBufferSize(wX, hY);
                    Console.Title = "Server";
                }
                ShowWindow(handle, 6);
            }
            /////////////////////////////////////////////////////////
            Process[] Proc;
            if ((Proc = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName)).Length > 1)
            {
                MessageBox.Show("Multiple Server Instances Not Suported!\nBut this Instance needs to work on a different port.", "SYSTEM", MessageBoxButton.OK, MessageBoxImage.Warning);
                {
                    Environment.Exit(0);
                }
            }
            /////////////////////////////////////////////////////////
            InitializeComponent();
            {
                Loaded += MainWindow_Loaded;
                Closing += MainWindow_Closing;
            }
            Culture.ChangeCulture();
        } 
        ///////////////////////////////////// Eventos
        private void MainWindow_Loaded(Object sender, RoutedEventArgs e)
        {
            GetPropsInResources();
        }
        int serverLenght;
        System.Collections.Generic.List<GameServer> ServerIsOn()
        {
            return AllServers.Where(x => x.isActive).ToList();
        }
        private void MainWindow_Closing(Object sender, CancelEventArgs e)
        {
            if (Started)
            {
                if (MessageBox.Show("Stop The Server?", "SERVER", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    uPnP.Close();
                    uPnP.WaitForThreads();
                    {
                        AllServers.Where(x => x.isActive).ToList().ForEach(y => y.Stop());
                    }
                    _lobbyServer.Stop();
                }
                else
                {
                    e.Cancel = true;
                }
            }
            if (!e.Cancel)
                Environment.Exit(0);
        }
        /////////////////////////////////////////// Funções
        void EnableComponents(bool _enabled, params Control[] _controls)
        {
            this.BeginInvoke(() =>
            {
                _controls.ToList().ForEach(x => x.IsEnabled = _enabled);
            });
        }
        async void AddPlayerToList()
        {
            try
            {
                while (ServerIsOn()[ServerActive].isActive && ServerIsOn()[ServerActive].players.Count > -1)
                {
                    listPlayers.Items.Clear();
                    foreach (var _t in ServerIsOn()[ServerActive].players)
                    {
                        if (listPlayers.FindChild<ListViewItem>() is null)
                        {
                            if (string.IsNullOrEmpty(filterPlayer.Text) || string.IsNullOrWhiteSpace(filterPlayer.Text))
                            {
                                listPlayers.Items.Add(new ListViewItem() { Content = _t.name });
                            }
                            else
                            {
                                if (_t.name.IndexOf(filterPlayer.Text, StringComparison.OrdinalIgnoreCase) > -1)
                                {
                                    listPlayers.Items.Add(new ListViewItem() { Content = _t.name });
                                }
                            }
                        }
                    }
                    await Task.Delay(100);
                }
            }
            catch
            { }
        }
        public void InitializeServer(GameServer gameServer_, int _tcp, int _udp, bool _enable, Button _sender)
        {
            gameServer_.onPlayerConnect += OnPlayerConnect;
            gameServer_.onCustomPacket += OnCustomPackets;
            //////////////////////////////////////////////
            if (gameServer_.Start(_tcp, _udp))
            {
                if (_enable)
                {
                    EnableComponents(true, GroupPlayers, GroupTools);
                    {  }
                }
                this.BeginInvoke(() =>
                {
                    AddPlayerToList();
                });
                {
                    this.BeginInvoke(() =>
                    {
                        _sender.Content = "Stop";
                        _sender.Background = Brushes.DeepSkyBlue;
                    });
                }
                Started = true;
                //////////////////////////////////////
                MessageBox.Show($"Server Estabilished TCP/UDP: {_tcp}:{_udp}", "SERVER", MessageBoxButton.OK, MessageBoxImage.Information);
                { }
            }
        }

        private void OnCustomPackets(TcpPlayer player, TNet.Buffer buffer, BinaryReader reader, Byte request, Boolean reliable)
        {
            string read = reader.ReadString();
            switch (read)
            {
                case "CheckBan":
                    CheckBan(player);
                    break;
            }
        }

        private void OnPlayerConnect(Player p)
        {      }
        void CheckBan(TcpPlayer p)
        {
            string _hwid_;
            ////////////////////////
            string[] FindHWID = File.ReadAllLines(Path);
            foreach (string _l in FindHWID)
            {
                try
                {
                    if (_l.Contains((_hwid_ = p.aliases.buffer[0])))
                    {
                        if (!AutoUnban(_hwid_, p.name))
                        {
                            Kick(p.name, _hwid_);
                        }
                        break;
                    }
                }
                catch
                {
                    Console.WriteLine("Um Usuário Excedeu o Tempo Limite de Conexão");
                    break;
                }
            }
        }
        bool AutoUnban(string hwid_, string name)
        {
            DateTime Date;
            ////////////////////////
            string[] FindHWID = File.ReadAllLines(Path);
            for (int i = 0; i < FindHWID.Length; i++)
            {
                if (FindHWID[i].Contains(hwid_))
                {
                    string DateSplit = FindHWID[i].Split(new string[] { "??", "??" }, StringSplitOptions.None)[1];
                    if (DateTime.TryParseExact(DateSplit, "dd/MM/yyyy hh:mm:ss", Culture.GetCulture(), DateTimeStyles.None, out Date))
                    {
                        if (DateTime.Compare(Date, DateTime.Today) <= 0)
                        {
                            FindHWID[i] = null; // Desban
                            {
                                File.WriteAllLines(Path, FindHWID.Where(x => !string.IsNullOrEmpty(x) || !string.IsNullOrWhiteSpace(x)));
                                {
                                    Console.WriteLine($"[{name}] -> ({hwid_}) Desban Automatico");
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[{name}] -> ({hwid_}) Desconectado -> Banido até {Date.ToString("dd/MM/yyyy hh:mm:ss")}");
                            return false;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[{name}] -> ({hwid_}) Desconectado -> Ban Permanente");
                        return false;
                    }
                }
            }
            return false;
        }
        public static int TCPPort()
        {
            return Properties.Settings.Default.TCPPort;
        }
        public static int UDPPort()
        {
            return Properties.Settings.Default.UDPPort;
        }
        void GetPropsInResources()
        {
            GameServerPortChange.Text = TCPPort().ToString();
        }
        public void Ban(string reason, DateTime? time)
        {
            using (StreamWriter writer = new StreamWriter(Path, true))
            {
                writer.WriteLine($"HWID -> ({HWID}) : [{player.Text}] -> \"{reason}\" ??{time}??");
                {
                    Kick(reason);
                    {
                        SendReason(reason, "Banido");
                        {
                            AddBanToDatabase(player.Text, time);
                        }
                    }
                }
            }
        }
        async void AddBanToDatabase(string username, DateTime? time)
        {
            using(HttpClient client = new HttpClient())
            {
                Dictionary<string, string> nameValueCollection = new Dictionary<string, string>()
                {
                    { "data", time.ToString() },
                    { "name", username }
                };
                //-----------------------------------------//
                var content = new FormUrlEncodedContent(nameValueCollection);
                //--------------------------------------//
                var response = await client.PostAsync("http://landmass.com.br/Achievements/ban.php", content);
                //---------------------------------------//
                var msg = await response.Content.ReadAsStringAsync();
            }
        }
        public void UnBan(string _hwid)
        {
            Console.WriteLine($"HWID: {_hwid} Desbanido", "SERVER", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        public void Kick(string reason)
        {
            ServerIsOn().ForEach(y => y.Kick(player.Text));
            {
                SendReason(reason, "Expulso");
            }
        }
        public void Kick(string playerName, string _HWID)
        {
            ServerIsOn().ForEach(y => y.Kick(_HWID));
            { }
        }
        void SendReason(string _reason, string type)
        {
            MessageBox.Show($"[{player.Text}] {type} por: {_reason}", "SERVER", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        ///////////////////////////////////// UI Events
        bool Started = false;
        private void Start_Click(Object sender, RoutedEventArgs e)
        {
            if (!Started)
            {
                UPnP = Properties.Settings.Default.UPnP;
                if (!UPnP)
                {
                    _lobbyServer.Start(_portServerLobby);
                    {
                        InitializeServer(AllServers[0], TCPPort(), UDPPort(), true, (Button)sender);
                        { }
                    }
                    return;
                }
                uPnP.OpenTCP(TCPPort(), OnOpenPort);
                uPnP.WaitForThreads();
                uPnP.OpenUDP(UDPPort(), OnOpenPort);
                uPnP.WaitForThreads();
                uPnP.OpenUDP(_portServerLobby, OnOpenPort);
                uPnP.WaitForThreads();
            }
            else
            {
                if (AllServers[1].isActive || AllServers[2].isActive)
                {
                    MessageBox.Show("The other servers depend on this to be active, stop them.", "SERVER", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    uPnP.Close();
                    uPnP.WaitForThreads();
                    {
                        AllServers.Where(x => x.isActive).ToList().ForEach(y => y.Stop());
                        {
                            Started = false;
                        }
                        _lobbyServer.Stop();
                    }
                    ((Button)sender).Content = "Start";
                    ((Button)sender).Background = Brushes.White;
                }
            }
        }

        private void OnOpenPort(UPnP up, Int32 port, ProtocolType protocol, Boolean success)
        {
            if (success)
            {
                if ((protocol == ProtocolType.Udp) && port == _portServerLobby)
                {
                    _lobbyServer.Start(_portServerLobby);
                    {
                        InitializeServer(AllServers[0], TCPPort(), UDPPort(), true, Start);
                        { }
                    }
                }
                Console.WriteLine($"{up.status} openned port on host {port} using protocol {protocol}");
            }
            else
            {
                Console.WriteLine($"{up.status} port on host {port} using protocol {protocol}");
            }
        }

        private void Restart_Click(Object sender, RoutedEventArgs e)
        {
            
        }

        private void listPlayers_MouseDoubleClick(Object sender, MouseButtonEventArgs e)
        {
            string _Player = ((ListViewItem)((ListView)sender).SelectedItem).Content.ToString();
            try
            {
                player.Text = _Player;
                HWID = AllServers[ServerActive].players.ToArray().Where(x => x.name == _Player).ToList()[0].aliases.buffer[0];
                hwidLabel.Content = $"HWID: {HWID}";
            }
            catch { }
        }

        private void banClick_Click(Object sender, RoutedEventArgs e)
        {
            InputBox.Command = "Ban";
        }
   
        private void listPlayers_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            /////
        }

        private void GameServerPortChange_TextChanged(Object sender, TextChangedEventArgs e)
        {
            /////
        }

        private void ComboServer_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            if (!Started) { ((ComboBox)sender).SelectedIndex = 0; return; }
            serverLenght = ServerIsOn().Count;
            {
                int index = ((ComboBox)sender).SelectedIndex;
                if (index < serverLenght)
                {
                    ServerActive = index;
                }
                else
                {
                    ((ComboBox)sender).SelectedIndex -= 1;
                    {
                        Console.WriteLine("This server is not running", "SERVER", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        static void OnServerChange()
        {
            singleton.ComboServer.SelectedIndex -= 1;
        }

        private void filterPlayer_TextChanged(Object sender, TextChangedEventArgs e)
        {

        }

        private void GameServerPortChange_KeyDown(Object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {
                    Properties.Settings.Default.TCPPort = int.Parse(((TextBox)sender).Text);
                    if (Properties.Settings.Default.TCPPort != _portServerLobby)
                    {
                        Properties.Settings.Default.Save();
                        {
                            ServerConfig.UnFocus();
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

        private void kickClick_Click(Object sender, RoutedEventArgs e)
        {
            InputBox.Command = "Kick";
        }
        
        private void Hilab_Click(Object sender, RoutedEventArgs e)
        {
            config.ShowDialog();
        }
    }
}
