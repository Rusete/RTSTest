using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DRC.RTS.Units;
using UnityEngine.AI;
using System.Linq;
using Unity.VisualScripting;

namespace DRC.RTS.Interactables
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class IUnit : SelectableBehaviour
    {
        private enum State
        {
            Idle,
            Moving,
            Animating,
        }

        protected NavMeshAgent navAgent;
        public UnitStatTypes.Base baseStats;
        public UnitData unitType;

        protected List<PositionData> targets = new();
        private State state;
        private int resources = 0;

        private void OnEnable()
        {
            navAgent = GetComponent<NavMeshAgent>();
        }
        protected new void Start()
        {
            navAgent.stoppingDistance = navAgent.radius * 4;
            base.Start();
        }

        protected void Update()
        {
            switch (state)
            {
                case State.Idle:
                    //Handle Animation
                    break;
                case State.Moving:
                    HandleMovement();
                    break;
                case State.Animating:
                    break;
            }
        }

        public void MoveTo(Vector3 position, float stopDistance, System.Action onArrivedAtPosition, bool multiTarget = false)
        {
            var stopD = stopDistance == 0 ? stopDistance : navAgent.stoppingDistance;
            if (!multiTarget)
            {
                if (targets.Count() > 0)
                {
                    // Eliminarse de la cola de construcción
                    switch (unitType.type)
                    {
                        case UnitData.EUnitType.Worker:
                            Collider[] hitColliders = Physics.OverlapSphere(transform.position, navAgent.stoppingDistance + .5f);
                            foreach (var hitCollider in hitColliders)
                            {
                                var building = hitCollider.GetComponent<IBuilding>();
                                if (building)
                                {
                                    building.RemoveFromConstrcutionWorkingQueue(transform);
                                }
                            }
                        break;
                    }
                }
                targets.Clear();
                print(position);
                targets.Add(new PositionData { position = position, stopDistance = stopD, action = onArrivedAtPosition });

                navAgent.SetDestination(targets[0].position);
                state = State.Moving;
            }
            else
            {

                targets.Add(new PositionData { position = position, stopDistance = stopD, action = onArrivedAtPosition });
                if (targets.Count() == 1)
                {
                    navAgent.SetDestination(targets[0].position);
                    state = State.Moving;
                }
            }
        }

        public void MoveToNextTarget()
        {
            targets.RemoveAt(0);
            if (targets.Count > 0)
            {
                Vector3 closestPointToTarget = targets[0].position + (transform.position - targets[0].position).normalized * targets[0].stopDistance;
                navAgent.SetDestination(closestPointToTarget);
                state = State.Moving;
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

        private void HandleMovement()
        {
            if (Vector3.Distance(transform.position, navAgent.destination) > targets[0].stopDistance)
            {
                // Vector3 moveDir = (targetPosition - transform.position).normalized;

                // float distanceBefore = Vector3.Distance(transform.position, target[0].position);
                // animatedWalker.SetMoveVector(moveDir);
                //handle sprite
            }
            else if (navAgent.velocity.magnitude == 0)
            {
                // Arrived
                //animatedWalker.SetMoveVector(Vector3.zero);
                if (targets.Count() > 0 && targets[0].action != null)
                {
                    System.Action tmpAction = targets[0].action;
                    state = State.Animating;
                    tmpAction();
                }
            }
        }

        public IEnumerator Gather(ResourceNode node)
        {
            if (unitType.type == UnitData.EUnitType.Worker)
            {
                do
                {
                    var amount = baseStats.gatherQuantity;
                    if (amount > baseStats.gatheringCapacity - resources) amount = baseStats.gatheringCapacity - resources;
                    node.GatherResource(amount, this, out int gatheredResources);
                    resources += gatheredResources;
                    yield return new WaitForSeconds(baseStats.gatherCD);
                } while (node && resources < baseStats.gatheringCapacity);
            }
            yield return null;
        }

        public struct PositionData
        {
            public Vector3 position;
            public float stopDistance;
            public System.Action action;
        }
    }
}
