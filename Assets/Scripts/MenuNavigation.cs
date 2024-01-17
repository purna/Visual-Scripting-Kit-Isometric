using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;


public class MenuNavigation : MonoBehaviour
    {
        public Selectable DefaultSelection;
        public bool ForceSelection = false;

        void Start()
        {
        Cursor.visible = true;
        Screen.lockCursor = false;
        EventSystem.current.SetSelectedGameObject(null);
        }

        void LateUpdate()
        {
            if (EventSystem.current.currentSelectedGameObject == null || ForceSelection)
            {
                EventSystem.current.SetSelectedGameObject(DefaultSelection.gameObject);
            }
        }

        void OnDisable()
        {
            if (ForceSelection && EventSystem.current.currentSelectedGameObject == DefaultSelection.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

