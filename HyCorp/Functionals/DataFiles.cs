using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp
{
    /// <summary>
    /// Raw Data Files - everything is strings
    /// Data is added by row
    /// </summary>
    public class RawDataFile
    {
        public List<RawColumn> Columns { get; private set; }

        public RawDataFile() 
        {
            Columns = new List<RawColumn>();
        }

        public void AddHeaders(string[] headers)
        {

            for (int i = 0; i < headers.Length; i++)
            {
                Columns.Add(new RawColumn(headers[i]));
            }
        }

        public void AddRow(string[] values)
        {
            for (int i = 0; i < Columns.Count; i++)
            {
                Columns[i].Add(values[i]);
            }
        }
    }

    /// <summary>
    /// Processed Data File - Everything is Features/Values
    /// Data is added by column
    /// Still holds all original data from the file, but might be cleaned up a bit
    /// </summary>
    public class ProcessedDataFile
    {
        public List<FeatureColumn> ColumnList { get; private set; }
        public Dictionary<Feature, FeatureColumn> ColumnDictionary { get; private set; }
        public List<Feature> Features { get; private set; }
        public Dictionary<string, Feature> FeatureDictionary { get; private set; }
        private List<List<IValue>> valueRows;

        public ProcessedDataFile()
        {
            ColumnList = new List<FeatureColumn>();
            ColumnDictionary = new Dictionary<Feature, FeatureColumn>();
            valueRows = new List<List<IValue>>();
            Features = new List<Feature>();
            FeatureDictionary = new Dictionary<string, Feature>();
        }

        public void Add(FeatureColumn column)
        {
            ColumnList.Add(column);
            ColumnDictionary[column.Feature] = column;
            Features.Add(column.Feature);
            FeatureDictionary.Add(column.Feature.Name, column.Feature);
        }

        public List<List<IValue>> GetRows()
        {
            if (valueRows.Count > 0) return valueRows;
            for (int i = 0; i < ColumnList[0].Values.Count; i++)
            {
                List<IValue> values = new List<IValue>();
                for (int j = 0; j < ColumnList.Count; j++)
                {
                    values.Add(ColumnList[j].Values[i]);
                }
                valueRows.Add(values);
            }
            return valueRows;
        }

        public void AddToExisting(FeatureColumn newColumn)
        {
            if (ColumnDictionary.ContainsKey(newColumn.Feature))
            {
                FeatureColumn existingColumn = ColumnDictionary[newColumn.Feature];
                foreach (IValue value in newColumn.Values)
                {
                    existingColumn.Add(value);
                }
            } else
            {
                Add(newColumn);
            }
        }

        public override string ToString()
        {
            string result = $"ProcessedDataFile Columns: ";
            result += string.Join(", ", ColumnList);
            return result;
        }

    }

    public class RawColumn
    {
        public string Name { get; private set; }
        public List<string> Values { get; private set; }
        public RawColumn(string name)
        {
            Name = name;
            Values = new List<string>();
        }
        public void Add(string value)
        {
            Values.Add(value);
        }
    }

    public class FeatureColumn
    {
        public Feature Feature { get; private set; }
        public List<IValue> Values { get; private set; }
        public FeatureColumn(Feature feature)
        {
            Feature = feature;
            Values = new List<IValue>();
        }

        public FeatureColumn(Feature feature, List<IValue> values)
        {
            Feature = feature;
            Values = values;
        }

        public void Add(IValue value)
        {
            Values.Add(value);
        }

        public override string ToString()
        {
            return $"({Feature.Name}, {Values.Count})";
        }

    }
}
