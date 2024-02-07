using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DRC.RTS.Formations
{
    public class TriangleFormation : FormationBase
    {

        [SerializeField] private uint unitDepth = 5;
        [SerializeField] private bool hollow = false;
        [SerializeField] private float nthOffset = 0;
        [SerializeField][Range(1, 10)] private int initialRowUnits = 1;

        public override IEnumerable<Vector3> EvaluatePoints(Vector3 forward)
        {
            //var middleOffset = new Vector3(0, 0, unitDepth * 0.5f); //sirve para centrar la formación
            int positionsCalculated = 0;

            unitDepth = !hollow ? (uint)Mathf.CeilToInt((-1 + Mathf.Sqrt(1 + 8 * totalUnits)) / 2 - initialRowUnits / 2) : (uint)Mathf.CeilToInt((Mathf.Sqrt(8 * totalUnits + 1) - 1) / 2 - initialRowUnits / 2);

            for (int z = 0; z < unitDepth; z++)
            {
                for (float x = -z - (float)initialRowUnits / 2; x < z + (float)initialRowUnits / 2; x++)
                {
                    if (positionsCalculated == totalUnits) continue;
                    if (hollow && z < unitDepth - 1 && x > -z - 1 + (float)initialRowUnits / 2 && x < z - (float)initialRowUnits / 2 && z != 0) continue;

                    var pos = new Vector3(x + (z % 2 == 0 ? 0 : nthOffset), 0, z);

                    //pos -= middleOffset;

                    pos += GetNoise(pos);

                    pos *= Spread;

                    positionsCalculated++;

                    pos = LookForwardFormation(pos, forward);

                    yield return pos;

                }
            }
        }
    }
}