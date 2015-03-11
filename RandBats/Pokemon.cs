using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandBats
{
    class Pokemon
    {
        public string name;
        public Dictionary<string, List<FileSet>> tiers_and_sets = new Dictionary<string, List<FileSet>>();
    }
}
