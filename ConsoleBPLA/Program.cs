using BestBPLAWay;
using BestBPLAWay.Models;
using System.Drawing;

namespace ConsoleBPLA
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string[] lines = File.ReadAllLines (@"C:\Users\UserPc\Desktop\3course\2sem\Operations-CourseWork\BPLAInput.txt");

            double timeResource = Convert.ToDouble(lines[0]);
            double averageSpeed = Convert.ToDouble(lines[1]);

            Vehicle startVehicle = new Vehicle(Convert.ToInt32(lines[2].Split(" ")[0]), Convert.ToInt32(lines[2].Split(" ")[1]), "Start");
            Vehicle endVehicle = new Vehicle(Convert.ToInt32(lines[3].Split(" ")[0]), Convert.ToInt32(lines[3].Split(" ")[1]), "End");

            List<ExamTarget> targets = new List<ExamTarget>();
            string targetName = "";
            for (int i = 5; i < Convert.ToInt32(lines[4]) * 2 + 5; i++)
            {
                if (i % 2 != 0)
                {
                    targetName = lines[i].ToString();
                }
                else
                {
                    targets.Add(new ExamTarget(Convert.ToInt32(lines[i].Split(" ")[0]), Convert.ToInt32(lines[i].Split(" ")[1]), targetName));
                }
            }

            BestWay bestWay = new BestWay(startVehicle, endVehicle, targets);

            bool first = true;
            foreach (Target target in bestWay.BuildTheOptimalRoute(timeResource, averageSpeed))
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    Console.WriteLine("    ↓");
                }

                Console.WriteLine($"{target.Name} [{target.X}, {target.Y}]");
            }
        }
    }
}