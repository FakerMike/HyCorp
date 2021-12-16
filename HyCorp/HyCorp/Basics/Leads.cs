using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp
{

    /// <summary>
    /// Leads (boosters):  Aggregate Worker product
    /// </summary>

    public abstract class TeamLead
    {
        public List<HeadCount> HeadCount { get; protected set; } = new List<HeadCount>();
        public double FireThreshold { get; protected set; } = 0.2;

        public abstract bool CanPlan(Type PlanningInput, Type PlanningOutput);
        public abstract bool CanProduce(Type ProductionInput, Type ProductionOutput);
        public abstract int MinHires();
        public abstract int MaxHires();
        public abstract bool CanUse(Worker worker);
        public void AddWorker(Worker worker) { HeadCount.Add(new HeadCount(worker)); }
        public abstract object Plan(Clerk clerk);
        public abstract object Produce(Clerk clerk);
    }

    public class HeadCount
    {
        public Worker Worker { get; protected set; }
        public double Rating = 1;

        public HeadCount(Worker worker)
        {
            Worker = worker;
        }
    }

    public abstract class PlanningTeamLead<TInput, TOutput> : TeamLead
    {
        public override bool CanPlan(Type PlanningInput, Type PlanningOutput)
        {
            if (PlanningInput == typeof(TInput) && PlanningOutput == typeof(TOutput)) return true;
            return false;
        }
        public override bool CanProduce(Type ProductionInput, Type ProductionOutput)
        {
            return false;
        }
    }

    public abstract class ProductionTeamLead<TInput, TOutput> : TeamLead
    {
        public override bool CanPlan(Type PlanningInput, Type PlanningOutput)
        {
            return false;
        }
        public override bool CanProduce(Type ProductionInput, Type ProductionOutput)
        {
            if (ProductionInput == typeof(TInput) && ProductionOutput == typeof(TOutput)) return true;
            return false;
        }
    }

    public abstract class CrossFunctionalTeamLead<TPlanningInput, TPlanningOutput, TProductionInput, TProductionOutput> : TeamLead
    {
        public override bool CanPlan(Type PlanningInput, Type PlanningOutput)
        {
            if (PlanningInput == typeof(TPlanningInput) && PlanningOutput == typeof(TPlanningOutput)) return true;
            return false;
        }
        public override bool CanProduce(Type ProductionInput, Type ProductionOutput)
        {
            if (ProductionInput == typeof(TProductionInput) && ProductionOutput == typeof(TProductionOutput)) return true;
            return false;
        }
    }


    public class PassThroughPlanningTeamLead<TInput, TOutput> : PlanningTeamLead<TInput, TOutput>
    {
        public override bool CanUse(Worker worker)
        {
            return worker.CanPlan(typeof(TInput), typeof(TOutput));
        }

        public override int MinHires()
        {
            return 1;
        }

        public override int MaxHires()
        {
            return 1;
        }

        public override object Plan(Clerk clerk)
        {
            return (HeadCount[0].Worker as PlanningWorker<TInput, TOutput>).Plan((TInput) clerk.PlanningInput);
        }

        public override object Produce(Clerk clerk)
        {
            throw new Exception("Not a production lead");
        }
    }

    public class PassThroughProductionTeamLead<TInput, TOutput> : ProductionTeamLead<TInput, TOutput>
    {
        public override bool CanUse(Worker worker)
        {
            return worker.CanProduce(typeof(TInput), typeof(TOutput));
        }

        public override int MinHires()
        {
            return 1;
        }

        public override int MaxHires()
        {
            return 1;
        }

        public override object Produce(Clerk clerk)
        {
            return (HeadCount[0].Worker as ProductionWorker<TInput, TOutput>).Produce((TInput)clerk.ProductionInput);
        }

        public override object Plan(Clerk clerk)
        {
            throw new Exception("Not a planning lead");
        }
    }

    public class PassThroughCrossFunctionalTeamLead<TPlanningInput, TPlanningOutput, TProductionInput, TProductionOutput> : CrossFunctionalTeamLead<TPlanningInput, TPlanningOutput, TProductionInput, TProductionOutput>
    {
        public override bool CanUse(Worker worker)
        {
            if (worker.CanPlan(typeof(TPlanningInput), typeof(TPlanningOutput)) && worker.CanProduce(typeof(TPlanningInput), typeof(TPlanningOutput))) return true;
            return false;
        }

        public override int MinHires()
        {
            return 1;
        }

        public override int MaxHires()
        {
            return 1;
        }

        public override object Produce(Clerk clerk)
        {
            return (HeadCount[0].Worker as CrossFunctionalWorker<TPlanningInput, TPlanningOutput, TProductionInput, TProductionOutput>).Produce((TProductionInput)clerk.ProductionInput);
        }

        public override object Plan(Clerk clerk)
        {
            return (HeadCount[0].Worker as CrossFunctionalWorker<TPlanningInput, TPlanningOutput, TProductionInput, TProductionOutput>).Plan((TPlanningInput)clerk.PlanningInput);
        }
    }


}
