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
        public Dictionary<Worker, PerformanceEvaluation> Workers { get; protected set; } = new Dictionary<Worker, PerformanceEvaluation>();
        public double FireThreshold { get; protected set; } = 0.2;

        public abstract bool CanLead(Type Input, Type Output);
        public abstract int MinHires();
        public abstract int MaxHires();
        public abstract bool CanUse(Worker worker);
        public void AddWorker(Worker worker) { Workers[worker] = new PerformanceEvaluation(); }
        public virtual void Prepare(Clerk clerk) { }
        public abstract object Plan(Clerk clerk);
        public abstract object Produce(Clerk clerk);
        public abstract void Evaluate();

        public virtual void DoLayoffs()
        {
            foreach (Worker w in Workers.Keys)
            {
                if (Workers[w].Rating <= FireThreshold) Workers.Remove(w);
            }
        }
    }

    public class PerformanceEvaluation
    {
        public double Rating = 1;
        public int Tenure = 0;
    }


    public abstract class CrossFunctionalTeamLead<TInput, TOutput> : TeamLead
    {
        public override bool CanLead(Type Input, Type Output)
        {
            if (Input == typeof(TInput) && Output == typeof(TOutput)) return true;
            return false;
        }
    }

    public class PassThroughCrossFunctionalTeamLead<TInput, TOutput> : CrossFunctionalTeamLead<TInput, TOutput>
    {
        public override bool CanUse(Worker worker)
        {
            if (worker.CanPlan(typeof(TInput), typeof(TOutput)) && worker.CanProduce(typeof(TInput), typeof(TOutput))) return true;
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
            return (Workers.Keys.First() as CrossFunctionalWorker<TInput, TOutput, TInput, TOutput>).Produce((TInput)clerk.ProductionInput);
        }

        public override object Plan(Clerk clerk)
        {
            return (Workers.Keys.First() as CrossFunctionalWorker<TInput, TOutput, TInput, TOutput>).Plan((TInput)clerk.PlanningInput);
        }

        public override void Evaluate()
        {
            // Nothing to evaluate
        }
    }


}
