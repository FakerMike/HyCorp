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
        public abstract bool Approves(Worker worker);
        public abstract List<Worker> Evaluate(List<Worker> workers);
    }

    public class NewManager
    {
        private readonly NewTeam team;

        public NewManager(NewTeam team)
        {
            this.team = team;
        }
        public void HireLead()
        {
            foreach(Type newTeamLead in LaborPool.TeamLeads)
            {
                NewTeamLead lead = Activator.CreateInstance(newTeamLead) as NewTeamLead;
                if ((team.IsPlanningTeam && !lead.CanPlan(team.TeamInput, team.TeamOutput)) || (team.IsProductionTeam && !lead.CanProduce(team.TeamInput, team.TeamOutput))) continue;
                team.Hire(lead);
                break;
            }
        }

        public void HireWorkers() {
            for (int i = 0; i < team.Lead.MinHires(); i++)
            {
                foreach (Type newWorker in LaborPool.Workers)
                {
                    NewWorker worker = Activator.CreateInstance(newWorker) as NewWorker;
                    if ((team.IsPlanningTeam && !worker.CanPlan(team.TeamInput, team.TeamOutput)) || (team.IsProductionTeam && !worker.CanProduce(team.TeamInput, team.TeamOutput))) continue;
                    team.Lead.AddWorker(worker);
                    break;
                }
            }
        }
    }


    public class AutoApprovingManager : Manager
    {
        public override bool Approves(Worker worker)
        {
            return true;
        }

        public override List<Worker> Evaluate(List<Worker> workers)
        {
            return new List<Worker>(workers);
        }
    }
}
