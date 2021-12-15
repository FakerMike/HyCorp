using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp.FantasyFootball
{
    public class PickerTeam : Team<List<Player>, FantasyFootballProduct>
    {
        public PickerTeam() : base()
        {
            Hire(new AnyInputClerk<List<Player>, FantasyFootballProduct>(this));
            Hire(new AutoApprovingManager());
            Hire(new OneWorkerTeamLead<List<Player>, FantasyFootballProduct>());
            Hire(new List<PickerWorker> { new ThreeSixPicker() });
            Staffed = true;
        }

    }


    public abstract class PickerWorker : Worker
    {
        protected const int TOTALTEAMS = 1000;
        protected const int SALARYCAP = 50000;
        protected const int SPINCOUNT = 1000;
        protected static Random rand = new Random();
        protected List<Player> players;
        protected Dictionary<PlayerRole, List<Player>> byRole;

        // Does not train
        public override object Train(object input)
        {
            return null;
        }


        public override object Work(object input)
        {
            List<List<Player>> castInput = (List<List<Player>>)input;
            players = castInput.Last();
            FantasyFootballProduct result = new FantasyFootballProduct();
            byRole = new Dictionary<PlayerRole, List<Player>>();
            byRole[PlayerRole.TE] = new List<Player>();
            byRole[PlayerRole.WR] = new List<Player>();
            byRole[PlayerRole.QB] = new List<Player>();
            byRole[PlayerRole.RB] = new List<Player>();
            byRole[PlayerRole.DST] = new List<Player>();
            byRole[PlayerRole.FLEX] = new List<Player>();


            foreach (Player p in players)
            {
                byRole[p.Role].Add(p);
                if (p.Role == PlayerRole.RB || p.Role == PlayerRole.WR || p.Role == PlayerRole.TE)
                {
                    byRole[PlayerRole.FLEX].Add(p);
                }
            }

            foreach (List<Player> list in byRole.Values) {
                list.Sort();
            }

            for (int i = 0; i < TOTALTEAMS; i++)
            {
                FantasyFootballTeam team = PickGoodTeam();
                result.AddTeam(team);
            }
            result.SortTeams();
            return result;
        }

        protected abstract FantasyFootballTeam PickGoodTeam();

        protected int RemainingSalary(Player[] team)
        {
            int currentCost = 0;
            foreach (Player p in team)
            {
                currentCost += p.Salary;
            }
            return SALARYCAP - currentCost;
        }


        protected Player PickGoodPlayerOfType(PlayerRole role, double p, Player[] team)
        {
            double max = 0;
            List<Player> target = byRole[role];
            Player result = target[0];
            foreach (Player player in target)
            {
                if (rand.NextDouble() > p) continue;
                if (team.Contains(player)) continue;
                double sum = 0;
                for (int i = 0; i < 30; i++)
                {
                    sum += player.ProjectedScore.Sample();
                }
                if (sum > max)
                {
                    result = player;
                    max = sum;
                } else if (sum == max)
                {
                    if (player.Salary < result.Salary) result = player;
                }
            }

            return result;
        }

        protected Player PickCheapPlayerOfType(PlayerRole role, Player[] team)
        {
            int min = int.MaxValue;
            List<Player> target = byRole[role];
            Player result = target[0];
            foreach (Player player in target)
            {
                if (team.Contains(player)) continue;
                double me = 0;
                double you = 0;
                if (player.Salary < min)
                {
                    result = player;
                    min = player.Salary;
                }
                else if (player.Salary == min)
                {
                    for (int i = 0; i < 30; i++)
                    {
                        me += player.ProjectedScore.Sample();
                    }
                    for (int i = 0; i < 30; i++)
                    {
                        you += result.ProjectedScore.Sample();
                    }
                    if (me > you) result = player;
                }
            }

            return result;
        }


        protected Player PickBetterPlayer(Player player, Player[] team, int MaxCashDifference, PlayerRole role)
        {
            List<Player> target = byRole[role];
            Player result = player;

            // Try 10 random players
            for (int i = 0; i < 10; i++)
            {
                Player p = target[rand.Next(0, target.Count)];
                if (team.Contains(p)) continue;
                if (p.Salary - player.Salary > MaxCashDifference) continue;

                double me = 0;
                double you = 0;
                for (int j = 0; j < 30; j++)
                {
                    me += p.ProjectedScore.Sample();
                }
                for (int j = 0; j < 30; j++)
                {
                    you += result.ProjectedScore.Sample();
                }
                if (me > you) result = p;
            }

            return result;
        }

    }

    public class ThreeSixPicker : PickerWorker
    {
        protected override FantasyFootballTeam PickGoodTeam()
        {
            // QB, RB, RB, WR, WR, WR, TE, FLEX, DST
            int[] fixedPositions = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            Player[] team = new Player[9];
            fixedPositions[rand.Next(0, 9)] = 1;
            int nextIndex = rand.Next(0, 9);
            while (fixedPositions[nextIndex] == 1) { nextIndex = rand.Next(0, 9); }
            fixedPositions[nextIndex] = 1;
            nextIndex = rand.Next(0, 9);
            while (fixedPositions[nextIndex] == 1) { nextIndex = rand.Next(0, 9); }
            fixedPositions[nextIndex] = 1;

            if (fixedPositions[0] == 1) team[0] = PickGoodPlayerOfType(PlayerRole.QB, 0.25, team); else team[0] = PickCheapPlayerOfType(PlayerRole.QB, team);
            if (fixedPositions[1] == 1) team[1] = PickGoodPlayerOfType(PlayerRole.RB, 0.25, team); else team[1] = PickCheapPlayerOfType(PlayerRole.RB, team);
            if (fixedPositions[2] == 1) team[2] = PickGoodPlayerOfType(PlayerRole.RB, 0.25, team); else team[2] = PickCheapPlayerOfType(PlayerRole.RB, team);
            if (fixedPositions[3] == 1) team[3] = PickGoodPlayerOfType(PlayerRole.WR, 0.25, team); else team[3] = PickCheapPlayerOfType(PlayerRole.WR, team);
            if (fixedPositions[4] == 1) team[4] = PickGoodPlayerOfType(PlayerRole.WR, 0.25, team); else team[4] = PickCheapPlayerOfType(PlayerRole.WR, team);
            if (fixedPositions[5] == 1) team[5] = PickGoodPlayerOfType(PlayerRole.WR, 0.25, team); else team[5] = PickCheapPlayerOfType(PlayerRole.WR, team);
            if (fixedPositions[6] == 1) team[6] = PickGoodPlayerOfType(PlayerRole.TE, 0.25, team); else team[6] = PickCheapPlayerOfType(PlayerRole.TE, team);
            if (fixedPositions[7] == 1) team[7] = PickGoodPlayerOfType(PlayerRole.FLEX, 0.25, team); else team[7] = PickCheapPlayerOfType(PlayerRole.FLEX, team);
            if (fixedPositions[8] == 1) team[8] = PickGoodPlayerOfType(PlayerRole.DST, 0.25, team); else team[8] = PickCheapPlayerOfType(PlayerRole.DST, team);

            
            for (int i = 0; i < SPINCOUNT; i++)
            {
                nextIndex = rand.Next(0, 9);
                while (fixedPositions[nextIndex] == 1) { nextIndex = rand.Next(0, 9); }
                if (nextIndex != 7)
                    team[nextIndex] = PickBetterPlayer(team[nextIndex], team, 100 * rand.Next(0, RemainingSalary(team) / 100 + 1), team[nextIndex].Role);
                else
                    team[nextIndex] = PickBetterPlayer(team[nextIndex], team, 100 * rand.Next(0, RemainingSalary(team) / 100 + 1), PlayerRole.FLEX);
            }


            if (RemainingSalary(team) >= 0)
                return new FantasyFootballTeam(team);
            else
            {
                Console.WriteLine("Failed!");
                return PickGoodTeam();
            }
        }
    }


}
