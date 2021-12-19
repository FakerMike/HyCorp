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
            LaborPool.AddWorker(typeof(WorkerHotCoPreviouslyHotDataEnrichment));
            LaborPool.AddWorker(typeof(WorkerHotCoModelingRandomTree));
            LaborPool.AddWorker(typeof(WorkerHotCoRandomPicker));
            LaborPool.AddWorker(typeof(WorkerHotCoSwapPicker));
            LaborPool.AddWorker(typeof(WorkerHotCoFilter));

            LaborPool.AddTeamLead(typeof(TeamLeadHotCoDataImport));
            LaborPool.AddTeamLead(typeof(TeamLeadHotCoExecutive));
            LaborPool.AddTeamLead(typeof(TeamLeadHotCoDataEnrichment));
            LaborPool.AddTeamLead(typeof(TeamLeadHotCoModeling));
            LaborPool.AddTeamLead(typeof(TeamLeadHotCoPicker));
            LaborPool.AddTeamLead(typeof(TeamLeadHotCoFilter));


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UI());
        }

        public static void RunNaiveCo()
        {

        }
        

    }
}
