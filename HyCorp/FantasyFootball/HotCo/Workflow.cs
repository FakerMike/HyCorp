using HyCorp.FantasyFootball;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp.FantasyFootball.Corps.HotCo
{
    public class HotCoDataImportTeam : Team
    {
        public HotCoDataImportTeam()
        {
            TeamInput = typeof(RawDataFile);
            TeamOutput = typeof(FullExampleSet);
            IsPlanningTeam = true;
            IsProductionTeam = true;

            Hire(new Clerk(this));
            Hire(new NoOptionManager(this));
            Manager.HireLead();
            Manager.HireWorkers();
        }
    }

    public class FullExampleSet : Intermediate<ExampleSet>
    {
        public FullExampleSet(ExampleSet product) : base(product) { }
    }

    public class HotCoExecutiveTeam : Team
    {
        public HotCoExecutiveTeam(HotCo company) : base()
        {
            TeamInput = typeof(FullExampleSet);
            TeamOutput = typeof(ByDatePairedExampleSet);
            IsPlanningTeam = true;
            IsProductionTeam = false;

            Hire(new ClerkHotCoExecutive(this, company));
            Hire(new NoOptionManager(this));
            Manager.HireLead();
            Manager.HireWorkers();
        }
    }

    public class PairedExampleSet
    {
        public ExampleSet TrainingSet { get; private set; }
        public ExampleSet TestSet { get; private set; }
        public PairedExampleSet(ExampleSet trainingSet, ExampleSet testSet)
        {
            TrainingSet = trainingSet;
            TestSet = testSet;
        }
    }

    public class ByDatePairedExampleSet : Intermediate<PairedExampleSet>
    {
        public ByDatePairedExampleSet(PairedExampleSet product) : base(product) { }
    }

    public class HotCoDataEnrichmentTeam : Team
    {

    }

    public class EnrichedByDatePairedExampleSet : Intermediate<PairedExampleSet>
    {
        public EnrichedByDatePairedExampleSet(PairedExampleSet product) : base(product) { }
    }

    public class HotCoModelingTeam : Team
    {

    
    }

    public class PlayersWithPredictedScores : Intermediate<List<Player>>
    {
        public PlayersWithPredictedScores(List<Player> product) : base(product) { }
    }

    public class HotCoPickerTeam : Team
    {

    }
}
