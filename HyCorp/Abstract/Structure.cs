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
    public abstract class HyCorp<Input, Output>
    {
        public PlanningOrganization<Input, Output> PlanningOrganization { get; protected set; }
        public ProductionOrganization<Input, Output> ProductionOrganization { get; protected set; }

        public HyCorp()
        {
            PlanningOrganization = new PlanningOrganization<Input, Output>();
            ProductionOrganization = new ProductionOrganization<Input, Output>();
            BuildOrganization();
        }

        protected abstract void BuildOrganization();

    }




    public interface ITeam
    {
        bool Work();
        bool Train();
    }
    public interface IConsumer<Input> : ITeam
    {
        void AddInput(Input input);
    }

    public interface IProducer<Output> : ITeam
    {
        Output GetProduct();
        void AddPlanningDownstreamTeam(IConsumer<Output> team);
        void AddProductionDownstreamTeam(IConsumer<Output> team);
    }

    public interface IPlanner
    {
        Type GetPlanningInputType();
        Type GetPlanningOutputType();
        void Plan();
    }

    public interface IProducer
    {
        Type GetProductionInputType();
        Type GetProductionOutputType();
        void Produce();
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
    public class Team<Input, Output> : IConsumer<Input>, IProducer<Output>
    {
        public Manager Manager { get; protected set; }
        public Clerk<Input, Output> Clerk { get; protected set; }
        public Lead<Input, Output> Lead { get; protected set; }
        public List<Worker> Workers { get; protected set; }
        public List<IConsumer<Output>> PlanningDownstreamTeams { get; protected set; }
        public List<IConsumer<Output>> ProductionDownstreamTeams { get; protected set; }
        public List<Input> Inputs { get; protected set; }
        public bool Staffed { get; protected set; }
        public string Name { get; protected set; }

        private Output product;

        public Team()
        {
            Staffed = false;
            Workers = new List<Worker>();
            PlanningDownstreamTeams = new List<IConsumer<Output>>();
            ProductionDownstreamTeams = new List<IConsumer<Output>>();
            Inputs = new List<Input>();
            Name = "Default Team";
        }

        public void Hire(Manager manager) { Manager = manager; }
        public void Hire(Clerk<Input, Output> clerk) { Clerk = clerk; }
        public void Hire(Lead<Input, Output> lead) { Lead = lead; }

        public void AddPlanningDownstreamTeam(IConsumer<Output> team) { PlanningDownstreamTeams.Add(team); }
        public void AddProductionDownstreamTeam(IConsumer<Output> team) { ProductionDownstreamTeams.Add(team); }

        public Output GetProduct()
        {
            return product;
        }

        public bool Work()
        {
            if (Clerk.CheckIfTeamReady())
            {
                product = Lead.Produce(Inputs, Workers);
                foreach (IConsumer<Output> team in ProductionDownstreamTeams)
                {
                    team.AddInput(product);
                    team.Work();
                }
                return true;
            }
            return false;
        }

        public bool Train()
        {
            if (Clerk.CheckIfTeamReady())
            {
                product = Lead.Train(Inputs, Workers);
                foreach (IConsumer<Output> team in PlanningDownstreamTeams)
                {
                    team.AddInput(product);
                    team.Train();
                }
                return true;
            }
            return false;
        }

        public void AddInput(Input input)
        {
            Inputs.Add(input);
        }

        public void Hire(IEnumerable<Worker> workers)
        {
            foreach (Worker worker in workers)
            {
                if (Manager.Approves(worker))
                    Workers.Add(worker);
            }
            Workers = Manager.Evaluate(Workers);
        }


        public override string ToString()
        {
            return $"{Name}: consumes {typeof(Input).Name}, produces {typeof(Output).Name}";
        }
    }


    /// <summary>
    /// Teams (Nodes): Perform a step in the big-picture process.
    /// </summary>
    public abstract class NewTeam
    {
        public Type TeamInput { get; protected set; }
        public Type TeamOutput { get; protected set; }
        public bool IsPlanningTeam { get; protected set; }
        public bool IsProductionTeam { get; protected set; }
        public NewManager Manager { get; protected set; }
        public NewClerk Clerk { get; protected set; }
        public NewTeamLead Lead { get; protected set; }
        public List<NewTeam> PlanningDownstreamTeams { get; protected set; } = new List<NewTeam>();
        public List<NewTeam> ProductionDownstreamTeams { get; protected set; } = new List<NewTeam>();

        public string Name { get; protected set; }


        public void Hire(NewManager manager) { Manager = manager; }
        public void Hire(NewClerk clerk) { Clerk = clerk; }
        public void Hire(NewTeamLead lead) { Lead = lead; }

        public void AddPlanningDownstreamTeam(NewTeam team) { PlanningDownstreamTeams.Add(team); }
        public void AddProductionDownstreamTeam(NewTeam team) { ProductionDownstreamTeams.Add(team); }

        public override string ToString()
        {
            return $"{Name}: consumes {TeamInput.Name}, produces {TeamOutput.Name}";
        }
    }
























    public abstract class Organization<Input, Output>
    {
        public List<IConsumer<Input>> StartingTeams { get; protected set; }
        public List<Input> Inputs;

        public Organization()
        {
            StartingTeams = new List<IConsumer<Input>>();
            Inputs = new List<Input>();
        }

        public void AddStartingTeam(IConsumer<Input> team)
        {
            StartingTeams.Add(team);
        }


        public abstract void AddDownstreamTeam<T>(IProducer<T> from, IConsumer<T> to);


        public void AddInput(Input input)
        {
            Inputs.Add(input);
        }
    }

    public class PlanningOrganization<Input, Output> : Organization<Input, Output>
    {
        public override void AddDownstreamTeam<T>(IProducer<T> from, IConsumer<T> to)
        {
                from.AddPlanningDownstreamTeam(to);
        }

        public bool Train()
        {
            foreach (IConsumer<Input> team in StartingTeams)
            {
                foreach (Input input in Inputs)
                {
                    team.AddInput(input);
                }
                team.Train();
            }
            return true;
        }
    }

    public class ProductionOrganization<Input, Output> : Organization<Input, Output>
    {
        public IProducer<Output> EndingTeam { get; protected set; }


        public bool Work()
        {
            foreach (IConsumer<Input> team in StartingTeams)
            {
                foreach (Input input in Inputs)
                {
                    team.AddInput(input);
                }
                team.Work();
            }
            return true;
        }

        public void AddEndingTeam<T>(IProducer<T> from, Team<T,Output> endingTeam)
        {
            from.AddProductionDownstreamTeam(endingTeam);
            EndingTeam = endingTeam;
        }

        public Output GetProduct()
        {
            return EndingTeam.GetProduct();
        }

        public override void AddDownstreamTeam<T>(IProducer<T> from, IConsumer<T> to)
        {
            from.AddProductionDownstreamTeam(to);
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
