using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp.FantasyFootball.Corps.HotCo
{
    class HotCo : HyCorp<RawDataFile, FantasyFootballProduct>
    {
        protected override void BuildOrganization()
        {
            HotCoDataImportTeam hotCoDataImportTeam = new HotCoDataImportTeam();
            
        }
    }
}
