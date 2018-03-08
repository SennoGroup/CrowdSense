using System;
using System.Collections.Generic;
using System.Linq;

namespace Main
{
    class CrowdSense
    {
        public class Example { }

        public class Labeler
        {
            public double[] QualityEstimate { get; set; }
            public int[] NumberOfConsistentContributions { get; set; }
            public int[] NumberOfContributions { get; set; }
            public int[] Votes { get; set; }
        }

        public void Calculate(List<Example> examples, List<Labeler> labelers, double threshold, double smoothingParameter)
        {
            Random rnd = new Random();
            var n = examples.Count;
            var m = labelers.Count;

            Init(labelers, n);

            for (var exampleIndex = 0; exampleIndex < n; exampleIndex++)
            {
                CalculateQualityEstimate(labelers, smoothingParameter, exampleIndex);

                var selectedLabelers = GetSelectedLabelers(labelers, threshold, exampleIndex, rnd, m);

                UpdateNumberOfContributions(selectedLabelers, exampleIndex);
            }


        }

        public static void Init(List<Labeler> labelers, int n)
        {
            foreach (var labeler in labelers)
            {
                labeler.QualityEstimate = new double[n];
                labeler.NumberOfContributions = new int[n];
                labeler.NumberOfConsistentContributions = new int[n];
            }
        }

        public static void CalculateQualityEstimate(List<Labeler> labelers, double smoothingParameter, int i)
        {
            foreach (var labeler in labelers)
            {
                labeler.QualityEstimate[i] = (labeler.NumberOfConsistentContributions[i] + smoothingParameter) /
                                             (labeler.NumberOfContributions[i] + 2 * smoothingParameter);
            }
        }

        public static List<Labeler> GetSelectedLabelers(List<Labeler> labelers, double threshold, int i, Random rnd, int m)
        {
            labelers.Sort((x, y) => x.QualityEstimate[i].CompareTo(y.QualityEstimate[i]));
            var randomLabelerIndex = rnd.Next(2, m);
            var selectedLabelers = new List<Labeler> { labelers[0], labelers[1], labelers[randomLabelerIndex] };

            for (var j = 2; j < m; j++)
            {
                if (j == randomLabelerIndex) continue;

                var score = selectedLabelers.Sum(sc => sc.QualityEstimate[i] * sc.Votes[i]);

                var current = (Math.Abs(score) - labelers[j].QualityEstimate[i]) / (selectedLabelers.Count + 1);
                if (current < threshold)
                {
                    selectedLabelers.Add(labelers[j]);
                }
                else
                {
                    break;
                }
            }

            return selectedLabelers;
        }

        public static void UpdateNumberOfContributions(List<Labeler> selectedLabelers, int exampleIndex)
        {
            var sign = Math.Sign(selectedLabelers.Sum(sc => sc.QualityEstimate[exampleIndex] * sc.Votes[exampleIndex]));
            foreach (var sel in selectedLabelers)
            {
                sel.NumberOfContributions[exampleIndex] += 1;
                sel.NumberOfConsistentContributions[exampleIndex] += (Math.Sign(sel.Votes[exampleIndex]) == sign ? 1 : 0);
            }
        }
    }
}