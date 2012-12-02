using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Info;

namespace the_flashlight
{
    public partial class InfoPage : PhoneApplicationPage
    {
        public InfoPage()
        {
            InitializeComponent();
        }

        private void rate_click(object sender, System.Windows.RoutedEventArgs e)
        {
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();

            marketplaceReviewTask.Show();
        }

        private void contact_click(object sender, System.Windows.RoutedEventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();

            string result = "unknown device";
            object deviceName;
            if(DeviceExtendedProperties.TryGetValue("DeviceName", out deviceName))
            {
                result = deviceName.ToString(); 
            }

            emailComposeTask.Subject = "The Flashlight Version 1.0 on " + result;
            emailComposeTask.To = "support.flashlight@floyd-ug.de";

            emailComposeTask.Show();
        }
    }
}
