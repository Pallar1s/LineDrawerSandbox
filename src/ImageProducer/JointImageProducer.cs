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

        public IEnumerable<Vector2> Tick(TimeSpan dt)
        {
            var currentTransform = Matrix3x2.Identity;
            
            foreach (var joint in joints)
            {
                var radians = joint.CurrentRotation +
                              joint.Speed * this.speed * (float)(dt.TotalSeconds * 10);
                
                joint.CurrentRotation = radians;
                var rotation = Matrix3x2.CreateRotation(radians);
                var translation = Matrix3x2.CreateTranslation(0, joint.Size);
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