using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using ApoObjectDetection.Lib;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Win32;

namespace ApoObjectDetection.Gui
{
    public partial class MainWindow : Window
    {
        public ObjectDetection ObjDet { get; set; }
        public Image<Bgr, byte> CvImage { get; set; }
        public bool MergeOverlapingRecs { get; set; } = true;
        private int _SureEdge = 31;
        private int _CloseSize = 4;
        private int _OpenSize = 3;
        private int _Open2Size = 59;
        private int _MinRecArea = 50;
        private int _RecThickness = 1;
        private bool pauseUpdate;
        private Image<Bgr, byte> ImgRectangles;
        private Image<Bgr, byte> ImgResult;
        private MCvScalar RecColor = new MCvScalar(0, 0, 255);

        public MainWindow()
        {
            InitializeComponent();
        }


        private void Calculate()
        {
            if (ObjDet == null || pauseUpdate) return;
            var sw = new Stopwatch();
            sw.Start();
            var recs = ObjDet.GetBoundingRectangles(CvImage,
                _SureEdge,
                _CloseSize,
                _OpenSize,
                _Open2Size,
                _MinRecArea, MergeOverlapingRecs, _AdaptiveThresholdSize);
            sw.Stop();
            Title = $"Apo object detection ({sw.ElapsedMilliseconds}ms)";
            ImgResult = CvImage.Clone();
            ImgRectangles = CvImage.Clone();
            ImgRectangles.SetValue(new MCvScalar(255, 255, 255));
            foreach (var rec in recs)
            {
                CvInvoke.Rectangle(ImgResult, rec, RecColor, _RecThickness);
                CvInvoke.Rectangle(ImgRectangles, rec, RecColor, _RecThickness);
            }

            Img.Source = ImgResult.ToImageSource();

            RefreshSteps();
        }

