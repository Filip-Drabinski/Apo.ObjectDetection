using System;
using System.Diagnostics;
using System.Windows;
using ApoObjectDetection.Lib;
using Emgu.CV;
using Emgu.CV.Structure;

namespace ApoObjectDetection.Gui
{
    /// <summary>
    ///     Interaction logic for SetupWizardDialog.xaml
    /// </summary>
    public partial class SetupWizardDialog : Window
    {
        public ObjectDetection ObjDet { get; set; } = null;
        private Image<Bgr, byte> _image;
        public int SureEdgeThreshold = -1;
        public int CloseSize = 0;
        public int OpenSize = 0;
        public int Open2Size = 0;
        public int MinRecArea = -1; 
        public int RecThickness = 1; 
       public bool MergeOverlapingRectangles = true;
        public int AdaptiveThresholdSize = 3;
        private bool init = false;
        public SetupWizardDialog(Image<Bgr, byte> image, ObjectDetection objDet)
        {
            _image = image.Clone();
            ObjDet = objDet;
            InitializeComponent();
            init = true;
            Calculate();
        }
        public SetupWizardDialog()
        {
            _image = new Image<Bgr, byte>(@"C:\Users\Filip\Desktop\Apo projekt Aplikacja\Filip_Drabinski_16923_Projekt_Apo2021\Pliki wykonywalne\Standalone\Obrazy przykladowe i model\bakłazny1.jpg");
            ObjDet = new ObjectDetection(@"C:\Users\Filip\Desktop\Apo projekt Aplikacja\Filip_Drabinski_16923_Projekt_Apo2021\Pliki wykonywalne\Standalone\Obrazy przykladowe i model\model.yml.gz");
            InitializeComponent();
            ImageAdTh.Visibility = Visibility.Collapsed;
            SpAdTh.Visibility = Visibility.Collapsed;
            SpObjectsOverlapping.Visibility = Visibility.Collapsed;
            SpSureEdges.Visibility = Visibility.Collapsed;
            ImageSureEdges.Visibility = Visibility.Collapsed;
            SpClose.Visibility = Visibility.Collapsed;
            ImageClose.Visibility = Visibility.Collapsed;
            SpOpen.Visibility = Visibility.Collapsed;
            ImageOpen.Visibility = Visibility.Collapsed;
            SpOpen2.Visibility = Visibility.Collapsed;
            ImageOpen2.Visibility = Visibility.Collapsed;
            init = true;
            Calculate();
        }

        private void Calculate()
        {
            if(!init)return;
            var recs = ObjDet.GetBoundingRectangles(_image,
                SureEdgeThreshold,
                CloseSize,
                OpenSize,
                Open2Size,
                MinRecArea, MergeOverlapingRectangles, AdaptiveThresholdSize);
            var ImgResult = _image.Clone();
            foreach (var rec in recs)
            {
                CvInvoke.Rectangle(ImgResult, rec, RecColor, RecThickness);
            }

            if(ObjDet.AdaptiveThreshold !=null) ImageAdTh.Source = ObjDet.AdaptiveThreshold.ToImageSource();
            if (ObjDet.AdaptiveWithEdges != null) ImageSureEdges.Source = ObjDet.AdaptiveWithEdges.ToImageSource();
            if (ObjDet.ClosedAdaptiveWithEdges != null) ImageClose.Source = ObjDet.ClosedAdaptiveWithEdges.ToImageSource();
            if (ObjDet.OpenSmalldAdaptiveWithEdges != null) ImageOpen.Source = ObjDet.OpenSmalldAdaptiveWithEdges.ToImageSource();
            if (ObjDet.OpenBigdAdaptiveWithEdges != null) ImageOpen2.Source = ObjDet.OpenBigdAdaptiveWithEdges.ToImageSource();
            ImageResult.Source = ImgResult.ToImageSource();
        }

        public MCvScalar RecColor { get; set; } = new MCvScalar(0,0,255);


        private bool AdaptiveThresholdVal_OnValueChanged(int arg)
        {
            if (arg % 2 == 0 || arg < 3) return false;
            AdaptiveThresholdSize = arg;
            Calculate();
            return true;
        }

        private bool EdgeThreshVal_OnValueChanged(int arg)
        {
            if (arg < -1 || arg >255) return false;
            SureEdgeThreshold = arg;
            Calculate();
            return true;
        }

        private bool MorphCloseVal_OnValueChanged(int arg)
        {
            if (arg < 0) return false;
            CloseSize = arg;
            Calculate();
            return true;
        }

        private bool MorphOpenVal_OnValueChanged(int arg)
        {
            if (arg < 0) return false;
            OpenSize = arg;
            Calculate();
            return true;
        }

        private bool MorphOpen2Val_OnValueChanged(int arg)
        {
            if (arg < 0) return false;
            Open2Size = arg;
            Calculate();
            return true;
        }

