using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp.FantasyFootball.Corps
{
    class NaiveCoModelingTeam : Team<ExampleSet, List<Player>>
    {
        public NaiveCoModelingTeam() : base()
        {
            Hire(new AnyInputClerk<ExampleSet, List<Player>>(this));
            Hire(new AutoApprovingManager());
            Hire(new OneWorkerTeamLead<ExampleSet, List<Player>>());
            Hire(new List<NaiveCoModelingWorker> { new NaiveCoModelingWorker() });
            Staffed = true;
        }


        public class NaiveCoModelingWorker : Worker
        {
            Dictionary<string, Histogram> projectedScores;
            ContinuousFeature Score;
            CategoricalFeature Name;
            // Train - exampleset contains average score
            public override object Train(object input)
            {
                List<ExampleSet> sets = (List<ExampleSet>)input;
                ExampleSet set = sets.Last();
                projectedScores = new Dictionary<string, Histogram>();
                Score = (ContinuousFeature)set.Features.ByName["Score"];
                Name = (IDFeature)set.Features.ByName["Name"];

                foreach (Example x in set.Examples)
                {
                    string name = ((IDValue)x.FeatureValues[Name]).Value;
                    double baseScore = ((ContinuousValue)x.FeatureValues[Score]).Value;

                    Histogram projectedScore = new Histogram(Score.Distribution.GetSettings());
                    for (int i = 0; i < 32; i++)
                    {
                        if (i < 16)
                        {
                            projectedScore.AddData(baseScore);
                            projectedScore.AddData(baseScore);
                        }
                        else if (i < 24)
                        {
                            projectedScore.AddData(1.1 * baseScore);
                            projectedScore.AddData(0.9 * baseScore);

                        }
                        else if (i < 28)
                        {
                            projectedScore.AddData(1.25 * baseScore);
                            projectedScore.AddData(0.75 * baseScore);
                        }
                        else
                        {
                            projectedScore.AddData(2 * baseScore);
                            projectedScore.AddData(0.5 * baseScore);
                        }
                    }

                    projectedScores[name] = projectedScore;
                }
                return null;
            }

            // Work - exampleset contains player prices only
            public override object Work(object input)
            {
                List<ExampleSet> sets = (List<ExampleSet>)input;
                ExampleSet set = sets.Last();

                List<Player> result = new List<Player>();

                foreach (Example x in set.Examples)
                {
                    string name = ((IDValue)x.FeatureValues[set.Features.ByName["Name"]]).Value;
                    double averageScore = ((ContinuousValue)x.FeatureValues[set.Features.ByName["AvgPointsPerGame"]]).Value;
                    Histogram scores;
                    if (averageScore == 0) continue;
                    if (projectedScores.ContainsKey(name))
                    {
                        scores = projectedScores[name];
                    } else
                    {
                        UI.Instance.Print("Missing scores for player " + name);
                        scores = new SingularHistogram(Score.Average);
                    }
                    string roleString = ((CategoricalValue)x.FeatureValues[set.Features.ByName["Position"]]).Value;
                    PlayerRole role;
                    switch (roleString)
                    {
                        case "QB":
                            role = PlayerRole.QB;
                            break;
                        case "RB":
                            role = PlayerRole.RB;
                            break;
                        case "WR":
                            role = PlayerRole.WR;
                            break;
                        case "TE":
                            role = PlayerRole.TE;
                            break;
                        default:
                            role = PlayerRole.DST;
                            break;
                    }

                    int salary = (int)((ContinuousValue)x.FeatureValues[set.Features.ByName["Salary"]]).Value;

                    Player player = new Player(name, role, salary, scores);
                    result.Add(player);
                }

                return result;
            }
        }
    }
}
