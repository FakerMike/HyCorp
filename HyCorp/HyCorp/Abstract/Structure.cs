using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp
{

    /// <summary>
    /// A HyCorp Deep Learner
    /// </summary>
    public abstract class HyCorp
    {
        public PlanningOrganization PlanningOrganization { get; protected set; }
        public ProductionOrganization ProductionOrganization { get; protected set; }

        public HyCorp()
        {
            PlanningOrganization = new PlanningOrganization();
            ProductionOrganization = new ProductionOrganization();
            BuildOrganization();
        }

        protected abstract void BuildOrganization();

        public void Plan(object PlanningInput)
        {
            PlanningOrganization.StartingTeam.Clerk.Plan(PlanningInput);
        }

        public void Produce(object ProductionInput)
        {
            PlanningOrganization.StartingTeam.Clerk.Produce(ProductionInput);
        }

    }

    public abstract class Intermediate<TEnclosed>
    {
        public TEnclosed Product {get; private set;}
        public Intermediate(TEnclosed product)
        {
            Product = product;
        }
    }


    /// <summary>
    /// Teams (Nodes): Perform a step in the big-picture process.
    /// </summary>
    public abstract class Team
    {
        public Type TeamInput { get; protected set; }
        public Type TeamOutput { get; protected set; }
        public bool IsPlanningTeam { get; protected set; }
        public bool IsProductionTeam { get; protected set; }
        public Manager Manager { get; protected set; }
        public Clerk Clerk { get; protected set; }
        public TeamLead Lead { get; protected set; }
        public Team PlanningDownstreamTeam { get; protected set; }
        public Team ProductionDownstreamTeam { get; protected set; }

        public int Depth { get; protected set; }

        public string Name { get; protected set; }


        public void Hire(TeamLead lead) { Lead = lead; }
        public void Hire(Clerk clerk) { Clerk = clerk; }
        public void Hire(Manager manager) { Manager = manager; }

        public void AddPlanningDownstreamTeam(Team team) { PlanningDownstreamTeam = team; }
        public void AddProductionDownstreamTeam(Team team) { ProductionDownstreamTeam = team; }
        public void SetDepth(int depth) { Depth = depth; }

        public override string ToString()
        {
            return $"{Name}: consumes {TeamInput.Name}, produces {TeamOutput.Name}";
        }
    }




    public abstract class Organization
    {
        public Team StartingTeam { get; protected set; }
        public Team EndingTeam { get; protected set; }

        public Organization()
        {
        }

        public void AddStartingTeam(Team team)
        {
            StartingTeam = team;
            StartingTeam.SetDepth(0);
            EndingTeam = team;
        }

        public abstract void AddNextTeam(Team next);

    }

    public class PlanningOrganization : Organization
    {
        public override void AddNextTeam(Team next)
        {
            EndingTeam.AddPlanningDownstreamTeam(next);
            next.SetDepth(EndingTeam.Depth + 1);
            EndingTeam = next;
        }
    }

    public class ProductionOrganization : Organization
    {
        public override void AddNextTeam(Team next)
        {
            EndingTeam.AddProductionDownstreamTeam(next);
            next.SetDepth(EndingTeam.Depth + 1);
            EndingTeam = next;
        }
    }


    public static class LaborPool
    {
        public static List<Type> Workers { get; private set; } = new List<Type>();
        public static List<Type> TeamLeads { get; private set; } = new List<Type>();
        public static void AddWorker(Type worker) { Workers.Add(worker); }
        public static void AddTeamLead(Type teamLead) { TeamLeads.Add(teamLead); }
    }




    public abstract class Product
    {
        public abstract double Value();
    }



    public class HistogramSettings
    {
        public int BucketCount { get; protected set; }
        public double Min { get; protected set; }
        public double Max { get; protected set; }
        public HistogramSettings(double min, double max, int count)
        {
            Min = min;
            Max = max;
            BucketCount = count;
        }
    }

    public class Histogram
    {
        public static Random rand = new Random();
        // The number of items in the histogram, sum of Buckets[]
        public int Count { get; protected set; }
        public int[] Buckets { get; protected set; }
        public double[] Boundaries { get; protected set; }
        public double[] Centers { get; protected set; }

        protected readonly double min;
        private readonly double max;
        private readonly int bucketCount;

        public Histogram(double _min, double _max, int _bucketCount)
        {
            min = _min;
            max = _max;
            bucketCount = _bucketCount;
            Create();
        }

        public Histogram(HistogramSettings settings)
        {
            min = settings.Min;
            max = settings.Max;
            bucketCount = settings.BucketCount;
            Create();
        }

        public HistogramSettings GetSettings()
        {
            return new HistogramSettings(min, max, bucketCount);
        }

        private void Create()
        {
            Buckets = new int[bucketCount];
            Centers = new double[bucketCount];
            Boundaries = new double[bucketCount + 1];

            double bucketWidth = (max - min) / bucketCount;
            Boundaries[0] = min - bucketWidth / 2;
            for (int i = 0; i < bucketCount; i++)
            {
                Centers[i] = min + bucketWidth * i;
                Boundaries[i + 1] = Boundaries[i] + bucketWidth;
                Buckets[i] = 0;
            }
        }

        public void AddData(IEnumerable<double> data)
        {
            foreach(double x in data)
            {
                AddData(x);
            }
        }

        public void AddData(double data)
        {
            Count++;
            for (int i = 0; i < bucketCount; i++)
            {
                if (data < Boundaries[i + 1])
                {
                    Buckets[i]++;
                    break;
                }
                if (i == bucketCount - 1)
                {
                    Buckets[i]++;
                }
            }
        }

        public double Sample()
        {
            int count = rand.Next(Count);
            int i = 0;
            while (count > 0)
            {
                if (count < Buckets[i])
                    return Centers[i];
                count -= Buckets[i++];
            }
            return 0; // Will not get here ever
        }
    }

    public class SingularHistogram : Histogram
    {
        public SingularHistogram(double value) : base (value, value, 1)
        {
        }

        public new void AddData(double data)
        {
            // No.
        }

        public new void AddData(IEnumerable<double> data)
        {
            // No.
        }

        public new double Sample()
        {
            return min;
        }
    }





}
