using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;



namespace SystemView
{
    /// <summary>
    /// Interaction logic for LEDIndicator.xaml
    /// </summary>
    public partial class LEDIndicator : UserControl
    {
        public enum LEDState
        {
            OFF, 
            ON, 
            NONE
        }

        public enum LEDColors
        {
            GREEN, 
            RED, 
            YELLOW, 
            BLUE, 
            PURPLE, 
            ORANGE
        }
                
        
        
        private LEDColors _ledColor;

        public LEDColors LEDColor
        {
            get
            {
                return _ledColor;
            }
            set
            {
                _ledColor = value;
                setLEDColor();
            }
        }

        public LEDState State
        {
            get { return (LEDState)GetValue(StateProperty); }
            set
            {
                SetValue(StateProperty, value);
            }
        }

        private Color ColorOn
        {
            get { return (Color)GetValue(ColorOnProperty); }
            set
            {
                SetValue(ColorOnProperty, value);
            }
        }

        private Color ColorOff
        {
            get { return (Color)GetValue(ColorOffProperty); }
            set
            {
                SetValue(ColorOffProperty, value);
            }
        }

        public LEDIndicator()
        {
            InitializeComponent();

            this._ledColor = LEDColors.GREEN;
            this.DataContext = this;
            ColorOn = (Color)ColorConverter.ConvertFromString("LimeGreen");
            ColorOff = (Color)ColorConverter.ConvertFromString("DarkGreen");
            State = LEDState.OFF;
        }

        public LEDIndicator(LEDColors color)
        {
            try
            {
                InitializeComponent();
                this.DataContext = this;

                if (color != null)
                {
                    this._ledColor = color;
                    setLEDColor();
                }

                State = LEDState.OFF;
            }
            catch
            {

            }
        }

        public void ON()
        {
            State = LEDState.ON;
        }

        public void OFF()
        {
            State = LEDState.OFF;
        }

        private void setLEDColor()
        {
            switch (this._ledColor)
            {
                case LEDColors.GREEN:
                    ColorOn = (Color)ColorConverter.ConvertFromString("LimeGreen");
                    ColorOff = (Color)ColorConverter.ConvertFromString("DarkGreen");
                    break;
                case LEDColors.BLUE:
                    ColorOn = (Color)ColorConverter.ConvertFromString("DodgerBlue");
                    ColorOff = (Color)ColorConverter.ConvertFromString("DarkBlue");
                    break;
                case LEDColors.ORANGE:
                    ColorOn = (Color)ColorConverter.ConvertFromString("DarkOrange");
                    ColorOff = (Color)ColorConverter.ConvertFromString("Chocolate");
                    break;
                case LEDColors.RED:
                    ColorOn = (Color)ColorConverter.ConvertFromString("Red");
                    ColorOff = (Color)ColorConverter.ConvertFromString("DarkRed");
                    break;
                case LEDColors.YELLOW:
                    ColorOn = (Color)ColorConverter.ConvertFromString("Yellow");
                    ColorOff = (Color)ColorConverter.ConvertFromString("DarkGoldenrod");
                    break;
                case LEDColors.PURPLE:
                    ColorOn = (Color)ColorConverter.ConvertFromString("DarkViolet");
                    ColorOff = (Color)ColorConverter.ConvertFromString("Purple");
                    break;
                default:
                    break;
            }
        }

        private static void StatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LEDIndicator led = (d as LEDIndicator);
            if (led.State == LEDState.ON)
            {
                led.backgroundColor.Color = led.ColorOn;
            }
            else
            {
                led.backgroundColor.Color = led.ColorOff;
            }
        }

        private static void OnColorOnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LEDIndicator led = (d as LEDIndicator);
            led.ColorOn = (Color)e.NewValue;
            if (led.State == LEDState.ON)
            {
                led.backgroundColor.Color = led.ColorOn;
            }
            else
            {
                led.backgroundColor.Color = led.ColorOff;
            }
        }

        private static void OnColorOffPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LEDIndicator led = (d as LEDIndicator);
            led.ColorOff = (Color)e.NewValue;
            if (led.State == LEDState.ON)
            {
                led.backgroundColor.Color = led.ColorOn;
            }
            else
            {
                led.backgroundColor.Color = led.ColorOff;
            }
        }

        public static readonly DependencyProperty ColorOnProperty =
            DependencyProperty.Register("ColorOn", typeof(Color), typeof(LEDIndicator),
                new PropertyMetadata(Colors.Green, new PropertyChangedCallback(LEDIndicator.OnColorOnPropertyChanged)));

        public static readonly DependencyProperty ColorOffProperty =
            DependencyProperty.Register("ColorOff", typeof(Color), typeof(LEDIndicator),
                new PropertyMetadata(Colors.ForestGreen, new PropertyChangedCallback(LEDIndicator.OnColorOffPropertyChanged)));

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(LEDState), typeof(LEDIndicator),
                new PropertyMetadata(new PropertyChangedCallback(LEDIndicator.StatePropertyChanged)));
    }
}
