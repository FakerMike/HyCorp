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

            Hire(new Clerk(this));
            Hire(new NoOptionManager(this));
            Manager.HireLead();
            Manager.HireWorkers();
        }
    }

    public class FullExampleSet : Intermediate<ExampleSet>
    {
        public int TrainingWeek = 5;
        public int TrainingYear = 2014;
        public FullExampleSet(ExampleSet product) : base(product) { }
    }

    public class HotCoExecutiveTeam : Team
    {
        public HotCoExecutiveTeam(HotCo company) : base()
        {
            TeamInput = typeof(FullExampleSet);
            TeamOutput = typeof(ByDatePairedExampleSet);

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
        public int Year;
        public int Week;
        public ByDatePairedExampleSet(PairedExampleSet product, int week, int year) : base(product) {
            Year = year;
            Week = week;
        }
    }

    public class HotCoDataEnrichmentTeam : Team
    {
        public HotCoDataEnrichmentTeam() : base()
        {
            TeamInput = typeof(ByDatePairedExampleSet);
            TeamOutput = typeof(EnrichedByDatePairedExampleSet);

            Hire(new Clerk(this));
            Hire(new RandomReplacementOnlyManager(this));
            Manager.HireLead();
            Manager.HireWorkers();
        }
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

    public class HotCoPlayerPickerTeam : Team
    {

    }

    public class HotCoPotentialTeamList : Intermediate<List<FantasyFootballTeam>>
    {
        public HotCoPotentialTeamList(List<FantasyFootballTeam> product) : base(product) { }
    }

    public class HotCoFilterTeam : Team
    {

    }

}
