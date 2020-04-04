using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LandMass_Manager
{
    /// <summary>
    /// Lógica interna para InputBox.xaml
    /// </summary>
    public partial class InputBox : MetroWindow
    {
        private static InputBox _this = new InputBox();
        /////////////////////////////////////////////////////////
        private static string _command;
        public static string Command {
            get => _command;
            set {
                _command = value;
                {
                    OnCommandChange(value);
                }
            }
        }
        /////////////////////////////////////////////////////////
        public InputBox()
        {
            InitializeComponent();
            {
                Culture.ChangeCulture();
            }
            Loaded += InputBox_Loaded;
            Closing += InputBox_Closing;
        }
        private void InputBox_Loaded(Object sender, RoutedEventArgs e)
        {
            dataPicker.SelectedDate = DateTime.Today;
        }
        void EnableComponents(Visibility _type, params Control[] _controls)
        {
            _controls.ToList().ForEach(x => x.Visibility = _type);
        }
        static void OnCommandChange(string value)
        {
            if (value.Contains("Kick"))
            {
                _this.EnableComponents(Visibility.Hidden, _this.dataPicker, _this.perma);
            }
            else if (value.Contains("Ban")) {
                _this.EnableComponents(Visibility.Visible, _this.dataPicker, _this.perma);
            }
            _this.ShowDialog();
        }
        private void InputBox_Closing(Object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            {
                this.Hide();
            }
        }
        private void ok_Click(Object sender, RoutedEventArgs e)
        {
            this.Hide();
            {
                if (_command.Contains("Kick"))
                {
                    MainWindow.singleton.Kick(reasonBox.Text);
                }
                else if (_command.Contains("Ban"))
                {
                    bool _permanent = perma.IsChecked ?? false;
                    MainWindow.singleton.Ban(reasonBox.Text, _permanent ? null : dataPicker.SelectedDate);
                }
            }
        }

        private void perma_Checked(Object sender, RoutedEventArgs e)
        {
            dataPicker.IsEnabled = false;
        }

        private void perma_Unchecked(Object sender, RoutedEventArgs e)
        {
            dataPicker.IsEnabled = true;
        }
    }
}
