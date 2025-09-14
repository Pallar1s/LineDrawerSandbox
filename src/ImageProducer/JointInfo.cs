
namespace ImageProducer
{
    public class JointInfo
    {
        public float Size { get; set; }
        
        public float Speed { get; set; }

        public float CurrentRotation { get; set; } = 0.0f;

        public bool PulseEnabled { get; set; }
        public float PulseMinCoef { get; set; }
        public float PulseSpeed { get; set; }
        public float PulsePhase { get; set; }
        public int ColorR { get; set; }
        public int ColorG { get; set; }
        public int ColorB { get; set; }
    }
}