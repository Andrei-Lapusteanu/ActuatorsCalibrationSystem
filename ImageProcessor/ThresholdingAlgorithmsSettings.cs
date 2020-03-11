using Controller;
using Entities;
using System;
using System.Collections.Generic;

namespace ImageProcessor
{
    /// <summary>
    /// This class contains settings for the thresholding algorithms
    /// </summary>
    public class ThresholdingAlgorithmsSettings
    {
        private int _HIST_HEIGHT = 300;
        private int _HIST_BINS_COUNT = 240;
        private int _THRESH_DIVIDE_VAL = 100;
        private int _THRESH_OFFSET_AMOUNT = 0;
        private int _BLUR_KERNEL_SIZE = 7;
        private int _MEDIAN_KERNEL_SIZE = 7;

        private bool isEnabled;
        private Enums.ImgProcAlgorithm imgProcAlgorithm;
        private Enums.CalibrationAlgorithm calibrationAlgorithm;
        private List<Enums.Axis> connectedAxisList;

        private int precision;
        private double currentPrecisionSliderValue;
        private int quality;
        private int brightness;
        private int staticThresholdValue;

        private const int defaultPrecisionSliderValue = 2;
        private const int defaultQualitySliderValue = 7;
        private const int defaultBrightnessSliderValue = 0;
        private const int defaultStaticThresholdValue = 128;

        private int defaultPrecision;


        /// <summary>
        /// Implicit constructor
        /// </summary>
        public ThresholdingAlgorithmsSettings()
        {
            this._HIST_HEIGHT = 300;
            this._HIST_BINS_COUNT = 240;
            this._THRESH_DIVIDE_VAL = 100;
            this._THRESH_OFFSET_AMOUNT = 0;
            this._BLUR_KERNEL_SIZE = 7;
            this._MEDIAN_KERNEL_SIZE = 7;

            this.IsEnabled = false;
            this.ImgProcAlgorithm = Enums.ImgProcAlgorithm.None;
            this.CalibrationAlgorithm = Enums.CalibrationAlgorithm.None;
            this.StaticThresholdValue = 128;

            this.precision = 10;
            this.quality = 1;
            this.brightness = 0;
            this.ConnectedAxisList = new List<Enums.Axis>();
        }

        /// <summary>
        /// This calculates the precision value for the dynamic thresholding algorithm, which represents
        /// how tolerant the algorithm is to differences in lighting 
        /// </summary>
        /// <param name="sliderValue">slider value, of type double</param>
        /// <returns>precision value, of type int</returns>
        public int CalculatePrecision(double sliderValue)
        {
            return (int)Math.Pow(10, sliderValue) / 10;
        }

        /// <summary>
        /// Sets a number of default values for the settings
        /// </summary>
        /// <param name="defPrecisionSliderValue"> default precision slider value, of type int</param>
        /// <param name="defQualitySliderValue">default quality slider value, of type int</param>
        /// <param name="defBrightnessSliderValue">default brightness slider value, of type int</param>
        public void SetSliderValues(int defPrecisionSliderValue, int defQualitySliderValue, int defBrightnessSliderValue)
        {
            //this.defaultPrecisionSliderValue = defPrecisionSliderValue;
            //this.defaultQualitySliderValue = defQualitySliderValue;
            //this.defaultBrightnessSliderValue = defBrightnessSliderValue;
            this.defaultPrecision = Precision = CalculatePrecision(defPrecisionSliderValue);

            //this.Precision = CalculatePrecision(defPrecisionSliderValue);
            this.Quality = defQualitySliderValue;
            this.Brightness = defBrightnessSliderValue;
        }

        /// <summary>
        /// Used to reset precision to default ImageProcessor.ThresholdingAlgorithmsSettings.DefaultPrecisionSliderValue
        /// </summary>
        /// <returns>precision value, of type int</returns>
        public int ResetPrecision()
        {
            Precision = CalculatePrecision(defaultPrecisionSliderValue);
            return defaultPrecisionSliderValue;
        }


        /// <summary>
        /// Used to reset precision to default ImageProcessor.ThresholdingAlgorithmsSettings.DefaultPrecisionSliderValue
        /// </summary>
        /// <returns>precision value, of type int</returns>
        public int ResetQuality()
        {
            return Quality = defaultQualitySliderValue;
        }


