using Entities;
using OpenCvSharp;
using System.Linq;
using System.Windows.Media.Imaging;

namespace ImageProcessor
{
    /*
    alpha 1  beta 0      --> no change  
    0 < alpha < 1        --> lower contrast  
    alpha > 1            --> higher contrast  
    -127 < beta < +127   --> good range for brightness values
    */

    /// <summary>
    /// This class is used to implement the image thresholding algorithms such as the static,
    /// dynamic, and otsu thresholding algorithms
    /// </summary>
    public class ThresholdingAlgorithms
    {
        ImageProcessingCore IPCore;
        Mat croppedImg, imgPreContours, imgGrayscale, imgBlurred, histogram, imgThreshold, largestContourArea;
        Mat[] allContours;

        /// <summary>
        /// Explicit constructor
        /// </summary>
        /// <param name="ipc">object, of type ImageProcessor.ImageProcessingCore</param>
        public ThresholdingAlgorithms(ImageProcessingCore ipc)
        {
            this.IPCore = ipc;
            this.croppedImg = new Mat();
            this.imgPreContours = new Mat();
            this.imgGrayscale = new Mat();
            this.imgBlurred = new Mat();
            this.histogram = new Mat();
            this.imgThreshold = new Mat();
            this.largestContourArea = new Mat();
        }

        // ELI
        public (BitmapSource, System.Drawing.Point) Run(System.Drawing.Point actuatorPositionPixels)
        {
            // Check is Video Capture hasn't been previously disposed, and start it if it has
           //IPCore.IsVideoCaptureDisposed();

            // Try reading frame from capture device
            //(Mat frame, bool isFrameValid) = IPCore.TryReadVideoCaptureFrame();

            // ELI
            // ENGINE WRAPPER
            //(Mat frame, bool isFrameValid) = IPCore.GetHamamatsuFrame();

            // ELI 
            // DCAM WRAPPER
            (Mat frame, bool isFrameValid) = IPCore.DummyHamamatsuInterop();

            // Mirror vertically and/or horizontally
            frame = IPCore.CheckImageMirroring(ref frame, IPCore.VideoFeedSettings.IsMirroredX, IPCore.VideoFeedSettings.IsMirroredY);

            if (isFrameValid == true)
            {
                Cv2.WaitKey(1);

                //Set image processing parameters
                IPCore.SetImageProcessingParameters();

                // TO DO - try catch!
                // Return frame is no algorithm is selected
                if (IPCore.TASettings.ImgProcAlgorithm == Enums.ImgProcAlgorithm.None)
                    return (OpenCvSharp.Extensions.BitmapSourceConverter.ToBitmapSource(frame), new System.Drawing.Point(int.MinValue, int.MinValue));

                // Call processing algorthm - C# 7 tuple syntax return
                (Mat processedImage, OpenCvSharp.Point centerPointCV) = ProcessImage(ref frame, ref actuatorPositionPixels);

                // Conversions & return
                return (OpenCvSharp.Extensions.BitmapSourceConverter.ToBitmapSource(processedImage), new System.Drawing.Point() { X = centerPointCV.X, Y = centerPointCV.Y });
            }
            else return (null, new System.Drawing.Point());
        }

