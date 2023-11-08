using System.Collections.Generic;
using System.Numerics;

namespace ImageProducer
{
    public class JointImageProducer : IImageProducer
    {
        private readonly List<JointInfo> joints;
        private List<Matrix3x2> jointTransforms;
        private float speed = 1.0f;
        
        public JointImageProducer(List<JointInfo> joints)
        {
            this.joints = joints;
            this.jointTransforms = new List<Matrix3x2>(joints.Count);
            
            foreach (var joint in joints)
            {
                this.jointTransforms.Add(Matrix3x2.CreateTranslation(0, joint.Size));
            }
        }

        public IEnumerable<Vector2> Tick()
        {
            var currentTransform = Matrix3x2.Identity;
            var i = 0;
            var newTransforms = new List<Matrix3x2>(this.jointTransforms.Count);
            
            foreach (var jointTransform in this.jointTransforms)
            {
                var currentJointTransform = jointTransform * Matrix3x2.CreateRotation(this.joints[i].Speed * this.speed);
                currentTransform = currentJointTransform * currentTransform;
                newTransforms.Add(currentJointTransform);
                yield return currentTransform.Translation;
                i++;
            }

            this.jointTransforms = newTransforms;
        }

        public float Speed
        {
            get => this.speed;
            set => this.speed = value;
        }
    }
}