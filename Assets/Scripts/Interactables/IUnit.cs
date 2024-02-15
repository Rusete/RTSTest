using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DRC.RTS.Units;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.AI;

namespace DRC.RTS.Interactables
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class IUnit : Interactable
    {
        protected NavMeshAgent navAgent;
        public UnitStatTypes.Base baseStats;
        public UnitData unitType;
        protected List<Transform> target = new List<Transform>();
        public enum EUnitAction
        {
            Attack,
            Heal,
            Repair,
            Move,
            Wait
        }
        public EUnitAction action = EUnitAction.Wait;

        public void MoveToNextTarget()
        {
            target.RemoveAt(0);
            if (target.Count > 0)
            {
                if (action == EUnitAction.Move)
                {
                    navAgent.SetDestination(target[0].position);
                }
                else
                {
                    navAgent.SetDestination(target[0].GetComponent<Collider>().ClosestPoint(transform.position));
                }
            }
            else
            {
                action = EUnitAction.Wait;
            }
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
