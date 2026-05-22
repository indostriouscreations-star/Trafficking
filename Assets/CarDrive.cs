using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrafficCheckPoint
{
    public Transform point;
    public float radius = 2f;
}

public class CarDrive : MonoBehaviour
{
    [Header("Ruch")]
    public float speed = 5f;
    public float rotationSpeed = 5f;

    [Header("Sprawdzanie ruchu")]
    public List<TrafficCheckPoint> checkPoints = new List<TrafficCheckPoint>();

    [Tooltip("Warstwa samochodów")]
    public LayerMask carLayer;

    [Tooltip("Odległość raycasta przed autem")]
    public float rayDistance = 3f;

    [Tooltip("Dystans zatrzymania od auta")]
    public float stopDistance = 1.5f;

    [Header("Punkt aktywacji sprawdzania")]
    public Transform checkTriggerPoint;

    [Tooltip("Odległość od triggera, przy której zaczyna sprawdzać skrzyżowanie")]
    public float triggerDistance = 3f;

    private Transform[] route;
    private int currentPoint = 0;

    private bool blocked = false;

    public void SetRoute(Transform[] newRoute)
    {
        route = newRoute;
    }

    void Update()
    {
        // Brak trasy
        if (route == null || route.Length == 0)
            return;

        blocked = false;

        //--------------------------------
        // SPRAWDZANIE STREF
        //--------------------------------

        if (checkTriggerPoint != null)
        {
            float distToTrigger =
                Vector3.Distance(transform.position, checkTriggerPoint.position);

            // Sprawdzaj dopiero blisko triggera
            if (distToTrigger <= triggerDistance)
            {
                foreach (var check in checkPoints)
                {
                    if (check.point == null)
                        continue;

                    Collider[] hits = Physics.OverlapSphere(
                        check.point.position,
                        check.radius,
                        carLayer
                    );

                    foreach (var colliderHit in hits)
                    {
                        // Ignoruj samego siebie
                        if (colliderHit.transform != transform)
                        {
                            blocked = true;
                            break;
                        }
                    }

                    if (blocked)
                        break;
                }
            }
        }

        //--------------------------------
        // RAYCAST DO PRZODU
        //--------------------------------

        Ray ray = new Ray(
            transform.position + Vector3.up * 0.5f,
            transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit rayHit, rayDistance, carLayer))
        {
            // Ignoruj samego siebie
            if (rayHit.transform != transform)
            {
                // Zatrzymaj jeśli za blisko
                if (rayHit.distance <= stopDistance)
                {
                    blocked = true;
                }
            }
        }

        //--------------------------------
        // STOP
        //--------------------------------

        if (blocked)
            return;

        //--------------------------------
        // STANDARDOWA JAZDA
        //--------------------------------

        Transform targetPoint = route[currentPoint];

        Vector3 direction =
            (targetPoint.position - transform.position).normalized;

        // Obrót auta
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Ruch auta
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint.position,
            speed * Time.deltaTime
        );

        // Sprawdzenie dojazdu do punktu
        float distance =
            Vector3.Distance(transform.position, targetPoint.position);

        if (distance < 0.2f)
        {
            currentPoint++;

            // Koniec trasy
            if (currentPoint >= route.Length)
            {
                Destroy(gameObject);
            }
        }
    }

    //--------------------------------
    // GIZMOS
    //--------------------------------

    void OnDrawGizmos()
    {
        // Punkty sprawdzania
        Gizmos.color = Color.yellow;

        foreach (var check in checkPoints)
        {
            if (check.point == null)
                continue;

            Gizmos.DrawWireSphere(
                check.point.position,
                check.radius
            );
        }

        // Trigger point
        if (checkTriggerPoint != null)
        {
            Gizmos.color = Color.cyan;

            Gizmos.DrawWireSphere(
                checkTriggerPoint.position,
                triggerDistance
            );
        }

        // Raycast
        Gizmos.color = Color.red;

        Vector3 start =
            transform.position + Vector3.up * 0.5f;

        Gizmos.DrawLine(
            start,
            start + transform.forward * rayDistance
        );
    }
}