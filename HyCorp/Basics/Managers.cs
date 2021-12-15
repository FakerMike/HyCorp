using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp
{

    /// <summary>
    /// Managers (baggers):  Track team performance.
    /// </summary>
    public abstract class Manager
    {
        public abstract bool Approves(Worker worker);
        public abstract List<Worker> Evaluate(List<Worker> workers);
    }

    public class AutoApprovingManager : Manager
    {
        public override bool Approves(Worker worker)
        {
            return true;
        }

        public override List<Worker> Evaluate(List<Worker> workers)
        {
            return new List<Worker>(workers);
        }
    }
}
