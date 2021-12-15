using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp.FantasyFootball
{
    // Does not enrich data at all
    public class PassthroughDataTeam : Team<ProcessedDataFile, ExampleSet>
    {
        public PassthroughDataTeam(): base()
        {
            Hire(new AnyInputClerk<ProcessedDataFile,ExampleSet>(this));
            Hire(new AutoApprovingManager());
            Hire(new OneWorkerTeamLead<ProcessedDataFile, ExampleSet>());
            Hire(new List<PassthroughDataWorker> { new PassthroughDataWorker() });
            Staffed = true;
        }
    }


    public class PassthroughDataWorker : Worker
    {
        public override object Work(object input)
        {
            List<ProcessedDataFile> processedFiles = (List<ProcessedDataFile>)input;
            ProcessedDataFile source = processedFiles.Last();
            ExampleSet result = new ExampleSet(source);
            return result;
        }

        public override object Train(object input)
        {
            // Does the same thing either way.
            return Work(input);
        }
    }

}
