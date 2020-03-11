using System.Windows.Media;

namespace View
{
    class UIBrushes
    {
        private Brush greenStart;
        private Brush redStop;
        private Brush gray;

        // Active palette
        private Color mediumBordeaux;
        private Color mediumDarkBordeaux;
        private Color darkBordeaux;
        private Color darkerBordeaux;
        private Brush bordeauxAccentLight;
        private Brush selectedPanelIndicatorTint;
        private Brush mediumBordeauxBrush;

        private Color lightTeal;
        private Color mediumTeal;

        private Color darkGrayPurple;
        private Color mediumLila;
        private Color lightMediumLila;

        private Brush notificationPurple;
        private Brush notificationYellow;
        private Brush notificationBlue;

        public UIBrushes()
        {
            this.GreenStart = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 14, 163, 111));
            this.RedStop = new SolidColorBrush(System.Windows.Media.Color.FromArgb(210, 210, 10, 75));
            this.Gray = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 60, 60, 60));

            this.MediumBordeaux = new Color() { A = 200, R = 100, G = 3, B = 46 };
            this.MediumDarkBordeaux = new Color() { A = 200, R = 80, G = 3, B = 46 };
            this.DarkBordeaux = new Color() { A = 150, R = 78, G = 1, B = 43 };
            this.DarkerBordeaux = new Color() { A = 150, R = 58, G = 1, B = 23 };
            this.BordeauxAccentLight = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 180, 16, 80));
            this.SelectedPanelIndicatorTint = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 230, 0, 60));
            this.MediumBordeauxBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 100, 3, 49));

            this.LightTeal = new Color() { A = 200, R = 127, G = 179, B = 177 };
            this.MediumTeal = new Color() { A = 200, R = 68, G = 117, B = 124 };

            this.DarkGrayPurple = new Color() { A = 230, R = 27, G = 23, B = 37 };
            this.MediumLila = new Color() { A = 200, R = 57, G = 47, B = 90 };
            this.LightMediumLila = new Color() { A = 230, R = 64, G = 55, B = 110 };

            this.NotificationPurple = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 152, 154, 255));
            this.NotificationYellow = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 240, 247, 97));
            this.NotificationBlue = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 93, 226, 247));
        }

        public Brush GreenStart { get => greenStart; set => greenStart = value; }
        public Brush RedStop { get => redStop; set => redStop = value; }
        public Brush Gray { get => gray; set => gray = value; }
        public Color MediumBordeaux { get => mediumBordeaux; set => mediumBordeaux = value; }
        public Brush MediumBordeauxBrush { get => mediumBordeauxBrush; set => mediumBordeauxBrush = value; }
        public Color MediumDarkBordeaux { get => mediumDarkBordeaux; set => mediumDarkBordeaux = value; }
        public Color DarkBordeaux { get => darkBordeaux; set => darkBordeaux = value; }
        public Color DarkerBordeaux { get => darkerBordeaux; set => darkerBordeaux = value; }
        public Brush BordeauxAccentLight { get => bordeauxAccentLight; set => bordeauxAccentLight = value; }
        public Color LightTeal { get => lightTeal; set => lightTeal = value; }
        public Color MediumTeal { get => mediumTeal; set => mediumTeal = value; }
        public Color DarkGrayPurple { get => darkGrayPurple; set => darkGrayPurple = value; }
        public Color MediumLila { get => mediumLila; set => mediumLila = value; }
        public Color LightMediumLila { get => lightMediumLila; set => lightMediumLila = value; }
        public Brush NotificationPurple { get => notificationPurple; set => notificationPurple = value; }
        public Brush NotificationYellow { get => notificationYellow; set => notificationYellow = value; }
        public Brush NotificationBlue { get => notificationBlue; set => notificationBlue = value; }
        public Brush SelectedPanelIndicatorTint { get => selectedPanelIndicatorTint; set => selectedPanelIndicatorTint = value; }
    }
}
