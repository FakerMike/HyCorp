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
            Name = "Data Import Team";
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
            Name = "Executive Team";
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
            Name = "Data Enrichment Team";
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
        public HotCoModelingTeam()
        {
            Name = "Modeling Team";
            TeamInput = typeof(EnrichedByDatePairedExampleSet);
            TeamOutput = typeof(PlayersWithPredictedHotChance);

            Hire(new Clerk(this));
            Hire(new ManagerHotCoModelingRandomForest(this));
            Manager.HireLead();
            Manager.HireWorkers();
        }
    }



    public class PlayersWithPredictedHotChance : Intermediate<PlayerPicker>
    {
        public PlayersWithPredictedHotChance(PlayerPicker product) : base(product) { }
    }

    public class HotCoPlayerPickerTeam : Team
    {
        public HotCoPlayerPickerTeam()
        {
            Name = "Picker Team";
            TeamInput = typeof(PlayersWithPredictedHotChance);
            TeamOutput = typeof(PotentialTeamList);

            Hire(new Clerk(this));
            Hire(new RandomReplacementOnlyManager(this));
            Manager.HireLead();
            Manager.HireWorkers();
        }
    }

    public class PotentialTeamList : Intermediate<List<FantasyFootballTeam>>
    {
        public PotentialTeamList(List<FantasyFootballTeam> product) : base(product) { }
    }

    public class HotCoFilterTeam : Team
    {
        public HotCoFilterTeam()
        {
            Name = "Filter Team";
            TeamInput = typeof(PotentialTeamList);
            TeamOutput = typeof(FantasyFootballProduct);

            Hire(new Clerk(this));
            Hire(new NoOptionManager(this));
            Manager.HireLead();
            Manager.HireWorkers();
        }
    }

}
