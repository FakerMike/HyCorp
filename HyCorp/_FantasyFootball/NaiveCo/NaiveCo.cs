using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp.FantasyFootball.Corps
{
    // NaiveCo takes in RawDataFiles and outputs a FantasyFootballProduct
    class NaiveCo : HyCorp<RawDataFile, FantasyFootballProduct>
    {
        protected override void BuildOrganization()
        {
            ProcessingTeam processingTeam = new ProcessingTeam();
            NaiveCoDataTeam naiveCoDataTeam = new NaiveCoDataTeam();
            NaiveCoModelingTeam naiveCoModelingTeam = new NaiveCoModelingTeam();
            PickerTeam pickerTeam = new PickerTeam();

            PlanningOrganization.AddStartingTeam(processingTeam);
            PlanningOrganization.AddDownstreamTeam(processingTeam, naiveCoDataTeam);
            PlanningOrganization.AddDownstreamTeam(naiveCoDataTeam, naiveCoModelingTeam);

            // Note that these will have a different input, but some teams are the same either way.
            ProductionOrganization.AddStartingTeam(processingTeam);

            PassthroughDataTeam passthroughDataTeam = new PassthroughDataTeam();
            ProductionOrganization.AddDownstreamTeam(processingTeam, passthroughDataTeam);
            ProductionOrganization.AddDownstreamTeam(passthroughDataTeam, naiveCoModelingTeam);
            ProductionOrganization.AddEndingTeam(naiveCoModelingTeam, pickerTeam);

        }
    }
}
