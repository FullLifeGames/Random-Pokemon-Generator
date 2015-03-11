using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RandBats
{
    public partial class Form1 : Form
    {

        #region Construktor
        public Form1()
        {
            InitializeComponent();
            pokemon = new Dictionary<string, Pokemon>();
            unselectedTiers = new List<string>();
            directoryRoot = Properties.Settings.Default.directoryRoot;
            if (directoryRoot != null)
            {
                textBox1.Text = directoryRoot;
            }
        }
        #endregion

        #region Variablen
        private static readonly string[] alltiers = { "OU", "UU", "RU", "NU", "PU", "LC", "VGC14", "VGC15", "Doubles", "Uber", "Unreleased" };
        private string directoryRoot;
        private Dictionary<string, Pokemon> pokemon;
        private List<string> unselectedTiers;
        private List<Process> processes = new List<Process>();
        private string setFile1;
        private string setFile2;
        private string setFile3;
        private string setFile4;
        private string setFile5;
        private string setFile6;
        private int rekcount = 0;
        private bool changing = false;
        private bool changeCheckBack = true;
        private static readonly int REK_GRAD = 500;
        #endregion

        #region Utilityfunctions
        private void fetchPokemon()
        {
            bool realfolder = false;
            try
            {
                pokemon.Clear();
                DirectoryInfo d = new DirectoryInfo(textBox1.Text);                
                directoryRoot = textBox1.Text;
                Properties.Settings.Default.directoryRoot = directoryRoot;
                Properties.Settings.Default.Save();
                try
                {
                    if (d.Exists)
                    {
                        foreach (DirectoryInfo tier in d.GetDirectories())
                        {
                            if (alltiers.Contains(tier.Name))
                            {
                                realfolder = true;
                            }
                            if (!unselectedTiers.Contains(tier.Name) && alltiers.Contains(tier.Name))
                            {
                                foreach (DirectoryInfo poke in tier.GetDirectories())
                                {
                                    foreach (FileInfo set in poke.GetFiles())
                                    {
                                        if (!pokemon.ContainsKey(poke.Name))
                                        {
                                            pokemon.Add(poke.Name, new Pokemon());
                                            pokemon[poke.Name].name = poke.Name;
                                        }
                                        Pokemon p = pokemon[poke.Name];
                                        if (!p.tiers_and_sets.ContainsKey(tier.Name))
                                        {
                                            p.tiers_and_sets.Add(tier.Name, new List<FileSet>());
                                        }
                                        FileSet fs = new FileSet();
                                        fs.set = set.Name;
                                        fs.file = set.FullName;
                                        p.tiers_and_sets[tier.Name].Add(fs);

                                    }
                                }
                            }
                        }
                    }
                }
                catch (IOException)
                {
                }
            }
            catch (ArgumentException)
            {
            }
            if (unselectedTiers.Count != 11)
            {
                changeCheckBack = false;
                if (pokemon.Count == 0)
                {
                    radioButton1.Checked = false;
                }
                else
                {
                    radioButton1.Checked = true;
                }
                changeCheckBack = true;
            }
            else if (!realfolder)
            {
                changeCheckBack = false;
                radioButton1.Checked = false;
                changeCheckBack = true;
            }
            else
            {
                changeCheckBack = false;
                radioButton1.Checked = true;
                changeCheckBack = true;
            }
        }

        private bool speciesContains(List<Species> species, string s)
        {
            foreach (Species sp in species)
            {
                if (s.Equals(sp.species))
                {
                    return true;
                }
            }
            return false;
        }

        private bool twoMegas(string[] setfiles)
        {
            if (CHK_Mega.Checked)
            {
                int count = 0;
                foreach (string s in setfiles)
                {
                    string item = s.Substring(s.LastIndexOf("\\") + 1);
                    item = item.Substring(item.IndexOf("_") + 1);
                    item = item.Substring(item.IndexOf("_") + 1);
                    item = item.Substring(0, item.IndexOf("_"));
                    if (item.Contains("ite") && !item.Equals("Eviolite"))
                    {
                        count++;
                    }
                }
                return count > 1;
            }
            else
            {
                return false;
            }
        }

        private bool twoRockers(string[] setfiles)
        {
            if (CHK_Stealth_Rock.Checked)
            {
                int count = 0;
                foreach (string s in setfiles)
                {
                    string item = s.Substring(s.LastIndexOf("\\") + 1);
                    item = item.Substring(item.IndexOf("_") + 1);
                    item = item.Substring(item.IndexOf("_") + 1);
                    item = item.Substring(item.IndexOf("_") + 1);
                    item = item.Substring(item.IndexOf("_") + 1);
                    item = item.Substring(item.IndexOf("_") + 1);
                    string temp = item.Substring(0, item.IndexOf("_"));
                    if (temp.Contains("Stealth Rock"))
                    {
                        count++;
                    }
                    item = item.Substring(item.IndexOf("_") + 1);
                    temp = item.Substring(0, item.IndexOf("_"));
                    if (temp.Contains("Stealth Rock"))
                    {
                        count++;
                    }
                    item = item.Substring(item.IndexOf("_") + 1);
                    temp = item.Substring(0, item.IndexOf("_"));
                    if (temp.Contains("Stealth Rock"))
                    {
                        count++;
                    }
                    item = item.Substring(item.IndexOf("_") + 1);
                    temp = item.Substring(0, item.IndexOf("."));
                    if (temp.Contains("Stealth Rock"))
                    {
                        count++;
                    }
                }
                return count > 1;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region button_Click
        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox1.Text = fbd.SelectedPath;
                fetchPokemon();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (pokemon.Count == 0)
            {
                fetchPokemon();
            }
            if (pokemon.Count == 0)
            {
                return;
            }
            ResourceManager rm = Properties.Resources.ResourceManager;
            Pokemon[] randomgennedPokes = new Pokemon[6];
            Random r = new Random();
            List<Species> speciesKey = new List<Species>();
            Dictionary<string, Species> dicspe = new Dictionary<string, Species>();
            if (CHK_Species.Checked)
            {
                foreach (string s in pokemon.Keys)
                {
                    string reworked = s;
                    if (reworked.Contains("-"))
                    {
                        reworked = reworked.Substring(0, reworked.IndexOf("-"));
                    }
                    if (!speciesContains(speciesKey, reworked))
                    {
                        Species sp = new Species();
                        sp.species = reworked;
                        if (s.Contains("-"))
                        {
                            sp.addons.Add(s.Substring(s.IndexOf("-")));
                        }
                        else
                        {
                            sp.addons.Add("");
                        }
                        speciesKey.Add(sp);
                        dicspe.Add(reworked, sp);
                    }
                    else
                    {
                        if (s.Contains("-"))
                        {
                            dicspe[reworked].addons.Add(s.Substring(s.IndexOf("-")));
                        }
                    }
                }
            }



            int one;
            int two;
            int three;
            int four;
            int five;
            int six;
            if (CHK_Species.Checked)
            {
                // No while true
                if (speciesKey.Count < 6)
                {
                    return;
                }
                int[] randomNumbers = new int[6];
                for (int i = 0; i < 6; i++)
                {
                    randomNumbers[i] = -1;
                }

                one = r.Next(speciesKey.Count);
                while (randomNumbers.Contains(one))
                {
                    one = r.Next(speciesKey.Count);
                }
                randomNumbers[0] = one;

                two = r.Next(speciesKey.Count);
                while (randomNumbers.Contains(two))
                {
                    two = r.Next(speciesKey.Count);
                }
                randomNumbers[1] = two;

                three = r.Next(speciesKey.Count);
                while (randomNumbers.Contains(three))
                {
                    three = r.Next(speciesKey.Count);
                }
                randomNumbers[2] = three;

                four = r.Next(speciesKey.Count);
                while (randomNumbers.Contains(four))
                {
                    four = r.Next(speciesKey.Count);
                }
                randomNumbers[3] = four;

                five = r.Next(speciesKey.Count);
                while (randomNumbers.Contains(five))
                {
                    five = r.Next(speciesKey.Count);
                }
                randomNumbers[4] = five;

                six = r.Next(speciesKey.Count);
                while (randomNumbers.Contains(six))
                {
                    six = r.Next(speciesKey.Count);
                }

            }
            else
            {
                one = r.Next(pokemon.Keys.Count);
                two = r.Next(pokemon.Keys.Count);
                three = r.Next(pokemon.Keys.Count);
                four = r.Next(pokemon.Keys.Count);
                five = r.Next(pokemon.Keys.Count);
                six = r.Next(pokemon.Keys.Count);
            }
            int count = 0;
            if (CHK_Species.Checked)
            {
                List<string> finalPokemon = new List<string>();
                foreach (Species sp in speciesKey)
                {
                    finalPokemon.Add(sp.species + sp.addons[r.Next(sp.addons.Count)]);
                }

                foreach (string s in finalPokemon)
                {
                    if (one == count)
                    {
                        randomgennedPokes[0] = pokemon[s];
                    }
                    if (two == count)
                    {
                        randomgennedPokes[1] = pokemon[s];
                    }
                    if (three == count)
                    {
                        randomgennedPokes[2] = pokemon[s];
                    }
                    if (four == count)
                    {
                        randomgennedPokes[3] = pokemon[s];
                    }
                    if (five == count)
                    {
                        randomgennedPokes[4] = pokemon[s];
                    }
                    if (six == count)
                    {
                        randomgennedPokes[5] = pokemon[s];
                    }
                    count++;
                }
            }
            else
            {
                foreach (string s in pokemon.Keys)
                {
                    if (one == count)
                    {
                        randomgennedPokes[0] = pokemon[s];
                    }
                    if (two == count)
                    {
                        randomgennedPokes[1] = pokemon[s];
                    }
                    if (three == count)
                    {
                        randomgennedPokes[2] = pokemon[s];
                    }
                    if (four == count)
                    {
                        randomgennedPokes[3] = pokemon[s];
                    }
                    if (five == count)
                    {
                        randomgennedPokes[4] = pokemon[s];
                    }
                    if (six == count)
                    {
                        randomgennedPokes[5] = pokemon[s];
                    }
                    count++;
                }
            }
            PictureBox[] pictureBoxes = { pictureBox1, pictureBox2, pictureBox3, pictureBox4, pictureBox5, pictureBox6 };
            string[] setfiles = { setFile1, setFile2, setFile3, setFile4, setFile5, setFile6 };
            Label[] labels = { label2, label3, label4, label5, label6, label7 };
            for (int i = 0; i < randomgennedPokes.Length; i++)
            {
                Pokemon pok = randomgennedPokes[i];
                string tier = "";
                int randomtier = r.Next(pok.tiers_and_sets.Keys.Count);
                count = 0;
                foreach (string s in pok.tiers_and_sets.Keys)
                {
                    if (count == randomtier)
                    {
                        tier = s;
                    }
                    count++;
                }

                int randomset = r.Next(pok.tiers_and_sets[tier].Count);
                string set = pok.tiers_and_sets[tier][randomset].set;
               
                setfiles[i] = pok.tiers_and_sets[tier][randomset].file;
            }
            setFile1 = setfiles[0];
            setFile2 = setfiles[1];
            setFile3 = setfiles[2];
            setFile4 = setfiles[3];
            setFile5 = setfiles[4];
            setFile6 = setfiles[5];

            if ((twoMegas(setfiles) || twoRockers(setfiles)) && rekcount < REK_GRAD)
            {
                rekcount++;
                button2_Click(null, null);
            }
            else
            {
                rekcount = 0;
                for (int i = 0; i < randomgennedPokes.Length; i++)
                {
                    Pokemon pok = randomgennedPokes[i];
                    Bitmap bitmap = (Bitmap)rm.GetObject(pok.name.ToLower().Replace("-", "_").Replace(".", "").Replace(" ", "_").Replace("'", "_"));
                    PictureBox pictureBox = pictureBoxes[i];
                    if (bitmap == null)
                    {
                        Console.WriteLine(pok.name);
                    }
                    pictureBox.Image = bitmap;
                    labels[i].Text = pok.name;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            changing = true;
            bool b = true;
            CHK_Doubles.Checked = b;
            CHK_LC.Checked = b;
            CHK_PU.Checked = b;
            CHK_NU.Checked = b;
            CHK_RU.Checked = b;
            CHK_UU.Checked = b;
            CHK_OU.Checked = b;
            CHK_Unreleased.Checked = b;
            CHK_VGC.Checked = b;
            CHK_Uber.Checked = b;
            changing = false;
            CHK_CheckedChanged(null, null);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            changing = true;
            bool b = false;
            CHK_Doubles.Checked = b;
            CHK_LC.Checked = b;
            CHK_PU.Checked = b;
            CHK_NU.Checked = b;
            CHK_RU.Checked = b;
            CHK_UU.Checked = b;
            CHK_OU.Checked = b;
            CHK_Unreleased.Checked = b;
            CHK_VGC.Checked = b;
            CHK_Uber.Checked = b;
            changing = false;
            CHK_CheckedChanged(null, null);
        }
        #endregion

        #region textBox_TextChanged
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            fetchPokemon();
        }
        #endregion

        #region PictureBox_Click
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (setFile1 != null)
            {
                FileInfo f = new FileInfo(setFile1);
                if (f.Exists)
                {
                    processes.Add(Process.Start("explorer.exe", "/select, \"" + f.FullName + "\""));
                }
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (setFile2 != null)
            {
                FileInfo f = new FileInfo(setFile2);
                if (f.Exists)
                {
                    processes.Add(Process.Start("explorer.exe", "/select, \"" + f.FullName + "\""));
                }
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            if (setFile3 != null)
            {
                FileInfo f = new FileInfo(setFile3);
                if (f.Exists)
                {
                    processes.Add(Process.Start("explorer.exe", "/select, \"" + f.FullName + "\""));
                }
            }
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            if (setFile4 != null)
            {
                FileInfo f = new FileInfo(setFile4);
                if (f.Exists)
                {
                    processes.Add(Process.Start("explorer.exe", "/select, \"" + f.FullName + "\""));
                }
            }
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            if (setFile5 != null)
            {
                FileInfo f = new FileInfo(setFile5);
                if (f.Exists)
                {
                    processes.Add(Process.Start("explorer.exe", "/select, \"" + f.FullName + "\""));
                }
            }
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            if (setFile6 != null)
            {
                FileInfo f = new FileInfo(setFile6);
                if (f.Exists)
                {
                    processes.Add(Process.Start("explorer.exe", "/select, \"" + f.FullName + "\""));
                }
            }
        }
        #endregion

        #region CHK_CheckedChanged
        private void CHK_CheckedChanged(object sender, EventArgs e)
        {
            rekcount = 0;
            if (!changing)
            {
                unselectedTiers.Clear();
                if (!CHK_Uber.Checked)
                {
                    unselectedTiers.Add("Uber");
                }
                if (!CHK_OU.Checked)
                {
                    unselectedTiers.Add("OU");
                }
                if (!CHK_UU.Checked)
                {
                    unselectedTiers.Add("UU");
                }
                if (!CHK_RU.Checked)
                {
                    unselectedTiers.Add("RU");
                }
                if (!CHK_NU.Checked)
                {
                    unselectedTiers.Add("NU");
                }
                if (!CHK_PU.Checked)
                {
                    unselectedTiers.Add("PU");
                }
                if (!CHK_LC.Checked)
                {
                    unselectedTiers.Add("LC");
                }
                if (!CHK_Unreleased.Checked)
                {
                    unselectedTiers.Add("Unreleased");
                }
                if (!CHK_VGC.Checked)
                {
                    unselectedTiers.Add("VGC14");
                    unselectedTiers.Add("VGC15");
                }
                if (!CHK_Doubles.Checked)
                {
                    unselectedTiers.Add("Doubles");
                }
                fetchPokemon();
            }
        }

        private void CHK_Mega_CheckedChanged(object sender, EventArgs e)
        {
            rekcount = 0;
        }        

        private void CHK_Stealth_Rock_CheckedChanged(object sender, EventArgs e)
        {
            rekcount = 0;
        }
        #endregion

        #region radioButtonblockade
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if(changeCheckBack){
                changeCheckBack = false;
                radioButton1.Checked = !radioButton1.Checked;
                changeCheckBack = true;
            }
        }
        #endregion

    }
}