        private bool MinRecAreaVal_OnValueChanged(int arg)
        {
            if (arg < -1) return false;
            MinRecArea = arg;
            Calculate();
            return true;
        }

        private void BtnOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private bool MinRecThicknessVal_OnValueChanged(int arg)
        {
            if (arg < -1) return false;
            RecThickness = arg;
            Calculate();
            return true;
        }

        private void Option_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void CorrectDetectionYes_OnClick(object sender, RoutedEventArgs e)
        {
            BoxIntSureEdges.Value = -1;
            ImageAdTh.Visibility = Visibility.Collapsed;
            SpAdTh.Visibility = Visibility.Collapsed;
            SpObjectsOverlapping.Visibility = Visibility.Collapsed;
            if (RbOverlap.IsChecked == true)
            {
                SpSureEdges.Visibility = Visibility.Visible;
                ImageSureEdges.Visibility = Visibility.Visible;
            }
            else
            {
                SpSureEdges.Visibility = Visibility.Collapsed;
                ImageSureEdges.Visibility = Visibility.Collapsed;
            }
            SpClose.Visibility = Visibility.Collapsed;
            ImageClose.Visibility = Visibility.Collapsed;
            SpOpen.Visibility = Visibility.Collapsed;
            ImageOpen.Visibility = Visibility.Collapsed;
            SpOpen2.Visibility = Visibility.Collapsed;
            ImageOpen2.Visibility = Visibility.Collapsed;
            Grid_OnSizeChanged(null, null);
            Calculate();
        }

        private void CorrectDetectionNo_OnClick(object sender, RoutedEventArgs e)
        {
            BoxIntSureEdges.Value = 255;
            ImageAdTh.Visibility = Visibility.Visible;
            SpAdTh.Visibility = Visibility.Visible;
            SpObjectsOverlapping.Visibility = Visibility.Visible;
            if (RbOverlap.IsChecked == true)
            {
                SpSureEdges.Visibility = Visibility.Visible;
                ImageSureEdges.Visibility = Visibility.Visible;
            }
            else
            {
                SpSureEdges.Visibility = Visibility.Collapsed;
                ImageSureEdges.Visibility = Visibility.Collapsed;
            }
            SpClose.Visibility = Visibility.Visible;
            ImageClose.Visibility = Visibility.Visible;
            SpOpen.Visibility = Visibility.Visible;
            ImageOpen.Visibility = Visibility.Visible;
            SpOpen2.Visibility = Visibility.Visible;
            ImageOpen2.Visibility = Visibility.Visible;
            Grid_OnSizeChanged(null, null);
            Calculate();
        }

        private void ObjectsOverlappingYes_OnClick(object sender, RoutedEventArgs e)
        {
            SpSureEdges.Visibility = Visibility.Visible;
            ImageSureEdges.Visibility = Visibility.Visible;
            Grid_OnSizeChanged(null, null);
            Calculate();
        }

        private void ObjectsOverlappingNo_OnClick(object sender, RoutedEventArgs e)
        {
            SpSureEdges.Visibility = Visibility.Collapsed;
            ImageSureEdges.Visibility = Visibility.Collapsed;
            Grid_OnSizeChanged(null, null);
            Calculate();
        }

        private void RectsOverlappingYes_OnClick(object sender, RoutedEventArgs e)
        {
            MergeOverlapingRectangles = true;
            Calculate();
        }

        private void RectsOverlappingNo_OnClick(object sender, RoutedEventArgs e)
        {
            MergeOverlapingRectangles = false;
            Calculate();
        }

        private void Grid_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var cnt = 1;
            if(ImageAdTh.Visibility == Visibility.Visible)            cnt++;
            if(ImageSureEdges.Visibility    == Visibility.Visible)    cnt++;
            if(ImageClose.Visibility        == Visibility.Visible)    cnt++;
            if(ImageOpen.Visibility         == Visibility.Visible)    cnt++;
            if(ImageOpen2.Visibility        == Visibility.Visible)    cnt++;
            if (ImageResult.Visibility == Visibility.Visible) cnt++;
                                    foreach (var gridInnerRowDefinition in GridInner.RowDefinitions)
            {
                gridInnerRowDefinition.MaxHeight = Grid.ActualHeight / cnt*2;
            }

            ImageAdTh.MaxHeight = Grid.ActualHeight / cnt*2;
            ImageSureEdges      .MaxHeight = Grid.ActualHeight / cnt*2;
            ImageClose          .MaxHeight = Grid.ActualHeight / cnt*2;
            ImageOpen           .MaxHeight = Grid.ActualHeight / cnt*2;
            ImageOpen2          .MaxHeight = Grid.ActualHeight / cnt*2;
            ImageResult.MaxHeight = Grid.ActualHeight / cnt*2;
        }
    }
}