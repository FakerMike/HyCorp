using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp
{
    public abstract class Feature
    {
        public abstract int Split(IValue value);
        public abstract void AutoSet(List<string> sourceStrings);
        public abstract List<IValue> CreateValues(List<string> sourceStrings);

        public int Count { get; protected set; }
        public string Name { get; protected set; }


        public Feature(string name)
        {
            Name = name;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == GetType())
            {
                Feature other = (Feature)obj;
                if (other.Name == Name)
                {
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    public class ContinuousFeature : Feature
    {
        public delegate int ContinuousSeparator(ContinuousValue value);

        public Histogram Distribution { get; protected set; }
        public ContinuousSeparator Separator { get; protected set; }
        public double Average { get; protected set; }


        public ContinuousFeature(string name) : base (name) { }

        public override int Split(IValue value)
        {
            ContinuousValue cv = (ContinuousValue)value;
            return Separator(cv);
        }

        public override void AutoSet(List<string> sourceStrings)
        {
            double min = double.PositiveInfinity, max = double.NegativeInfinity, sum = 0, count = 0;

            foreach (string s in sourceStrings)
            {
                if (double.TryParse(s, out double result))
                {
                    if (result > max) max = result;
                    if (result < min) min = result;
                    sum += result;
                    count++;
                }
            }
            Average = sum / count;
            Separator = x => { if (x.Value > Average) return 1; return 0; };
            Count = 2;

            Distribution = new Histogram(min, max, (int)count / 100 + 1);

            foreach (string s in sourceStrings)
            {
                if (double.TryParse(s, out double result))
                {
                    Distribution.AddData(result);
                } else
                {
                    Distribution.AddData(Average);
                }
            }
        }
        
        public void AutoSet(List<double> sourceDoubles)
        {
            double min = double.PositiveInfinity, max = double.NegativeInfinity, sum = 0;
            foreach (double x in sourceDoubles)
            {
                 if (x > max) max = x;
                 if (x < min) min = x;
                 sum += x;
            }
            Average = sum / sourceDoubles.Count;
            Separator = x => { if (x.Value > Average) return 1; return 0; };
            Count = 2;
            Distribution = new Histogram(min, max, sourceDoubles.Count / 100 + 1);
            foreach (double x in sourceDoubles)
            {
                Distribution.AddData(x);
            }
        }

        public void ChangeSplittingCondition(ContinuousSeparator separator, int count)
        {
            Separator = separator;
            Count = count;
        }

        public override List<IValue> CreateValues(List<string> sourceStrings)
        {
            List<IValue> values = new List<IValue>();
            foreach (string s in sourceStrings)
            {
                if (double.TryParse(s, out double result))
                {
                    values.Add(new ContinuousValue(result));
                }
                else
                {
                    values.Add(new ContinuousValue(Average));
                }
            }
            return values;
        }
    }

    public class CategoricalFeature : Feature
    {
        public CategoricalFeature(string name) : base(name) { }
        public List<string> PossibleValues { get; protected set; }

        public override int Split(IValue value)
        {
            CategoricalValue cv = (CategoricalValue)value;
            return PossibleValues.IndexOf(cv.Value);
        }

        public override void AutoSet(List<string> sourceStrings)
        {
            HashSet<string> values = new HashSet<string>();
            foreach (string s in sourceStrings)
            {
                values.Add(s);
            }
            PossibleValues = new List<string>(values);
            Count = PossibleValues.Count;
        }

        public override List<IValue> CreateValues(List<string> sourceStrings)
        {
            List<IValue> values = new List<IValue>();
            foreach (string s in sourceStrings)
            {
                values.Add(new CategoricalValue(s));
            }
            return values;
        }
    }

    // Special case for something that should always be split (player name, for example)
    public class IDFeature : CategoricalFeature
    {
        public IDFeature(string name) : base(name) { }

        public override List<IValue> CreateValues(List<string> sourceStrings)
        {
            List<IValue> values = new List<IValue>();
            foreach (string s in sourceStrings)
            {
                values.Add(new IDValue(s));
            }
            return values;
        }
    }

    /// <summary>
    /// A value from our data set
    /// </summary>
    public interface IValue
    {
    }

    public abstract class TypedValue<T> : IValue
    {
        public T Value { get; protected set; }
        public TypedValue(T value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == GetType())
            {
                TypedValue<T> other = (TypedValue<T>)obj;
                if (other.Value.Equals(Value))
                {
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class ContinuousValue : TypedValue<double>
    {
        public ContinuousValue(double value) : base(value) { }
    }



    public class CategoricalValue : TypedValue<string>
    {
        public CategoricalValue(string value) : base(value) { }
    }


    public class IDValue : TypedValue<string>
    {
        public IDValue(string value) : base(value) { }
    }




    public class Prediction
    {
        public enum PredictionMode
        {
            VALUE = 0,
            DISTRIBUTION
        }
        public PredictionMode Mode { get; private set; }
        public IValue Value { get; private set; }
        public Histogram Distribution { get; private set; }
        public double Confidence { get; private set; }
        
        public Prediction(Histogram histogram)
        {
            Distribution = histogram;
            Mode = PredictionMode.DISTRIBUTION;
        }

        public Prediction(IValue value, double confidence)
        {
            Mode = PredictionMode.VALUE;
            Value = value;
            Confidence = confidence;
        }
    }



    public class FeatureVector
    {
        public List<Feature> Features { get; private set; }
        public IDFeature ID { get; private set; }
        public Feature Label { get; private set; }
        public Dictionary<string, Feature> ByName { get; private set; }

        public FeatureVector(IEnumerable<Feature> features)
        {
            Features = new List<Feature>(features);
            ByName = new Dictionary<string, Feature>();
            foreach (Feature f in features) {
                ByName[f.Name] = f;
            }
            Label = null;
        }

        public FeatureVector(IEnumerable<Feature> features, Feature label)
        {
            Features = new List<Feature>(features);
            ByName = new Dictionary<string, Feature>();
            foreach (Feature f in features)
            {
                ByName[f.Name] = f;
            }
            Label = label;
        }

        public FeatureVector(IEnumerable<Feature> features, Feature label, IDFeature id)
        {
            Features = new List<Feature>(features);
            ByName = new Dictionary<string, Feature>();
            foreach (Feature f in features)
            {
                ByName[f.Name] = f;
            }
            Label = label;
            ID = id;
        }

        public void AddFeature(Feature feature)
        {
            Features.Add(feature);
            ByName[feature.Name] = feature;
        }
    }
}
