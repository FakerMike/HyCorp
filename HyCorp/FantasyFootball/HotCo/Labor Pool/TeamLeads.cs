using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp.FantasyFootball.Corps.HotCo
{
    /*
     * 
     *  Types of Team Leads
     * 
     */


    /*
     * 
     *  Generics
     * 
     */
    public class TeamLeadHotCoDataImport : PassThroughCrossFunctionalTeamLead<RawDataFile, FullExampleSet> { }
    public class TeamLeadHotCoExecutive : PassThroughCrossFunctionalTeamLead<FullExampleSet, ByDatePairedExampleSet> { }




    public class TeamLeadHotCoModeling : CrossFunctionalTeamLead<EnrichedByDatePairedExampleSet, PlayersWithPredictedHotChance>
    {
        public override bool CanUse(Worker worker)
        {
            if (worker is WorkerProfileHotCoModeling) return true;
            return false;
        }

        public override void Evaluate()
        {
            // TODO : worst 10% by accuracy get fired
        }

        public override int MaxHires()
        {
            return 500;
        }

        public override int MinHires()
        {
            return 10;
        }

        public override object Plan(Clerk clerk)
        {
            throw new NotImplementedException();
        }

        public override object Produce(Clerk clerk)
        {
            throw new NotImplementedException();
        }
    }



    public class CIHotCoModeling
    {
        public ExampleSet TrainingSet;
        public ExampleSet TestSet;
    }

    public class COHotCoModeling
    {
        public Dictionary<Example, double> Predictions = new Dictionary<Example, double>();
    }




    public class TeamLeadHotCoDataEnrichment : CrossFunctionalTeamLead<ByDatePairedExampleSet, EnrichedByDatePairedExampleSet>
    {
        private ExampleSet TrainingSet;
        private ExampleSet TestSet;
        private CIDataEnrichment workerInput;

        public override bool CanUse(Worker worker)
        {
            if (worker is WorkerProfileHotCoDataEnrichment) return true;
            return false;
        }

        public override void Evaluate()
        {
            // Does this one at a time
        }

        public override int MaxHires()
        {
            return 10;
        }

        public override int MinHires()
        {
            return 1;
        }

        public override void Prepare(Clerk clerk)
        {
            ByDatePairedExampleSet input = clerk.PlanningInput as ByDatePairedExampleSet;
            workerInput = new CIDataEnrichment
            {
                TestYear = input.Year,
                TestWeek = input.Week,
            };
            TestSet = new ExampleSet(input.Product.TestSet); // Copy this, we will edit
            TrainingSet = new ExampleSet(input.Product.TrainingSet);
            Feature week = TrainingSet.Features.ByName["Week"];
            Feature year = TrainingSet.Features.ByName["Year"];
            workerInput.Features = TrainingSet.Features;
            foreach (Example x in TrainingSet.Examples)
            {
                if (workerInput.ExampleByID.ContainsKey(x.ID.Value))
                {
                    workerInput.ExampleByID[x.ID.Value].Add(x);
                }
                else
                {
                    workerInput.ExampleByID[x.ID.Value] = new List<Example> { x };
                }

                if (((ContinuousValue)x.FeatureValues[week]).Value == input.Week - 1 && ((ContinuousValue)x.FeatureValues[year]).Value == input.Year)
                {
                    workerInput.MostRecent.Add(x);
                }
            }
            foreach (Example x in TestSet.Examples)
            {
                workerInput.TestExamples.Add(x);
            }

        }

        public override object Plan(Clerk clerk)
        {
            foreach (Worker w in Workers.Keys)
            {
                COHotCoDataEnrichment workerOutput = (w as WorkerProfileHotCoDataEnrichment).Plan(workerInput);
                if (workerOutput.EnrichedFeature.Name == "BadIdea")
                {
                    Workers[w].Rating = 0;  // Already got this feature, fired.
                    continue;
                }

                TrainingSet.Features.AddFeature(workerOutput.EnrichedFeature);
                TestSet.Features.AddFeature(workerOutput.EnrichedFeature);
                foreach (Example x in workerInput.MostRecent)
                {
                    x.SetAttributeValue(workerOutput.EnrichedFeature, workerOutput.TrainingFeatureValues[x]);
                }
                foreach (Example x in workerInput.TestExamples)
                {
                    x.SetAttributeValue(workerOutput.EnrichedFeature, workerOutput.TestFeatureValues[x]);
                }
            }

            ExampleSet finishedTrainingSet = new ExampleSet(TrainingSet.Features);
            ExampleSet finishedTestSet = new ExampleSet(TestSet.Features);

            foreach (Example x in workerInput.MostRecent)
            {
                finishedTrainingSet.Add(x);
            }
            foreach (Example x in workerInput.TestExamples)
            {
                finishedTestSet.Add(x);
            }

            return new EnrichedByDatePairedExampleSet(new PairedExampleSet(finishedTrainingSet, finishedTestSet));
        }

        public override object Produce(Clerk clerk)
        {
            throw new NotImplementedException();
        }
    }

    public class CIDataEnrichment
    {
        public Dictionary<string, List<Example>> ExampleByID = new Dictionary<string, List<Example>>();
        public List<Example> MostRecent = new List<Example>();
        public List<Example> TestExamples = new List<Example>();
        public FeatureVector Features;
        public int TestYear;
        public int TestWeek;
    }

    public class COHotCoDataEnrichment
    {
        public Feature EnrichedFeature;
        public Dictionary<Example, IValue> TrainingFeatureValues = new Dictionary<Example, IValue>();
        public Dictionary<Example, IValue> TestFeatureValues = new Dictionary<Example, IValue>();
    }
}
