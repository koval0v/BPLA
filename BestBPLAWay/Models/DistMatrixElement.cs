using System.Collections.Generic;

namespace BestBPLAWay.Models
{
    public class DistMatrixElement
    {
        public Target Target { get; set; }
        public Dictionary<Target, double> SecondTargetDistance { get; set; } = new Dictionary<Target, double>();
    }
}
