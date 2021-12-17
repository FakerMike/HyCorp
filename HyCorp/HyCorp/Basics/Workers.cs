using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp
{
    /// <summary>
    /// Workers (classifiers):  Produce individual product
    /// TYPE UNSAFE:  Make sure you use the right workers!
    /// </summary> 

    public abstract class Worker
    {
        public abstract bool CanPlan(Type PlanningInput, Type PlanningOutput);
        public abstract bool CanProduce(Type ProductionInput, Type ProductionOutput);
    }


    public abstract class CrossFunctionalWorker<TPlanningInput, TPlanningOutput, TProductionInput, TProductionOutput> : Worker
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

}
