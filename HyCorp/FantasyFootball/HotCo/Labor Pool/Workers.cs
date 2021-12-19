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

    public abstract class WorkerProfileHotCoDataImport : CrossFunctionalWorker<RawDataFile,FullExampleSet,RawDataFile,FullExampleSet>
    {
    }

    public abstract class WorkerProfileHotCoExecutive : CrossFunctionalWorker<FullExampleSet, ByDatePairedExampleSet, FullExampleSet, ByDatePairedExampleSet>
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

    public abstract class WorkerProfileHotCoPicker : CrossFunctionalWorker<PlayerPicker, FantasyFootballTeam, PlayerPicker, FantasyFootballTeam>
    {
        protected static Random rand = new Random();
        protected bool initialized = false;
    }

    public abstract class WorkerProfileHotCoFilter : CrossFunctionalWorker<PotentialTeamList, FantasyFootballProduct, PotentialTeamList, FantasyFootballProduct>
    {
    }



    public class WorkerHotCoFilter : WorkerProfileHotCoFilter
    {
        public override FantasyFootballProduct Plan(PotentialTeamList input)
        {
            List<FantasyFootballTeam> chosenTeams = new List<FantasyFootballTeam>();
            foreach (FantasyFootballTeam team in input.Product)
            {
                if (team.RemainingSalary() > 0)
                {
                    bool add = true;
                    foreach (FantasyFootballTeam chosenTeam in chosenTeams)
                    {
                        if (team.SharesEightPlayersWith(chosenTeam)) { add = false; break; }
                    }
                    if (add)
                    {
                        chosenTeams.Add(team);
                        if (chosenTeams.Count == 10) break;
                    }
                }
            }
            return new FantasyFootballProduct(chosenTeams);
        }

        public override FantasyFootballProduct Produce(PotentialTeamList input)
        {
            throw new NotImplementedException();
        }
    }



    public class WorkerHotCoSwapPicker : WorkerProfileHotCoPicker
    {
        public override FantasyFootballTeam Plan(PlayerPicker input)
        {
            FantasyFootballTeam team = input.GetRandomTeam();
            for (int i = 0; i < 200; i++)
            {
                int swapIndex = rand.Next(9);
                int salaryRangeMax = team.Team[swapIndex].Salary + team.RemainingSalary() - rand.Next(0,5) * 100;
                int salaryRangeMin = salaryRangeMax - 100 * (15 - i/20);
                if (salaryRangeMin > team.Team[swapIndex].Salary) salaryRangeMin = team.Team[swapIndex].Salary;
                input.SmartSwap(team, swapIndex, salaryRangeMin, salaryRangeMax);
            }
            return team;
        }

        public override FantasyFootballTeam Produce(PlayerPicker input)
        {
            throw new NotImplementedException();
        }
    }


    public class WorkerHotCoRandomPicker : WorkerProfileHotCoPicker
    {
        public override FantasyFootballTeam Plan(PlayerPicker input)
        {
            return input.GetRandomTeam();
        }

        public override FantasyFootballTeam Produce(PlayerPicker input)
        {
            throw new NotImplementedException();
        }
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



    public class WorkerHotCoPreviouslyHotDataEnrichment : WorkerProfileHotCoDataEnrichment
    {
        private int weeks;
        private string idea;
        private CategoricalFeature feature;
        public override COHotCoDataEnrichment Plan(CIDataEnrichment input)
        {
            if (!initialized)
            {
                initialized = true;
                weeks = rand.Next(1, 5);
                idea = $"Hot{weeks}WeeksAgo";
                if (featureIdeas.Contains(idea))
                {
                    feature = new CategoricalFeature("BadIdea");
                    return new COHotCoDataEnrichment { EnrichedFeature = new ContinuousFeature("BadIdea") };
                }
                feature = new CategoricalFeature(idea);
            }

            COHotCoDataEnrichment output = new COHotCoDataEnrichment();
            Feature week = input.Features.ByName["Week"];
            Feature salary = input.Features.ByName["DK salary"];
            foreach (Example x in input.MostRecent)
            {
                output.TrainingFeatureValues[x] = new CategoricalValue("Not");
                foreach (Example y in input.ExampleByID[x.ID.Value])
                {
                    if ((x.FeatureValues[week] as ContinuousValue).Value == (y.FeatureValues[week] as ContinuousValue).Value + weeks)
                    {
                        if (y.ContinuousLabel.Value / (y.FeatureValues[salary] as ContinuousValue).Value >= 0.0035)
                        {
                            output.TrainingFeatureValues[x] = new CategoricalValue("Hot");
                        } 
                    }
                }
            }

            foreach (Example x in input.TestExamples)
            {
                output.TestFeatureValues[x] = new CategoricalValue("Not");
                if (input.ExampleByID.ContainsKey(x.ID.Value))
                {
                    foreach (Example y in input.ExampleByID[x.ID.Value])
                    {
                        if ((x.FeatureValues[week] as ContinuousValue).Value == (y.FeatureValues[week] as ContinuousValue).Value + weeks)
                        {
                            if (y.ContinuousLabel.Value / (y.FeatureValues[salary] as ContinuousValue).Value >= 0.0035)
                            {
                                output.TestFeatureValues[x] = new CategoricalValue("Hot");
                            }
                        }
                    }
                } 
            }

            feature.AutoSet(new List<string> { "Not", "Hot" });
            output.EnrichedFeature = feature;
            return output;
        }

        public override COHotCoDataEnrichment Produce(CIDataEnrichment input)
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

    public class HotCoExecutiveWorker : WorkerProfileHotCoExecutive
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
            //UI.Instance.Print($"Year: {input.TrainingYear}, Week: {input.TrainingWeek}.  Training Data: {paired.TrainingSet.Examples.Count} lines, Test Data: {paired.TestSet.Examples.Count}");
            return new ByDatePairedExampleSet(paired, input.TrainingWeek, input.TrainingYear);
        }

        public override ByDatePairedExampleSet Produce(FullExampleSet input)
        {
            // TODO:  Production side
            throw new NotImplementedException();
        }
    }


    public class HotCoDataImportWorker : WorkerProfileHotCoDataImport
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
