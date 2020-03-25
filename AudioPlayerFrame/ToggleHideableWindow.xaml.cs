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
        public double openedOffset { get; set; }
        private string toolTip;
        private string arrow;
        public string window;
        public Window windowToOpen;
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

            this.openedOffset = 0;

            UpdatePosition();

            switch (window)
            {
                case "Equalizer":
                    //MessageBox.Show("Открытие эквалайзера");
                    windowToOpen = new EqualizerWindow();
                    windowToOpen.Owner = Application.Current.MainWindow;
                    break;
                case "Visualizer":
                    windowToOpen = new Visualizer();
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
                if (opened) //Если окно уже открыто и его надо закрыть
                {
                    UpdateOpenedOffsetOnClose();
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
                else //Если окно ещё не открыто и его надо открыть
                {
                    UpdateOpenedOffsetOnOpen();
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

        //Отступ сверху для кнопки = 
        //Отступ сверху которым обладает родитель этого окна + 
        //30 (высота шапки родителя) + 
        //22 (высота кнопки + 2) * индекс этой кнопки в списке боковых окон родителя +
        //Свдиг сверху, который равен сумме высот всех открытых боковых окон
        public void UpdatePosition()
        {
            try
            {
                if (opened) //Если окно уже открыто
                {
                    switch (arrow)
                    {
                        case "<": //Если окно справа
                            this.Top = this.Owner.Top + 30 + (22 * ((MainWindow)this.Owner).childWindowsRight.IndexOf(this)) + this.openedOffset;
                            this.Left = this.Owner.Left + this.Owner.Width + windowToOpen.Width - 5;
                            UpdateWindowToOpenPosition();
                            break;
                        case ">": //Если окно слева
                            this.Top = this.Owner.Top + 30 + (22 * ((MainWindow)this.Owner).childWindowsLeft.IndexOf(this)) + this.openedOffset;
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
                            this.Top = this.Owner.Top + 30 + (22 * ((MainWindow)this.Owner).childWindowsRight.IndexOf(this)) + this.openedOffset;
                            this.Left = this.Owner.Left + this.Owner.Width - 5;
                            break;
                        case "<": //Если кнопка слева
                            this.Top = this.Owner.Top + 30 + (22 * ((MainWindow)this.Owner).childWindowsLeft.IndexOf(this)) + this.openedOffset;
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

        //Апдейт позиции приреплённого окна
        public void UpdateWindowToOpenPosition()
        {
            if (opened)
            {
                switch (arrow)
                {
                    case "<":
                        windowToOpen.Top = this.Top;
                        windowToOpen.Left = this.Owner.Left + this.Owner.Width - 5;
                        break;
                    case ">":
                        windowToOpen.Top = this.Top;
                        windowToOpen.Left = this.Owner.Left - windowToOpen.Width + 5;
                        break;
                    default:
                        break;
                }
            }
        }

        //Update offset of all windows lower than current on close
        public void UpdateOpenedOffsetOnClose()
        {
            try
            {
                switch (arrow)
                {
                    case "<": //Если обновить смещение нужно справа
                        for (int i = ((MainWindow)this.Owner).childWindowsRight.IndexOf(this) + 1;
                             i < ((MainWindow)this.Owner).childWindowsRight.Count;
                             i++)
                        {
                            ((ToggleHideableWindow)((MainWindow)this.Owner).childWindowsRight[i]).openedOffset -= this.windowToOpen.Height - 20;
                            ((ToggleHideableWindow)((MainWindow)this.Owner).childWindowsRight[i]).UpdatePosition();
                        }
                        break;
                    case ">": //Если обновить смещение нужно слева
                        for (int i = ((MainWindow)this.Owner).childWindowsLeft.IndexOf(this) + 1;
                             i < ((MainWindow)this.Owner).childWindowsLeft.Count;
                             i++)
                        {
                            ((ToggleHideableWindow)((MainWindow)this.Owner).childWindowsLeft[i]).openedOffset -= this.windowToOpen.Height - 20;
                            ((ToggleHideableWindow)((MainWindow)this.Owner).childWindowsLeft[i]).UpdatePosition();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        //Update offset of all windows lower than current on open
        public void UpdateOpenedOffsetOnOpen()
        {
            try
            {
                switch (arrow)
                {
                    case "<": //Если обновить смещение нужно слева
                        for (int i = ((MainWindow)this.Owner).childWindowsLeft.IndexOf(this) + 1; 
                             i < ((MainWindow)this.Owner).childWindowsLeft.Count; 
                             i++)
                        {
                            ((ToggleHideableWindow)((MainWindow)this.Owner).childWindowsLeft[i]).openedOffset += this.windowToOpen.Height - 20;
                            ((ToggleHideableWindow)((MainWindow)this.Owner).childWindowsLeft[i]).UpdatePosition();
                        }
                        break;
                    case ">": //Если обновить смещение нужно справа
                        for (int i = ((MainWindow)this.Owner).childWindowsRight.IndexOf(this) + 1;
                             i < ((MainWindow)this.Owner).childWindowsRight.Count;
                             i++)
                        {
                            ((ToggleHideableWindow)((MainWindow)this.Owner).childWindowsRight[i]).openedOffset += this.windowToOpen.Height - 20;
                            ((ToggleHideableWindow)((MainWindow)this.Owner).childWindowsRight[i]).UpdatePosition();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

    }
}
