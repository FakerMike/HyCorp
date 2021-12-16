using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp
{
    /// <summary>
    /// Workers (classifiers):  Produce individual product
    /// TYPE UNSAFE:  Make sure you use the right workers!
    /// </summary> 
    public abstract class Worker
    {
        public abstract object Work(object input);
        public abstract object Train(object input);
    }

    public abstract class NewWorker
    {
        public abstract bool CanPlan(Type PlanningInput, Type PlanningOutput);
        public abstract bool CanProduce(Type ProductionInput, Type ProductionOutput);
    }

    public abstract class TypedWorker<TrainingInput, PredictionInput, Output> : Worker
    {
        public abstract void Train(TrainingInput input);
        public abstract Output Predict(PredictionInput input);
    }

    public interface IWorker
    {
        // Nothing yet.
    }

    public abstract class PlanningWorker<TInput, TOutput> : NewWorker
    {
        protected TInput input;
        public abstract TOutput Plan(TInput input);
        public override bool CanPlan(Type PlanningInput, Type PlanningOutput)
        {
            if (PlanningInput is TInput && PlanningOutput is TOutput) return true;
            return false;
        }
        public override bool CanProduce(Type ProductionInput, Type ProductionOutput)
        {
            return false;
        }
    }

    public abstract class ProductionWorker<TInput, TOutput> : NewWorker
    {
        public abstract TOutput Produce(TInput input);
        public override bool CanPlan(Type PlanningInput, Type PlanningOutput)
        {
            return false;
        }
        public override bool CanProduce(Type ProductionInput, Type ProductionOutput)
        {
            if (ProductionInput is TInput && ProductionOutput is TOutput) return true;
            return false;
        }
    }

    public abstract class CrossFunctionalWorker<TPlanningInput, TPlanningOutput, TProductionInput, TProductionOutput> : NewWorker
    {
        public abstract TPlanningOutput Plan(TPlanningInput input);
        public abstract TProductionOutput Produce(TProductionInput input);
        public override bool CanPlan(Type PlanningInput, Type PlanningOutput)
        {
            if (PlanningInput is TPlanningInput && PlanningOutput is TPlanningOutput) return true;
            return false;
        }
        public override bool CanProduce(Type ProductionInput, Type ProductionOutput)
        {
            if (ProductionInput is TPlanningInput && ProductionOutput is TPlanningOutput) return true;
            return false;
        }
    }

    public class ChaosWorker : TypedWorker<ExampleSet, Example, Prediction>
    {
        private static Random rand = new Random();
        private int count;
        private List<Feature> features;
        private bool trained = false;
        private Dictionary<ValueHolder, Prediction> predictions;
        private int[] splitTracker;
        private int totalLeafNodes;

        public ChaosWorker()
        {
        }

        public override Prediction Predict(Example input)
        {
            if (!trained) return null;

            return predictions[Convert(input)];
        }

        public override void Train(ExampleSet input)
        {
            FillFeatures(input);
            FillPredictions(input);
        }

        public override object Work(object input)
        {
            return null;
        }


        private void FillFeatures(ExampleSet input)
        {
            features = new List<Feature>();

            int max = input.Features.Features.Count;
            count = 0;
            bool[] taken = new bool[max];
            List<int> splits = new List<int>();
            totalLeafNodes = 1;


            for (int i = 0; i < max; i++)
            {
                taken[i] = false;
            }

            for (int i = 0; i < max; i++)
            {
                int next = rand.Next(max);
                while (taken[next])
                {
                    next = rand.Next(max);
                }
                taken[next] = true;
                Feature nextFeature = input.Features.Features[next];

                // If we split on a continuous feature, pick a random value for the split - Max chaos!
                if(nextFeature.GetType() == typeof(ContinuousFeature))
                {
                    ContinuousFeature feat = (ContinuousFeature)nextFeature;
                    Example random = input.GetRandom();
                    ContinuousValue val = (ContinuousValue)random.FeatureValues[nextFeature];
                    feat.ChangeSplittingCondition(x => { if (x.Value > val.Value) return 1; return 0; }, 2);
                }

                features.Add(nextFeature);
                totalLeafNodes *= nextFeature.Count;
                splits.Add(nextFeature.Count);
                count++;
                if (rand.NextDouble() < 0.2 || input.Examples.Count < totalLeafNodes * 50) break;
            }

            splitTracker = new int[count];
            for (int i = 0; i < count; i++)
            {
                splitTracker[i] = splits[i];
            }

        }


        private void FillPredictions(ExampleSet input)
        {
            predictions = new Dictionary<ValueHolder, Prediction>();

            ChaosNode root;

            root = new ChaosNode(input, 0, null, 0);

            Queue<ChaosNode> toSplit = new Queue<ChaosNode>();
            toSplit.Enqueue(root);

            while (toSplit.Count > 0)
            {
                ChaosNode current = toSplit.Dequeue();
                List<ExampleSet> split = current.Examples.Split(features[current.Depth]);
                for (int i = 0; i < split.Count; i++)
                {
                    ExampleSet set = split[i];
                    ChaosNode next = new ChaosNode(set, current.Depth + 1, current, i);
                    current.AddChild(next);
                    toSplit.Enqueue(next);
                }
            }

            Stack<ChaosNode> toFill = new Stack<ChaosNode>();
            toFill.Push(root);

            while (toFill.Count > 0)
            {
                ChaosNode current = toFill.Pop();
                if (current.Children.Count == 0)
                {
                    predictions[new ValueHolder(current.Values)] = current.Prediction;
                }
                else
                {
                    foreach (ChaosNode child in current.Children)
                    {
                        if (child.NeedsPrediction)
                        {
                            child.AddPrediction(current.Prediction);
                        }
                        toFill.Push(child);
                    }
                }
            }
        }



        private ValueHolder Convert(Example example)
        {
            int[] values = new int[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = features[i].Split(example.FeatureValues[features[i]]);
            }
            return new ValueHolder(values);
        }


        public static Prediction BestPrediction(List<Example> examples)
        {
            return new Prediction(new ContinuousValue(1.5),0.9);
        }

        public override object Train(object input)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// For fast lookups
        /// </summary>
        private class ValueHolder
        {
            public int[] Values { get; private set; }

            public ValueHolder(int[] values)
            {
                Values = values;
            }

            public override int GetHashCode()
            {
                int result = 0;
                for (int i = 0; i < Values.Length; i++) result += Values[i].GetHashCode();
                return result;
            }

            public override bool Equals(object obj)
            {
                if (obj.GetType() == GetType())
                {
                    ValueHolder vh = (ValueHolder)obj;
                    if (vh.Values.SequenceEqual(Values))
                    {
                        return true;
                    }
                }
                return false;
            }

            public override string ToString()
            {
                return "{" + string.Join(", ", Values) + "}";

            }
        }


        /// <summary>
        /// For training
        /// </summary>
        private class ChaosNode
        {
            public List<ChaosNode> Children { get; private set; }
            public Prediction Prediction { get; private set; }
            public ExampleSet Examples { get; private set; }
            public bool NeedsPrediction { get; private set; }
            public int Depth { get; private set; }
            public ChaosNode Parent { get; private set; }
            public int[] Values { get; private set; }

            public ChaosNode(ExampleSet examples, int depth, ChaosNode parent, int value)
            {
                Depth = depth;
                Examples = examples;
                Parent = parent;
                if (examples.Examples.Count > 0)
                {
                    NeedsPrediction = false;
                    Prediction = BestPrediction(examples.Examples);
                } else
                {
                    NeedsPrediction = true;
                }
                Values = new int[depth];
                for (int i = 0; i < depth - 2; i++)
                {
                    Values[i] = parent.Values[i];
                }
                if (depth > 0)
                    Values[depth - 1] = value;
            }

            public void AddChild(ChaosNode child)
            {
                Children.Add(child);
            }

            public void AddPrediction(Prediction prediction)
            {
                Prediction = prediction;
                NeedsPrediction = false;
            }
        }
    }
}