        private void RefreshSteps()
        {
            ImgBlur.Visibility = Visibility.Visible;
            ImgDetector.Visibility = Visibility.Visible;
            ImgSureEdges.Visibility = Visibility.Visible;
            ImgAdaptiveThreshold.Visibility = Visibility.Visible;
            ImgClosedAdaptiveWithEdges.Visibility = Visibility.Visible;
            ImgOpenSmalldAdaptiveWithEdges.Visibility = Visibility.Visible;
            ImgOpenBigdAdaptiveWithEdges.Visibility = Visibility.Visible;
            ImgContours.Visibility = Visibility.Visible;

            if (ObjDet.Blur != null) ImgBlur.Source = ObjDet.Blur.ToImageSource();
            if (ObjDet.Detector != null) ImgDetector.Source = ObjDet.Detector.ToImageSource();
            if (ObjDet.SureEdges != null) ImgSureEdges.Source = ObjDet.SureEdges.ToImageSource();
            if (ObjDet.AdaptiveThreshold != null)
                ImgAdaptiveThreshold.Source = ObjDet.AdaptiveThreshold.ToImageSource();
            if (ObjDet.ClosedAdaptiveWithEdges != null)
                ImgClosedAdaptiveWithEdges.Source = ObjDet.ClosedAdaptiveWithEdges.ToImageSource();
            if (ObjDet.OpenSmalldAdaptiveWithEdges != null)
                ImgOpenSmalldAdaptiveWithEdges.Source = ObjDet.OpenSmalldAdaptiveWithEdges.ToImageSource();
            if (ObjDet.OpenSmalldAdaptiveWithEdges != null)
                ImgOpenSmalldAdaptiveWithEdges.Source = ObjDet.OpenSmalldAdaptiveWithEdges.ToImageSource();
            if (ObjDet.OpenBigdAdaptiveWithEdges != null)
                ImgOpenBigdAdaptiveWithEdges.Source = ObjDet.OpenBigdAdaptiveWithEdges.ToImageSource();
            var imgCpy = CvImage.Clone();
            var imgContours = CvImage.Clone();
            if (_SureEdge < 0)
            {
                ImgSureEdges.Visibility = Visibility.Collapsed;
                ImgAdaptiveThreshold.Visibility = Visibility.Collapsed;
                ImgClosedAdaptiveWithEdges.Visibility = Visibility.Collapsed;
                ImgOpenSmalldAdaptiveWithEdges.Visibility = Visibility.Collapsed;
                ImgOpenBigdAdaptiveWithEdges.Visibility = Visibility.Collapsed;

                ImgBlur.Source = ObjDet.Blur.ToImageSource();
                ImgDetector.Source = ObjDet.Detector.ToImageSource();

                var imgCpy1 = CvImage.Clone();
                imgCpy1.SetValue(new MCvScalar(0, 0, 0));
                var i = 0;
                for (; i < ObjDet.Contours.Size; i++)
                {
                    var color = i % 254 + 1;
                    CvInvoke.DrawContours(imgCpy1, ObjDet.Contours, i, new MCvScalar(color, color, color),
                        Math.Abs(_RecThickness));
                }

                var tmp = imgCpy1.Clone();
                CvInvoke.Threshold(imgCpy1, tmp, 0, 255, ThresholdType.Binary);
                imgCpy1 = ObjectDetection.StretchHistogram(imgCpy1);
                CvInvoke.ApplyColorMap(imgCpy1, imgContours, ColorMapType.Jet);
                CvInvoke.Threshold(imgCpy1, tmp, 0, 255, ThresholdType.BinaryInv);
                CvInvoke.Subtract(imgContours, tmp, imgContours);
            }
            else
            {
                if (_Open2Size != _OpenSize)
                {
                    ImgRectangles = CvImage.Clone();
                    ImgRectangles.SetValue(new MCvScalar(0, 0, 0));
                    var contours1 = CvImage.Clone();
                    var contours2 = CvImage.Clone();
                    var contoursBoth = CvImage.Clone();
                    contours1.SetValue(new MCvScalar(0, 0, 0));
                    contours2.SetValue(new MCvScalar(0, 0, 0));
                    contoursBoth.SetValue(new MCvScalar(0, 0, 0));
                    for (var i = 0; i < ObjDet.Contours.Size; i++)
                    {
                        CvInvoke.DrawContours(contours1, ObjDet.Contours, i,
                            new MCvScalar(i % 254 + 1, i % 254 + 1, i % 254 + 1), Math.Abs(_RecThickness));
                        CvInvoke.DrawContours(contoursBoth, ObjDet.Contours, i, new MCvScalar(255, 255, 255),
                            Math.Abs(_RecThickness));
                    }

                    for (var i = 0; i < ObjDet.Contours2.Size; i++)
                    {
                        CvInvoke.DrawContours(contours2, ObjDet.Contours2, i,
                            new MCvScalar(i % 254 + 1, i % 254 + 1, i % 254 + 1), Math.Abs(_RecThickness));
                        CvInvoke.DrawContours(contoursBoth, ObjDet.Contours2, i, new MCvScalar(255, 255, 255),
                            Math.Abs(_RecThickness));
                    }

                    var bin = contours1.Clone().Gray();
                    var contours1Color = contours1.Clone();
                    var contours2Color = contours1.Clone();
                    CvInvoke.Threshold(contours1.Gray(), bin, 0, 255, ThresholdType.Binary);
                    contours1 = ObjectDetection.StretchHistogram(contours1);

                    CvInvoke.ApplyColorMap(contours1, contours1Color, ColorMapType.Summer);
                    CvInvoke.Threshold(contours1.Gray(), bin, 0, 255, ThresholdType.BinaryInv);
                    CvInvoke.Subtract(contours1Color, bin.Bgr(), contours1Color);

                    var bin2 = contours2.Clone().Gray();
                    CvInvoke.Threshold(contours2.Gray(), bin2, 0, 255, ThresholdType.Binary);
                    contours2 = ObjectDetection.StretchHistogram(contours2);
                    CvInvoke.ApplyColorMap(contours2, contours2Color, ColorMapType.Cool);
                    CvInvoke.Threshold(contours2.Gray(), bin2, 0, 255, ThresholdType.BinaryInv);
                    CvInvoke.Subtract(contours2Color, bin2.Bgr(), contours2Color);


                    var xor = bin.Clone();
                    CvInvoke.BitwiseXor(bin, bin2, xor);

                    CvInvoke.Subtract(contoursBoth, xor.Bgr(), contoursBoth);


                    CvInvoke.Add(contours1Color, contours2Color, imgContours);
                    CvInvoke.Subtract(imgContours, contoursBoth, imgContours);
                    CvInvoke.Threshold(contoursBoth.Gray(), xor, 0, 255, ThresholdType.BinaryInv);

                    CvInvoke.ApplyColorMap(contoursBoth, contoursBoth, ColorMapType.Hsv);
                    CvInvoke.Subtract(contoursBoth, xor.Bgr(), contoursBoth);
                    CvInvoke.Add(imgContours, contoursBoth, imgContours);
                }
                else
                {
                    var contours = CvImage.Clone();
                    contours.SetValue(new MCvScalar(0, 0, 0));
                    for (var i = 0; i < ObjDet.Contours.Size; i++)
                        CvInvoke.DrawContours(contours, ObjDet.Contours, i,
                            new MCvScalar(i % 254 + 1, i % 254 + 1, i % 254 + 1), Math.Abs(_RecThickness));
                    var bin = contours.Clone().Gray();
                    CvInvoke.Threshold(contours.Gray(), bin, 0, 255, ThresholdType.BinaryInv);
                    contours = ObjectDetection.StretchHistogram(contours);
                    CvInvoke.ApplyColorMap(contours, imgContours, ColorMapType.Jet);
                    CvInvoke.Subtract(imgContours, bin.Bgr(), imgContours);
                }
            }

            var binConts = imgContours.Clone().Gray();
            CvInvoke.Threshold(imgContours.Gray(), binConts, 0, 255, ThresholdType.Binary);

            CvInvoke.Subtract(imgCpy, binConts.Bgr(), imgCpy);
            CvInvoke.Add(imgCpy, imgContours, imgCpy);
            ImageContours = imgContours.Clone();
            ImageImageContours = imgCpy.Clone();
            ImgContours.Source = imgCpy.ToImageSource();
        }

