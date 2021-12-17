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
            Organization.AddStartingTeam(new HotCoDataImportTeam());
            Organization.AddNextTeam(new HotCoExecutiveTeam(this));
            Organization.AddNextTeam(new HotCoDataEnrichmentTeam());
        }
    }
}
