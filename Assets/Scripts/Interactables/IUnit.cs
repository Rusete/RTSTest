using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DRC.RTS.Units;

namespace DRC.RTS.Interactables
{
    public class IUnit : Interactable
    {
        public UnitStatTypes.Base baseStats;
        public BasicUnit unitType;

        private void Start()
        {
            baseStats = unitType.baseStats;
            Units.UnitHandler.instance.GetBasicUnitStats(name.ToLower());
            GetComponent<Units.Health>().SetupHealth();
        }
        public override void OnInteractEnter()
        {
            base.OnInteractEnter();
        }

        public override void OnInteractExit()
        {
            base.OnInteractExit();
        }
    }
}
