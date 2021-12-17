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

        public int Budget { get; protected set; } = 10;
        public bool MadeChange { get; protected set; } = false;

        public Manager(Team team)
        {
            this.team = team;
        }
        public void HireLead()
        {
            foreach(Type newTeamLead in LaborPool.TeamLeads)
            {
                TeamLead lead = Activator.CreateInstance(newTeamLead) as TeamLead;
                if (!lead.CanLead(team.TeamInput, team.TeamOutput)) continue;
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
                    if (!team.Lead.CanUse(worker)) continue;
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
            team.Lead.DoLayoffs();
            for (int i = 0; i < (team.Lead.MaxHires() - team.Lead.Workers.Count); i++)
            {
                team.Lead.AddWorker(Activator.CreateInstance(availableWorkers[rand.Next(0, availableWorkers.Count)]) as Worker);
                MadeChange = true;
            }
        }
    }
}
