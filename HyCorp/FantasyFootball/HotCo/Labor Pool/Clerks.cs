using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp.FantasyFootball.Corps.HotCo
{
    public class ClerkHotCoExecutive : Clerk
    {
        HotCo Company;
        public ClerkHotCoExecutive(Team team, HotCo company) : base(team) {
            Company = company;
        }

        public override void Plan(object planningInput)
        {
            PlanningInput = planningInput;
            RunSimulation();
        }

        public override void Produce(object productionInput)
        {
            
        }

        public void RunSimulation()
        {
            FullExampleSet input = PlanningInput as FullExampleSet;
            while (input.TrainingYear < 2021 || input.TrainingWeek <= 9)
            {
                PlanningOutput = team.Lead.Plan(this);
                team.DownstreamTeam.Clerk.Plan(PlanningOutput);
                HotCoFilterTeam filterTeam = Company.Organization.FindTeamOfType<HotCoFilterTeam>();
                (team.Lead as TeamLeadHotCoExecutive).AddProduct(filterTeam.GetPlanningOutput() as FantasyFootballProduct, input.TrainingWeek, input.TrainingYear);
                //(team.Lead as TeamLeadHotCoExecutive).SummarizePerformance();
                input.TrainingWeek++;
                if (input.TrainingWeek > 17)
                {
                    input.TrainingWeek = 5;
                    input.TrainingYear++;
                }
            }
            (team.Lead as TeamLeadHotCoExecutive).SummarizePerformance();
        }

    }


}
