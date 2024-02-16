using DRC.RTS.Buildings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DRC.RTS.UI.HUD
{
    public class Action : MonoBehaviour
    {
        public enum EActionType
        {
            InstantiateUnit,
            InstantiatBuilding,
            Improvement,
            InvestigateTechnology
        }
        public EActionType type;
        public void OnClick()
        {
            switch (type)
            {
                case EActionType.InstantiateUnit:
                    ActionFrame.instance.StartSpawnTimer(name);
                    break;
                case EActionType.InstantiatBuilding:
                    ActionFrame.instance.InstantiateBuildingConstruction(name);
                    break;
                case EActionType.Improvement:
                    break;
                case EActionType.InvestigateTechnology: 
                    break;
            }
        }
    }
}

