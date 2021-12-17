using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HyCorp.FantasyFootball.Corps;
using HyCorp.FantasyFootball.Corps.HotCo;

namespace HyCorp.FantasyFootball
{
    /// <summary>
    /// Example HyCorps to try and predict Fantasy Football teams
    /// </summary>
    public static class FantasyFootball
    {
        
        static void Main(string[] args)
        {
            LaborPool.AddWorker(typeof(HotCoDataImportWorker));
            LaborPool.AddWorker(typeof(HotCoExecutiveWorker));
            LaborPool.AddWorker(typeof(WorkerHotCoHistoricalAverageDataEnrichment));

            LaborPool.AddTeamLead(typeof(TeamLeadHotCoDataImport));
            LaborPool.AddTeamLead(typeof(TeamLeadHotCoExecutive));
            LaborPool.AddTeamLead(typeof(TeamLeadHotCoDataEnrichment));
            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UI());
        }

        public static void RunNaiveCo()
        {

        }
        

    }
}
