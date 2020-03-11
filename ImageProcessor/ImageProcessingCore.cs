using DcamWrapper;
using Entities;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

/// <summary>
/// This namespace contains all classes and methods which allow the image processing algorithms to function
/// </summary>
namespace ImageProcessor
{
    /// <summary>
    /// This class contains the image processing algorithms and settings. Each one of these can be accessed through this class
    /// </summary>
    // Image processesing core. Every algorithm or setting can be accesed through here
    public class ImageProcessingCore
    {
        //[DllImport("DCAM.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        //public static extern IntPtr DCAMDLL_GetMatFrame();

        //[DllImport("DCAM.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        //public static extern void DCAMDLL_FreeMatPtr();

        //[DllImport("DCAM.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        //public static extern string External_GetFrameBuffer();

        private static VideoCapture capture;
        ThresholdingAlgorithms threshAlgorithms;
        ThresholdingAlgorithmsSettings taSettings;
        ImageDistanceCalibration idc;
        ImageDistanceCalibrationSettings idcSettings;
        TrackingSettings trackingSettings;
        VideoFeedSettings videoFeedSettings;
        BoundingBoxCalibration boundingBoxCalib;
        DcamCoreWrapper dcam;


        //EngineWrapper.EngineWrapper engineWrapper;

        OpenCvSharp.Rect ROI;
        Mat frame, dummyImg, imgGrayscale, newImgROI, invertedImg, imgBlurred, imgThreshold, imgMedian, imgComposed;
        Moments mom;

        /// <summary>
        /// Implicit constructor, initializes algorithms and settings
        /// </summary>
        public ImageProcessingCore()
        {
            this.threshAlgorithms = new ThresholdingAlgorithms(this);
            this.TASettings = new ThresholdingAlgorithmsSettings();
            this.idc = new ImageDistanceCalibration(this);
            this.IDCSettings = new ImageDistanceCalibrationSettings();
            this.TrackingSettings = new TrackingSettings();
            this.VideoFeedSettings = new VideoFeedSettings();
            this.boundingBoxCalib = new BoundingBoxCalibration();
            dcam = new DcamCoreWrapper();

            this.frame = new Mat();
            this.ROI = new OpenCvSharp.Rect();
            this.dummyImg = new Mat();
            this.imgGrayscale = new Mat();
            this.newImgROI = new Mat();
            this.invertedImg = new Mat();
            this.imgBlurred = new Mat();
            this.imgThreshold = new Mat();
            this.imgMedian = new Mat();
            this.imgComposed = new Mat();
            this.mom = new Moments();
        }

        /// <summary>
        /// Initialize the image capturing process of the Hamamatsu digital camera. Call once at start of capture
        /// </summary>
        /// <returns>object which can be used to access the exposed methods of Hamamatsu's API, of type EngineWrapper.EngineWrapper</returns>
        public void StartHamamatsu()
        {
            dcam.Init();
        }

        // See dummy documentation for implementation
        public (Mat, bool) GetHamamatsuFrame()
        {
            Bitmap bitmap;

            //EngineWrapper.EngineWrapper engine = new EngineWrapper.EngineWrapper();
            //engineWrapper.GetNewFrame();


            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                //enc.Frames.Add(BitmapFrame.Create(engineWrapper.GetNewFrame()));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }

            return (OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap), true);
        }



        public (Mat, bool) DummyHamamatsuInterop()
        {

            //V1
            //Mat dcamFrame = OpenCvSharp.Extensions.BitmapSourceConverter.ToMat(dcam.GetBitmapSourceFrame());

            //V2 (dangerous)
            //IntPtr matPtr = DCAMDLL_GetMatFrame();
            //Mat dcamFrame = new Mat(matPtr).Clone();
            //DCAMDLL_FreeMatPtr();
            //DcamWrapper.DcamCoreWrapper dcam = new DcamWrapper.DcamCoreWrapper();

            //V3
            Mat dcamFrame = OpenCvSharp.Extensions.BitmapConverter.ToMat(dcam.GetBitmapFrame());

            // V4
            //Mat dcamFrame = OpenCvSharp.Extensions.BitmapConverter.ToMat(dcam.GetBitmapFrameFromBuffer());

            return (dcamFrame, true);
        }

