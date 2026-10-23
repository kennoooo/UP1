using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    internal class Core
    {
        public static MuseumTicketsDBEntities1 DB = new MuseumTicketsDBEntities1();

        public static Users CurrentUser;
    }
}
