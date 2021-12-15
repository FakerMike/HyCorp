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
    public abstract class Lead<Input, Output>
    {
        public abstract Output Produce(List<Input> inputs, List<Worker> workers);
        public abstract Output Train(List<Input> trainingInputs, List<Worker> workers);
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
