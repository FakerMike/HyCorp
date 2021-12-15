using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp.FantasyFootball
{
    // Does not enrich data at all
    public class NaiveCoDataTeam : Team<ProcessedDataFile, ExampleSet>
    {
        public NaiveCoDataTeam(): base()
        {
            Hire(new AnyInputClerk<ProcessedDataFile,ExampleSet>(this));
            Hire(new AutoApprovingManager());
            Hire(new OneWorkerTeamLead<ProcessedDataFile, ExampleSet>());
            Hire(new List<NaiveCoDataWorker> { new NaiveCoDataWorker() });
            Staffed = true;
        }
    }


    public class NaiveCoDataWorker : Worker
    {
        public override object Work(object input)
        {
            List<ProcessedDataFile> processedFiles = (List<ProcessedDataFile>)input;
            ProcessedDataFile source = processedFiles.Last();

            
            // We're going to build a new data file with only two things:  Name and Average Score this Season
            List<IValue> year = source.ColumnDictionary[source.FeatureDictionary["Year"]].Values;
            List<IValue> name = source.ColumnDictionary[source.FeatureDictionary["Name"]].Values;
            List<IValue> points = source.ColumnDictionary[source.FeatureDictionary["DK points"]].Values;

            Dictionary<string, (double, int)> playerScores = new Dictionary<string, (double, int)>();

            for (int i = 0; i < name.Count; i++)
            {
                ContinuousValue targetYear = new ContinuousValue(2021);
                if (year[i].Equals(targetYear))
                {
                    IDValue playerNameValue = (IDValue)name[i];
                    ContinuousValue playerPointsValue = (ContinuousValue)points[i];
                    if (playerScores.ContainsKey(playerNameValue.Value))
                    {
                        playerScores[playerNameValue.Value] = (playerScores[playerNameValue.Value].Item1 + playerPointsValue.Value, playerScores[playerNameValue.Value].Item2 + 1);
                    } else
                    {
                        playerScores[playerNameValue.Value] = (playerPointsValue.Value, 1);
                    }
                }
            }

            List<string> playerNameList = new List<string>();
            List<double> playerScoreList = new List<double>();
            List<IValue> playerNameValues = new List<IValue>();
            List<IValue> playerScoreValues = new List<IValue>();
            foreach (string key in playerScores.Keys)
            {
                double val = playerScores[key].Item1 / playerScores[key].Item2;
                playerNameList.Add(key);
                playerScoreList.Add(val);
                playerNameValues.Add(new IDValue(key));
                playerScoreValues.Add(new ContinuousValue(val));
            }

            IDFeature nameFeature = new IDFeature("Name");
            ContinuousFeature averageScoreFeature = new ContinuousFeature("Score");

            nameFeature.AutoSet(playerNameList);
            averageScoreFeature.AutoSet(playerScoreList);

            FeatureColumn nameColumn = new FeatureColumn(nameFeature, playerNameValues);
            FeatureColumn scoreColumn = new FeatureColumn(averageScoreFeature, playerScoreValues);

            ProcessedDataFile newFile = new ProcessedDataFile();
            newFile.Add(nameColumn);
            newFile.Add(scoreColumn);
            

            ExampleSet result = new ExampleSet(newFile);
            return result;
        }

        public override object Train(object input)
        {
            // Does the same thing either way.
            return Work(input);
        }
    }

}
