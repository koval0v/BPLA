namespace BestBPLAWay.Models
{
    public abstract class Target // точка-ціль, яку необхідно обстежити
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool isVisited { get; set; } = false;        

        public Target(int x, int y, string name)
        {
            X = x;
            Y = y; 
            Name = name;
        }

        public override bool Equals(object obj)
        {
            Target target = (Target)obj;
            return (X == target.X && Y == target.Y && Name == target.Name);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
