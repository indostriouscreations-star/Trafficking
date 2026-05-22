using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrafficCheckZone
{
    public Transform point;
    public float radius = 2f;
}

public class CarDrive : MonoBehaviour
{
    [Header("Ruch")]
    public float speed = 5f;
    public float rotationSpeed = 5f;

    [Header("Warstwa aut")]
    public LayerMask carLayer;

    [Header("Raycast")]
    public float rayDistance = 3f;
    public float stopDistance = 1.5f;

    [Header("Niebieskie kółko - aktywacja")]
    public Transform triggerPoint;
    public float triggerRadius = 3f;

    [Header("Żółte kółka - sprawdzanie")]
    public List<TrafficCheckZone> checkZones =
        new List<TrafficCheckZone>();

    private Transform[] route;
    private int currentPoint = 0;

    private bool blocked = false;

    public void SetRoute(Transform[] newRoute)
    {
        route = newRoute;
    }

    void Update()
    {
        if (route == null || route.Length == 0)
            return;

        blocked = false;

        //--------------------------------
        // CZY AUTO JEST W NIEBIESKIM KÓŁKU
        //--------------------------------

        bool insideTrigger = false;

        if (triggerPoint != null)
        {
            float distToTrigger =
                Vector3.Distance(
                    transform.position,
                    triggerPoint.position
                );

            insideTrigger = distToTrigger <= triggerRadius;
        }

        //--------------------------------
        // JEŚLI TAK -> SPRAWDZAJ ŻÓŁTE
        //--------------------------------

        if (insideTrigger)
        {
            foreach (var zone in checkZones)
            {
                if (zone.point == null)
                    continue;

                Collider[] hits = Physics.OverlapSphere(
                    zone.point.position,
                    zone.radius,
                    carLayer
                );

                foreach (var col in hits)
                {
                    // ignoruj siebie
                    if (col.transform != transform)
                    {
                        blocked = true;
                        break;
                    }
                }

                if (blocked)
                    break;
            }
        }

        //--------------------------------
        // RAYCAST PRZÓD
        //--------------------------------

        Ray ray = new Ray(
            transform.position + Vector3.up * 0.5f,
            transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit rayHit, rayDistance, carLayer))
        {
            if (rayHit.transform != transform)
            {
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
        // RUCH
        //--------------------------------

        Transform targetPoint = route[currentPoint];

        Vector3 direction =
            (targetPoint.position - transform.position).normalized;

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

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint.position,
            speed * Time.deltaTime
        );

        //--------------------------------
        // NASTĘPNY PUNKT
        //--------------------------------

        float distance =
            Vector3.Distance(
                transform.position,
                targetPoint.position
            );

        if (distance < 0.2f)
        {
            currentPoint++;

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
        // NIEBIESKIE
        if (triggerPoint != null)
        {
            Gizmos.color = Color.cyan;

            Gizmos.DrawWireSphere(
                triggerPoint.position,
                triggerRadius
            );
        }

        // ŻÓŁTE
        Gizmos.color = Color.yellow;

        foreach (var zone in checkZones)
        {
            if (zone.point == null)
                continue;

            Gizmos.DrawWireSphere(
                zone.point.position,
                zone.radius
            );
        }

        // RAYCAST
        Gizmos.color = Color.red;

        Vector3 start =
            transform.position + Vector3.up * 0.5f;

        Gizmos.DrawLine(
            start,
            start + transform.forward * rayDistance
        );
    }
}