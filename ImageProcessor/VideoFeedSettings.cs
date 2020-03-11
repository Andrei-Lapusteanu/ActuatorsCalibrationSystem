namespace ImageProcessor
{
    /// <summary>
    /// This class contains settings for the video feed
    /// </summary>
    public class VideoFeedSettings
    {
        private bool isEnabled;
        private bool isProcessedImageFed;
        private int frameCount;
        private float frameTimeMs;
        private float framesPerSecond;
        private int framesPerSecondUpdateSkip;
        private int imageHeight;
        private int imageWidth;
        private bool isInverted;
        private bool isMirroredX;
        private bool isMirroredY;
        private string imageCaptureFolderPath;

        /// <summary>
        /// Implicit constructor
        /// </summary>
        public VideoFeedSettings()
        {
            this.isEnabled = false;
            this.IsProcessedImageFed = true;
            this.frameCount = 0;
            this.frameTimeMs = 0f;
            this.framesPerSecond = 0f;
            this.FramesPerSecondUpdateSkip = 5;
            this.ImageHeight = 0;
            this.imageWidth = 0;
            this.isInverted = false;
            this.isMirroredX = false;
            this.isMirroredY = false;
            this.imageCaptureFolderPath = @"..\..\Image Capture";
        }

        /// <summary>
        /// Explicit constructor
        /// </summary>
        /// <param name="isEnabled">is video feed enabled, of type bool</param>
        /// <param name="isProcessedImageFed">is processed image used, of type bool</param>
        /// <param name="frameCount">number of frames, of type int</param>
        /// <param name="frameTimeMs">frame time in miliseconds, of type float</param>
        /// <param name="framesPerSecond">number of frames per second, of type float</param>
        /// <param name="framesPerSecondUpdateSkip">number of frames skipped in displaying the frames per second number, of type int</param>
        /// <param name="imageHeight">number of vertical pixels, of type int</param>
        /// <param name="imageWidth">numbe of horizontal pixel, of type int</param>
        public VideoFeedSettings(bool isEnabled, bool isProcessedImageFed, int frameCount, float frameTimeMs, float framesPerSecond, int framesPerSecondUpdateSkip, int imageHeight, int imageWidth)
        {
            this.isEnabled = isEnabled;
            this.isProcessedImageFed = isProcessedImageFed;
            this.frameCount = frameCount;
            this.frameTimeMs = frameTimeMs;
            this.framesPerSecond = framesPerSecond;
            this.framesPerSecondUpdateSkip = framesPerSecondUpdateSkip;
            this.imageHeight = imageHeight;
            this.imageWidth = imageWidth;
        }

        /// <summary>
        /// Calculates the number of frames per second for the video feed
        /// </summary>
        /// <param name="frameTimeMs"></param>
        /// <returns>frame time in miliseconds, of type float</returns>
        public float CalculateFramesPerSecond(float frameTimeMs)
        {
            return 1000 / frameTimeMs;
        }

        /// <summary>
        /// Is video feed enabled, getter and setter
        /// </summary>
        public bool IsEnabled { get => isEnabled; set => isEnabled = value; }

        /// <summary>
        /// Is processed image used, getter and setter
        /// </summary>
        public bool IsProcessedImageFed { get => isProcessedImageFed; set => isProcessedImageFed = value; }

        /// <summary>
        /// Number of frames, getter and setter
        /// </summary>
        public int FrameCount { get => frameCount; set => frameCount = value; }

        /// <summary>
        /// Frame time in miliseconds, getter and setter
        /// </summary>
        public float FrameTimeMs { get => frameTimeMs; set => frameTimeMs = value; }
        /// <summary>
        /// Number of frames per second, getter and setter
        /// </summary>
        public float FramesPerSecond { get => framesPerSecond; set => framesPerSecond = value; }

        /// <summary>
        /// Number of frames skipped in displaying the frames per second number, getter and setter
        /// </summary>
        public int FramesPerSecondUpdateSkip { get => framesPerSecondUpdateSkip; set => framesPerSecondUpdateSkip = value; }

        /// <summary>
        /// Number of verical pixels, getter and setter
        /// </summary>
        public int ImageHeight { get => imageHeight; set => imageHeight = value; }

        /// <summary>
        /// Number of horizontal pixels, getter and setter
        /// </summary>
        public int ImageWidth { get => imageWidth; set => imageWidth = value; }
        public bool IsInverted { get => isInverted; set => isInverted = value; }
        public bool IsMirroredX { get => isMirroredX; set => isMirroredX = value; }
        public bool IsMirroredY { get => isMirroredY; set => isMirroredY = value; }
        public string ImageCaptureFolderPath { get => imageCaptureFolderPath; set => imageCaptureFolderPath = value; }
    }
}