        /// <summary>
        /// Used to reset precision to default ImageProcessor.ThresholdingAlgorithmsSettings.DefaultBrightnessSliderValue
        /// </summary>
        /// <returns>brightness value, of type int</returns>
        public int ResetBrightness()
        {
            return Brightness = defaultBrightnessSliderValue;
        }


        /// <summary>
        /// Used to reset precision to default static threshold value
        /// </summary>
        /// <returns>static threshold value, of type int</returns>
        public int ResetStaticThresholdValue()
        {
            return StaticThresholdValue = defaultStaticThresholdValue;
        }

        /// <summary>
        /// Used to set connected axis, helpful in order to properly use the thresholding algorithms on the correct axis of motion
        /// </summary>
        /// <param name="actuators"><object, of type Controller.Actuators/param>
        public void SetConnectedAxis(Actuators actuators)
        {
            ConnectedAxisList = new List<Enums.Axis>();

            for (int i = 0; i < Actuators.List.Count; i++)
                ConnectedAxisList.Add(Actuators.List[i].Axis);
        }

        /// <summary>
        /// Is any thresholding algorithm enabled, getter and setter
        /// </summary>
        public bool IsEnabled { get => isEnabled; set => isEnabled = value; }

        /// <summary>
        /// Used to select thresholding algorithm, getter and setter
        /// </summary>
        public Enums.ImgProcAlgorithm ImgProcAlgorithm { get => imgProcAlgorithm; set => imgProcAlgorithm = value; }

        /// <summary>
        /// Static threshold value, getter and setter
        /// </summary>
        public int StaticThresholdValue { get => staticThresholdValue; set => staticThresholdValue = value; }

        /// <summary>
        /// Pixel precision value, getter and setter
        /// </summary>
        public int Precision { get => precision; set => precision = value; }

        /// <summary>
        /// Image quality value, getter and setter
        /// </summary>
        public int Quality { get => quality; set => quality = value; }

        /// <summary>
        /// Image brightness value, getter and setter
        /// </summary>
        public int Brightness { get => brightness; set => brightness = value; }

        /// <summary>
        /// Connected axis list, used only for displayin purposes, getter and setter
        /// </summary>
        public List<Enums.Axis> ConnectedAxisList { get => connectedAxisList; set => connectedAxisList = value; }

        /// <summary>
        /// Hsitogram height in pixles, getter and setter
        /// </summary>
        public int HIST_HEIGHT { get => _HIST_HEIGHT; set => _HIST_HEIGHT = value; }

        /// <summary>
        /// Histogram width in pixels, getter and setter
        /// </summary>
        public int HIST_BINS_COUNT { get => _HIST_BINS_COUNT; set => _HIST_BINS_COUNT = value; }

        /// <summary>
        /// Threshold division value, getter and setter
        /// </summary>
        public int THRESH_DIVIDE_VAL { get => _THRESH_DIVIDE_VAL; set => _THRESH_DIVIDE_VAL = value; }

        /// <summary>
        /// Threshold offset amount, getter and setter
        /// </summary>
        public int THRESH_OFFSET_AMOUNT { get => _THRESH_OFFSET_AMOUNT; set => _THRESH_OFFSET_AMOUNT = value; }

        /// <summary>
        /// Blur filter kenrel size, getter and setter
        /// </summary>
        public int BLUR_KERNEL_SIZE { get => _BLUR_KERNEL_SIZE; set => _BLUR_KERNEL_SIZE = value; }

        /// <summary>
        /// Median filter kernel size, getter and setter
        /// </summary>
        public int MEDIAN_KERNEL_SIZE { get => _MEDIAN_KERNEL_SIZE; set => _MEDIAN_KERNEL_SIZE = value; }
        public Enums.CalibrationAlgorithm CalibrationAlgorithm { get => calibrationAlgorithm; set => calibrationAlgorithm = value; }
        public double CurrentPrecisionSliderValue { get => currentPrecisionSliderValue; set => currentPrecisionSliderValue = value; }
    }
}
