using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROIReader
{
    class Program
    {
        static string version = "0.01";
        static void Main(string[] args)
        {
            Console.WriteLine(version);
            Helper.WriteResult(false);
            if(args.Count() != 2)
            {
                Console.WriteLine("Arguments count must be 2!\r\n Press any key to exit");
                Console.ReadKey();
                return;
            }
            int batchNum = 0;
            bool bok =  int.TryParse(args[1], out batchNum);
            if(!bok)
            {
                Console.WriteLine("Second argument must be integer!\r\n Press any key to exit");
                Console.ReadKey();
                return;
            }
            try
            {
                string folder = ConfigurationManager.AppSettings["workingFolder"];
                AscFile ascFile = new AscFile(string.Format("{0}{1}.asc",folder,args[0]));
                int wellCntInBatch = int.Parse(ConfigurationManager.AppSettings["wellsPerTime"]);
                List<double> vals = ascFile.GetBatch(batchNum, wellCntInBatch);
                Helper.WriteValue(vals.Select(x => x.ToString()).ToList());
                bool bRemoveSmallest = bool.Parse(ConfigurationManager.AppSettings["removeSmallest"]);

                if (bRemoveSmallest)
                    vals.Remove(vals.Min());
                double avg = vals.Average();
                Helper.WriteAvg(Math.Round(avg,4));
            }
            catch(Exception ex)
            {
                 Console.WriteLine( ex.Message + "\r\n Press any key to exit");
                 Console.ReadKey();
                 return;
            }
            Helper.WriteResult(true);
        }
    }


    class AscFile
    {
        Dictionary<int, double> wellPos_Val = new Dictionary<int, double>();
        public AscFile(string file)
        {
            List<string> strs = new List<string>();
            strs = File.ReadAllLines(file).ToList();
            strs = strs.Where(x => x != "").ToList();
            if(strs.Count != 9)
            {
                throw new Exception("lines in the file != 9");
            }
            strs.RemoveAt(0);
            for(int row = 0; row < 8; row++)
            {
                List<string> sColumns = strs[row].Split('\t').ToList();
                sColumns.RemoveAt(0);
                for( int col = 0; col < sColumns.Count; col++)
                {
                    int wellID = col * 8 + row + 1;
                    string sVal = sColumns[col];
                    if (sVal == "")
                        continue;
                    double val = double.Parse(sVal);
                    wellPos_Val.Add(wellID, val);
                }
            }
        }


        internal List<double> GetBatch(int batchNum, int wellCntInBatch)
        {
            int wellIDStart = (batchNum - 1) * wellCntInBatch + 1;
            int wellIDEnd = wellIDStart + wellCntInBatch - 1;
            return wellPos_Val.Where(x => x.Key >= wellIDStart && x.Key <= wellIDEnd).Select(x => x.Value).ToList();
        }
    }
}
