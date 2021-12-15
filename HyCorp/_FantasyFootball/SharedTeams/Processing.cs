using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp.FantasyFootball
{
    public class ProcessingTeam : Team<RawDataFile, ProcessedDataFile>
    {
        public ProcessingTeam(): base()
        {
            Hire(new AnyInputClerk<RawDataFile,ProcessedDataFile>(this));
            Hire(new AutoApprovingManager());
            Hire(new OneWorkerTeamLead<RawDataFile, ProcessedDataFile>());
            Hire(new List<ProcessingWorker> { new ProcessingWorker() });
            Staffed = true;
        }
    }


    public class ProcessingWorker : Worker
    {
        public override object Train(object input)
        {
            List<RawDataFile> rawFiles = (List<RawDataFile>)input;
            RawDataFile source = rawFiles.Last();
            ProcessedDataFile result = new ProcessedDataFile();
            foreach(RawColumn rawColumn in source.Columns)
            {
                Feature feature;
                FeatureColumn column;
                List<IValue> values;
                switch (rawColumn.Name)
                {
                    case "DK points":
                    case "DK salary":
                    case "Week":
                    case "Year":
                    case "Salary":
                        feature = new ContinuousFeature(rawColumn.Name);
                        break;
                    case "Name":
                        feature = new IDFeature(rawColumn.Name);
                        break;
                    default:
                        feature = new CategoricalFeature(rawColumn.Name);
                        break;
                }
                feature.AutoSet(rawColumn.Values);
                values = feature.CreateValues(rawColumn.Values);
                column = new FeatureColumn(feature, values);
                result.Add(column);
            }
            return result;
        }

        // TODO: Actually has a different kind of file!
        public override object Work(object input)
        {
            List<RawDataFile> rawFiles = (List<RawDataFile>)input;
            RawDataFile source = rawFiles.Last();
            ProcessedDataFile result = new ProcessedDataFile();
            foreach (RawColumn rawColumn in source.Columns)
            {
                Feature feature;
                FeatureColumn column;
                List<IValue> values;
                switch (rawColumn.Name)
                {
                    case "DK points":
                    case "DK salary":
                    case "Week":
                    case "Year":
                    case "AvgPointsPerGame":
                    case "Salary":
                        feature = new ContinuousFeature(rawColumn.Name);
                        break;
                    case "Name":
                        feature = new IDFeature(rawColumn.Name);
                        break;
                    default:
                        feature = new CategoricalFeature(rawColumn.Name);
                        break;
                }
                feature.AutoSet(rawColumn.Values);
                values = feature.CreateValues(rawColumn.Values);
                column = new FeatureColumn(feature, values);
                result.Add(column);
            }
            return result;
        }
    }

}
