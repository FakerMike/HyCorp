using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp.FantasyFootball.Corps.HotCo
{
    /*
     * 
     *  Types of Team Leads
     * 
     */


    /*
     * 
     *  Generics
     * 
     */
    public class TeamLeadHotCoDataImport : PassThroughCrossFunctionalTeamLead<RawDataFile, FullExampleSet, RawDataFile, FullExampleSet> { }
    public class TeamLeadHotCoExecutive : PassThroughPlanningTeamLead<FullExampleSet, ByDatePairedExampleSet> { }

}
