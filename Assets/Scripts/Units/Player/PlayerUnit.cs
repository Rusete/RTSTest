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
        private void OnEnable()
        {
            navAgent = GetComponent<NavMeshAgent>();
        }
        private void Start()
        {
            baseStats = unitType.baseStats;
            Units.UnitHandler.instance.GetBasicUnitStats(name.ToLower());
            navAgent.stoppingDistance = navAgent.radius + navAgent.radius / 2;
            // GetComponent<Health>().SetupHealth();
        }
        private void Update()
        {
            if (action == EUnitAction.Wait) return;
            if (action == EUnitAction.Move) return;
            switch (action)
            {
                case EUnitAction.Attack:
                    break;
                case EUnitAction.Heal:
                    break;
                case EUnitAction.Repair:
                    if(target == null)
                    {
                        Debug.LogWarning(name + " does not have a target.");

                        return;
                    }
                    if (navAgent.remainingDistance <= navAgent.stoppingDistance)
                    {
                        IBuilding build;
                        if (build = target[0].GetComponent<IBuilding>())
                        {
                            build.AddToConstructionWorkingQueue(transform);
                        }
                        else
                        {
                            Debug.Log(target[0].name + " does not have IBuilding component");
                        }
                    }
                    break;
            }
        }

        public void MoveUnit(Vector3 destination, EUnitAction unitAction = EUnitAction.Move)
        {
            if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();
            if(action ==  EUnitAction.Repair)
            {
                target[0].GetComponent<IBuilding>().RemoveFromConstrcutionWorkingQueue(transform);
            }
            navAgent.SetDestination(destination);
            action = unitAction;
        }

        public void MoveUnit(Transform newTarget, EUnitAction unitAction = EUnitAction.Move, bool multiTarget = false)
        {
            if(!multiTarget)
                target.Clear();
            target.Add(newTarget);
            action = unitAction;
            if (navAgent == null) navAgent=GetComponent<NavMeshAgent>();
            if (action == EUnitAction.Move)
            {
                navAgent.SetDestination(target[0].position);
            }
            else
            {
                navAgent.SetDestination(target[0].GetComponent<Collider>().ClosestPoint(transform.position));
            }
        }

        public override void OnInteractEnter()
        {
            if(actions)
                UI.HUD.ActionFrame.instance.SetActionButtons(actions);
            base.OnInteractEnter();
        }
    }
}
