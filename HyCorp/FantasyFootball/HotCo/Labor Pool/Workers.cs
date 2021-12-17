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

    public abstract class WorkerProfileHotCoDataEnrichment : CrossFunctionalWorker<CustomInputHotCoDataEnrichment,CustomOutputHotCoDataEnrichment,CustomInputHotCoDataEnrichment,CustomOutputHotCoDataEnrichment>
    {
        protected static Random rand = new Random();
    }

    public class WorkerHotCoHistoricalAverageDataEnrichment : WorkerProfileHotCoDataEnrichment
    {
        public override CustomOutputHotCoDataEnrichment Plan(CustomInputHotCoDataEnrichment input)
        {
            CustomOutputHotCoDataEnrichment output = new CustomOutputHotCoDataEnrichment();
            int weeks = rand.Next(2, input.TestWeek);
            ContinuousFeature feature = new ContinuousFeature($"{weeks}WeekAverageScore");
            List<double> values = new List<double>();
            Feature week = input.Features.ByName["Week"];
            foreach (Example x in input.MostRecent)
            {
                double result = 0;
                double count = 0;
                foreach (Example y in input.ExampleByID[x.ID.Value])
                {
                    if ((x.FeatureValues[week] as ContinuousValue).Value <= (y.FeatureValues[week] as ContinuousValue).Value + weeks){
                        result += (y.Label as ContinuousValue).Value;
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
                        if ((x.FeatureValues[week] as ContinuousValue).Value <= (y.FeatureValues[week] as ContinuousValue).Value + weeks)
                        {
                            result += (y.Label as ContinuousValue).Value;
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

        public override CustomOutputHotCoDataEnrichment Produce(CustomInputHotCoDataEnrichment input)
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
