﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Core
{
    public interface ICommandLineControllerInterface
    {
        void ProcessCommandLine(List<string> commands);
    }
}
