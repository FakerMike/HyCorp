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

        public Clerk(Team team)
        {
            this.team = team;
        }

        public virtual void Plan(object planningInput)
        {
            PlanningInput = planningInput;
            try
            {
                if (!team.IsPlanningTeam) throw new Exception("Wrong type of team!");
                PlanningOutput = team.Lead.Plan(this);
                team.PlanningDownstreamTeam.Clerk.Plan(PlanningOutput);
            } catch(Exception e)
            {
                UI.Instance.Print(e.Message);
            }
        }

        public virtual void Produce(object productionInput)
        {
            ProductionInput = productionInput;
            try
            {
                if (!team.IsProductionTeam) throw new Exception("Wrong type of team!");
                ProductionOutput = team.Lead.Produce(this);
                team.ProductionDownstreamTeam.Clerk.Produce(ProductionOutput);
            }
            catch (Exception e)
            {
                UI.Instance.Print(e.Message);
            }
        }
    }



}
