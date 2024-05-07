using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DRC.RTS.Interactables;
using System.Linq;

namespace DRC.RTS.Units.Player
{
    public class PlayerUnit : IUnit
    {
        protected new void Start()
        {
            baseStats = unitType.baseStats;
            Units.UnitHandler.instance.GetBasicUnitStats(name.ToLower());
            base.Start();
            // navAgent.stoppingDistance = navAgent.radius + navAgent.radius / 2;
            // GetComponent<Health>().SetupHealth();
        }

        public override void OnInteractEnter()
        {
            if(actions)
                UI.HUD.ActionFrame.instance.SetActionButtons(actions);
            base.OnInteractEnter();
        }
    }
}
