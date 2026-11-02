using UnityEngine;

public class Billboard : MonoBehaviour
{
    private void LateUpdate()
    {
        // Ensure the billboard (Health Slider) always faces the main camera, adjusting its rotation accordingly.
        if (Camera.main == null)
            return;

        transform.rotation = Quaternion.LookRotation(
            -Camera.main.transform.forward,
            Camera.main.transform.up
        );
    }
}