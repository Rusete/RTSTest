using System;
using System.Collections.Generic;
using UnityEngine;

namespace DRC.RTS.Formations
{
    public class TriangleEqFormation : FormationBase
    {

        [SerializeField] private uint unitDepth = 5;
        [SerializeField] private float nthOffset = 0;
        [SerializeField] private uint widthFinalRow = 0;
        public override IEnumerable<Vector3> EvaluatePoints(Vector3 forward)
        {
            // var middleOffset = new Vector3(0, 0, unitDepth * 0.5f); //sirve para centrar la formación
            uint CalculatedPositions = 0;
            CalculateDepth();
            var previous = TriangularNumber(0);
            var current = TriangularNumber(1);

            for (uint z = 1; z <= unitDepth; z++)
            {
                var width = current - previous;
                for (uint x = 0; x < current - previous; x++)
                {
                    if (CalculatedPositions == totalUnits) continue;

                    var pos = new Vector3(x + (z % 2 == 0 ? 0 : nthOffset) - (float)width / 2, 0, z - 1) * Spread;

                    if (z == unitDepth && widthFinalRow != width) pos = new Vector3(x + (z % 2 == 0 ? 0 : nthOffset) + (float)(-widthFinalRow) / 2, 0, z - 1) * Spread;

                    //pos -= middleOffset;

                    pos += GetNoise(pos);

                    pos = LookForwardFormation(pos, forward);

                    CalculatedPositions++;

                    yield return pos;

                }
                previous = TriangularNumber(z);
                current = TriangularNumber(z + 1);
            }
        }

        void CalculateDepth()
        {
            uint n = 1;
            unitDepth = TriangularNumber(n);

            while (TriangularNumber(n + 1) <= totalUnits)
            {
                n++;
                uint nextTriangular = TriangularNumber(n);

                // Check if the next triangular number is closer to totalUnits
                if (Mathf.Abs(totalUnits - nextTriangular) < Mathf.Abs(totalUnits - unitDepth))
                {
                    widthFinalRow = (uint)Mathf.Abs(totalUnits - nextTriangular);
                    unitDepth = n + 1;
                }
            }
        }
        uint TriangularNumber(uint n)
        {
            return (n * (n + 1)) / 2;
        }
    }
}