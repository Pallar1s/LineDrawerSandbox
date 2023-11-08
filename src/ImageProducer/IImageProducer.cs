using System.Collections.Generic;
using System.Numerics;

namespace ImageProducer
{
    public interface IImageProducer
    {
        IEnumerable<Vector2> Tick();
        
        float Speed { get; set; }
    }
}