using UnityEngine;

namespace DRC.RTS.Units 
{
    [CreateAssetMenu(fileName ="New Unit", menuName ="New Unit/Basic")]
    public class UnitData : ScriptableObject
    {
        public enum EUnitType
        {
            Worker,
            Warrior,
            Healer
        }

        [Space(15)]
        [Header("Unit Settings")]
        public EUnitType type;
        public new string name;
        public GameObject unitPrefab;
        public GameObject icon;
        public float spawnTime;

        [Space(15)]
        [Header("Unit Stats")]
        [Space(40)]

        public UnitStatTypes.Base baseStats;
    }
}

