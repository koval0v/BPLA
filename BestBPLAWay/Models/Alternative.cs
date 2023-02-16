namespace BestBPLAWay.Models
{
    public class Alternative
    {
        public Target Target { get; set; }
        public double XValue { get; set; }
        public double YValue { get; set; }
        public double PISDistance { get; set; }
        public double NISDistance { get; set; }
        public double PISProximity { get; set; }

        public Alternative(Target target)
        {
            Target = target;
        }
    }
}
