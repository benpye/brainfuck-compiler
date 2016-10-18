using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class Command
    {
        public Token Op { get; set; }
        public Command Inner { get; set; }
        public Command Next { get; set; }
    }
}
