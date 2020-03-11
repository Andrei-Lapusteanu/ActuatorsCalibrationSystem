using System.Drawing;
using Entities;

namespace ImageProcessor
{
    /// <summary>
    /// This class contains settings for the tracking of a point
    /// </summary>
    public class TrackingSettings
    {
        private bool isEnabled;
        private bool justChangedAxis;
        private int frameCount;
        private int frameSkipCount;
        private int pixelTolerance;
        private System.Drawing.Point lastestTrackedStepsPoint;
        private System.Drawing.Point lastestTrackedPixelPoint;
        private System.Drawing.Point lastestCenterPixelPoint;
        private System.Drawing.Point currentlyDetectedPoint;

        private object comboBoxTrackingAlgorithmItemOne;
        private object comboBoxTrackingAlgorithmItemTwo;
        private object comboBoxTrackingAlgorithmItemThree;

        private Enums.TrackingAlgoirthm trackingAlgorithm;

        private int defaultPixelTolerance = 4;
        private int defaultFrameSkipCount = 4;

        /// <summary>
        /// Imoplicit constructor
        /// </summary>
        // TO DO - parametrizare pe UI
        public TrackingSettings()
        {
            this.isEnabled = false;
            this.JustChangedAxis = false;
            this.frameCount = 0;
            this.frameSkipCount = defaultPixelTolerance;
            this.PixelTolerance = defaultFrameSkipCount;
            this.lastestTrackedStepsPoint = new Point();
            this.lastestTrackedPixelPoint = new Point();
            this.lastestCenterPixelPoint = new Point();
            this.currentlyDetectedPoint = new Point();
            this.comboBoxTrackingAlgorithmItemOne = new object();
            this.comboBoxTrackingAlgorithmItemTwo = new object();
            this.comboBoxTrackingAlgorithmItemThree = new object();
            this.trackingAlgorithm = Enums.TrackingAlgoirthm.None;
        }

        /// <summary>
        /// Explicit constructor
        /// </summary>
        /// <param name="isEnabled">is tracking enablel, of type bool</param>
        /// <param name="frameCount">number of frames, of tyoe int</param>
        /// <param name="frameSkipCount">number of skipped frames, of type System.Drawing.Point</param>
        /// <param name="lastestTrackedStepsPoint">lastest tracked point is steps, of type System.Drawing.Point</param>
        /// <param name="lastestTrackedPixelPoint">latest tracked pixel point, of type System.Drawing.Point</param>
        /// <param name="lastestCenterPixelPoint">latest tracked center pixel point, of type System.Drawing.Point</param>
        public TrackingSettings(bool isEnabled, int frameCount, int frameSkipCount, Point lastestTrackedStepsPoint, Point lastestTrackedPixelPoint, Point lastestCenterPixelPoint, Point currentlyDetectedPoint)
        {
            this.isEnabled = isEnabled;
            this.frameCount = frameCount;
            this.frameSkipCount = frameSkipCount;
            this.lastestTrackedStepsPoint = lastestTrackedStepsPoint;
            this.lastestTrackedPixelPoint = lastestTrackedPixelPoint;
            this.lastestCenterPixelPoint = lastestCenterPixelPoint;
            this.currentlyDetectedPoint = currentlyDetectedPoint;
        }

        /// <summary>
        /// Sets default values for a number of settings
        /// </summary>
        /// <param name="defPixelTolerance"></param>
        /// <param name="defFrameSkipCount"></param>
        public void SetDefaultValues(int defPixelTolerance, int defFrameSkipCount)
        {
            defaultPixelTolerance = defPixelTolerance;
            defaultFrameSkipCount = defFrameSkipCount + 1;
        }

        /// <summary>
        /// Used to reset pixel tolerance, which represents the amount in pixels by which the 
        /// tracking point must translate in order to be tracked again, using a slider of the user interface. Low slider values 
        /// (low tolerance values) drive the actuators to follow the tracking point quicker, but cause numerous small 
        /// movements of the actuator which could potentially stress the motor over long periods of functioning
        /// </summary>
        /// <returns>pixel tolrance value, of type int</returns>
        public int ResetPixelTolerance()
        {
            return PixelTolerance = defaultPixelTolerance;
        }

        /// <summary>
        /// Used to reset frame skip count, which represents the number of frames ignored until a frame in considered 
        /// for the tracking algorithm. This way not every frames is used in the tracking process and thus the actuator
        /// moves less frequently, implying less stress on the driver. For applications that require a quick response time, 
        /// the frame skip count value should be set to low values
        /// </summary>
        /// <returns></returns>
        public int ResetFrameSkipCount()
        {
            return FrameSkipCount = defaultFrameSkipCount;
        }

        /// <summary>
        /// Is tracking enabled, getter and setter
        /// </summary>
        public bool IsEnabled { get => isEnabled; set => isEnabled = value; }

        /// <summary>
        /// Have axis just been switched, getter and setter
        /// </summary>
        public bool JustChangedAxis { get => justChangedAxis; set => justChangedAxis = value; }

        /// <summary>
        /// Number of frames, getter and setter
        /// </summary>
        public int FrameCount { get => frameCount; set => frameCount = value; }

        /// <summary>
        /// Number of skipped frames, getter and setter
        /// </summary>
        public int FrameSkipCount { get => frameSkipCount; set => frameSkipCount = value; }

        /// <summary>
        /// Latest tracked point in steps, getter and setter
        /// </summary>
        public Point LastestTrackedStepsPoint { get => lastestTrackedStepsPoint; set => lastestTrackedStepsPoint = value; }

        /// <summary>
        /// Latest tracked pixel point, getter and setter
        /// </summary>
        public Point LastestTrackedPixelPoint { get => lastestTrackedPixelPoint; set => lastestTrackedPixelPoint = value; }

        /// <summary>
        /// Latest tracked center pixel point, getter and setter
        /// </summary>
        public Point LastestCenterPixelPoint { get => lastestCenterPixelPoint; set => lastestCenterPixelPoint = value; }

        /// <summary>
        /// Pixel tolerance value, getter and setter
        /// </summary>
        public int PixelTolerance { get => pixelTolerance; set => pixelTolerance = value; }
        public Point CurrentlyDetectedPoint { get => currentlyDetectedPoint; set => currentlyDetectedPoint = value; }
        public object ComboBoxTrackingAlgorithmItemOne { get => comboBoxTrackingAlgorithmItemOne; set => comboBoxTrackingAlgorithmItemOne = value; }
        public object ComboBoxTrackingAlgorithmItemTwo { get => comboBoxTrackingAlgorithmItemTwo; set => comboBoxTrackingAlgorithmItemTwo = value; }
        public object ComboBoxTrackingAlgorithmItemThree { get => comboBoxTrackingAlgorithmItemThree; set => comboBoxTrackingAlgorithmItemThree = value; }
        public Enums.TrackingAlgoirthm TrackingAlgorithm { get => trackingAlgorithm; set => trackingAlgorithm = value; }
    }
}
