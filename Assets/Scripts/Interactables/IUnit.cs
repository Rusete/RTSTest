using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DRC.RTS.Units;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.AI;
using System.Linq;

namespace DRC.RTS.Interactables
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class IUnit : Interactable
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
        protected List<Vector3> target = new();
        private List<System.Action> onArrivedAtPosition = new();
        private State state;


        private void OnEnable()
        {
            navAgent = GetComponent<NavMeshAgent>();
        }
        protected void Start()
        {
            navAgent.stoppingDistance = navAgent.radius * 4;
            print(navAgent.stoppingDistance);
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
            if (!multiTarget)
            {
                if (target.Count() > 0)
                {
                    Collider[] hitColliders = Physics.OverlapSphere(transform.position, navAgent.stoppingDistance + .5f);
                    foreach (var hitCollider in hitColliders)
                    {
                        var building = hitCollider.GetComponent<IBuilding>();
                        if (building)
                        {
                            building.RemoveFromConstrcutionWorkingQueue(transform);
                        }
                    }
                }
                target.Clear();
                this.onArrivedAtPosition.Clear();
                target.Add(position);
                this.onArrivedAtPosition.Add(onArrivedAtPosition);
                Vector3 closestPointToTarget = target[0] + (transform.position - target[0]).normalized * stopDistance;
                navAgent.SetDestination(closestPointToTarget);
                state = State.Moving;
            }
            else
            {
                target.Add(position);
                this.onArrivedAtPosition.Add(onArrivedAtPosition);
            }
        }

        public void MoveToNextTarget()
        {
            target.RemoveAt(0);
            onArrivedAtPosition.RemoveAt(0);
            if (target.Count > 0)
            {
                Vector3 closestPointToTarget = target[0] + (transform.position - target[0]).normalized * navAgent.stoppingDistance;
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
            if (Vector3.Distance(transform.position, navAgent.destination) > navAgent.stoppingDistance)
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
                if (onArrivedAtPosition.Count() > 0)
                {
                    System.Action tmpAction = onArrivedAtPosition[0];
                    state = State.Animating;
                    tmpAction();
                }
            }
        }
    }
}