        /// <summary>
        /// Start the capturing of a video feed using a connected and valid USB camera. Be sure
        /// to set the caputure index in VideoCapture(index) accordingly <br/>
        ///Frames per second maximum limit can be set from here (also see ImageProcessor.ImageProcessingCore.SetFramesPerSecond()) 
        /// </summary>
        public void StartCapture()
        {
            int fps = 60;

            capture = new VideoCapture(0);
            SetFramesPerSecond(fps);
        }

        /// <summary>
        /// Start the capturing of a video feed using a connected and valid USB camera with a desired resolution
        /// </summary>
        /// <param name="width">number of horizontal pixels, of type int</param>
        /// <param name="height">number of vertical pixels, of type int</param>
        public void StartCaptureWithResolution(int width, int height)
        {
            capture = new VideoCapture(0);

            if (capture != null && capture.IsDisposed == false)
            {
                capture.Set(CaptureProperty.FrameHeight, height);
                capture.Set(CaptureProperty.FrameWidth, width);
            }
        }

        /// <summary>
        /// Stop the capturing of a video feed using a connected and valid USB camera
        /// </summary>
        public void StopCapture()
        {
            if (capture != null && capture.IsDisposed == false)
            {
                capture.Release();
                capture.Dispose();
                Notification.NotifList.Add(new Notification(Enums.NotificationType.Camera, "Video feed was stopped"));
            }
        }

        /// <summary>
        /// Used for setting the maximum frames per second amount of the capturing stream using an USB camera
        /// </summary>
        /// <param name="value">frames per second, of type int</param>
        public void SetFramesPerSecond(int value)
        {
            if (capture != null && capture.IsDisposed == false)
                capture.Set(CaptureProperty.Fps, value);
        }

        // See dummy reference for documentation
        public (int, int) GetResolution()
        {
            if (capture != null && capture.IsDisposed == false)
            {
                double width = capture.Get(CaptureProperty.FrameWidth);
                double height = capture.Get(CaptureProperty.FrameHeight);

                return ((int)width, (int)height);
            }
            else return (VideoFeedSettings.ImageWidth, VideoFeedSettings.ImageHeight);
        }

        // See dummy reference for documentation
        public (BitmapSource, System.Drawing.Point) StartThresholdingAlgorithms(System.Drawing.Point actuatorPositionPixels)
        {
            return threshAlgorithms.Run(actuatorPositionPixels);
        }

        // See dummy reference for documentation
        public (BitmapSource, int) StartImageDistanceCalibration()
        {
            return idc.Run();
        }

        /// <summary>
        /// Sets a number of image processing parameters
        /// </summary>
        public void SetImageProcessingParameters()
        {
            TASettings.THRESH_DIVIDE_VAL = TASettings.Precision;
            TASettings.BLUR_KERNEL_SIZE = TASettings.MEDIAN_KERNEL_SIZE = TASettings.Quality;
        }

        /// <summary>
        /// Checks whether the USB camera capture object is disposed, and starts capturing using
        /// ImageProcessor.ImageProcessingCore.StartCapture() if true
        /// </summary>
        // TO DO - check functionality
        public void IsVideoCaptureDisposed()
        {
            if (Capture.IsDisposed)
                StartCapture();
        }

        // See dummy reference for documentation
        public (Mat, bool) TryReadVideoCaptureFrame()
        {
            // Read frame from capturing device
            Capture.Read(frame);

            // If size is invalid => frame is invalid
            if (frame.Height * frame.Width == 0)
                return (dummyImg, false);

            else return (frame, true);
        }

