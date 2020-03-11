using Entities;
using System.Collections.Generic;
using System.Windows.Controls;

namespace View
{
    public class TextboxAxisLinkLUT
    {
        private Dictionary<TextBoxPair, Enums.Axis> lookUpTable;
        private TextBoxPair pairDeviceOne;
        private TextBoxPair pairDeviceTwo;
        private TextBoxPair pairDeviceThree;
        private TextBoxPair pairOneDeviceMoveTo;
        private TextBoxPair pairOneDeviceShift;

        private TextBoxAxisPair textBoxAxisPairDevOne;
        private TextBoxAxisPair textBoxAxisPairDevTwo;
        private TextBoxAxisPair textBoxAxisPairDevThree;
        private TextBoxAxisPair textBoxAxisPairMoveTo;
        private TextBoxAxisPair textBoxAxisPairShift;

        public TextboxAxisLinkLUT()
        {
            lookUpTable = new Dictionary<TextBoxPair, Enums.Axis>();
        }

        public TextboxAxisLinkLUT(TextBoxAxisPair pair_1, TextBoxAxisPair pair_2 = null, TextBoxAxisPair pair_3 = null)
        {
            lookUpTable = new Dictionary<TextBoxPair, Enums.Axis>
            {
                { pair_1.BoxPair, pair_1.Axis },
                { pair_2.BoxPair, pair_2.Axis },
                { pair_3.BoxPair, pair_3.Axis }
            };
        }

        public void CreateTextBoxAxisLink(TextBoxPair pair, Enums.Axis axis, Enums.AxisDeviceType axisDevType)
        {
            this.lookUpTable.Add(pair, axis);

            switch (axisDevType)
            {
                case Enums.AxisDeviceType.DeviceOne:
                    textBoxAxisPairDevOne = new TextBoxAxisPair(pair, axis);
                    break;

                case Enums.AxisDeviceType.DeviceTwo:
                    textBoxAxisPairDevTwo = new TextBoxAxisPair(pair, axis);
                    break;

                case Enums.AxisDeviceType.DeviceThree:
                    textBoxAxisPairDevThree = new TextBoxAxisPair(pair, axis);
                    break;

                case Enums.AxisDeviceType.MoveTo:
                    textBoxAxisPairMoveTo = new TextBoxAxisPair(pair, axis);
                    break;

                case Enums.AxisDeviceType.Shift:
                    textBoxAxisPairShift = new TextBoxAxisPair(pair, axis);
                    break;
            }
        }

        public Enums.Axis GetAxisFromTextBox(TextBox textBox)
        {
            Enums.Axis targetAxis = new Enums.Axis();

            if (textBox != null)
                // TO DO
                switch (textBox.Name)
                {
                    case "textBoxMoveRelativelyToStepsOne":
                        targetAxis = textBoxAxisPairDevOne.Axis;
                        break;

                    case "textBoxMoveRelativelyToMicroStepsOne":
                        targetAxis = textBoxAxisPairDevOne.Axis;
                        break;

                    case "textBoxMoveRelativelyToStepsTwo":
                        targetAxis = textBoxAxisPairDevTwo.Axis;
                        break;

                    case "textBoxMoveRelativelyToMicroStepsTwo":
                        targetAxis = textBoxAxisPairDevTwo.Axis;
                        break;

                    case "textBoxMoveRelativelyToStepsThree":
                        targetAxis = textBoxAxisPairDevThree.Axis;
                        break;

                    case "textBoxMoveRelativelyToMicroStepsThree":
                        targetAxis = textBoxAxisPairDevThree.Axis;
                        break;

                    case "textBoxMoveToPositionStepsOneDevice":
                        targetAxis = textBoxAxisPairMoveTo.Axis;
                        break;

                    case "textBoxMoveToPositionMicroStepsOneDevice":
                        targetAxis = textBoxAxisPairMoveTo.Axis;
                        break;

                    case "textBoxShiftToStepsOneDevice":
                        targetAxis = textBoxAxisPairShift.Axis;
                        break;

                    case "textBoxShiftToMicroStepsOneDevice":
                        targetAxis = textBoxAxisPairShift.Axis;
                        break;

                    default:
                        break;
                }

            return targetAxis;
        }

        public Dictionary<TextBoxPair, Enums.Axis> LookUpTable { get => lookUpTable; set => lookUpTable = value; }
        public TextBoxPair PairDeviceOne { get => pairDeviceOne; set => pairDeviceOne = value; }
        public TextBoxPair PairDeviceTwo { get => pairDeviceTwo; set => pairDeviceTwo = value; }
        public TextBoxPair PairDeviceThree { get => pairDeviceThree; set => pairDeviceThree = value; }
        public TextBoxPair PairOneDeviceMoveTo { get => pairOneDeviceMoveTo; set => pairOneDeviceMoveTo = value; }
        public TextBoxPair PairOneDeviceShift { get => pairOneDeviceShift; set => pairOneDeviceShift = value; }
        public TextBoxAxisPair TextBoxAxisPairDevOne { get => textBoxAxisPairDevOne; set => textBoxAxisPairDevOne = value; }
        public TextBoxAxisPair TextBoxAxisPairDevTwo { get => textBoxAxisPairDevTwo; set => textBoxAxisPairDevTwo = value; }
        public TextBoxAxisPair TextBoxAxisPairDevThree { get => textBoxAxisPairDevThree; set => textBoxAxisPairDevThree = value; }
        public TextBoxAxisPair TextBoxAxisPairMoveTo { get => textBoxAxisPairMoveTo; set => textBoxAxisPairMoveTo = value; }
        public TextBoxAxisPair TextBoxAxisPairShift { get => textBoxAxisPairShift; set => textBoxAxisPairShift = value; }
    }

    public class TextBoxAxisPair
    {
        private TextBoxPair boxPair;
        private Enums.Axis axis;

        public TextBoxAxisPair()
        {
            this.boxPair = new TextBoxPair();
            this.axis = Enums.Axis.X;
        }

        public TextBoxAxisPair(TextBoxPair boxPair, Enums.Axis axis)
        {
            this.boxPair = boxPair;
            this.axis = axis;
        }

        public TextBoxPair BoxPair { get => boxPair; set => boxPair = value; }

        public Enums.Axis Axis { get => axis; set => axis = value; }
    }

    public class TextBoxPair
    {
        private TextBox stepsTextBox;
        private TextBox microStepsTextBox;

        public TextBoxPair()
        {
            this.stepsTextBox = new TextBox();
            this.microStepsTextBox = new TextBox();
        }

        public TextBoxPair(TextBox stepsBox, TextBox microstepsBox)
        {
            this.stepsTextBox = stepsBox;
            this.microStepsTextBox = microstepsBox;
        }

        public TextBox StepsTextBox { get => stepsTextBox; set => stepsTextBox = value; }
        public TextBox MicroStepsTextBox { get => microStepsTextBox; set => microStepsTextBox = value; }
    }

}
