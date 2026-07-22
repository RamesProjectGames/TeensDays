using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Item : MonoBehaviour
{
    public enum DetectionShape
    {
        Box,
        Sphere
    }

    [Header("Detection")]
    [SerializeField] private DetectionShape shape = DetectionShape.Box;
    [SerializeField] private Vector3 boxSize = Vector3.one;
    [SerializeField] private float sphereRadius = 1f;
    [SerializeField] private Vector3 offset;

    [SerializeField] private LayerMask layerMask = ~0;
    [SerializeField] private bool detectTriggers = true;

    [Header("Filter")]
    [SerializeField] private string requiredTag = "";
    public string textBubble = "";

    [Header("Events")]
    public UnityEvent<GameObject> OnEnter;
    public UnityEvent<GameObject> OnStay;
    public UnityEvent<GameObject> OnExit;
    public UnityEvent onInteract;

    private readonly HashSet<Collider> currentColliders = new();
    private readonly HashSet<Collider> previousColliders = new();

    private readonly Collider[] results = new Collider[32];

    private void FixedUpdate()
    {
        previousColliders.Clear();

        foreach (var c in currentColliders)
            previousColliders.Add(c);

        currentColliders.Clear();

        Query();

        // Enter & Stay
        foreach (var collider in currentColliders)
        {
            if (!previousColliders.Contains(collider))
                OnEnter?.Invoke(collider.gameObject);
            else
                OnStay?.Invoke(collider.gameObject);
        }

        // Exit
        foreach (var collider in previousColliders)
        {
            if (!currentColliders.Contains(collider))
                OnExit?.Invoke(collider.gameObject);
        }
    }

    private void Query()
    {
        QueryTriggerInteraction triggerMode =
            detectTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;

        int count = 0;

        switch (shape)
        {
            case DetectionShape.Box:
                count = Physics.OverlapBoxNonAlloc(
                    transform.position + offset,
                    boxSize * 0.5f,
                    results,
                    transform.rotation,
                    layerMask,
                    triggerMode);
                break;

            case DetectionShape.Sphere:
                count = Physics.OverlapSphereNonAlloc(
                    transform.position + offset,
                    sphereRadius,
                    results,
                    layerMask,
                    triggerMode);
                break;
        }

        for (int i = 0; i < count; i++)
        {
            Collider col = results[i];

            if (!string.IsNullOrEmpty(requiredTag) &&
                !col.CompareTag(requiredTag))
                continue;

            currentColliders.Add(col);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        switch (shape)
        {
            case DetectionShape.Box:
                Matrix4x4 old = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(transform.position + offset, transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, boxSize);
                Gizmos.matrix = old;
                break;

            case DetectionShape.Sphere:
                Gizmos.DrawWireSphere(transform.position + offset, sphereRadius);
                break;
        }
    }
#endif
}
