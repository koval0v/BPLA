using BestBPLAWay.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BestBPLAWay
{
    public class BestWay
    {
        public Vehicle Start { get; set; }
        public Vehicle End { get; set; }
        public List<ExamTarget> Targets { get; set; } = new List<ExamTarget>();

        private List<DistMatrixElement> targetsDistMatrix = new List<DistMatrixElement>();

        public BestWay(Vehicle startVehiclePosition, Vehicle endVehiclePosition,
            List<ExamTarget> examinationTargets)
        {
            Start = startVehiclePosition;
            End = endVehiclePosition;
            Targets = examinationTargets;

            foreach(var target in Targets)
            {
                targetsDistMatrix.Add(new DistMatrixElement() { Target = target });
            }

            targetsDistMatrix.Add(new DistMatrixElement() { Target = Start });
            targetsDistMatrix.Add(new DistMatrixElement() { Target = End });

            GenerateDistancesMatrix();
        }

        public List<Target> BuildTheOptimalRoute(double timeResource, double averageSpeed) // повернути впорядкований список точок знайденого оптимального маршруту
        {
            List<Target> targets = new List<Target>(); // порядок цілей оптимального маршруту
            int n = Targets.Count(); // кількість точок-цілей для обстеження
            double l = 0; // відстань, яку вже пролетів БПЛА
            double lMax = timeResource * averageSpeed; // запас довжини шляху для польоту
            bool isEnded = false; // змінна, що вказує на переліт в кінцеву точку 

            Target currentPosition = Start; 
            targets.Add(Start);

            while (!isEnded) // цикл для знаходження шляху БПЛА, поки не буде досягнуто кінцевої точки ТЗ
            {
                Dictionary<Target, double> matrixRow = targetsDistMatrix.Where(x => x.Target == currentPosition)
                    .FirstOrDefault().SecondTargetDistance; // виділяємо відстані від поточної точки до інших точок з матриці відстаней

                double theNearestNeighbourDistance = Math.Round((matrixRow.ToList().Where(x => !x.Key.isVisited)
                        .Select(o => o.Value).Where(x => x != 0)
                        .OrderBy(x => x).FirstOrDefault()), 2);

                List<Target> theNearestNeighbours = matrixRow.Where(x => Math.Round(x.Value, 2) == theNearestNeighbourDistance && x.Key != Start)
                    .Select(x => x.Key).ToList();

                double distanceToEndVehicle; // відстань для повернення до кінцевої точки ТЗ

                if (theNearestNeighbours.FirstOrDefault() == End) // якщо найближчим сусідом є кінцева точка ТЗ - не враховувати її двічі
                {
                    distanceToEndVehicle = 0;
                }
                else
                {
                    distanceToEndVehicle = matrixRow.Where(o => o.Key == End).FirstOrDefault().Value;
                }

                if (l + theNearestNeighbourDistance + distanceToEndVehicle <= lMax) // якщо БПЛА має змогу долетіти до кінцевої точки ПЗ після найближчого сусіда 
                {
                    Target bestNearest; // найкращий найближчий сусід

                    if (theNearestNeighbours.Count > 1) // якщо найкращих найближчих сусідів декілька - порівняти альтернативи (TOPSIS)
                    {
                        List<Target> allAlternatives = new List<Target>();
                        foreach (Target target in Targets)
                        {
                            allAlternatives.Add(target);
                        }
                        allAlternatives.Add(Start);
                        allAlternatives.Add(End);

                        bestNearest = SortByTOPSIS(theNearestNeighbours, allAlternatives);
                    }
                    else
                    {
                        bestNearest = theNearestNeighbours.FirstOrDefault();
                    }

                    if (bestNearest == End) // якщо найближчим сусідом є кінцева точка ТЗ - перевірити чи є ресурс перевірити перед цим ще якісь найближчі точки
                    {
                        double nextTheNearestNeighbourDistance = Math.Round((matrixRow.ToList().Where(x => !x.Key.isVisited && x.Key != End)
                        .Select(o => o.Value).Where(x => x != 0)
                        .OrderBy(x => x).FirstOrDefault()), 2);

                        if (l + nextTheNearestNeighbourDistance + matrixRow.Where(o => o.Key == End).FirstOrDefault().Value <= lMax && nextTheNearestNeighbourDistance != 0)
                        {
                            List<Target> nextTheNearestNeighbours = matrixRow
                                .Where(x => Math.Round(x.Value, 2) == nextTheNearestNeighbourDistance && x.Key != Start).Select(x => x.Key).ToList();

                            bestNearest = nextTheNearestNeighbours.FirstOrDefault();

                            theNearestNeighbourDistance = nextTheNearestNeighbourDistance;
                        }
                    }

                    foreach (var target in targetsDistMatrix) // позначити точку відвіданою
                    {
                        foreach (var key in target.SecondTargetDistance.Keys)
                        {
                            if (key == currentPosition)
                            {
                                key.isVisited = true;
                            }
                        }
                    }

                    l += theNearestNeighbourDistance; 
                    targets.Add(bestNearest);
                    currentPosition = bestNearest;
                }
                else
                {
                    l += distanceToEndVehicle;
                    targets.Add(End);
                    currentPosition = End;
                }

                if (currentPosition == End) // закінчити цикл, якщо БПЛА долетів до кінцевої точки ТЗ
                {
                    isEnded = true;
                }
            }

            Console.WriteLine("-------------------------------------------");
            Console.WriteLine(String.Format("Route distance: {0:0.00} km", l));
            Console.WriteLine(String.Format("Route time: {0:0.00} min", (l / averageSpeed) * 60));
            Console.WriteLine($"Targets: {targets.Count() - 2}");
            Console.WriteLine("-------------------------------------------");

            return targets;
        }

        private Target SortByTOPSIS(List<Target> alternatives, List<Target> allTargets) // повернути впорядкований список точок у разі альтернативних варіантів
        {
            List<Alternative> alternativesTOPSIS = new List<Alternative>();

            // 1 - розрахунок нормалізованих оцінок
            double allTargetsAverageX = Math.Sqrt(allTargets.Sum(x => x.X * x.X));
            double allTargetsAverageY = Math.Sqrt(allTargets.Sum(x => x.Y * x.Y));

            foreach (var target in allTargets)
            {
                alternativesTOPSIS.Add(new Alternative(target)
                {
                    XValue = target.X / allTargetsAverageX,
                    YValue = target.Y / allTargetsAverageY
                });
            }

            // 2 - розрахунок зважених нормалізованих оцінок
            double xWeight = 0.5;
            double yWeight = 0.5;

            foreach (var alternative in alternativesTOPSIS)
            {
                alternative.XValue *= xWeight;
                alternative.YValue *= yWeight;
            }

            // 3 - розрахунок відстаней точки до утопічної точки (PIS) та неутопічної точки (NIS)
            double xPIS = alternativesTOPSIS.OrderBy(x => x.XValue).LastOrDefault().XValue;
            double yPIS = alternativesTOPSIS.OrderBy(x => x.YValue).LastOrDefault().YValue;

            double xNIS = alternativesTOPSIS.OrderBy(x => x.XValue).FirstOrDefault().XValue;
            double yNIS = alternativesTOPSIS.OrderBy(x => x.YValue).FirstOrDefault().YValue;

            foreach (var alternative in alternativesTOPSIS)
            {
                alternative.PISDistance = DistanceBetweenPoints(alternative.XValue, alternative.YValue, xPIS, yPIS);
                alternative.NISDistance = DistanceBetweenPoints(alternative.XValue, alternative.YValue, xNIS, yNIS);
            }

            // 4 - розрахунок наближеності точки до утопічної точки (PIS)
            foreach (var alternative in alternativesTOPSIS)
            {
                alternative.PISProximity = alternative.NISDistance / (alternative.NISDistance + alternative.PISDistance);
            }

            return alternativesTOPSIS.Where(x => alternatives.Any(y => y == x.Target)).OrderByDescending(x => x.PISProximity).LastOrDefault().Target;
        }

        private double DistanceBetweenPoints(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        private void GenerateDistancesMatrix()
        {
            foreach(var target in targetsDistMatrix)
            {
                foreach(var t in Targets)
                {
                    target.SecondTargetDistance.Add(t, DistanceBetweenPoints(target.Target.X, target.Target.Y, t.X, t.Y));
                }
                target.SecondTargetDistance.Add(Start, DistanceBetweenPoints(target.Target.X, target.Target.Y, Start.X, Start.Y));
                target.SecondTargetDistance.Add(End, DistanceBetweenPoints(target.Target.X, target.Target.Y, End.X, End.Y));
            }
        }
    }
}
