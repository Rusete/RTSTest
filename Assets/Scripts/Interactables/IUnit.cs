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

        private void OnEnable()
        {
            navAgent = GetComponent<NavMeshAgent>();
        }
        private void Start()
        {
            navAgent.stoppingDistance = navAgent.radius * 2;
        }
        public void MoveToNextTarget()
        {
            target.RemoveAt(0);
            if (target.Count > 0)
            {
                if (!target[0]) MoveToNextTarget();
                else
                {
                    if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();

                    Vector3 closestPointToTarget = target[0].position + (transform.position - target[0].position).normalized * navAgent.stoppingDistance;
                    navAgent.SetDestination(closestPointToTarget);
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
