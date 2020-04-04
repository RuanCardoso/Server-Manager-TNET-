using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TNET_Manager
{
    class Culture
    {
        public static void ChangeCulture()
        {
            CultureInfo cultureInfo = new CultureInfo("pt-BR");
            /////////////////////////////////////////////////////////
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }

        public static CultureInfo GetCulture()
        {
            return Thread.CurrentThread.CurrentCulture;
        }
    }
}
