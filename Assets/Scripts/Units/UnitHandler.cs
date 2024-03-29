using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using DRC.RTS.Player;

namespace DRC.RTS.Units
{    public class UnitHandler : MonoBehaviour
    {
        public static UnitHandler instance;
        [SerializeField] private UnitData worker, warrior, healer;

        public LayerMask pUnitLayer, eUnitLayer;

        private void Awake()
        {
            instance = this;
        }

        public UnitStatTypes.Base GetBasicUnitStats(string type)
        {
            UnitData unit;
            switch (type)
            {
                case "worker":
                    unit = worker;
                    break;
                case "warrior":
                    unit = warrior;
                    break;
                case "healer":
                    unit = healer;
                    break;
                default:
                    Debug.Log($"Unit Type: {type} could not be found or does not exist");
                    return null;
            }
            return unit.baseStats;
        }
    }

}
