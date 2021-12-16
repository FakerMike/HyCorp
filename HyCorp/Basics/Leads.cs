﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp
{

    /// <summary>
    /// Leads (boosters):  Aggregate Worker product
    /// </summary>
    public abstract class Lead<Input, Output>
    {
        public abstract Output Produce(List<Input> inputs, List<Worker> workers);
        public abstract Output Train(List<Input> trainingInputs, List<Worker> workers);
    }

    public abstract class NewTeamLead
    {
        public Dictionary<NewWorker, double> Workers { get; protected set; } = new Dictionary<NewWorker, double>();
        public abstract bool CanPlan(Type PlanningInput, Type PlanningOutput);
        public abstract bool CanProduce(Type ProductionInput, Type ProductionOutput);
        public abstract int MinHires();
        public abstract int MaxHires();
        public abstract bool CanUse(NewWorker worker);
        public void AddWorker(NewWorker worker) { Workers[worker] =  1.0; }
    }

    public abstract class PlanningTeamLead<TInput, TOutput> : NewTeamLead
    {
        public abstract TOutput Plan(TInput input);
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

    public abstract class ProductionTeamLead<TInput, TOutput> : NewTeamLead
    {
        public abstract TOutput Produce(TInput input);
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

    public abstract class CrossFunctionalTeamLead<TPlanningInput, TPlanningOutput, TProductionInput, TProductionOutput> : NewTeamLead
    {
        public abstract TPlanningOutput Plan(TPlanningInput input);
        public abstract TProductionOutput Produce(TProductionInput input);
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
        public override bool CanUse(NewWorker worker)
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

        public override TOutput Plan(TInput input)
        {
            return (Workers.Keys.First() as PlanningWorker<TInput, TOutput>).Plan(input);
        }
    }

    public class PassThroughProductionTeamLead<TInput, TOutput> : ProductionTeamLead<TInput, TOutput>
    {
        public override bool CanUse(NewWorker worker)
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

        public override TOutput Produce(TInput input)
        {
            return (Workers.Keys.First() as ProductionWorker<TInput, TOutput>).Produce(input);
        }
    }

    public class PassThroughCrossFunctionalTeamLead<TPlanningInput, TPlanningOutput, TProductionInput, TProductionOutput> : CrossFunctionalTeamLead<TPlanningInput, TPlanningOutput, TProductionInput, TProductionOutput>
    {
        public override bool CanUse(NewWorker worker)
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

        public override TProductionOutput Produce(TProductionInput input)
        {
            return (Workers.Keys.First() as CrossFunctionalWorker<TPlanningInput, TPlanningOutput, TProductionInput, TProductionOutput>).Produce(input);
        }

        public override TPlanningOutput Plan(TPlanningInput input)
        {
            return (Workers.Keys.First() as CrossFunctionalWorker<TPlanningInput, TPlanningOutput, TProductionInput, TProductionOutput>).Plan(input);
        }
    }





    public class OneWorkerTeamLead<Input, Output> : Lead<Input, Output>
    {
        public override Output Produce(List<Input> inputs, List<Worker> workers)
        {
            return (Output) workers[0].Work(inputs);
        }

        public override Output Train(List<Input> trainingInputs, List<Worker> workers)
        {
            return (Output) workers[0].Train(trainingInputs);
        }
    }

}
