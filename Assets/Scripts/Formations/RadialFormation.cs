using System.Collections.Generic;
using UnityEngine;

namespace DRC.RTS.Formations
{
    public class RadialFormation : FormationBase
    {
        [SerializeField] private float radius = 1;
        [SerializeField] private float radiusGrowthMultiplier = 0;
        [SerializeField] private float rotations = 1;
        [SerializeField][Range(1, 10)] private uint rings = 1;
        [SerializeField] private float ringOffset = 1;
        [SerializeField] private float nthOffset = 0;

        public override IEnumerable<Vector3> EvaluatePoints(Vector3 forward)
        {
            var amountPerRing = totalUnits / rings;
            var ringOffset = 0f;
            for (var i = 0; i < rings; i++)
            {
                var startingAngle = i * 2 * Mathf.PI / rings; // Adjust the starting angle for centering

                for (var j = 0; j < amountPerRing; j++)
                {
                    var angle = startingAngle + j * Mathf.PI * (2 * rotations) / amountPerRing + (i % 2 != 0 ? nthOffset : 0);

                    var radius = this.radius + ringOffset + j * radiusGrowthMultiplier;
                    var x = Mathf.Cos(angle) * radius;
                    var z = Mathf.Sin(angle) * radius;

                    var pos = new Vector3(x, 0, z);

                    pos += GetNoise(pos);

                    pos *= Spread;

                    pos = LookForwardFormation(pos, forward);

                    yield return pos;
                }

                ringOffset += this.ringOffset;
            }
        }
    }
}