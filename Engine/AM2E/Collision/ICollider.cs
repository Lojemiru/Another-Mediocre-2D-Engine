using System;
using System.Collections.Generic;
using System.Text;

namespace AM2E.Collision
{
    public interface ICollider
    {
        public Collider Collider { get; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
