using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Globalization;

namespace the_flashlight
{
    public class LocalizedStrings
    {
        public LocalizedStrings()
        {
        }

        private static the_flashlight.AppResources localizedResources = new the_flashlight.AppResources();

        public the_flashlight.AppResources LocalizedResources { get { return localizedResources; } }
    }
}
