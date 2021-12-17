using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp.FantasyFootball.Corps.HotCo
{
    public class ClerkHotCoExecutive : Clerk
    {
        public ClerkHotCoExecutive(Team team, HotCo company) : base(team) {}

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
                input.TrainingWeek++;
                if (input.TrainingWeek > 17)
                {
                    input.TrainingWeek = 5;
                    input.TrainingYear++;
                }
            }
        }

    }


}
