using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.XImgproc;

namespace ApoObjectDetection.Lib
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public class ObjectDetection
    {
        public Image<Bgr, byte> Blur;
        public Image<Bgr, byte> Detector;

        public Image<Bgr, byte> SureEdges;
        public Image<Gray, byte> AdaptiveThreshold;
        public Image<Bgr, byte> ClosedAdaptiveWithEdges;
        public Image<Bgr, byte> OpenSmalldAdaptiveWithEdges;
        public Image<Bgr, byte> OpenBigdAdaptiveWithEdges;
        public List<Rectangle> BoundingRectanglesB;

        public VectorOfVectorOfPoint Contours;
        public VectorOfVectorOfPoint Contours2;

        public List<Rectangle> BoundingRectangles;


        public Image<Bgr, byte> AdaptiveWithEdges;


        private readonly StructuredEdgeDetection _detector;
        private List<Rectangle> BoundingRectanglesA;

        public
            ObjectDetection(
                string model) //https://github.com/opencv/opencv_extra/blob/master/testdata/cv/ximgproc/model.yml.gz?raw=true
        {
            _detector = new StructuredEdgeDetection(model, null);
        }

        public List<Rectangle> GetBoundingRectangles(Image<Bgr, byte> img, int sureEdgeThreshold = 31,
            int closeSize = 4, int openSize = 3, int open2Size = 59, int minRecArea = 50,
            bool mergeOverlapingRectangles = true, int AdaptiveThresholdSize = 201)
        {
            ResetVariables();
            Detector = EdgeDetector(img);
            Detector = StretchHistogram(Detector);
            Detector = RemoveSaltAndPepper(Detector, 3);
            Detector = StretchHistogram(Detector);
            //if (!DarkObjects) img = Invert(img);
            Blur = GaussianBlur(img, 9);
            var imgOrig = img.Clone();
            if (sureEdgeThreshold > -1)
            {
                SureEdges = Threshold(Detector, sureEdgeThreshold, ThresholdType.Binary);


                AdaptiveThreshold = imgOrig.Clone().Gray();
                CvInvoke.AdaptiveThreshold(Blur.Gray(), AdaptiveThreshold, 255, AdaptiveThresholdType.GaussianC,
                    ThresholdType.Binary, AdaptiveThresholdSize, 2);


                AdaptiveWithEdges = Add(AdaptiveThreshold.Bgr(), SureEdges);

                ClosedAdaptiveWithEdges = closeSize > 0
                    ? Morphology(AdaptiveWithEdges, MorphOp.Close, 1, closeSize)
                    : AdaptiveWithEdges.Clone();

                OpenSmalldAdaptiveWithEdges = openSize > 0
                    ? Morphology(ClosedAdaptiveWithEdges, MorphOp.Open, 1,
                        openSize)
                    : ClosedAdaptiveWithEdges.Clone();

                CvInvoke.Rectangle(OpenSmalldAdaptiveWithEdges, new Rectangle(0, 0, img.Width, img.Height),
                    new MCvScalar(255, 255, 255), 2);

                Contours = GetContours(OpenSmalldAdaptiveWithEdges);
                BoundingRectanglesA = GetBoundingRectangles(Contours, img.Width * img.Height, minRecArea);
                BoundingRectangles = new List<Rectangle>();
                BoundingRectangles.AddRange(BoundingRectanglesA);
                if (openSize != open2Size)
                {
                    OpenBigdAdaptiveWithEdges = open2Size > 0
                        ? Morphology(ClosedAdaptiveWithEdges, MorphOp.Open, 1, open2Size)
                        : ClosedAdaptiveWithEdges.Clone();
                    CvInvoke.Rectangle(OpenBigdAdaptiveWithEdges, new Rectangle(0, 0, img.Width, img.Height),
                        new MCvScalar(255, 255, 255), 2);
                    Contours2 = GetContours(OpenBigdAdaptiveWithEdges);
                    BoundingRectanglesB = GetBoundingRectangles(Contours2, img.Width * img.Height, minRecArea);
                    BoundingRectangles.AddRange(BoundingRectanglesB);
                }
                else
                {
                    OpenBigdAdaptiveWithEdges = OpenSmalldAdaptiveWithEdges;
                    Contours2 = Contours;
                }

            }
            else
            {
                Contours = Contours2 = GetContours(Detector);
                BoundingRectangles = GetBoundingRectangles(Contours, img.Width * img.Height, minRecArea);
            }

            if (mergeOverlapingRectangles)
                BoundingRectangles = FilterBoundingRectangles(BoundingRectangles, img.Width * img.Height);

            BoundingRectangles.Sort((rectangle, rectangle1) =>
                rectangle1.Width * rectangle1.Height - rectangle.Width * rectangle.Height);
            return BoundingRectangles;
        }

        private void ResetVariables()
        {
            Blur = null;
            Detector = null;
            SureEdges = null;
            AdaptiveThreshold = null;
            ClosedAdaptiveWithEdges = null;
            OpenSmalldAdaptiveWithEdges = null;
            BoundingRectanglesA = null;
            OpenBigdAdaptiveWithEdges = null;
            BoundingRectanglesB = null;
            Contours = null;
            Contours2 = null;
            BoundingRectangles = null;
            AdaptiveWithEdges = null;
        }


        private List<Rectangle> FilterBoundingRectangles(List<Rectangle> boundingRectangles, int imageArea,
            double maxImgPerc = 0.5, double maxRecOverlap = 0.5)
        {
            var result = boundingRectangles.ToList();
            result.Sort((rectangle, rectangle1) =>
                rectangle1.Width * rectangle1.Height - rectangle.Width * rectangle.Height);
            for (var i = 0; i < result.Count; i++)
            {
                if (result[i].Width * result[i].Height > imageArea * maxImgPerc) continue;
                for (var j = i + 1; j < result.Count; j++)
                {
                    if (result[j] == Rectangle.Empty) continue;
                    var recArea = result[j].Width * result[j].Height;
                    var intesection = Rectangle.Intersect(result[i], result[j]);
                    if (!intesection.IsEmpty)
                    {
                        var intersectionArea = intesection.Width * intesection.Height;
                        if (intersectionArea > recArea * maxRecOverlap)
                        {
                            result[i] = Rectangle.Union(result[i], result[j]);
                            result[j] = Rectangle.Empty;
                        }
                    }
                }
            }

            return result.Where(a => !a.IsEmpty).ToList();
        }

        private List<Rectangle> GetBoundingRectangles(VectorOfVectorOfPoint contours, int imageArea, int minimumArea)
        {
            var recsOutp = new List<Rectangle>();
            for (var i = 0; i < contours.Size; i++)
            {
                var cntArea = CvInvoke.ContourArea(contours[i]);
                if (cntArea > 100)
                {
                    var rec = CvInvoke.BoundingRectangle(contours[i]);
                    var recArea = rec.Width * rec.Height;
                    if (recArea > minimumArea && recArea < imageArea * 0.9)
                        recsOutp.Add(rec);
                }
            }

            return recsOutp;
        }

        private VectorOfVectorOfPoint GetContours(Image<Bgr, byte> image)
        {
            var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(image.Gray(), contours, new Mat(), RetrType.List,
                ChainApproxMethod.ChainApproxNone);
            return contours;
        }

        private Image<Bgr, byte> Add(Image<Bgr, byte> img, IInputArray other)
        {
            var result = new Mat();
            CvInvoke.Add(img, other, result);
            return result.ToImage<Bgr, byte>();
        }

        private Image<Bgr, byte> Subtract(Image<Bgr, byte> img, IInputArray other)
        {
            var result = new Mat();
            CvInvoke.Subtract(img, other, result);
            return result.ToImage<Bgr, byte>();
        }

        private Image<Bgr, byte> Invert(Image<Bgr, byte> img)
        {
            var result = new Image<Bgr, byte>(img.Width, img.Height);
            for (var y = 0; y < img.Height; y++)
            for (var x = 0; x < img.Width; x++)
            for (var c = 0; c < img.NumberOfChannels; c++)
                result.Data[y, x, c] = (byte) (255 - img.Data[y, x, c]);

            return result;
        }

        private Image<Bgr, byte> Morphology(Image<Bgr, byte> img,
            MorphOp operation,
            int iterations,
            int size,
            ElementShape shape = ElementShape.Ellipse)
        {
            var result = new Mat();
            var kernel = CvInvoke.GetStructuringElement(shape, new Size(size, size), new Point(-1, -1));
            CvInvoke.MorphologyEx(img, result, operation, kernel, new Point(-1, -1), iterations, BorderType.Replicate,
                new MCvScalar(0));

            return result.ToImage<Bgr, byte>();
        }


        private Image<Bgr, byte> Threshold(Image<Bgr, byte> img, int threshold,
            ThresholdType thresholdType)
        {
            var result = new Mat();
            CvInvoke.Threshold(img, result, threshold, 255, thresholdType);
            return result.ToImage<Bgr, byte>();
        }

        private Image<Bgr, byte> GaussianBlur(Image<Bgr, byte> img, int size)
        {
            var result = new Mat();
            CvInvoke.GaussianBlur(img, result, new Size(size, size), 0, 0, BorderType.Replicate);
            return result.ToImage<Bgr, byte>();
        }

        private Image<Bgr, byte> EdgeDetector(Image<Bgr, byte> img)
        {
            var result = new Mat(img.Width, img.Height, DepthType.Cv32F, 1);
            var imageFloat = new Mat(img.Width, img.Height, DepthType.Cv32F, img.NumberOfChannels);
            img.Mat.ConvertTo(imageFloat, DepthType.Cv32F, 1.0 / 255.0);
            _detector.DetectEdges(imageFloat, result);
            result.ConvertTo(result, DepthType.Cv8U, 255);
            return result.ToImage<Bgr, byte>();
        }

        public static Image<Bgr, byte> StretchHistogram(Image<Bgr, byte> img)
        {
            var result = new Mat();
            CvInvoke.MinMaxIdx(img.Gray(), out var min, out var max, new int[2], new int[2]);
            var dist = max - min;
            var multip = 255.0 / dist;
            img.Mat.ConvertTo(result, DepthType.Default, multip);
            return result.ToImage<Bgr, byte>();
        }


        private Image<Bgr, byte> RemoveSaltAndPepper(Image<Bgr, byte> img, int size)
        {
            var result = img.Convert<Gray, byte>();
            CvInvoke.MedianBlur(result, result, size);
            var imagesXor = new Mat();
            CvInvoke.BitwiseXor(result, result, imagesXor);
            var non0 = CvInvoke.CountNonZero(imagesXor);
            while (non0 > 0)
            {
                CvInvoke.MedianBlur(result, result, size);
                CvInvoke.BitwiseXor(result, result, imagesXor);
                non0 = CvInvoke.CountNonZero(imagesXor);
            }

            return result.Convert<Bgr, byte>();
        }
    }
}