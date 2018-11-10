using System;
using System.Collections.Generic;
using System.Text;

namespace ApiGuard.TestApi
{
    public class TheThirdService
    {
        [Obsolete]
        public void Boom(string name, int number)
        {

        }
    }
}
