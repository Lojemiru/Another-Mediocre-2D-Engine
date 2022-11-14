using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Diagnostics;

namespace AM2E
{
    public class State
    {
        public Action Step { get; set; } = () => { };
        public Action Enter { get; set; } = () => { };
        public Action Leave { get; set; } = () => { };
    }
}