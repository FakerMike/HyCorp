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


    public class TeamLeadHotCoDataEnrichment : CrossFunctionalTeamLead<ByDatePairedExampleSet, EnrichedByDatePairedExampleSet>
    {

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

        public override object Plan(Clerk clerk)
        {
            ByDatePairedExampleSet input = clerk.PlanningInput as ByDatePairedExampleSet;
            CustomInputHotCoDataEnrichment workerInput = new CustomInputHotCoDataEnrichment
            {
                TestYear = input.Year,
                TestWeek = input.Week,
            };
            ExampleSet TestSet = new ExampleSet(input.Product.TestSet); // Copy this, we will edit
            ExampleSet TrainingSet = new ExampleSet(input.Product.TrainingSet);
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
                    workerInput.ExampleByID[x.ID.Value] = new List<Example>{x};
                }
                
                if (((ContinuousValue) x.FeatureValues[week]).Value == input.Week - 1 && ((ContinuousValue)x.FeatureValues[year]).Value == input.Year)
                {
                    workerInput.MostRecent.Add(x);
                }
            }
            foreach (Example x in TestSet.Examples)
            { 
                workerInput.TestExamples.Add(x);
            }


            HashSet<string> featureNames = new HashSet<string>();
            foreach (Worker w in Workers.Keys)
            {
                CustomOutputHotCoDataEnrichment workerOutput = (w as WorkerProfileHotCoDataEnrichment).Plan(workerInput);
                if (featureNames.Contains(workerOutput.EnrichedFeature.Name))
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

    public class CustomInputHotCoDataEnrichment
    {
        public Dictionary<string, List<Example>> ExampleByID = new Dictionary<string, List<Example>>();
        public List<Example> MostRecent = new List<Example>();
        public List<Example> TestExamples = new List<Example>();
        public FeatureVector Features;
        public int TestYear;
        public int TestWeek;
    }

    public class CustomOutputHotCoDataEnrichment
    {
        public Feature EnrichedFeature;
        public Dictionary<Example, IValue> TrainingFeatureValues = new Dictionary<Example, IValue>();
        public Dictionary<Example, IValue> TestFeatureValues = new Dictionary<Example, IValue>();
    }
}
