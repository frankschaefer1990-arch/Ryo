using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TempleCameraIntro : MonoBehaviour
{
    [Header("References")]
    public Transform introPoint;
    public Transform player;
    public CameraFollow cameraFollow;

    [Header("Intro Timing")]
    public float waitAtIntroPoint = 1.5f;
    public float panDuration = 2.5f;

    [Header("Camera Offset")]
    public float playerYOffset = 2f;

    private bool introRunning = false;

    private void Start()
    {
        StartCoroutine(BeginIntro());
    }

    private IEnumerator BeginIntro()
    {
        // Schutz vor Doppelstart
        if (introRunning)
            yield break;

        introRunning = true;

        // WICHTIG:
        // Player nach Szenenwechsel neu suchen
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        // Sicherheitscheck
        if (introPoint == null)
        {
            Debug.LogError("TempleIntroPoint fehlt!");
            yield break;
        }

        if (player == null)
        {
            Debug.LogError("Player nicht gefunden!");
            yield break;
        }

        // CameraFollow neu suchen falls nötig
        if (cameraFollow == null)
        {
            cameraFollow = GetComponent<CameraFollow>();
        }

        // CameraFollow deaktivieren
        if (cameraFollow != null)
        {
            cameraFollow.enabled = false;
        }

        // Kamera startet beim Intro Point
        transform.position = new Vector3(
            introPoint.position.x,
            introPoint.position.y,
            transform.position.z
        );

        // Kurz warten
        yield return new WaitForSeconds(waitAtIntroPoint);

        // Falls Player währenddessen zerstört wurde
        if (player == null)
        {
            Debug.LogError("Player wurde während Intro zerstört!");
            yield break;
        }

        Vector3 startPosition = transform.position;

        Vector3 targetPosition = new Vector3(
            player.position.x,
            player.position.y + playerYOffset,
            transform.position.z
        );

        float elapsedTime = 0f;

        // Langsames Gleiten
        while (elapsedTime < panDuration)
        {
            // Extra Schutz:
            if (player == null)
            {
                Debug.LogError("Player Referenz verloren!");
                yield break;
            }

            transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                elapsedTime / panDuration
            );

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Exakte Endposition
        transform.position = targetPosition;

        // CameraFollow wieder aktivieren
        if (cameraFollow != null)
        {
            cameraFollow.enabled = true;
        }

        introRunning = false;
    }
}