        /// <summary>
        /// Crops an image to a square shape <br/>
        ///Image processing algorithm 
        /// </summary>
        /// <param name="img">input image, of type OpenCvSharp.Mat</param>
        /// <returns>output image, of type OpenCvSharp.Mat</returns>
        public Mat CropToROI(ref Mat img)
        {
            // If images are the same height and width, no crop is necessary
            if (img.Width == img.Height)
                return img;

            // If image width is larger than image height
            else if (img.Width > img.Height)
                this.ROI = new OpenCvSharp.Rect((img.Width - img.Height) / 2, 0, img.Height, img.Height);

            // If image height is larger than image width
            else this.ROI = new OpenCvSharp.Rect((img.Height - img.Width) / 2, 0, img.Width, img.Width);

            // Create empty image with specified size and fill it with data
            new Mat(img, ROI).CopyTo(newImgROI);

            return newImgROI;
        }

        /// <summary>
        /// Converts an image to grayscale <br/>
        ///Image processing algorithm 
        /// </summary>
        /// <param name="img">input image, of type OpenCvSharp.Mat</param>
        /// <returns>output image, of type OpenCvSharp.Mat</returns>
        public Mat ConvertToGrayscale(ref Mat img)
        {
            Cv2.CvtColor(img, this.imgGrayscale, ColorConversionCodes.BGR2GRAY);

            return imgGrayscale;
        }

        /// <summary>
        /// Sets image brightness level
        ///Image processing algorithm 
        /// </summary>
        /// <param name="img">input image, of type OpenCvSharp.Mat</param>
        /// <returns>output image, of type OpenCvSharp.Mat</returns>
        public Mat SetImageBrightness(ref Mat imgGrayscale)
        {
            imgGrayscale.ConvertTo(imgGrayscale, MatType.CV_8UC1, 1, TASettings.Brightness);

            return imgGrayscale;
        }

        /// <summary>
        /// Applies a box filter blur on an image <br/>
        ///Image processing algorithm 
        /// </summary>
        /// <param name="img">input image, of type OpenCvSharp.Mat</param>
        /// <returns>output image, of type OpenCvShart\p.Mat</returns>
        public Mat SimpleImageBlur(ref Mat img)
        {
            Cv2.Blur(img, imgBlurred, new OpenCvSharp.Size(TASettings.BLUR_KERNEL_SIZE, TASettings.BLUR_KERNEL_SIZE), new OpenCvSharp.Point(-1, -1));

            return imgBlurred;
        }

        /// <summary>
        /// Calculated the histogram of an image <br/>
        ///Image processing algorithm 
        /// </summary>
        /// <param name="img">input image, of type OpenCvSharp.Mat</param>
        /// <returns>output histogram image, of type OpenCvSharp.Mat</returns>
        public Mat CalculateHistogram(Mat img)
        {
            Mat histogram = new Mat();
            Cv2.CalcHist(new Mat[] { img }, new int[] { 0 }, new Mat(), histogram, 1, new int[] { TASettings.HIST_BINS_COUNT }, new Rangef[] { new Rangef(0, TASettings.HIST_BINS_COUNT) }, true, false);

            // Create a window and display historgam
            //ShowHistogram(Mat.Zeros(IPP.HIST_HEIGHT, IPP.HIST_BINS_COUNT, MatType.CV_8UC1), histogram, IPP.HIST_HEIGHT, IPP.HIST_BINS_COUNT);

            return histogram;
        }

