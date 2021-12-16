using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp.FantasyFootball
{

    public class FantasyFootballProduct : Product
    {
        public List<FantasyFootballTeam> PredictedTeams { get; protected set; }

        public FantasyFootballProduct() : base()
        {
            PredictedTeams = new List<FantasyFootballTeam>();
        }

        public void AddTeam(FantasyFootballTeam team)
        {
            PredictedTeams.Add(team);
        }

        public void SortTeams()
        {
            PredictedTeams.Sort();
        }

        public override double Value()
        {
            return 0;
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, PredictedTeams);
        }
    }

    public class FantasyFootballTeam : IComparable<FantasyFootballTeam>
    {
        private const int SAMPLECOUNT = 10000;

        public List<Player> Team { get; protected set; }
        public Histogram ProjectedScore { get; protected set; }

        public double AverageScore { get; protected set; }

        public FantasyFootballTeam(IEnumerable<Player> team)
        {
            Team = new List<Player>(team);
            if (Team.Count != 9) throw new Exception("Somehow got a team with other than 9 players");
            ProjectedScore = new Histogram(10, 310, 15);

            Histogram[] histograms = new Histogram[Team.Count];
            for (int i = 0; i < Team.Count; i++)
            {
                histograms[i] = Team[i].ProjectedScore;
            }

            double[] samples = new double[SAMPLECOUNT];
            for (int i = 0; i < SAMPLECOUNT; i++)
            {
                samples[i] = 0;
                for (int j = 0; j < Team.Count; j++)
                {
                    double sample = histograms[j].Sample();
                    samples[i] += sample;
                    AverageScore += sample;
                }
            }
            AverageScore /= SAMPLECOUNT;
            ProjectedScore.AddData(samples);
        }

        public double OddsOverTwoHundred()
        {
            int count = 0;
            for (int i=0; i<ProjectedScore.Buckets.Length; i++)
            {
                if (ProjectedScore.Boundaries[i] >= 200)
                {
                    count += ProjectedScore.Buckets[i];
                }
            }
            //if (count > 0) Console.WriteLine("YES!");
            return (double)count / ProjectedScore.Count;
        }

        public int CompareTo(FantasyFootballTeam other)
        {
            // Reverse default sort order
            int result = -OddsOverTwoHundred().CompareTo(other.OddsOverTwoHundred());
            if (result == 0) return other.AverageScore.CompareTo(AverageScore);
            return result;
        }

        public override string ToString()
        {
            return string.Join(", ", Team) + "   Odds: " + OddsOverTwoHundred() + " Average: " + AverageScore;
        }
    }

    public class Player : IComparable<Player>
    {
        public string Name { get; private set; }
        public PlayerRole Role { get; private set; }
        public int Salary { get; private set; }
        public Histogram ProjectedScore { get; private set; }

        public Player(string name, PlayerRole role, int salary, Histogram projectedScore)
        {
            Name = name;
            Role = role;
            Salary = salary;
            ProjectedScore = projectedScore;
        }

        public int CompareTo(Player other)
        {
            return Salary.CompareTo(other.Salary);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public enum PlayerRole
    {
        QB = 0,
        RB,
        WR,
        TE,
        DST,
        FLEX
    }



}
