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
        public double scaleY = 1.5;
        public double scaleX = 1.2;


        public EqualizerWindow()
        {
            InitializeComponent();
            foreach(System.Windows.UIElement element in equalizerStackPanel.Children)
            {
                ((Slider)element).Value = double.Parse(MainWindow.settingsFile.DocumentElement["Band" + ((Slider)element).Name.Split('_')[1]].InnerText);
                ((Slider)element).AutoToolTipPlacement = System.Windows.Controls.Primitives.AutoToolTipPlacement.TopLeft;
                ((Slider)element).AutoToolTipPrecision = 1;
            }

            BandValueChanged(Band_1, null);

        }

        private void BandValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                MainWindow.bands[int.Parse(((Slider)sender).Name.Split('_')[1]) - 1].Gain = Convert.ToSingle(((Slider)sender).Value);
                if (MainWindow.equalizer != null) MainWindow.equalizer.Update();

                foreach (System.Windows.UIElement element in equalizerChartGrid.Children)
                {
                    if (curveChoiceCombo.SelectedIndex == 1)
                    {
                        ((Plot)element).Series[0].ItemsSource = null;
                        continue;
                    }

                    ((Plot)element).Series[0].ItemsSource = new GraphBuilder(MainWindow.bands[int.Parse(((Plot)element).Name.Split('_')[1]) - 1].Gain,
                                                                             MainWindow.bands[int.Parse(((Plot)element).Name.Split('_')[2]) - 1].Gain).Points;
                }
                if (curveChoiceCombo.SelectedIndex == 1) drawBezie(); else canvas.Children.Clear();
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
                List<DataPoint> pointsListTest = new List<DataPoint>();

                int multipllier = 0;
                
                foreach (EqualizerBand band in MainWindow.bands)
                {
                    pointsListTest.Add(new DataPoint(50 * multipllier, band.Gain));
                    multipllier++;
                }

                plotTest.Series[0].ItemsSource = pointsListTest;

                Debug.WriteLine("Добавление серии в график");

                plotTest.InvalidatePlot(true);
                Debug.WriteLine("Инвалидация");

                multipllier = 0;
                double canvasStep = 50;
                Debug.WriteLine("Шаг канваса: " + canvasStep);

                Point[] pointsArrayTest = new Point[MainWindow.bands.Length];

                foreach (EqualizerBand band in MainWindow.bands)
                {
                    pointsArrayTest[multipllier] = new Point(canvasStep * multipllier * scaleX, yConvert(band.Gain));
                    multipllier++;
                }

                CustomBezierBuilder testBezierBuilder = new CustomBezierBuilder(pointsArrayTest, canvas);
                testBezierBuilder.Draw();

            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        private void drawBezie()
        {
            try
            {
                int multipllier = 0;
                double canvasStep = 50;
                //Debug.WriteLine("Шаг канваса: " + canvasStep);

                Point[] pointsArrayTest = new Point[MainWindow.bands.Length];

                foreach (EqualizerBand band in MainWindow.bands)
                {
                    pointsArrayTest[multipllier] = new Point(canvasStep * multipllier * scaleX, yConvert(band.Gain));
                    multipllier++;
                }

                CustomBezierBuilder testBezierBuilder = new CustomBezierBuilder(pointsArrayTest, canvas);
                testBezierBuilder.Draw();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        double yConvert(double y)
        {
            return ((y * (-1)) + 30) * scaleY;
        }

        private void curveChoiceCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BandValueChanged(Band_1, null);
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
            int count = (int)Math.Ceiling((Math.PI) / step);
            //Debug.WriteLine("\nКол-во точек: " + count + "\n");
            for (int i = 0; i <= count; i++)
            {
                this.Points.Add(new DataPoint(i*step, Calculate(i*step)));
            }
        }

        public double Calculate(double x)
        {
            //return ((-1) * ((this.leftVal * x) / 50)) +
            //       this.leftVal +
            //       ((this.rightVal * x) / 50);

            //y = a + b * cos(c * x + d) - формула синусоиды

            //y = heightDifference * cos(x + (PI / 2) - тест формула

            return (heightDifference / 2) + ((heightDifference / 2) * Math.Cos(x)) + this.rightVal;
        }

        public IList<DataPoint> Points { get; set; }
    }

}
