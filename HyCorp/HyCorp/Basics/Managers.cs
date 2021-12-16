using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp
{

    /// <summary>
    /// Managers (baggers):  Track team performance.
    /// </summary>

    public abstract class Manager
    {
        protected static Random rand = new Random();
        protected readonly Team team;
        protected List<Type> availableLeads = new List<Type>();
        protected List<Type> availableWorkers = new List<Type>();

        public Manager(Team team)
        {
            this.team = team;
        }
        public void HireLead()
        {
            foreach(Type newTeamLead in LaborPool.TeamLeads)
            {
                TeamLead lead = Activator.CreateInstance(newTeamLead) as TeamLead;
                if ((team.IsPlanningTeam && !lead.CanPlan(team.TeamInput, team.TeamOutput)) || (team.IsProductionTeam && !lead.CanProduce(team.TeamInput, team.TeamOutput))) continue;
                availableLeads.Add(newTeamLead);
                break;
            }
            team.Hire(Activator.CreateInstance(availableLeads[0]) as TeamLead);
        }

        public void HireWorkers() {
            for (int i = 0; i < team.Lead.MinHires(); i++)
            {
                foreach (Type newWorker in LaborPool.Workers)
                {
                    Worker worker = Activator.CreateInstance(newWorker) as Worker;
                    if ((team.IsPlanningTeam && !worker.CanPlan(team.TeamInput, team.TeamOutput)) || (team.IsProductionTeam && !worker.CanProduce(team.TeamInput, team.TeamOutput))) continue;
                    availableWorkers.Add(newWorker);
                    break;
                }
                team.Lead.AddWorker(Activator.CreateInstance(availableWorkers[rand.Next(0,availableWorkers.Count)]) as Worker);
            }
        }

        public abstract void UpdateHeadCount();
    }

    public class NoOptionManager : Manager
    {
        public NoOptionManager(Team team) : base(team) { }

        public override void UpdateHeadCount()
        {
           // No.
        }
    }

    public class RandomReplacementOnlyManager : Manager
    {
        public RandomReplacementOnlyManager(Team team) : base(team) { }

        public override void UpdateHeadCount()
        {
            team.Lead.HeadCount.RemoveAll(x => x.Rating <= team.Lead.FireThreshold);
            for (int i = 0; i < (team.Lead.MaxHires() - team.Lead.HeadCount.Count); i++)
            {
                team.Lead.AddWorker(Activator.CreateInstance(availableWorkers[rand.Next(0, availableWorkers.Count)]) as Worker);
            }
        }
    }
}
