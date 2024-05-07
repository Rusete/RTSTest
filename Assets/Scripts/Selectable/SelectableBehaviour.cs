using DRC.RTS.Player;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DRC.RTS.Interactables
{
    public class SelectableBehaviour : MonoBehaviour
    {
        public bool isInteracting = false;
        public GameObject highlight = null;
        public GameObject hpBar = null;
        public UI.HUD.PlayerActions actions;
        Health health;
        protected void Start()
        {
            InputManager.InputHandler.Instance.SelectionBoxEnded.AddListener(HandleLeftClickEnds);
            health = GetComponent<Health>();
        }
        private void OnDisable()
        {
            InputManager.InputHandler.Instance.SelectionBoxEnded.RemoveListener(HandleLeftClickEnds);
        }

        private void HandleLeftClickEnds()
        {
            if (InputManager.InputHandler.Instance.IsWithinSelectionBounds(transform))
            {
                OnInteractEnter();
                PlayerManager.Instance.AddToSelection(transform);
            }
            else if (isInteracting && !InputManager.InputHandler.Instance.Multi)
            {
                OnInteractExit();
            }
        }

        public virtual void Awake()
        {
            highlight.SetActive(false);
        }

        public virtual void OnInteractEnter()
        {
            isInteracting = true;
            ShowHighlight();
            ShowHpbar();
        }
        public virtual void OnInteractExit()
        {
            isInteracting = false;
            HideHighlight();
            HideHpBar();
            health.Killed.RemoveAllListeners();
        }
        public virtual void ShowHighlight()
        {
            highlight.SetActive(true);
        }
        public virtual void HideHighlight()
        {
            if (isInteracting) return;
            highlight.SetActive(false);
        }
        public virtual void ShowHpbar()
        {
            hpBar.SetActive(true);
        }
        public virtual void HideHpBar()
        {
            if (isInteracting) return;
            hpBar.SetActive(false);
        }
    }
}