        /// <summary>
        /// Create a window and display a visual representation of the histogram of an image
        /// </summary>
        /// <param name="imgHistogram">empty base image, of type OpenCvSharp.Mat</param>
        /// <param name="histogram">histogram of an image, of type OpenCvSharp.Mat</param>
        /// <param name="histogramHeight">height of histogram representation, in pixel, of type int</param>
        /// <param name="binCount">bin count of histogram representation, in pixels, of type int</param>
        // TO DO - fail for IDC high brightness
        public void ShowHistogram(Mat imgHistogram, Mat histogram, int histogramHeight, int binCount)
        {
            double maxHistValue = FindMaxHistogramValue(histogram);

            for (int binIndex = 0; binIndex < binCount; binIndex++)
                Cv2.Line(imgHistogram,
                         new OpenCvSharp.Point(binIndex, histogramHeight - Convert.ToInt32(histogram.At<float>(binIndex) * histogramHeight / maxHistValue)),
                         new OpenCvSharp.Point(binIndex, histogramHeight),
                         Scalar.All(255));

            Cv2.ImShow("Hitogram", imgHistogram);
        }

        /// <summary>
        /// Get the maximum value of a histogram, using a MinMaxLoc() method
        /// </summary>
        /// <param name="hisogram">input histogram, of type OpenCvSharp.Mat</param>
        /// <returns>maximum histogram value, of type double</returns>
        public double FindMaxHistogramValue(Mat hisogram)
        {
            Cv2.MinMaxLoc(hisogram, out double minValue, out double maxValue);

            return maxValue;
        }

        /// <summary>
        /// Get the index of the maximum value of a histogram, using a MinMaxLoc() method <br/>
        /// </summary>
        /// <param name="hisogram">input histogram, of type OpenCvSharp.Mat</param>
        /// <param name="maxHistValue">DEPRECATED, maximum histogram value, of type double</param>
        /// <returns>index of maximum histogram value, of type int</returns>
        public int FindMaxHistogramIndex(Mat histogram, double maxHistValue)
        {
            Cv2.MinMaxLoc(histogram, out double minValue, out double maxValue, out OpenCvSharp.Point minHistIndex, out OpenCvSharp.Point maxHistIndex);

            return maxHistIndex.Y;
        }

        /// <summary>
        /// Find the divided index for a histogram, used in the dynamic threshiolding algorithms
        /// </summary>
        /// <param name="histogram">input histogram, of type OpenCvSharp.Mat</param>
        /// <param name="histCalculation">histogram calculation direction, of type Entities.Enums.HistogramCalculation</param>
        /// <param name="maxHistValue">maximum value in histogram, of type double</param>
        /// <param name="maxHistIndex">index of maximum value in histogram, of type int</param>
        /// <returns>division value, of type int</returns>
        public int FindHistogramDividedIndex(Mat histogram, Enums.HistogramCalculation histCalculation, double maxHistValue, int maxHistIndex)
        {
            if (histCalculation == Enums.HistogramCalculation.Normal)
            {
                // Divided threshold value will me maximum threshold value divided by predefined value (THRESH_DIVIDE_VAL)
                for (int dividedValIdx = maxHistIndex; dividedValIdx < TASettings.HIST_BINS_COUNT; dividedValIdx++)
                    if (histogram.At<float>(dividedValIdx) <= ((float)maxHistValue / TASettings.THRESH_DIVIDE_VAL))
                        return dividedValIdx;

                // Return maximum histogram value if no other value was found
                return (int)maxHistValue;
            }
            else
            {
                for (int dividedValIdx = maxHistIndex; dividedValIdx >= 0; dividedValIdx--)
                    if (histogram.At<float>(dividedValIdx) <= ((float)maxHistValue / TASettings.THRESH_DIVIDE_VAL))
                        return dividedValIdx;

                // Return maximum histogram value if no other value was found
                return (int)maxHistValue;
            }
        }

        /// <summary>
        /// Get threshold value used in the dynamic thresholding algoritm and apply an offset if necessary
        /// </summary>
        /// <param name="dividedValIndex">value calculated in ImageProcessor.ImageProcessingCore.FindHistogramDividedIndex(), of type int</param>
        /// <returns>threshold value, of type int</returns>
        public int GetThresholdValue(int dividedValIndex)
        {
            return dividedValIndex + TASettings.THRESH_OFFSET_AMOUNT;
        }

