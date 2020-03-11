using OpenCvSharp;

namespace ImageProcessor
{
    /// <summary>
    /// This class contains settings for the image distance calibration algorithm
    /// </summary>
    public class ImageDistanceCalibrationSettings
    {
        private bool isCalibrated;
        private bool isEnabled;
        private int knownDistanceMilimeters;
        private int distancePixels;
        private float milimetersPerPixel;
        private float translatedDistanceMilimeters;
        private float stepsPerPixel;
        private bool isTrackingPointSet;

        private System.Drawing.Point boxTopLeft;
        private System.Drawing.Point boxBottomRight;

        /// <summary>
        /// Implicit constructor
        /// </summary>
        public ImageDistanceCalibrationSettings()
        {
            this.isCalibrated = false;
            this.IsEnabled = false;
            this.KnownDistanceMilimeters = 0;
            this.DistancePixels = 0;
            this.MilimetersPerPixel = 0f;
            this.TranslatedDistanceMilimeters = 0f;
            this.StepsPerPixel = 0f;
            this.isTrackingPointSet = false;
            this.boxTopLeft = new System.Drawing.Point();
            this.boxBottomRight = new System.Drawing.Point();
        }

        /// <summary>
        /// Explicit constructor, used to initialize settings
        /// </summary>
        /// <param name="isEnabled">is image distance calibration algorithm enabled, of type bool</param>
        /// <param name="knownDistanceMilimeters">distance known in milimeters, of type int</param>
        /// <param name="distancePixels">distance in pixels, of type int</param>
        /// <param name="milimetersPerPixel">milimeters per pixel, of type float</param>
        /// <param name="translatedDistanceMilimeters">distance translated from pixels to milimeters, of type float</param>
        /// <param name="stepsPerPixel"></param>
        public ImageDistanceCalibrationSettings(bool isEnabled, int knownDistanceMilimeters, int distancePixels, float milimetersPerPixel, float translatedDistanceMilimeters, float stepsPerPixel)
        {
            this.isEnabled = isEnabled;
            this.knownDistanceMilimeters = knownDistanceMilimeters;
            this.distancePixels = distancePixels;
            this.milimetersPerPixel = milimetersPerPixel;
            this.translatedDistanceMilimeters = translatedDistanceMilimeters;
            this.stepsPerPixel = stepsPerPixel;
        }

        /// <summary>
        /// Calculate milimeters per pixel using ImageProcessor.ImageDistanceCalibrationSettings.KnownDistanceMilimeters and
        /// ImageProcessor.ImageDistanceCalibrationSettings.MilimetersPerPixel
        /// </summary>
        /// <param name="knownDistMM">distance known in milimeters, of type int</param>
        /// <param name="distPx">distance in pixels, of type int</param>
        public void CalculateMilimetersPerPixel(int knownDistMM, int distPx)
        {
            this.KnownDistanceMilimeters = knownDistMM;
            this.DistancePixels = distPx;
            this.MilimetersPerPixel = (float)KnownDistanceMilimeters / (float)DistancePixels;
        }

        /// <summary>
        /// Calculate the distance in milimeters, using ImageProcessor.ImageDistanceCalibrationSettings.MilimetersPerPixel and
        /// ImageProcessor.ImageDistanceCalibrationSettings.DistancePixels
        /// </summary>
        public void CalculateTranslatedDistanceMilimeters()
        {
            this.TranslatedDistanceMilimeters = (float)MilimetersPerPixel * (float)DistancePixels;
        }

        /// <summary>
        /// Calculates steps per pixel using DistancePixels.MilimetersPerPixel and a constant value
        /// </summary>
        public void CalculateStepsPerPixel()
        {
            // 800 because 1mm = 800 steps
            this.StepsPerPixel = (float)MilimetersPerPixel * 800;
        }

        public void SetTopLeftBoxCoords(int pixelX, int pixelY)
        {
            this.boxTopLeft = new System.Drawing.Point(pixelX, pixelY);
        }

        public void SetBottomRightBoxCoords(int pixelX, int pixelY)
        {
            this.boxBottomRight = new System.Drawing.Point(pixelX, pixelY);
        }

        public void ResetDistanceCalibration()
        {
            this.isCalibrated = false;
            this.IsTrackingPointSet = false;
            this.milimetersPerPixel = 0;
            this.stepsPerPixel = 0;
        }

        /// <summary>
        /// Is image distance calibration algorithm enabled, getter and setter
        /// </summary>
        public bool IsEnabled { get => isEnabled; set => isEnabled = value; }

        /// <summary>
        /// Known distance in milimeters, getter and setter
        /// </summary>
        public int KnownDistanceMilimeters { get => knownDistanceMilimeters; set => knownDistanceMilimeters = value; }

        /// <summary>
        /// Distance in pixels, getter and setter
        /// </summary>
        public int DistancePixels { get => distancePixels; set => distancePixels = value; }

        /// <summary>
        /// Milimeters per pixel, getter and setter
        /// </summary>
        public float MilimetersPerPixel { get => milimetersPerPixel; set => milimetersPerPixel = value; }

        /// <summary>
        /// Translated pixel distance to milimeters, getter and setter
        /// </summary>
        public float TranslatedDistanceMilimeters { get => translatedDistanceMilimeters; set => translatedDistanceMilimeters = value; }

        /// <summary>
        /// Steps per pixel, getter and setter
        /// </summary>
        public float StepsPerPixel { get => stepsPerPixel; set => stepsPerPixel = value; }
        public bool IsCalibrated { get => isCalibrated; set => isCalibrated = value; }
        public System.Drawing.Point BoxTopLeft { get => boxTopLeft; set => boxTopLeft = value; }
        public System.Drawing.Point BoxBottomRight { get => boxBottomRight; set => boxBottomRight = value; }
        public bool IsTrackingPointSet { get => isTrackingPointSet; set => isTrackingPointSet = value; }
    }
}
