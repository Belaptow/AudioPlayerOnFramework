using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;

namespace AudioPlayer
{
    /// <summary>
    /// Interaction logic for ToggleHideableWindow.xaml
    /// </summary>
    public partial class ToggleHideableWindow : Window
    {
        private string toolTip;
        private string arrow;
        private string window;
        private Window windowToOpen;
        public bool opened = false;
        public ToggleHideableWindow(string arrow, string window, string toolTip)
        {
            InitializeComponent();

            this.Owner = Application.Current.MainWindow;

            this.arrow = arrow;
            this.window = window;
            this.toolTip = toolTip;

            toggleAttachedWindowButton.Content = arrow;
            toggleAttachedWindowButton.ToolTip = this.toolTip;

            UpdatePosition();

            switch (window)
            {
                case "Equalizer":
                    //MessageBox.Show("Открытие эквалайзера");
                    windowToOpen = new EqualizerWindow();
                    windowToOpen.Owner = Application.Current.MainWindow;
                    break;
                default:
                    //MessageBox.Show("Некорретный аргумент для открытия окна");
                    toggleAttachedWindowButton.IsEnabled = false;
                    break;
            }
        }

        private void toggleAttachedWindowButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (opened) //Если окно уже открыто
                {
                    switch (arrow)
                    {
                        case "<": //Если закрыть надо справа
                            arrow = ">";
                            toggleAttachedWindowButton.Content = arrow;
                            opened = false;
                            windowToOpen.Hide();
                            UpdatePosition();
                            break;
                        case ">": //Если закрыть надо слева
                            arrow = "<";
                            toggleAttachedWindowButton.Content = arrow;
                            opened = false;
                            windowToOpen.Hide();
                            UpdatePosition();
                            break;
                    }
                }
                else //Если окно ещё не открыто
                {
                    switch (arrow)
                    {
                        case ">": //Если открыть надо справа
                            arrow = "<";
                            toggleAttachedWindowButton.Content = arrow;
                            opened = true;
                            windowToOpen.Show();
                            UpdatePosition();
                            break;
                        case "<": //Если открыть надо слева
                            arrow = ">";
                            toggleAttachedWindowButton.Content = arrow;
                            opened = true;
                            windowToOpen.Show();
                            UpdatePosition();
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public void UpdatePosition()
        {
            try
            {
                if (opened) //Если окно уже открыто
                {
                    switch (arrow)
                    {
                        case "<": //Если окно справа
                            this.Top = this.Owner.Top + 30 + (22 * ((MainWindow)this.Owner).childWindowsRight.IndexOf(this));
                            this.Left = this.Owner.Left + this.Owner.Width + windowToOpen.Width - 5;
                            UpdateWindowToOpenPosition();
                            break;
                        case ">": //Если окно слева
                            this.Top = this.Owner.Top + 30 + (22 * ((MainWindow)this.Owner).childWindowsLeft.IndexOf(this));
                            this.Left = this.Owner.Left - windowToOpen.Width - this.Width + 5;
                            UpdateWindowToOpenPosition();
                            break;
                    }
                }
                else //Если окно закрыто
                {
                    switch (arrow)
                    {
                        case ">": //Если кнопка справа
                            //Debug.WriteLine("Индекс окна справа: " + ((MainWindow)this.Owner).childWindowsRight.IndexOf(this));
                            this.Top = this.Owner.Top + 30 + (22 * ((MainWindow)this.Owner).childWindowsRight.IndexOf(this));
                            this.Left = this.Owner.Left + this.Owner.Width - 5;
                            break;
                        case "<": //Если кнопка слева
                            //Debug.WriteLine("Индекс окна слева: " + ((MainWindow)this.Owner).childWindowsLeft.IndexOf(this));
                            this.Top = this.Owner.Top + 30 + (22 * ((MainWindow)this.Owner).childWindowsLeft.IndexOf(this));
                            this.Left = this.Owner.Left - this.Width + 5;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public void UpdateWindowToOpenPosition()
        {
            if (opened)
            {
                switch (arrow)
                {
                    case "<":
                        windowToOpen.Top = this.Owner.Top + 30;
                        windowToOpen.Left = this.Owner.Left + this.Owner.Width - 5;
                        break;
                    case ">":
                        windowToOpen.Top = this.Owner.Top + 30;
                        windowToOpen.Left = this.Owner.Left - windowToOpen.Width + 5;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
