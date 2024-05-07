using DRC.RTS.Player;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DRC.RTS.UI
{
    public class UIText : MonoBehaviour
    {
        private TMP_Text container;
        public Resources.GameResources.EResourceType type;
        public uint resources;

        private void OnDisable()
        {
            // Suscribirse al evento del GatheringBag
            // Esto debería hacerse después de que el GatheringBag haya sido creado y configurado
            var gatheringBag = PlayerManager.Instance.storage;
            gatheringBag.OnResourceUpdated -= OnResourceUpdated;
        }

        private void Start()
        {
            container = GetComponent<TMP_Text>();
            container.text = "0";

            // Suscribirse al evento del GatheringBag
            // Esto debería hacerse después de que el GatheringBag haya sido creado y configurado
            var gatheringBag = PlayerManager.Instance.storage;
            gatheringBag.OnResourceUpdated += OnResourceUpdated;
        }

        // Método que se llama cuando se actualiza un recurso
        private void OnResourceUpdated(Resources.GameResources.EResourceType updatedResourceType, int newValue, string valueChange)
        {
            // Verificar si el tipo de recurso actual coincide con el tipo de recurso actualizado
            if (updatedResourceType == type)
            {
                container.text = newValue.ToString();
            }

            //TODO: Sacar un numerito que pinte valueChange a modo de que visualmente se vean los cambios de recursos
        }
    }
}

