using Entities;
using OpenCvSharp;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

namespace ImageProcessor
{
    /// <summary>
    /// This class is used to implement the image distance calibration algorithm. This algorithm
    /// detects two points, and using the knows physical distance, translates pixel distance to
    /// physical distance
    /// </summary>
    class ImageDistanceCalibration
    {
        ImageProcessingCore IPCore;
        List<OpenCvSharp.Point> pointsList;
        Mat croppedImg, imgGrayscale, imgBlurred, histogram, imgThreshold, invertedThreshold, imgMedian;
        Mat[] allContours, largestTwoContours;

        /// <summary>
        /// Explicit constructor
        /// </summary>
        /// <param name="ipc">object, of type ImageProcessor.ImageProcessingCore</param>
        public ImageDistanceCalibration(ImageProcessingCore ipc)
        {
            this.IPCore = ipc;
            this.pointsList = new List<OpenCvSharp.Point>();
            this.croppedImg = new Mat();
            this.imgGrayscale = new Mat();
            this.imgBlurred = new Mat();
            this.histogram = new Mat();
            this.imgThreshold = new Mat();
            this.invertedThreshold = new Mat();
            this.imgMedian = new Mat();
        }

        // See dummy reference for documentation
        public (BitmapSource, int) Run()
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

            if (isFrameValid == true)
            {
                Cv2.WaitKey(1);

                //Set image processing parameters
                IPCore.SetImageProcessingParameters();

                // Call processing algorthm - C# 7 tuple syntax return
                (Mat processedImage, int pixelDistance) = ProcessImage(ref frame);

                // Conversions & return
                return (OpenCvSharp.Extensions.BitmapSourceConverter.ToBitmapSource(processedImage), pixelDistance);
            }
            else return (null, 0);
        }

        // C# 7 tuple syntax
        // See dummy reference for documentation
        public (Mat, int) ProcessImage(ref Mat frame)
        {
            // Crop image to square
            croppedImg = IPCore.CropToROI(ref frame);

            // Convert image to grayscale
            imgGrayscale = IPCore.ConvertToGrayscale(ref croppedImg);

            // If checkBoxVideoFeedShowProcessedImage is not checked, return cropped and grayscale image
            if (IPCore.VideoFeedSettings.IsProcessedImageFed == false)
                return (imgGrayscale, 0);

            // Set image brightness if necessary
            if (IPCore.TASettings.Brightness != 0)
                imgGrayscale = IPCore.SetImageBrightness(ref imgGrayscale);

            // Blur grayscale image
            imgBlurred = IPCore.SimpleImageBlur(ref imgGrayscale);

            // Calculate image histogram
            histogram = IPCore.CalculateHistogram(imgBlurred);

            // Get maximum histogram value
            double maxHistValue = IPCore.FindMaxHistogramValue(histogram);

            // Get index of maximum value of histogram
            int maxHistIndex = IPCore.FindMaxHistogramIndex(histogram, maxHistValue);

            // Get value right of histogram, which is smaller or equal that the maximum value divided by a predefined value
            int dividedValIndex = IPCore.FindHistogramDividedIndex(histogram, Enums.HistogramCalculation.Reversed, maxHistValue, maxHistIndex);

            // Get threshold value & apply right offset if necessary
            int thresholdValue = IPCore.GetThresholdValue(dividedValIndex);

            // Apply threshold filter
            imgThreshold = IPCore.ThresholdFilter(ref imgBlurred, thresholdValue, ThresholdTypes.Binary);

            // Invert Image
            invertedThreshold = IPCore.InvertImage(ref imgThreshold);

            // Apply median filter
            imgMedian = IPCore.MedianFilter(ref invertedThreshold);

            // Find all contours from image
            allContours = IPCore.FindAllContours(ref imgMedian);

            // Find largest 2 contors
            largestTwoContours = IPCore.FindLargestTwoContours(ref imgMedian, ref allContours);

            if (largestTwoContours.Count() == 2)
            {
                this.pointsList = new List<OpenCvSharp.Point>();

                foreach (Mat contour in largestTwoContours)
                    pointsList.Add(IPCore.FindContourCenter(ref imgMedian, contour));

                double distance = IPCore.CalculateEuclidianDistance(pointsList);

                return (IPCore.ComposeImageIDC(imgGrayscale, largestTwoContours, pointsList, (int)distance), (int)distance);
            }
            else return (imgGrayscale, 0);
        }

        #region Dummy prototypes for API documentation only

        /// <summary>
        /// Used to execute the image distance calibration algorithm. Calls the processing method from here <br/>
        ///See method without "_" in source code for actual implementation. This methods exists because of limitations in documenting the API
        /// </summary>
        /// <returns>OpenCvSharp.Mat processed image, int found distance in pixles</returns>
        public void _Run() { }

        /// <summary>
        /// Used to implement the image distance calibration algorithm <br/>
        ///See method without "_" in source code for actual implementation. This methods exists because of limitations in documenting the API
        /// </summary>
        /// <param name="frame">input acquired image, of type OpenCvSharp.Mat</param>
        /// <returns>OpenCvSharp.Mat processed image</returns>
        public void _ProcessImage(Mat frame) { }

        #endregion
    }
}

