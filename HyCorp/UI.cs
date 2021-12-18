using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HyCorp.FantasyFootball;
using HyCorp.FantasyFootball.Corps;
using HyCorp.FantasyFootball.Corps.HotCo;

namespace HyCorp
{
    public partial class UI : Form
    {
        public static UI Instance;

        public static void Print(object o)
        {
            Instance.PrintOut(o);
        }

        public UI()
        {
            InitializeComponent();
            Instance = this;
        }

        private void RunButton_Click(object sender, EventArgs e)
        {

            HyCorp HotCo = new HotCo();
            HotCo.Plan(FileTools.Load(@"FantasyFootball\Data\Training\DraftKingsRaw2014-2021W9.csv"));



            /*
            HyCorp<RawDataFile, FantasyFootballProduct> naiveCo = new NaiveCo();

            naiveCo.PlanningOrganization.AddInput(FileTools.Load(@"_FantasyFootball\Data\Training\DraftKingsRaw2014-2021W9.csv"));

            naiveCo.PlanningOrganization.Train();

            naiveCo.ProductionOrganization.AddInput(FileTools.Load(@"_FantasyFootball\Data\Production\DKSalaries.csv"));

            naiveCo.ProductionOrganization.Work();

            FantasyFootballProduct product = naiveCo.ProductionOrganization.GetProduct();
            product.SortTeams();



            Print(product);*/

        }


        public void Print(string s)
        {
            OutputText.AppendText(s + Environment.NewLine);
        }

        public void PrintOut(object o)
        {
            OutputText.AppendText(o.ToString() + Environment.NewLine); ;
        }

    }
}
