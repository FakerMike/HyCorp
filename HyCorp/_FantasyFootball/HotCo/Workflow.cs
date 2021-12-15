using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp._FantasyFootball.HotCo
{


    public class HotCoDataImportTeam : Team<RawDataFile, FullExampleSet>
    {

    }

    public class FullExampleSet : ExampleSet
    {
        public FullExampleSet(ProcessedDataFile file) : base (file)
        {
        }
    }

    public class HotCoDataTeam : Team<FullExampleSet, CompressedToActiveExampleSet>
    {

    }

    public class CompressedToActiveExampleSet : ExampleSet
    {
        public CompressedToActiveExampleSet(IEnumerable<Feature> features, Feature label) : base(features, label)
        {
        }
    }



}
