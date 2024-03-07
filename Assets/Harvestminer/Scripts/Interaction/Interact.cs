using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact : MonoBehaviour
{
    public float range = 100f;
    public Transform interactionUI;

    void Update()
    {
        bool isHovered = false;

        Ray r = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(r, out RaycastHit hit, range))
        {
            if (hit.collider.gameObject.TryGetComponent(out IInteractable interactable))
            {
                isHovered = true;

                if (Input.GetKeyDown(KeyCode.E))
                    interactable.Interact();

                if (!interactionUI.gameObject.activeSelf)
                {
                    interactionUI.transform.position = hit.transform.position;
                    interactionUI.transform.parent = hit.transform;
                }
            }
        }

        interactionUI.gameObject.SetActive(isHovered);
    }
}