        private void BtnOpenModel_OnClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog {Filter = "(*.yml.gz)|*.yml.gz;"};
            if (ofd.ShowDialog() == true)
            {
                ObjDet = new ObjectDetection(ofd.FileName);
                BtnOpenImage.IsEnabled = true;
            }
        }

        private void BtnOpenImage_OnClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                CvImage = new Image<Bgr, byte>(ofd.FileName);
                ScrollViewerSettings.IsEnabled = true;
                BtnSave.IsEnabled = true;
                Calculate();
            }
        }

        private void BtnDownloadLink_OnClick(object sender, RoutedEventArgs e)
        {
            new Window
            {
                Content = new TextBox
                {
                    Text =
                        "https://github.com/opencv/opencv_extra/blob/master/testdata/cv/ximgproc/model.yml.gz"
                },
                Width = 600,
                Height = 70,
                ResizeMode = ResizeMode.NoResize
            }.Show();
        }

        private void BtnSetup_OnClick(object sender, RoutedEventArgs e)
        {
            var setup = new SetupWizardDialog(CvImage, ObjDet);
            if (setup.ShowDialog() == true)
            {
                pauseUpdate = true;
                SureEdge.Value = setup.SureEdgeThreshold;
                CloseSize.Value = setup.CloseSize;
                OpenSize.Value = setup.OpenSize;
                Open2Size.Value = setup.Open2Size;
                MinRecArea.Value = setup.MinRecArea;
                AdaptiveThresholdSize.Value = setup.AdaptiveThresholdSize;
                pauseUpdate = false;
                RecThickness.Value = setup.RecThickness;
            }
        }


        private bool SureNotEdge_OnValueChanged(int arg)
        {
            if (arg < -1 || arg > 255) return false;
            _SureEdge = arg;
            Calculate();
            return true;
        }

        private bool CloseSize_OnValueChanged(int arg)
        {
            if (arg < 0) return false;
            _CloseSize = arg;
            Calculate();
            return true;
        }

        private bool OpenSize_OnValueChanged(int arg)
        {
            if (arg < 0) return false;
            _OpenSize = arg;
            Calculate();
            return true;
        }

        private bool Open2Size_OnValueChanged(int arg)
        {
            if (arg < 0) return false;
            _Open2Size = arg;
            Calculate();
            return true;
        }

        private bool MinRecArea_OnValueChanged(int arg)
        {
            if (arg < -1) return false;
            _MinRecArea = arg;
            Calculate();
            return true;
        }

        private void About_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this,
                "Tytuł projektu: Segmentacja obrazu monochromatycznego zawierających drobne obrazy symboliczne obserwowane przez kamerę na taśmie produkcyjnej i otoczenie ich prostokątem dopasowanym do ich rozmiarów.\n" +
                " Autor: Filip Drabinski\n" +
                " Prowadzący: mgr inż. Łukasz Roszkowiak\n" +
                " Algorytmy Przetwarzania Obrazów 2021\n" +
                " WIT grupa ID: ID06IO2\n", "About");
        }

        private bool RecThickness_OnValueChanged(int arg)
        {
            if (arg < -1) return false;
            _RecThickness = arg;
            Calculate();
            return true;
        }

        private void BtnSaveImage_OnClick(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog {Filter = "*.png, *.jpeg, *.jpg | *.png; *.jpeg; *.jpg;", DefaultExt = ".jpg"};
            if (sfd.ShowDialog() == true) ImgResult.Save(sfd.FileName);
        }

        private void BtnSaveRctangles_OnClick(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog {Filter = "*.png, *.jpeg, *.jpg | *.png; *.jpeg; *.jpg;", DefaultExt = ".jpg"};
            if (sfd.ShowDialog() == true) ImgRectangles.Save(sfd.FileName);
        }

        private bool RectangleColorR_OnValueChanged(int arg)
        {
            if (arg < 0 || arg > 255) return false;
            RecColor.V2 = arg;
            Calculate();
            return true;
        }

        private bool RectangleColoG_OnValueChanged(int arg)
        {
            if (arg < 0 || arg > 255) return false;
            RecColor.V1 = arg;
            Calculate();
            return true;
        }

        private bool RectangleColoB_OnValueChanged(int arg)
        {
            if (arg < 0 || arg > 255) return false;
            RecColor.V0 = arg;
            Calculate();
            return true;
        }

        private void ChbMerge_OnChecked(object sender, RoutedEventArgs e)
        {
            MergeOverlapingRecs = true;
            Calculate();
        }

        private void ToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            MergeOverlapingRecs = false;
            Calculate();
        }

        private void ToggleButtonSteps_OnChecked(object sender, RoutedEventArgs e)
        {
            RefreshSteps();
            Img.Visibility = Visibility.Collapsed;
            SpSteps.Visibility = Visibility.Visible;
        }

        private void ToggleButtonSteps_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Img.Visibility = Visibility.Visible;
            SpSteps.Visibility = Visibility.Collapsed;
        }

        private void ChbDark_OnChecked(object sender, RoutedEventArgs e)
        {
            Calculate();
        }

        public int _AdaptiveThresholdSize { get; set; }

        private bool AdaptiveThresholdSize_OnValueChanged(int arg)
        {
            if (arg % 2 == 0 || arg < 2) return false;
            _AdaptiveThresholdSize = arg;
            Calculate();
            return true;
        }

        private void BtnSaveContours_OnClick(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog { Filter = "*.png, *.jpeg, *.jpg | *.png; *.jpeg; *.jpg;", DefaultExt = ".jpg" };
            if (sfd.ShowDialog() == true)
            {
                var tmp = ImageContours.Clone();
                var bin = ImageContours.Clone().Gray();
                CvInvoke.Threshold(ImageContours.Gray(), bin, 0, 255, ThresholdType.BinaryInv);
                CvInvoke.Add(ImageContours,bin.Bgr(),tmp);
                tmp.Save(sfd.FileName);
            }
        }

        private void BtnSaveImageContours_OnClick(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog { Filter = "*.png, *.jpeg, *.jpg | *.png; *.jpeg; *.jpg;", DefaultExt = ".jpg" };
            if (sfd.ShowDialog() == true) ImageImageContours.Save(sfd.FileName);
        }

        public Image<Bgr,byte> ImageContours { get; set; }
        public Image<Bgr,byte> ImageImageContours { get; set; }
    }
}