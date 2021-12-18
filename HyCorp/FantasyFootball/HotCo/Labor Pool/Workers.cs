using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp.FantasyFootball.Corps.HotCo
{
    /*
     * 
     *  TYPES OF WORKERS
     * 
     */

    public abstract class HotCoDataImportWorkerProfile : CrossFunctionalWorker<RawDataFile,FullExampleSet,RawDataFile,FullExampleSet>
    {
    }

    public abstract class HotCoExecutiveWorkerProfile : CrossFunctionalWorker<FullExampleSet, ByDatePairedExampleSet, FullExampleSet, ByDatePairedExampleSet>
    {
    }

    public abstract class WorkerProfileHotCoDataEnrichment : CrossFunctionalWorker<CIDataEnrichment,COHotCoDataEnrichment,CIDataEnrichment,COHotCoDataEnrichment>
    {
        protected static Random rand = new Random();
        protected static HashSet<string> featureIdeas = new HashSet<string>();
        protected bool initialized = false;
    }

    public abstract class WorkerProfileHotCoModeling : CrossFunctionalWorker<CIHotCoModeling, COHotCoModeling, CIHotCoModeling, COHotCoModeling>
    {
        protected static Random rand = new Random();
        protected bool initialized = false;
    }


    public class WorkerHotCoModelingRandomTree : WorkerProfileHotCoModeling
    {
        int[] Positives;
        int[] Totals;
        int runningCount;
        double overallAverage;
        List<Feature> Features = new List<Feature>();
        List<int> Counts = new List<int>();

        public override COHotCoModeling Plan(CIHotCoModeling input)
        {
            if (!initialized)
            {
                runningCount = 1;
                while (true)
                {
                    Feature f = input.TrainingSet.Features.Features[rand.Next(0, input.TrainingSet.Features.Features.Count)];
                    while (Features.Contains(f))
                    {
                        f = input.TrainingSet.Features.Features[rand.Next(0, input.TrainingSet.Features.Features.Count)];
                    }
                    if (runningCount * f.Count > 4092) break;
                    runningCount *= f.Count;
                    Features.Add(f);
                    Counts.Add(f.Count);
                }
                initialized = true;
            }

            Positives = new int[runningCount * Features.Count];
            Totals = new int[runningCount * Features.Count];
            CategoricalFeature label = input.TrainingSet.Features.CategoricalLabel;
            double totalPositives = 0;
            double total = 0;
            foreach (Example x in input.TrainingSet.Examples)
            {
                List<int> splits = new List<int>();
                for (int i = 0; i < Features.Count; i++)
                {
                    splits.Add(Features[i].Split(x.FeatureValues[Features[i]]));
                    int index = Index(splits);
                    Totals[index] += 1;
                    total++;
                    int positive = label.Split(x.CategoricalLabel);
                    Positives[index] += positive;
                    totalPositives += positive;
                }
            }

            overallAverage = totalPositives / total;


            COHotCoModeling output = new COHotCoModeling();
            foreach (Example x in input.TestSet.Examples)
            {
                List<int> splits = new List<int>();
                double best = overallAverage;
                for (int i = 0; i < Features.Count; i++)
                {
                    splits.Add(Features[i].Split(x.FeatureValues[Features[i]]));
                    int index = Index(splits);
                    if (Totals[index] < 3) break;
                    best = (double)Positives[index] / Totals[index];
                }
                output.Predictions[x] = best;
            }

            return output;
        }

        private int Index(List<int> splits)
        {
            int m = 1;
            int index = runningCount * (splits.Count - 1);
            for (int i = 0; i < splits.Count; i++)
            {
                index += splits[i] * m;
                m *= Counts[i];
            }
            return index;
        }

        public override COHotCoModeling Produce(CIHotCoModeling input)
        {
            throw new NotImplementedException();
        }
    }





    public class WorkerHotCoHistoricalAverageDataEnrichment : WorkerProfileHotCoDataEnrichment
    {
        private int weeks;
        private string idea;
        private ContinuousFeature feature;
        public override COHotCoDataEnrichment Plan(CIDataEnrichment input)
        {
            if (!initialized)
            {
                initialized = true;
                weeks = rand.Next(2, input.TestWeek);
                idea = $"{weeks}WeekAverageScore";
                if (featureIdeas.Contains(idea))
                {
                    feature = new ContinuousFeature("BadIdea");
                    return new COHotCoDataEnrichment { EnrichedFeature = new ContinuousFeature("BadIdea") };
                }
                feature = new ContinuousFeature(idea);
            }
            
            COHotCoDataEnrichment output = new COHotCoDataEnrichment();
            List<double> values = new List<double>();
            Feature week = input.Features.ByName["Week"];
            foreach (Example x in input.MostRecent)
            {
                double result = 0;
                double count = 0;
                foreach (Example y in input.ExampleByID[x.ID.Value])
                {
                    if (((x.FeatureValues[week] as ContinuousValue).Value <= (y.FeatureValues[week] as ContinuousValue).Value + weeks + 1)
                        && ((x.FeatureValues[week] as ContinuousValue).Value > (y.FeatureValues[week] as ContinuousValue).Value))
                    {
                        result += y.ContinuousLabel.Value;
                        count++;
                    }
                }
                if (count > 0)
                {
                    result /= count;
                }
                values.Add(result);
                output.TrainingFeatureValues[x] = new ContinuousValue(result);
            }

            foreach (Example x in input.TestExamples)
            {
                double result = 0;
                double count = 0;
                if (input.ExampleByID.ContainsKey(x.ID.Value))
                {
                    foreach (Example y in input.ExampleByID[x.ID.Value])
                    {
                        if (((x.FeatureValues[week] as ContinuousValue).Value <= (y.FeatureValues[week] as ContinuousValue).Value + weeks + 1)
                        && ((x.FeatureValues[week] as ContinuousValue).Value > (y.FeatureValues[week] as ContinuousValue).Value))
                        {
                            result += y.ContinuousLabel.Value;
                            count++;
                        }
                    }
                }
                if (count > 0)
                {
                    result /= count;
                }
                output.TestFeatureValues[x] = new ContinuousValue(result);
            }

            feature.AutoSet(values);
            output.EnrichedFeature = feature;
            return output;
        }

        public override COHotCoDataEnrichment Produce(CIDataEnrichment input)
        {
            throw new NotImplementedException();
        }
    }

    public class HotCoExecutiveWorker : HotCoExecutiveWorkerProfile
    {
        public override ByDatePairedExampleSet Plan(FullExampleSet input)
        {
            ExampleSet OriginalSet = input.Product;
            ContinuousFeature year = OriginalSet.Features.ByName["Year"] as ContinuousFeature;
            ContinuousFeature week = OriginalSet.Features.ByName["Week"] as ContinuousFeature;
            year.ChangeSplittingCondition(x => { if (x.Value == input.TrainingYear) return 0; return 1; }, 2);
            week.ChangeSplittingCondition(x => { if (x.Value < input.TrainingWeek) return 0; if (x.Value == input.TrainingWeek) return 1; return 2; }, 3);
            List<ExampleSet> split = OriginalSet.Split(year)[0].Split(week);
            PairedExampleSet paired = new PairedExampleSet(split[0], split[1]);
            UI.Instance.Print($"Year: {input.TrainingYear}, Week: {input.TrainingWeek}.  Training Data: {paired.TrainingSet.Examples.Count} lines, Test Data: {paired.TestSet.Examples.Count}");
            return new ByDatePairedExampleSet(paired, input.TrainingWeek, input.TrainingYear);
        }

        public override ByDatePairedExampleSet Produce(FullExampleSet input)
        {
            // TODO:  Production side
            throw new NotImplementedException();
        }
    }


    public class HotCoDataImportWorker : HotCoDataImportWorkerProfile
    {
        public override FullExampleSet Plan(RawDataFile input)
        {
            ProcessedDataFile pdf = new ProcessedDataFile();
            List<Feature> coreFeatures = new List<Feature>();
            Dictionary<Feature, int> index = new Dictionary<Feature, int>();
            int currentIndex = 0;
            IDFeature idFeature = null;
            ContinuousFeature label = null;
            foreach (RawColumn rawColumn in input.Columns)
            {
                Feature feature;
                FeatureColumn column;
                List<IValue> values;
                switch (rawColumn.Name)
                {
                    case "GID":
                        continue;
                    case "DK points":
                        label = new ContinuousFeature(rawColumn.Name);
                        feature = label;
                        break;
                    case "DK salary":
                    case "Week":
                    case "Year":
                    case "Salary":
                        feature = new ContinuousFeature(rawColumn.Name);
                        coreFeatures.Add(feature);
                        break;
                    case "Name":
                        idFeature = new IDFeature(rawColumn.Name);
                        feature = idFeature;
                        break;
                    default:
                        feature = new CategoricalFeature(rawColumn.Name);
                        coreFeatures.Add(feature);
                        break;
                }
                feature.AutoSet(rawColumn.Values);
                values = feature.CreateValues(rawColumn.Values);
                column = new FeatureColumn(feature, values);
                pdf.Add(column);
                index[feature] = currentIndex++;
            }

            FeatureVector Features = new FeatureVector(coreFeatures, label, idFeature);
            ExampleSet set = new ExampleSet(Features);
            foreach (List<IValue> row in pdf.GetRows())
            {
                Example example = new Example(new Dictionary<Feature, IValue>(), 1);
                for (int i = 0; i < row.Count; i++)
                {
                    if (i == index[label]) example.SetLabel(row[i]);
                    else if (i == index[idFeature]) example.SetID((IDValue)row[i]);
                    else example.SetAttributeValue(pdf.Features[i], row[i]);
                }
                set.Add(example);
            }

            return new FullExampleSet(set);
        }

        public override FullExampleSet Produce(RawDataFile input)
        {
            return Plan(input); //Same file
        }
    }
}
