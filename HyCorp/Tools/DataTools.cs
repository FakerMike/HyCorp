using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCorp
{
    public static class FileTools
    {
        public static ProcessedDataFile Process(RawDataFile file, ProcessSettings settings)
        {
            return null;
        }

        public static RawDataFile Load(string path)
        {
            RawDataFile result = new RawDataFile();
            using (StreamReader reader = new StreamReader(path))
            {
                string[] values = reader.ReadLine().Split(',');
                result.AddHeaders(values);
                while (!reader.EndOfStream)
                {
                    values = reader.ReadLine().Split(',');
                    result.AddRow(values);
                }
            }
            return result;
        }

        public static ProcessedDataFile CombineColumns(ProcessedDataFile fileOne, ProcessedDataFile fileTwo)
        {
            return null;
        }

    }

    public class ProcessSettings
    {

        public string test;
    }
}
