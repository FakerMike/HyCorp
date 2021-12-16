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

    public abstract class HotCoExecutiveWorkerProfile : PlanningWorker<FullExampleSet, ByDatePairedExampleSet>
    {
    }



    public class HotCoExecutiveWorker : HotCoExecutiveWorkerProfile
    {
        public override ByDatePairedExampleSet Plan(FullExampleSet input)
        {
            ExampleSet OriginalSet = input.Product;
            ContinuousFeature year = OriginalSet.Features.ByName["Year"] as ContinuousFeature;
            ContinuousFeature week = OriginalSet.Features.ByName["Week"] as ContinuousFeature;
            //year.ChangeSplittingCondition()
            return null;
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
