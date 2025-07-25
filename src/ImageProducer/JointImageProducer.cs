using System;
using System.Collections.Generic;
using System.Numerics;

namespace ImageProducer
{
    public class JointImageProducer : IImageProducer
    {
        private readonly List<JointInfo> joints;
        private float speed = 1.0f;
        
        public JointImageProducer(List<JointInfo> joints)
        {
            this.joints = joints;
        }

        private const float Pi2 = 2 * MathF.PI;

        public IEnumerable<Vector2> Tick(TimeSpan dt)
        {
            var currentTransform = Matrix3x2.Identity;
            
            foreach (var joint in joints)
            {
                float size = joint.Size;
                
                if (joint.PulseEnabled)
                {
                    joint.PulsePhase += joint.PulseSpeed * (float)dt.TotalSeconds;
                    if (joint.PulsePhase > Pi2) 
                        joint.PulsePhase -= Pi2;
                    
                    float amplitude = 1 - joint.PulseMinCoef;
                    float pulse = joint.PulseMinCoef + amplitude * (0.5f + 0.5f * MathF.Sin(joint.PulsePhase));
                    size = joint.Size * pulse;
                }

                var radians = joint.CurrentRotation +
                              joint.Speed * this.speed * (float)(dt.TotalSeconds * 10);
                
                if (radians > Pi2)
                    radians -= Pi2;
                
                joint.CurrentRotation = radians;
                var rotation = Matrix3x2.CreateRotation(radians);
                var translation = Matrix3x2.CreateTranslation(0, size);
                var currentJointTransform = translation * rotation;
                currentTransform = currentJointTransform * currentTransform;
                yield return currentTransform.Translation;
            }
        }

        public float Speed
        {
            get => this.speed;
            set => this.speed = value;
        }
    }
}