using System.Collections.Generic;
using UnityEngine;

namespace DRC.RTS.Formations
{
    public abstract class FormationBase : MonoBehaviour
    {
        [SerializeField][Range(1, 500)] protected int totalUnits = 10;
        [SerializeField][Range(0, 1)] protected float _noise = 0;
        [SerializeField] protected float Spread = 1;
        public abstract IEnumerable<Vector3> EvaluatePoints(Vector3 forward);
        public IEnumerable<Vector3> EvaluatePoints(Vector3 forward, int totalPoints) { totalUnits = totalPoints; return EvaluatePoints(forward); }

        public Vector3 GetNoise(Vector3 pos)
        {
            var noise = Mathf.PerlinNoise(pos.x * _noise, pos.z * _noise);

            return new Vector3(noise, 0, noise);
        }
        public Vector3 LookForwardFormation(Vector3 pos, Vector3 forward)
        {
            var rotationY = (Quaternion.LookRotation(forward).eulerAngles.y + 180);

            return Quaternion.Euler(new Vector3(0, rotationY, 0)) * pos;
        }
    }
}
