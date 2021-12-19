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
    
    public class TeamLeadHotCoFilter : PassThroughCrossFunctionalTeamLead<PotentialTeamList, FantasyFootballProduct> { }

    public class TeamLeadHotCoExecutive : CrossFunctionalTeamLead<FullExampleSet, ByDatePairedExampleSet>
    {
        Dictionary<double, EvaluationPacket> packets = new Dictionary<double, EvaluationPacket>();

        public override bool CanUse(Worker worker)
        {
            if (worker is WorkerProfileHotCoExecutive) return true;
            return false;
        }

        public override void Evaluate()
        {
            //
        }

        public override int MaxHires()
        {
            return 1;
        }

        public override int MinHires()
        {
            return 1;
        }

        public override object Plan(Clerk clerk)
        {
            ByDatePairedExampleSet result = (Workers.Keys.First() as WorkerProfileHotCoExecutive).Plan((FullExampleSet)clerk.PlanningInput);
            packets[result.Year + (double)result.Week / 100] = new EvaluationPacket(result.Product.TestSet, result.Week, result.Year);
            return result;
        }

        public void AddProduct(FantasyFootballProduct product, int week, int year)
        {
            packets[year + (double)week / 100].AddProduct(product);
        }

        public void SummarizePerformance()
        {
            int totalBigWins = 0;
            int totalSmallWins = 0;
            foreach (EvaluationPacket packet in packets.Values)
            {
                UI.Print(packet);
                totalBigWins += packet.BigWins;
                totalSmallWins += packet.SmallWins;
            }
            UI.Print($"Total Big Wins: {totalBigWins}");
            UI.Print($"Total Small Wins: {totalSmallWins}");
            UI.Print($"Total Entries: {packets.Values.Count * 10}");
        }


        public override object Produce(Clerk clerk)
        {
            throw new NotImplementedException();
        }

        private class EvaluationPacket
        {
            Dictionary<string, double> ActualScore = new Dictionary<string, double>();
            FantasyFootballProduct Product;
            public readonly int Week;
            public readonly int Year;
            public int BigWins { get; protected set; }
            public int SmallWins { get; protected set; }

            public EvaluationPacket(ExampleSet testSet, int week, int year)
            {
                Week = week;
                Year = year;
                foreach (Example x in testSet.Examples)
                {
                    ActualScore[x.ID.Value] = x.ContinuousLabel.Value;
                }
            }

            public void AddProduct(FantasyFootballProduct product)
            {
                Product = product;
            }

            public override string ToString()
            {
                List<double> scores = new List<double>();
                List<int> salaries = new List<int>();
                //List<double> odds = new List<double>();
                BigWins = 0;
                SmallWins = 0;
                string salary = Environment.NewLine + "Remaining Salaries {";
                //string odd = Environment.NewLine + "Odds {";
                foreach (FantasyFootballTeam team in Product.PredictedTeams)
                {
                    double score = 0;
                    foreach (Player p in team.Team)
                    {
                        score += ActualScore[p.Name];
                    }
                    scores.Add(score);
                    salaries.Add(team.RemainingSalary());
                    //odds.Add(team.HotOdds());
                    if (score >= 200) BigWins++;
                    else if (score >= 160) SmallWins++;
                }
                salary += string.Join(", ", salaries) + "}";
                //odd += string.Join(", ", odds) + "}";
                return $"Year {Year}, Week {Week}:  Scores {{" + string.Join(", ", scores) + $"}}, {SmallWins} Small wins, {BigWins} Big wins" + salary;
            }
        }
    }




    public class TeamLeadHotCoPicker : CrossFunctionalTeamLead<PlayersWithPredictedHotChance, PotentialTeamList>
    {
        public override bool CanUse(Worker worker)
        {
            if (worker is WorkerProfileHotCoPicker) return true;
            return false;
        }

        public override void Evaluate()
        {
            // Not yet
        }

        public override int MaxHires()
        {
            return 500;
        }

        public override int MinHires()
        {
            return 500;
        }

        public override object Plan(Clerk clerk)
        {
            List<FantasyFootballTeam> potentialTeams = new List<FantasyFootballTeam>();
            foreach (Worker w in Workers.Keys)
            {
                potentialTeams.Add((w as WorkerProfileHotCoPicker).Plan((clerk.PlanningInput as PlayersWithPredictedHotChance).Product));
            }
            potentialTeams.Sort();
            return new PotentialTeamList(potentialTeams);
        }

        public override object Produce(Clerk clerk)
        {
            throw new NotImplementedException();
        }
    }



    public class TeamLeadHotCoModeling : CrossFunctionalTeamLead<EnrichedByDatePairedExampleSet, PlayersWithPredictedHotChance>
    {
        public double averageAccuracy = -0.862;
        private CIHotCoModeling workerInput;

        public override bool CanUse(Worker worker)
        {
            if (worker is WorkerProfileHotCoModeling) return true;
            return false;
        }

        public override void Evaluate()
        {
            // Done in planning phase
        }

        public override int MaxHires()
        {
            return 500;
        }

        public override int MinHires()
        {
            return 10;
        }

        public override void Prepare(Clerk clerk)
        {
            workerInput = new CIHotCoModeling { 
                TrainingSet = (clerk.PlanningInput as EnrichedByDatePairedExampleSet).Product.TrainingSet, 
                TestSet = (clerk.PlanningInput as EnrichedByDatePairedExampleSet).Product.TestSet 
            };
            foreach (Feature f in workerInput.TrainingSet.Features.Features)
            {
                if (f is ContinuousFeature)
                {
                    (f as ContinuousFeature).SwitchToQuartileSplitting();
                }
            }
            Feature HotOrNot = new CategoricalFeature("HotOrNot");
            HotOrNot.AutoSet(new List<string> { "Not", "Hot" });
            workerInput.TrainingSet.Features.SetLabel(HotOrNot);
            workerInput.TestSet.Features.SetLabel(HotOrNot);
            Feature salary = workerInput.TrainingSet.Features.ByName["DK salary"];
            foreach (Example x in workerInput.TrainingSet.Examples)
            {
                if ((x.ContinuousLabel.Value / (x.FeatureValues[salary] as ContinuousValue).Value) >= 0.0035){
                    x.SetLabel(new CategoricalValue("Hot"));
                } else
                {
                    x.SetLabel(new CategoricalValue("Not"));
                }
            }
            foreach (Example x in workerInput.TestSet.Examples)
            {
                if ((x.ContinuousLabel.Value / (x.FeatureValues[salary] as ContinuousValue).Value) >= 0.0035)
                {
                    x.SetLabel(new CategoricalValue("Hot"));
                }
                else
                {
                    x.SetLabel(new CategoricalValue("Not"));
                }
            }
        }

        public override object Plan(Clerk clerk)
        {
            List<Player> players = new List<Player>();
            List<double> accuracies = new List<double>();
            Dictionary<Worker, COHotCoModeling> outputs = new Dictionary<Worker, COHotCoModeling>();
            double TotalHot = 0;
            foreach (Example x in workerInput.TestSet.Examples)
            {
                TotalHot += workerInput.TestSet.Features.CategoricalLabel.Split(x.CategoricalLabel);
            }
            

            foreach (Worker w in Workers.Keys)
            {
                double Hot = 0;
                double Not = 0;
                COHotCoModeling workerOutput = (w as WorkerProfileHotCoModeling).Plan(workerInput);
                outputs[w] = workerOutput;
                foreach (Example x in workerOutput.Predictions.Keys)
                {
                    if (x.CategoricalLabel.Value == "Hot")
                    {
                        Hot += workerOutput.Predictions[x];
                    } else
                    {
                        Not += workerOutput.Predictions[x];
                    }
                }
                Workers[w].Accuracy = 0.97 * Workers[w].Accuracy + 0.03 * (Hot - Not) / TotalHot ;
                accuracies.Add(Workers[w].Accuracy);
            }

            accuracies.Sort();
            double cutoff = accuracies[accuracies.Count / 3];
            double totalAccuracy = 0;
            double retainedWorkerCount = 0;
            foreach (Worker w in Workers.Keys)
            {
                double accuracy = Workers[w].Accuracy;
                if (accuracy >= cutoff)
                {
                    Workers[w].VoteShare = 1;
                    totalAccuracy += accuracy;
                    retainedWorkerCount++;
                    Workers[w].Rating = 1;
                } else
                {
                    Workers[w].VoteShare = 0;
                    Workers[w].Rating = 0;
                }
            }

            averageAccuracy = totalAccuracy / retainedWorkerCount;
            if (clerk.PlanningIteration == 9) UI.Print($"Average Accuracy for most recent dataset: {averageAccuracy}");
            Feature role = workerInput.TestSet.Features.ByName["Pos"];
            Feature salary = workerInput.TestSet.Features.ByName["DK salary"];
            foreach (Example x in workerInput.TestSet.Examples)
            {
                double chance = 0;
                double voteshare = 0;
                foreach (Worker w in Workers.Keys)
                {
                    chance += outputs[w].Predictions[x] * Workers[w].VoteShare;
                    voteshare += Workers[w].VoteShare;
                }
                Player player = new Player(x.ID.Value, Player.ConvertToRole(x.FeatureValues[role]), (int)(x.FeatureValues[salary] as ContinuousValue).Value, chance / voteshare);
                players.Add(player);
            }
            return new PlayersWithPredictedHotChance(new PlayerPicker(players));
        }

        public override object Produce(Clerk clerk)
        {
            throw new NotImplementedException();
        }

        public override void AddWorker(Worker worker)
        {
            Workers[worker] = new PerformanceEvaluation { Accuracy = averageAccuracy };
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
