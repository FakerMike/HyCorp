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

        public FantasyFootballProduct(List<FantasyFootballTeam> teams) : base()
        {
            PredictedTeams = new List<FantasyFootballTeam>(teams);
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
            foreach (Player p in Team) { result += p.Salary * 0.004; }
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

        public bool SharesEightPlayersWith(FantasyFootballTeam other)
        {
            int count = 0;
            foreach (Player mine in Team)
            {
                foreach (Player theirs in other.Team) 
                {
                    if (mine == theirs)
                    {
                        count++;
                        if (count >= 8) return true;
                        break;
                    }
                }
            }
            return false;
        }


        public int CompareTo(FantasyFootballTeam other)
        {
            if (HotScore() > other.HotScore() + 10) return -1;
            if (HotScore() < other.HotScore() - 10) return 1;

            return (other.HotScore() + 20 * other.HotOdds() / (HotOdds() + other.HotOdds())).CompareTo(HotScore() + 20 * HotOdds() / (HotOdds() + other.HotOdds()));


            //int result = -HotOdds().CompareTo(other.HotOdds());
            //if (result == 0) return other.HotScore().CompareTo(HotScore());
            //return result;
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
            List<Player> players = new List<Player>();
            players.Add(PickRandomPlayerOfType(PlayerRole.QB, players));
            players.Add(PickRandomPlayerOfType(PlayerRole.RB, players));
            players.Add(PickRandomPlayerOfType(PlayerRole.RB, players));
            players.Add(PickRandomPlayerOfType(PlayerRole.WR, players));
            players.Add(PickRandomPlayerOfType(PlayerRole.WR, players));
            players.Add(PickRandomPlayerOfType(PlayerRole.WR, players));
            players.Add(PickRandomPlayerOfType(PlayerRole.TE, players));
            players.Add(PickRandomPlayerOfType(PlayerRole.FLEX, players));
            players.Add(PickRandomPlayerOfType(PlayerRole.DST, players));

            return new FantasyFootballTeam(players);
        }

        public Player PickRandomPlayerOfType(PlayerRole role, List<Player> team)
        {
            Player next = byRole[role][rand.Next(0, byRole[role].Count)];
            while (team.Contains(next))
            {
                next = byRole[role][rand.Next(0, byRole[role].Count)];
            }
            return next;
        }

        public void SmartSwap(FantasyFootballTeam team, int index, int minSalary, int maxSalary)
        {
            PlayerRole role = team.Team[index].Role;
            if (index == 7) role = PlayerRole.FLEX;
            Player current = team.Team[index];
            Player next;
            for (int i = 0; i < 100; i++)
            {
                next = byRole[role][rand.Next(0, byRole[role].Count)];
                while (team.Team.Contains(next))
                {
                    next = byRole[role][rand.Next(0, byRole[role].Count)];
                }
                if (next.Salary >= minSalary && next.Salary <= maxSalary)
                {
                    double chance = next.HotChance / (current.HotChance - next.HotChance);
                    if (rand.NextDouble() < chance)
                    {
                        team.Team[index] = next;
                        return;
                    }
                }
            }

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
