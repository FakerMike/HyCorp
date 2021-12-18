using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp
{


    public class Clerk
    {
        protected Team team;
        public object PlanningInput { get; protected set; }
        public object PlanningOutput { get; protected set; }
        public object ProductionInput { get; protected set; }
        public object ProductionOutput { get; protected set; }

        public int PlanningIteration { get; protected set; } = 0;

        public Clerk(Team team)
        {
            this.team = team;
        }

        public virtual void Plan(object planningInput)
        {
            UI.Print($"{team.Name} is planning...");
            PlanningInput = planningInput;
            team.Lead.Prepare(this);
            for (PlanningIteration = 0; PlanningIteration < team.Manager.Budget; PlanningIteration++)
            {
                PlanningOutput = team.Lead.Plan(this);
                team.Lead.Evaluate();
                team.Manager.UpdateHeadCount();
                if (!team.Manager.MadeChange) break;
            }
            if (team.DownstreamTeam != null)
            {
                team.DownstreamTeam.Clerk.Plan(PlanningOutput);
            }
        }

        public virtual void Produce(object productionInput)
        {
            ProductionInput = productionInput;
            try
            {
                ProductionOutput = team.Lead.Produce(this);
                team.DownstreamTeam.Clerk.Produce(ProductionOutput);
            }
            catch (Exception e)
            {
                UI.Instance.Print(e.Message);
            }
        }
    }



}