        /// <summary>
        /// Apply a threshold filter on an image <br/>
        ///Image processing algorithm 
        /// </summary>
        /// <param name="img">input image, of type OpenCvSharp.Mat</param>
        /// <param name="thresholdValue">threshold value, of type int</param>
        /// <param name="threshType">threshold type, of type OpenCvSharp.ThresholdTypes</param>
        /// <returns>output image, of type OpenCvSharp.Mat</returns>
        public Mat ThresholdFilter(ref Mat img, int thresholdValue, ThresholdTypes threshType)
        {
            Cv2.Threshold(img, imgThreshold, thresholdValue, 255, threshType);

            return imgThreshold;
        }

        /// <summary>
        /// Apply a median filter on an image <br/>
        ///Image processing algorithm 
        /// </summary>
        /// <param name="img">input image, of type OpenCvSharp.Mat</param>
        /// <returns>output image, of type OpenCvSharp.Mat</returns>
        public Mat MedianFilter(ref Mat img)
        {
            for (int i = 1; i < TASettings.MEDIAN_KERNEL_SIZE; i += 2)
                Cv2.MedianBlur(img, imgMedian, i); ;

            return imgMedian;
        }

        /// <summary>
        /// Find all contours in an image <br/>
        ///Image processing algorithm
        /// </summary>
        /// <param name="img">input image, of type OpenCvSharp.Mat</param>
        /// <returns>all contours, of type OpenCvSharp.Mat[]</returns>
        public Mat[] FindAllContours(ref Mat img)
        {
            Cv2.FindContours(img, out Mat[] allContours, dummyImg, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            return allContours;
        }

        /// <summary>
        /// Find the contour with the largest area in an image <br/>
        ///Image processing algorithm
        /// </summary>
        /// <param name="img">input image, of type OpenCvSharp.Mat</param>
        /// <param name="allContours">all found contours in an image, of type OpenCvSharp.Mat[]</param>
        /// <returns>contour with largest area, of type OpenCvSharp.Mat</returns>
        // TO DO
        // Simplify?
        public ref Mat FindLargestContourArea(ref Mat img, ref Mat[] allContours)
        {
            double maxAreaValue = -1;
            int maxAreaIndex = -1;

            for (int i = 0; i < allContours.Count(); i++)
            {
                // Compute contour area
                double newArea = Cv2.ContourArea(allContours[i]);

                if (newArea >= maxAreaValue)
                {
                    maxAreaValue = newArea;
                    maxAreaIndex = i;
                }
            }

            return ref allContours[maxAreaIndex];
        }

        /// <summary>
        /// Find largest 2 contours by area in an image, used for ImageProcessor.ImageDistanceClaibration <br/>
        ///Image processing algorithm
        /// </summary>
        /// <param name="img">input image, of type OpenCvSharp.Mat</param>
        /// <param name="allContours">all found contours in an image, of type OpenCvSharp.Mat[]</param>
        /// <returns>largest 2 contours by area, of type OpenCvSharp.Mat[]</returns>
        public Mat[] FindLargestTwoContours(ref Mat img, ref Mat[] allContours)
        {
            if (allContours.Length > 1)
            {
                for (int i = 0; i < allContours.Length - 1; i++)
                    for (int j = i + 1; j < allContours.Length; j++)
                        if ((allContours[i].Height * allContours[i].Width) < (allContours[j].Height * allContours[j].Width))
                        {
                            Mat temp = allContours[i];
                            allContours[i] = allContours[j];
                            allContours[j] = temp;
                        }

                return new Mat[] { allContours[0], allContours[1] };
            }
            else
                return new Mat[] { };
        }

        /// <summary>
        /// Find the center point of a contour <br/>
        ///Image processing algorithm
        /// </summary>
        /// <param name="img">input image, of type OpenCvSharp.Mat</param>
        /// <param name="largestContourArea">desired contour, of type OpenCvSharp.Mat</param>
        /// <returns>contour center point, of type OpenCvSharp.Point</returns>
        public OpenCvSharp.Point FindContourCenter(ref Mat img, Mat largestContourArea)
        {
            Moments mom = new Moments(largestContourArea);

            return new OpenCvSharp.Point(mom.M10 / mom.M00, mom.M01 / mom.M00);
        }

        /// <summary>
        /// Apply an invert filter on an image <br/>
        ///Image processing algorithm
        /// </summary>
        /// <param name="img">input image, of type OpenCvSharp.Mat</param>
        /// <returns>output image, of type OpenCvSharp.Mat</returns>
        public Mat InvertImage(ref Mat img)
        {

            Cv2.BitwiseNot(img, invertedImg);

            return invertedImg;
        }

        /// <summary>
        /// Calculate the euclidian distance. The euclidian distance is the linear (shortest) distance in euclidian space between 2 points
        /// </summary>
        /// <param name="pointsList">points used for calculating the euclidian distance, of type List<OpenCvSharp.Point></param>
        /// <returns>euclidian distance, of type double</returns>
        public double CalculateEuclidianDistance(List<OpenCvSharp.Point> pointsList)
        {
            return Math.Sqrt(Math.Pow(pointsList[0].X - pointsList[1].X, 2) + Math.Pow(pointsList[0].Y - pointsList[1].Y, 2));
        }

        public Mat DrawBoundingBox(ref Mat img, OpenCvSharp.Point topLeftPoint, OpenCvSharp.Point bottomRightPoint, Scalar color, LineTypes lineType)
        {
            Cv2.Rectangle(img, topLeftPoint, bottomRightPoint, color, 2, lineType, 0);

            return img;
        }

        public Mat CheckImageMirroring(ref Mat frame, bool mirrorX, bool mirrorY)
        {
            if (!mirrorX && !mirrorY)
                return frame;
            else if (mirrorX && !mirrorY)
                return frame.Flip(FlipMode.X);
            else if (!mirrorX && mirrorY)
                return frame.Flip(FlipMode.Y);
            else
                return frame.Flip(FlipMode.XY);
        }

        /// <summary>
        /// Create a processed image of the dynamic threshold algorithm used as visual feedback for the user 
        /// and rendered in the video feed panel on the user interface <br/>
        ///Image processing algorithm
        /// </summary>
        /// <param name="img">input image, of type OpenCvSharp.Mat</param>
        /// <param name="largestContourArea">desired contour, of type OpenCvSharp.Mat</param>
        /// <param name="contourCenter">center point of desired contour, of type OpenCvSharp.Point</param>
        /// <param name="actuatorPositionPixels">position of an actuator translated into pixels, of type System.Drawing.Point</param>
        /// <returns>output image, of type OpenCvSharp.Mat</returns>
        public Mat ComposeImageDTC(ref Mat img, ref Mat largestContourArea, ref OpenCvSharp.Point contourCenter, ref System.Drawing.Point actuatorPositionPixels)
        {
            // Convert to RGB, draw largest contour, draw largest contour center, put coordinates text, draw actuators position
            Cv2.CvtColor(img, imgComposed, ColorConversionCodes.GRAY2BGR);
            Cv2.DrawContours(imgComposed, new Mat[] { largestContourArea }, -1, Scalar.FromRgb(0, 128, 255), 2);
            Cv2.Circle(imgComposed, contourCenter, 2, Scalar.FromRgb(250, 50, 50), 5);
            Cv2.PutText(imgComposed, contourCenter.X.ToString() + "," + contourCenter.Y.ToString(), contourCenter, HersheyFonts.HersheySimplex, 1, Scalar.FromRgb(250, 50, 50), 2);
            //Cv2.Rectangle(imgComposed, new OpenCvSharp.Point(100, 100), new OpenCvSharp.Point(300, 300), Scalar.Azure, 2, LineTypes.AntiAlias, 0);

            // Draw actuator position line - Either axis X, either Y, either both
            foreach (Enums.Axis axis in TASettings.ConnectedAxisList)
                if (axis == Enums.Axis.X)
                    Cv2.Line(imgComposed, new OpenCvSharp.Point(actuatorPositionPixels.X, 0), new OpenCvSharp.Point(actuatorPositionPixels.X, imgComposed.Height - 1), Scalar.FromRgb(0, 250, 100), 3);

                else if (axis == Enums.Axis.Y)
                    Cv2.Line(imgComposed, new OpenCvSharp.Point(0, actuatorPositionPixels.Y), new OpenCvSharp.Point(imgComposed.Width - 1, actuatorPositionPixels.Y), Scalar.FromRgb(0, 250, 100), 3);

            // Draw IDC bounding box if necessary
            if (TrackingSettings.TrackingAlgorithm == Enums.TrackingAlgoirthm.ImageDistance)
            {
                imgComposed = DrawBoundingBox(ref imgGrayscale,
                    new OpenCvSharp.Point() { X = IDCSettings.BoxTopLeft.X, Y = IDCSettings.BoxTopLeft.Y },
                    new OpenCvSharp.Point() { X = IDCSettings.BoxBottomRight.X, Y = IDCSettings.BoxBottomRight.Y },
                    Scalar.Orange,
                    LineTypes.Link4
                    );
            }

            // Draw bounding box if necessary
            if (TrackingSettings.TrackingAlgorithm == Enums.TrackingAlgoirthm.BoundingBox)
                imgComposed = DrawBoundingBox(ref imgGrayscale,
                    new OpenCvSharp.Point() { X = BoundingBoxCalib.TopLeftPixelPos.X, Y = BoundingBoxCalib.TopLeftPixelPos.Y },
                    new OpenCvSharp.Point() { X = BoundingBoxCalib.BottomRightPixelPos.X, Y = BoundingBoxCalib.BottomRightPixelPos.Y },
                    Scalar.BlueViolet,
                    LineTypes.Link4
                    );

            return imgComposed;
        }

        /// <summary>
        /// Create a processed image of the image distance calibration algorithm used as visual feedback for the user 
        /// and rendered in the video feed panel on the user interface <br/>
        ///Image processing algorithm
        /// </summary>
        /// <param name="img">input image, of type OpenCvSharp.Mat</param>
        /// <param name="allContours">all countours found in an image, of type OpenCvSharp.Mat[]. Note: only index 0 and 1 will be used</param>
        /// <param name="pointsList">center point of the 2 largest contours by area, of type List<OpenCvSharp.Point></param>
        /// <param name="distance">Distance between center points of the 2 largest contours by area, of type int. Note: int because it's used only to roughly display</param>
        /// <returns>output image, of type OpenCvSharp.Mat</returns>
        public Mat ComposeImageIDC(Mat img, Mat[] allContours, List<OpenCvSharp.Point> pointsList, int distance)
        {
            Mat imgComposed = new Mat();

            Cv2.CvtColor(img, imgComposed, ColorConversionCodes.GRAY2BGR);
            Cv2.DrawContours(imgComposed, allContours, -1, Scalar.FromRgb(0, 128, 255), 2);        ///See method without "_" in source code for actual implementation. This methods exists because of limitations in documenting the API

            for (int i = 0; i < allContours.Count(); i++)
                Cv2.Circle(imgComposed, pointsList[i], 2, Scalar.FromRgb(250, 50, 50), 5);

            Cv2.Line(imgComposed, pointsList[0], pointsList[1], Scalar.FromRgb(0, 250, 100), 1);
            Cv2.PutText(imgComposed, distance.ToString() + " px", new OpenCvSharp.Point((pointsList[0].X + pointsList[1].X) / 2 - 10, (pointsList[0].Y + pointsList[1].Y) / 2 - 20), HersheyFonts.HersheySimplex, 1, Scalar.FromRgb(250, 50, 50), 2);

            // Draw IDC bounding box if necessary
            if (TrackingSettings.TrackingAlgorithm == Enums.TrackingAlgoirthm.ImageDistance)
            {
                imgComposed = DrawBoundingBox(ref imgComposed,
                    new OpenCvSharp.Point() { X = IDCSettings.BoxTopLeft.X, Y = IDCSettings.BoxTopLeft.Y },
                    new OpenCvSharp.Point() { X = IDCSettings.BoxBottomRight.X, Y = IDCSettings.BoxBottomRight.Y },
                    Scalar.Orange,
                    LineTypes.Link4
                    );
            }

            return imgComposed;
        }

        #region Dummy prototypes for API documentation only

        /// <summary>
        /// Acquires a frame from the Hamamatsu digital camera of type BitmapSource using
        /// EngineWrapper.EngineWrapper.GetNewFrame(), convert it to Bitmap using a BitmapEncoder,
        /// and finally convert to OpenCvSharp.Mat in order to be used for image processing <br/>
        ///See method without "_" in source code for actual implementation. This methods exists because of limitations in documenting the API
        /// </summary>
        /// <returns>OpenCvSharp.Mat frame, bool frame validity</returns>
        public void _GetHamamatsuFrame() { }

        /// <summary>
        /// Get resolution from capturing stream using an USB camera <br/>
        ///See method without "_" in source code for actual implementation. This methods exists because of limitations in documenting the API
        /// </summary>
        /// <returns>int widht, int height</returns>
        public void _GetResolution() { }

        /// <summary>
        /// Main funtion used to start the thresholding image processing algorithms. Calls ImageProcessor.ThresholdingAlgorithms.Run() <br/>
        ///See method without "_" in source code for actual implementation. This methods exists because of limitations in documenting the API
        /// </summary>
        /// <param name="actuatorPositionPixels">Position of actuators corresponding to video feed, used to display actuator position 
        /// on the user interface video feed, of type System.Drawing.Point</param>
        /// <returns>Bitmap processed image, System.Drawing.Point detected tracking point</returns>
        public void _StartThresholdingAlgorithms(System.Drawing.Point actuatorPositionPixels) { }

        /// <summary>
        /// Main function used to start the image distance calibration processing algorithm. Calls
        /// ImageProcessor.ImageDistanceCalibration.Run() <br/>
        ///See method without "_" in source code for actual implementation. This methods exists because of limitations in documenting the API
        /// </summary>
        /// <returns>Bitmap processed image, int distance in pixels</returns>
        public void _StartImageDistanceCalibration() { }

        /// <summary>
        /// Tries to read and return a frame from the USB camera stream <br/>
        ///See method without "_" in source code for actual implementation. This methods exists because of limitations in documenting the API
        /// </summary>
        /// <returns>OpenCvSharp.Mat captured frame, bool is frame valid</returns>
        public void _TryReadVideoCaptureFrame() { }

        #endregion

        /// <summary>
        /// Capture property, getter and setter. Used for accessing OpenCvSharp stream capturing methods and properties
        /// </summary>
        public static VideoCapture Capture { get => capture; set => capture = value; }

        /// <summary>
        /// Thresholding algorithms settings property, getter and setter
        /// </summary>
        public ThresholdingAlgorithmsSettings TASettings { get => taSettings; set => taSettings = value; }

        /// <summary>
        /// Image distance calibration settings property, getter and setter
        /// </summary>
        public ImageDistanceCalibrationSettings IDCSettings { get => idcSettings; set => idcSettings = value; }

        /// <summary>
        /// Tracking settings property, getter and setter
        /// </summary>
        public TrackingSettings TrackingSettings { get => trackingSettings; set => trackingSettings = value; }

        /// <summary>
        /// Video feed settings, getter and setter
        /// </summary>
        public VideoFeedSettings VideoFeedSettings { get => videoFeedSettings; set => videoFeedSettings = value; }

        public BoundingBoxCalibration BoundingBoxCalib { get => boundingBoxCalib; set => boundingBoxCalib = value; }
    }
}
