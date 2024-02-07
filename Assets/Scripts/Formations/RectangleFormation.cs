using System.Collections.Generic;
using UnityEngine;

namespace DRC.RTS.Formations
{
    public class RectangleFormation : FormationBase
    {
        [SerializeField] private int unitWidth = 5;
        [SerializeField] private int unitDepth = 5;
        [SerializeField] private bool hollow = false;
        [SerializeField] private float offset = 0;
        [SerializeField] private int widthFinalRow = 0;

        public override IEnumerable<Vector3> EvaluatePoints(Vector3 forward)
        {
            CalculateDepth();

            int positionsCalculated = 0;
            widthFinalRow = hollow ? unitWidth : unitWidth - (unitWidth * unitDepth - totalUnits);
            var uWidth = unitWidth;

            for (var z = 0; z < unitDepth; z++)
            {
                if (z == unitDepth - 1) uWidth = widthFinalRow;
                for (float x = -uWidth / 2; x <= uWidth / 2; x++)
                {
                    if (positionsCalculated == totalUnits) continue;
                    if (hollow && x != 0 && x != unitWidth - 1 && z != 0 && z != unitDepth - 1) continue;

                    var pos = new Vector3(x + (z % 2 == 0 ? 0 : offset), 0, z) * Spread;

                    if (!hollow && z == unitDepth - 1 && widthFinalRow != unitWidth) pos = new Vector3((float)(unitWidth - widthFinalRow) / 2 + x, 0, z) * Spread;

                    pos += GetNoise(pos);

                    pos = LookForwardFormation(pos, forward);

                    positionsCalculated++;

                    yield return pos;

                }
            }
        }
        public void CalculateDepth()
        {
            float sideLength = Mathf.Sqrt(totalUnits);


            int rows = Mathf.CeilToInt(sideLength);
            int columns = Mathf.CeilToInt(totalUnits / (float)rows);


            if (hollow)
            {
                rows = Mathf.Max(2, rows);
                columns = Mathf.Max(2, columns);
            }

            unitWidth = rows;
            unitDepth = columns;
        }
    }
}