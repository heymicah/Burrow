using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Transform sun;
    public Transform moon;
    [Range(0.1f, 10f)]
    public float dayLengthInMinutes = 1.0f;

    void Update()
    {
        // Calculate the rotation speed
        float rotationSpeed = 360f / (dayLengthInMinutes * 60f);
        
        // Rotate the Sun around its local X axis
        sun.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);

        // Keep the Moon exactly opposite the Sun
        // We set the Moon's rotation to the Sun's rotation + 180 degrees
        moon.rotation = sun.rotation * Quaternion.Euler(180, 0, 0);
    }
}