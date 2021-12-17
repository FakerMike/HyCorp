using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp
{

    public class Example
    {
        public Dictionary<Feature, IValue> FeatureValues { get; private set; }
        public IDValue ID { get; private set; }
        public IValue Label { get; private set; }
        public double Weight { get; private set; }

        public Example(Dictionary<Feature, IValue> values, double weight)
        {
            FeatureValues = values;
            Weight = weight;
        }

        public Example(Dictionary<Feature, IValue> values, double weight, IValue label)
        {
            FeatureValues = values;
            Weight = weight;
            Label = label;
        }

        public void SetAttributeValue(Feature feature, IValue value)
        {
            FeatureValues[feature] = value;
        }

        public void SetID(IDValue value)
        {
            ID = value;
        }

        public void SetLabel(IValue value)
        {
            Label = value;
        }
    }

    public class ExampleSet
    {
        public static Random rand = new Random();
        public FeatureVector Features { get; private set; }
        public List<Example> Examples { get; private set; }


        public ExampleSet(ExampleSet set)
        {
            Features = new FeatureVector(set.Features.Features, set.Features.Label, set.Features.ID);
            Examples = new List<Example>();
            foreach (Example x in set.Examples)
            {
                Example y = new Example(new Dictionary<Feature, IValue>(), x.Weight);
                foreach (Feature f in x.FeatureValues.Keys)
                {
                    y.SetAttributeValue(f, x.FeatureValues[f]);
                }
                y.SetID(x.ID);
                y.SetLabel(x.Label);
                Examples.Add(y);
            }
        }


        public ExampleSet(IEnumerable<Feature> features, Feature label)
        {
            Features = new FeatureVector(features, label);
            Examples = new List<Example>();
        }

        public ExampleSet(FeatureVector features)
        {
            Features = features;
            Examples = new List<Example>();
        }

        public ExampleSet(ProcessedDataFile file)
        {
            List<Feature> features = new List<Feature>();
            foreach (FeatureColumn column in file.ColumnList)
            {
                features.Add(column.Feature);
            }
            Features = new FeatureVector(features);
            Examples = new List<Example>();
            for (int i = 0; i < file.ColumnList[0].Values.Count; i++)
            {
                Dictionary<Feature, IValue> values = new Dictionary<Feature, IValue>();
                foreach (Feature feature in Features.Features)
                {
                    values[feature] = file.ColumnDictionary[feature].Values[i];
                }
                Example example = new Example(values, 1);
                Examples.Add(example);
            }
        }

        public void Add(Example example)
        {
            Examples.Add(example);
        }

        public Example GetRandom()
        {
            return Examples[rand.Next(Examples.Count)];
        }

        public List<ExampleSet> Split(Feature feature)
        {
            List<ExampleSet> resultingSets = new List<ExampleSet>();
            for (int i = 0; i < feature.Count; i++)
            {
                resultingSets.Add(new ExampleSet(Features.Features, Features.Label));
            }
            for (int i = 0; i < Examples.Count; i++)
            {
                resultingSets[feature.Split(Examples[i].FeatureValues[feature])].Add(Examples[i]);
            }
            return resultingSets;
        }
    }
}
