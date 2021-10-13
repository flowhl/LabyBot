using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace LabyBot
{
    /// <summary>
    /// Interaction logic for toggleswitch.xaml
    /// </summary>
    public partial class ToggleSwitch : UserControl
    {
        Thickness LeftSide = new Thickness(-39, 0, 0, 0);
        Thickness RightSide = new Thickness(0, 0, -39, 0);
        readonly SolidColorBrush Off = new SolidColorBrush(Color.FromRgb(160, 160, 160));
        readonly SolidColorBrush On = new SolidColorBrush(Color.FromRgb(130, 190, 125));
        private bool Toggled = false;
        public ToggleSwitch()
        {
            InitializeComponent();
            Back.Fill = Off;
            Toggled = false;
            Dot.Margin = LeftSide;
        }
        public bool Toggled1 { get => Toggled; set => Toggled = value; }

        private void Dot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Toggled)
            {
                Back.Fill = On;
                Toggled = true;
                Dot.Margin = RightSide;

            }
            else
            {

                Back.Fill = Off;
                Toggled = false;
                Dot.Margin = LeftSide;

            }
        }
        private void Back_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Toggled)
            {
                Back.Fill = On;
                Toggled = true;
                Dot.Margin = RightSide;

            }
            else
            {

                Back.Fill = Off;
                Toggled = false;
                Dot.Margin = LeftSide;

            }
        }
        public void SetStatus(Boolean status)
        {
            if (status)
            {
                Back.Fill = On;
                Toggled = true;
                Dot.Margin = RightSide;

            }
            else
            {
                Back.Fill = Off;
                Toggled = false;
                Dot.Margin = LeftSide;
            }

        }
        public Boolean GetStatus()
        {
            return Toggled;
        }
    }
}
