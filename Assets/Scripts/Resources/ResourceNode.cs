using DRC.RTS.Interactables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;

namespace DRC.RTS.Resources
{
    public class ResourceNode : MonoBehaviour
    {
        public GameResources.EResourceType resourceType;
        public int resourceAmount;
        private int resourceAmountMax;
        private Transform resourceNodeTransform;
        //events
        public UnityEvent onQuantityChange; private
        const RegenerationMethod regenerationMethod = RegenerationMethod.None;

        private enum RegenerationMethod
        {
            Constant,
            TimedFull,
            None
        }

        public Vector3 GetPosition()
        {
            return resourceNodeTransform.position;
        }

        public ResourceNode(Transform resourceNodeTransform, GameResources.EResourceType resourceType)
        {
            this.resourceNodeTransform = resourceNodeTransform;
            this.resourceType = resourceType;
            resourceAmountMax = 3;
            resourceAmount = resourceAmountMax;
        }

        public Resources.GameResources.EResourceType GrabResource()
        {
            if(resourceAmount > 0) resourceAmount -= 1;
            if (resourceAmount == 0)
            {
                // Node is depleted
                //UpdateSprite();
                switch (regenerationMethod)
                {
                    case RegenerationMethod.Constant:
                        // Crear funci�n de Regeneraci�n de Nodo
                        break;
                    case RegenerationMethod.TimedFull:
                        StartCoroutine(Regenerate());
                        break;
                    case RegenerationMethod.None:
                        // No se regenera
                        break;
                }
            }
            return resourceType;
        }

        public int QuantityRemaining()
        {
            return resourceAmount;
        }

        IEnumerator Regenerate()
        {
            yield return new WaitForSeconds(resourceAmount);
            resourceAmount = resourceAmountMax;
        }
        /*
        public void GatherResource(int amount, IUnit gatheringUnit, out int gatheredResources)
        {
            resourceAmount -= amount;
            print(resourceAmount);

            int amountToGive = amount;

            if(resourceAmount < 0)
            {
                amountToGive = amount + resourceAmount;
            }
            gatheredResources = amountToGive;
            if(resourceAmount <= 0)
            {
                Destroy(gameObject);
            }
            if(onQuantityChange != null)
            {
                onQuantityChange.Invoke();
            }
        }*/
        public void Gather()
        {
            print("TO DO: Actualizar el display de recursos para que el jugador sepa cu�ntos recursos quedan en el nodo. Adem�s se puede plantear un cambio de sprite para que visualmente se vea c�mo reduce la cantidad de recurso");
        }
    }
}