﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public enum Token
    {
        IncPtr,
        DecPtr,
        IncData,
        DecData,
        Out,
        In,
        LoopStart,
        LoopEnd
    }
}