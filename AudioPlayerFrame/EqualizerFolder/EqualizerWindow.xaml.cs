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
using OxyPlot;
using OxyPlot.Wpf;
using OxyPlot.Axes;

namespace AudioPlayer
{
    /// <summary>
    /// Interaction logic for Equalizer.xaml
    /// </summary>
    public partial class EqualizerWindow : Window
    {
        public Dictionary<string, GraphBuilder> graphs = new Dictionary<string, GraphBuilder>();

        public EqualizerWindow()
        {
            InitializeComponent();
            foreach(System.Windows.UIElement element in equalizerStackPanel.Children)
            {
                ((Slider)element).Value = double.Parse(MainWindow.settingsFile.DocumentElement["Band" + ((Slider)element).Name.Split('_')[1]].InnerText);
            }

            foreach(System.Windows.UIElement element in equalizerChartGrid.Children)
            {
                graphs.Add(((Plot)element).Name, new GraphBuilder(MainWindow.bands[int.Parse(((Plot)element).Name.Split('_')[1]) - 1].Gain,
                                                                  MainWindow.bands[int.Parse(((Plot)element).Name.Split('_')[2]) - 1].Gain));
                ((Plot)element).Series[0].ItemsSource = graphs[((Plot)element).Name].Points;
                plot_4_5.InvalidatePlot(true);
            }

        }

        private void BandValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                MainWindow.bands[int.Parse(((Slider)sender).Name.Split('_')[1]) - 1].Gain = Convert.ToSingle(((Slider)sender).Value);
                if (MainWindow.equalizer != null) MainWindow.equalizer.Update();

                foreach (System.Windows.UIElement element in equalizerChartGrid.Children)
                {
                    ((Plot)element).Series[0].ItemsSource = new GraphBuilder(MainWindow.bands[int.Parse(((Plot)element).Name.Split('_')[1]) - 1].Gain,
                                                                             MainWindow.bands[int.Parse(((Plot)element).Name.Split('_')[2]) - 1].Gain).Points;
                    plot_4_5.InvalidatePlot(true);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        #region Отображение кнопок-картинок
        //Увеличение/уменьшение размера изображения при наведении мышью
        private void ImageButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ((Image)sender).Width += 1;
            ((Image)sender).Height += 1;
        }

        private void ImageButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ((Image)sender).Width -= 1;
            ((Image)sender).Height -= 1;
        }
        #endregion

        private void resetEqualizer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                foreach(System.Windows.UIElement element in equalizerStackPanel.Children)
                {
                    ((Slider)element).Value = 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        private void saveEqualizer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                foreach(System.Windows.UIElement element in equalizerStackPanel.Children)
                {
                    MainWindow.settingsFile.DocumentElement["Band" + ((Slider)element).Name.Split('_')[1]].InnerText = ((Slider)element).Value.ToString();
                }
                MainWindow.settingsFile.Save(MainWindow.pathToSettingsFile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        private void testButtonChart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("Начало добавления хлама в график");

                GraphBuilder testBuilder = new GraphBuilder(0, 0);
                //LineSeries testSeries = new LineSeries();

                //plot_1_2.Axes.Add(new OxyPlot.Wpf.LinearAxis()
                //{
                //    Position = AxisPosition.Bottom,
                //    IsAxisVisible = false
                //});

                //plot_1_2.Axes.Add(new OxyPlot.Wpf.LinearAxis()
                //{
                //    Position = AxisPosition.Left,
                //    IsAxisVisible = false
                //});

                //testSeries.ItemsSource = testBuilder.Points;
                //plot_1_2.Series.Add(testSeries);

                //foreach(DataPoint point in testBuilder.Points)
                //{
                //    Debug.WriteLine(point.X + "  :  " + point.Y);
                //}

                plot_4_5.Series[0].ItemsSource = testBuilder.Points;

                Debug.WriteLine("Добавление серии в график");

                plot_4_5.InvalidatePlot(true);
                Debug.WriteLine("Инвалидация");
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }
    }

    public class GraphBuilder
    {
        public int id { get; set; }

        public string plotName { get; set; }

        public double leftVal { get; set; }

        public double rightVal { get; set; }

        public double heightDifference;

        public GraphBuilder(double leftVal, double rightVal)
        {
            double step = 0.1;
            this.leftVal = leftVal;
            this.rightVal = rightVal;
            this.heightDifference = leftVal - rightVal;
            this.Points = new List<DataPoint>();
            int count = (int)Math.Ceiling((50) / step);
            Debug.WriteLine("\nКол-во точек: " + count + "\n");
            for (int i = 0; i <= count; i++)
            {
                this.Points.Add(new DataPoint(i*step, Calculate(i*step)));
            }
        }

        public double Calculate(double x)
        {
            return ((-1) * ((this.leftVal * x) / 50)) +
                   this.leftVal +
                   ((this.rightVal * x) / 50);
        }

        public IList<DataPoint> Points { get; set; }
    }

}
