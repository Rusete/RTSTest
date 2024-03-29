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
                    if (navAgent.remainingDistance <= navAgent.stoppingDistance && navAgent.velocity.magnitude == 0)
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
            if (target.Count() > 0)
            {
                var building = target[0].GetComponent<IBuilding>();
                if (building)
                {
                    building.RemoveFromConstrcutionWorkingQueue(transform);
                }
                target.Clear();
            }
            navAgent.SetDestination(destination);
            action = unitAction;
        }

        public void MoveUnit(Transform newTarget, EUnitAction unitAction = EUnitAction.Move, bool multiTarget = false)
        {
            if (!multiTarget)
            {
                if(target.Count() > 0)
                {
                    var building = target[0].GetComponent<IBuilding>();
                    if (building)
                    {
                        building.RemoveFromConstrcutionWorkingQueue(transform);
                    }
                    target.Clear();
                }
            }
            target.Add(newTarget);
            action = unitAction;
            if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();

            Vector3 closestPointToTarget = target[0].position + (transform.position - target[0].position).normalized * navAgent.stoppingDistance;
            navAgent.SetDestination(closestPointToTarget);
        }

        public override void OnInteractEnter()
        {
            if(actions)
                UI.HUD.ActionFrame.instance.SetActionButtons(actions);
            base.OnInteractEnter();
        }
    }
}
