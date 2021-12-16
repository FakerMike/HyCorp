using HyCorp.FantasyFootball;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp.FantasyFootball.Corps.HotCo
{
    public class HotCoDataImportTeam : NewTeam
    {
        public HotCoDataImportTeam()
        {
            TeamInput = typeof(RawDataFile);
            TeamOutput = typeof(FullExampleSet);
            IsPlanningTeam = true;
            IsProductionTeam = true;

            Hire(new TypedClerk<RawDataFile, FullExampleSet>());
            Hire(new NewManager(this));
            Manager.HireLead();
            Manager.HireWorkers();
        }
    }

    public class FullExampleSet : Intermediate<ExampleSet>
    {
        public FullExampleSet(ExampleSet product) : base(product) { }
    }

    public class HotCoExecutiveTeam : Team<FullExampleSet, ByDatePairedExampleSet>
    {

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

    public class HotCoDataEnrichmentTeam : Team<ByDatePairedExampleSet, EnrichedByDatePairedExampleSet>
    {

    }

    public class EnrichedByDatePairedExampleSet : Intermediate<PairedExampleSet>
    {
        public EnrichedByDatePairedExampleSet(PairedExampleSet product) : base(product) { }
    }

    public class HotCoModelingTeam : Team<EnrichedByDatePairedExampleSet, PlayersWithPredictedScores>
    {

    }

    public class PlayersWithPredictedScores : Intermediate<List<Player>>
    {
        public PlayersWithPredictedScores(List<Player> product) : base(product) { }
    }

    public class HotCoPickerTeam : Team<PlayersWithPredictedScores, FantasyFootballProduct>
    {

    }
}
