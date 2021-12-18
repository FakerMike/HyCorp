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
        protected const int SALARYCAP = 50000;

        public List<Player> Team { get; protected set; }



        public FantasyFootballTeam(IEnumerable<Player> team)
        {
            Team = new List<Player>(team);
        }

        public double HotOdds()
        {
            double result = 1;
            foreach (Player p in Team) { result *= p.HotChance; }
            return result;
        }

        public double HotScore()
        {
            double result = 0;
            foreach (Player p in Team) { result += p.Salary * 0.04; }
            return result;
        }

        public int RemainingSalary()
        {
            int currentCost = 0;
            foreach (Player p in Team)
            {
                currentCost += p.Salary;
            }
            return SALARYCAP - currentCost;
        }


        public int CompareTo(FantasyFootballTeam other)
        {
            // Reverse default sort order
            int result = -HotOdds().CompareTo(other.HotOdds());
            if (result == 0) return other.HotScore().CompareTo(HotScore());
            return result;
        }

        public override string ToString()
        {
            return string.Join(", ", Team) + "   Odds: " + HotOdds() + " Target Score: " + HotScore();
        }
    }


    public class PlayerPicker
    {
        protected List<Player> Players;
        protected const int SALARYCAP = 50000;
        protected static Random rand = new Random();
        protected Dictionary<PlayerRole, List<Player>> byRole;

        public PlayerPicker(List<Player> players)
        {
            Players = players;

            byRole = new Dictionary<PlayerRole, List<Player>>
            {
                [PlayerRole.TE] = new List<Player>(),
                [PlayerRole.WR] = new List<Player>(),
                [PlayerRole.QB] = new List<Player>(),
                [PlayerRole.RB] = new List<Player>(),
                [PlayerRole.DST] = new List<Player>(),
                [PlayerRole.FLEX] = new List<Player>()
            };

            foreach (Player p in players)
            {
                byRole[p.Role].Add(p);
                if (p.Role == PlayerRole.RB || p.Role == PlayerRole.WR || p.Role == PlayerRole.TE)
                {
                    byRole[PlayerRole.FLEX].Add(p);
                }
            }

            foreach (List<Player> list in byRole.Values)
            {
                list.Sort();
            }

        }

        public FantasyFootballTeam GetRandomTeam()
        {
            List<Player> players = new List<Player>
            {
                PickRandomPlayerOfType(PlayerRole.QB),
                PickRandomPlayerOfType(PlayerRole.RB),
                PickRandomPlayerOfType(PlayerRole.RB),
                PickRandomPlayerOfType(PlayerRole.WR),
                PickRandomPlayerOfType(PlayerRole.WR),
                PickRandomPlayerOfType(PlayerRole.WR),
                PickRandomPlayerOfType(PlayerRole.TE),
                PickRandomPlayerOfType(PlayerRole.FLEX),
                PickRandomPlayerOfType(PlayerRole.DST)
            };
            return new FantasyFootballTeam(players);
        }

        public Player PickRandomPlayerOfType(PlayerRole role)
        {
            return byRole[role][rand.Next(0, byRole[role].Count)];
        }

    }

    public class Player : IComparable<Player>
    {
        public string Name { get; private set; }
        public PlayerRole Role { get; private set; }
        public int Salary { get; private set; }
        public Histogram ProjectedScore { get; private set; }
        public double HotChance { get; private set; }

        public Player(string name, PlayerRole role, int salary, Histogram projectedScore)
        {
            Name = name;
            Role = role;
            Salary = salary;
            ProjectedScore = projectedScore;
        }

        public Player(string name, PlayerRole role, int salary, double hotChance)
        {
            Name = name;
            Role = role;
            Salary = salary;
            HotChance = hotChance;
        }

        public int CompareTo(Player other)
        {
            return Salary.CompareTo(other.Salary);
        }

        public override string ToString()
        {
            return Name;
        }

        public void SetHotChance(double chance)
        {
            HotChance = chance;
        }

        public static PlayerRole ConvertToRole(IValue value)
        {
            switch((value as CategoricalValue).Value)
            {
                case "QB":
                    return PlayerRole.QB;
                case "RB":
                    return PlayerRole.RB;
                case "WR":
                    return PlayerRole.WR;
                case "TE":
                    return PlayerRole.TE;
                default:
                    return PlayerRole.DST;
            }
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
