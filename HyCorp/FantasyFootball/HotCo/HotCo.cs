using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp.FantasyFootball.Corps.HotCo
{
    public class HotCo : HyCorp
    {
        protected override void BuildOrganization()
        {
            HotCoDataImportTeam hotCoDataImportTeam = new HotCoDataImportTeam();
            HotCoExecutiveTeam hotCoExecutiveTeam = new HotCoExecutiveTeam(this);


            PlanningOrganization.AddStartingTeam(hotCoDataImportTeam);
            PlanningOrganization.AddNextTeam(hotCoExecutiveTeam);

            ProductionOrganization.AddStartingTeam(hotCoDataImportTeam);
        }
    }
}
