using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp.FantasyFootball.Corps.HotCo
{
    public class ClerkHotCoExecutive : Clerk
    {
        private int Epoch = 0;

        public ClerkHotCoExecutive(Team team, HotCo company) : base(team) {}

        public override void Plan(object planningInput)
        {
            PlanningInput = planningInput;
            try
            {
                if (!team.IsPlanningTeam) throw new Exception("Wrong type of team!");
                RunSimulation();
            }
            catch (Exception e)
            {
                UI.Instance.Print(e.Message);
            }
        }

        public override void Produce(object productionInput)
        {
            UI.Instance.Print("This is a planning clerk only!");
        }

        public void RunSimulation()
        {
            PlanningOutput = team.Lead.Plan(this);
            team.PlanningDownstreamTeam.Clerk.Plan(PlanningOutput);
        }

    }


}
