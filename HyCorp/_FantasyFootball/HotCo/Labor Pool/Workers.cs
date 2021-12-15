using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp._FantasyFootball.HotCo.Labor_Pool
{
    /*
     * 
     *  TYPES OF WORKERS
     * 
     */

    public abstract class HotCoDataWorker : IWorker
    {
        protected int setting1;
        protected int setting2;
        protected int setting3;

        public HotCoDataWorker(int setting1 = 0, int setting2 = 0, int setting3 = 0)
        {
            this.setting1 = setting1;
            this.setting2 = setting2;
            this.setting3 = setting3;
        }
        public virtual int Setting1Max() { return 0; }
        public virtual int Setting2Max() { return 0; }
        public virtual int Setting3Max() { return 0; }

        public abstract void AddDataToExamples();
    }
}
