using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp.FantasyFootball.Corps.HotCo
{

    public class ManagerHotCoModelingRandomForest : Manager
    {
        private TeamLeadHotCoModeling modelingLead;

        public ManagerHotCoModelingRandomForest(Team team) : base(team) { }

        public override void HireLead()
        {
            foreach (Type newTeamLead in LaborPool.TeamLeads)
            {
                TeamLead lead = Activator.CreateInstance(newTeamLead) as TeamLead;
                if (!(lead is TeamLeadHotCoModeling)) continue;
                availableLeads.Add(newTeamLead);
                break;
            }
            team.Hire(Activator.CreateInstance(availableLeads[0]) as TeamLead);
            modelingLead = team.Lead as TeamLeadHotCoModeling;
        }

        public override void UpdateHeadCount()
        {
            MadeChange = false;
            team.Lead.DoLayoffs();
            for (int i = 0; i < (team.Lead.MaxHires() - team.Lead.Workers.Count); i++)
            {
                team.Lead.AddWorker(Activator.CreateInstance(availableWorkers[rand.Next(0, availableWorkers.Count)]) as Worker);
                MadeChange = true;
            }
        }
    }
}
