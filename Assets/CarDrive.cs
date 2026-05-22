using UnityEngine;

public class CarDrive : MonoBehaviour
{
    [Header("Ruch")]
    public float speed = 5f;
    public float rotationSpeed = 5f;

    private Transform[] route;
    private int currentPoint = 0;

    public void SetRoute(Transform[] newRoute)
    {
        route = newRoute;
    }

    void Update()
    {
        // Brak trasy
        if (route == null || route.Length == 0)
            return;

        // Aktualny punkt
        Transform targetPoint = route[currentPoint];

        // Kierunek
        Vector3 direction = (targetPoint.position - transform.position).normalized;

        // Obrót auta
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Ruch
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint.position,
            speed * Time.deltaTime
        );

        // Czy dojechał
        float distance = Vector3.Distance(transform.position, targetPoint.position);

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
}