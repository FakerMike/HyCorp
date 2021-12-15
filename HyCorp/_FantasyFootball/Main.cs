using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HyCorp.FantasyFootball.Corps;

namespace HyCorp.FantasyFootball
{
    /// <summary>
    /// Example HyCorps to try and predict Fantasy Football teams
    /// </summary>
    public static class FantasyFootball
    {
        
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UI());
        }

        public static void RunNaiveCo()
        {

        }
        

    }
}
