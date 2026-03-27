using UnityEngine;
using UnityEngine.Rendering;

public class DayNightCycle : MonoBehaviour
{
    [Header("Lights")]
    public Light sun;
    public Light moon;

    [Header("Timing")]
    [Range(0.1f, 10f)]
    public float dayLengthInMinutes = 1.0f;

    [Header("Sun Settings")]
    public float sunMaxIntensity = 1.75f;
    public Color sunColor = new Color(1f, 0.95f, 0.8f);

    [Header("Moon Settings")]
    public float moonMaxIntensity = 0.2f;
    public Color moonColor = new Color(0.5f, 0.6f, 0.8f);

    [Header("Ambient Light")]
    public Color dayAmbient = new Color(0.5f, 0.5f, 0.5f);
    public Color nightAmbient = new Color(0.02f, 0.02f, 0.06f);

    [Header("Skybox")]
    public Material skyboxMaterial;

    // Day/night tint colors
    private Color daytimeTint = new Color(1f, 1f, 1f);
    private Color nightTint = new Color(0.05f, 0.08f, 0.18f);

    void Update()
    {
        float rotationSpeed = 360f / (dayLengthInMinutes * 60f);
        sun.transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);
        moon.transform.rotation = sun.transform.rotation * Quaternion.Euler(180, 0, 0);

        UpdateLighting();
    }

    void UpdateLighting()
    {
        float sunAngle = Vector3.Dot(sun.transform.forward, Vector3.down);
        
        // Wider band = smoother transition. Was (sunAngle + 0.1f) / 0.3f
        float dayFactor = Mathf.Clamp01((sunAngle + 0.3f) / 0.8f);
        
        // Smooth it further with a curve so it eases in and out
        dayFactor = Mathf.SmoothStep(0f, 1f, dayFactor);

        // Sun & moon intensity and color
        sun.intensity = Mathf.Lerp(0f, sunMaxIntensity, dayFactor);
        sun.color = sunColor;
        moon.intensity = Mathf.Lerp(0f, moonMaxIntensity, 1f - dayFactor);
        moon.color = moonColor;

        // Ambient light
        RenderSettings.ambientLight = Color.Lerp(nightAmbient, dayAmbient, dayFactor);

        // Fog
        RenderSettings.fogColor = Color.Lerp(
            new Color(0.01f, 0.01f, 0.05f),
            new Color(0.7f, 0.85f, 1f),
            dayFactor
        );

        // Skybox
        if (skyboxMaterial != null)
        {
            skyboxMaterial.SetColor("_Tint", Color.Lerp(nightTint, daytimeTint, dayFactor));
            skyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(0.08f, 1f, dayFactor)); // 0.08 = dim but visible night sky
            DynamicGI.UpdateEnvironment();
        }
    }
}