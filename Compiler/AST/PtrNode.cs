using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AST
{
    class PtrNode : Node
    {
        public int Change { get; set; }
        public override string ToString() => $"{base.ToString()}(Change: {Change})";
    }
}
