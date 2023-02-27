using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Diagnostics;

namespace AM2E
{
    public class State
    {
        public Action Step { get; init; } = () => { };
        public Action Enter { get; init; } = () => { };
        public Action Leave { get; init; } = () => { };
    }
}