        // C# 7 tuple syntax
        public (Mat, OpenCvSharp.Point) ProcessImage(ref Mat frame, ref System.Drawing.Point actuatorPositionPixels)
        {
            // Crop image to square
            croppedImg = IPCore.CropToROI(ref frame);

            // Convert image to grayscale
            imgGrayscale = IPCore.ConvertToGrayscale(ref croppedImg);

            // Invert image, if necessary
            if (IPCore.VideoFeedSettings.IsInverted == true)
                imgGrayscale = IPCore.InvertImage(ref imgGrayscale);

            // If checkBoxVideoFeedShowProcessedImage is not checked, return grayscale image
            if (IPCore.VideoFeedSettings.IsProcessedImageFed == false)
                return (imgGrayscale, new OpenCvSharp.Point(int.MinValue, int.MinValue));

            // Set image brightness if necessary
            if (IPCore.TASettings.Brightness != 0)
                imgGrayscale = IPCore.SetImageBrightness(ref imgGrayscale);

            // Blur grayscale image
            imgBlurred = IPCore.SimpleImageBlur(ref imgGrayscale);

            if (IPCore.TASettings.ImgProcAlgorithm == Enums.ImgProcAlgorithm.StaticThresh)
                imgPreContours = IPCore.ThresholdFilter(ref imgBlurred, IPCore.TASettings.StaticThresholdValue, ThresholdTypes.Binary);

            else if (IPCore.TASettings.ImgProcAlgorithm == Enums.ImgProcAlgorithm.DynamicThresh)
            {
                // Calculate image histogram
                histogram = IPCore.CalculateHistogram(imgBlurred);

                // Get maximum histogram value
                double maxHistValue = IPCore.FindMaxHistogramValue(histogram);

                // Get index of maximum value of histogram
                int maxHistIndex = IPCore.FindMaxHistogramIndex(histogram, maxHistValue);

                // Get value right of histogram, which is smaller or equal that the maximum value divided by a predefined value
                int dividedValIndex = IPCore.FindHistogramDividedIndex(histogram, Enums.HistogramCalculation.Normal, maxHistValue, maxHistIndex);

                // Get threshold value & apply right offset if necessary
                int thresholdValue = IPCore.GetThresholdValue(dividedValIndex);

                // Apply threshold filter
                imgThreshold = IPCore.ThresholdFilter(ref imgBlurred, thresholdValue, ThresholdTypes.Binary);

                // Apply median filter
                imgPreContours = IPCore.MedianFilter(ref imgThreshold);
            }
            else if (IPCore.TASettings.ImgProcAlgorithm == Enums.ImgProcAlgorithm.OstuThresh)
            {
                imgPreContours = IPCore.ThresholdFilter(ref imgBlurred, IPCore.TASettings.StaticThresholdValue, ThresholdTypes.Otsu);
            }

            return ProcessContours(ref imgPreContours, ref imgGrayscale, ref actuatorPositionPixels);
        }

        public (Mat, OpenCvSharp.Point) ProcessContours(ref Mat imgPreContours, ref Mat imgGrayscale, ref System.Drawing.Point actuatorPositionPixels)
        {
            // Find all contours from image
            allContours = IPCore.FindAllContours(ref imgPreContours);

            if (allContours.Count() > 0)
            {
                // Get the contours that has the largest area
                largestContourArea = IPCore.FindLargestContourArea(ref imgGrayscale, ref allContours);

                // Get largest contour's area center point imgPreContours
                OpenCvSharp.Point contourCenter = IPCore.FindContourCenter(ref imgPreContours, largestContourArea);

                // Put center coordinates on image and return it
                return (IPCore.ComposeImageDTC(ref imgGrayscale, ref largestContourArea, ref contourCenter, ref actuatorPositionPixels), contourCenter);
            }
            else

                // Return the grayscale image if no contours were found
                return (imgGrayscale, new OpenCvSharp.Point(int.MinValue, int.MinValue));
        }

        #region Dummy prototypes for API documentation only

        /// <summary>
        /// Used to execute the thresholding algoritms. Calls the processing method from here <br/>
        ///See method without "_" in source code for actual implementation. This methods exists because of limitations in documenting the API
        /// </summary>
        /// <param name="actuatorPositionPixels">Position of actuators corresponding to video feed, used to display actuator position 
        /// on the user interface video feed, of type System.Drawing.Point</param>        
        /// returns>OpenCvSharp.Mat processed image, System.Drawing.Point tracking point</returns>
        public void _Run(System.Drawing.Point actuatorPositionPixels) { }

        /// <summary>
        /// Used to implement the thresholding algorithms <br/>
        ///See method without "_" in source code for actual implementation. This methods exists because of limitations in documenting the API
        /// </summary>
        /// <param name="frame">input acquired image, of type OpenCvSharp.Mat</param>
        /// <param name="actuatorPositionPixels">position of actuators corresponding to video feed, used to display actuator position 
        /// on the user interface video feed, of type System.Drawing.Point</param>
        /// <returns>OpenCvSharp.Mat processed image, System.Drawing.Point tracking point</returns>
        public void _ProcessImage(Mat frame, System.Drawing.Point actuatorPositionPixels) { }

        /// <summary>
        /// Used to detect contours in an image <br/>
        ///See method without "_" in source code for actual implementation. This methods exists because of limitations in documenting the API
        /// </summary>
        /// <param name="imgPreContours">the processed image before detection of contours, of type OpenCvSharp.Mat</param>
        /// <param name="imgGrayscale">the grayscale image, of type OpenCvSharp.Mat</param>
        /// <param name="actuatorPositionPixels">position of actuators corresponding to video feed, used to display actuator position 
        /// on the user interface video feed, of type System.Drawing.Point</param>
        /// <returns>OpenCvSharp.Mat processed image, System.Drawing.Point tracking point</returns>
        public void _ProcessContours(Mat imgPreContours, Mat imgGrayscale, System.Drawing.Point actuatorPositionPixels) { }

        #endregion
    }
}

//Console.WriteLine(s.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L)));