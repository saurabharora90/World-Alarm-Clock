using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Country_Specific_Alarm
{
    public class TimeData
    {
        public string City { get; set; }

        public string Country { get; set; }

        public override string ToString()
        {
            return City + ", " + Country;
        }

        public double offset { get; set; }
    }
}
