using UnityEngine;
using System.Collections;

public class TempleCameraIntro : MonoBehaviour
{
    [Header("References")]
    public Transform introPoint;          // Startpunkt oben beim Skelett/Enemy
    public Transform player;              // Persistenter Player
    public CameraFollow cameraFollow;

    [Header("Intro Timing")]
    public float waitAtIntroPoint = 1.5f;
    public float panDuration = 2.5f;

    private bool introRunning = false;

    private void Start()
    {
        // Wenn die Cutscene läuft, macht diese das Intro
        if (QuestManager.Instance != null && !QuestManager.Instance.visitedTemple)
        {
            gameObject.SetActive(false);
            return;
        }
        
        StartCoroutine(BeginIntro());
    }

    private IEnumerator BeginIntro()
    {
        // Doppelstart verhindern
        if (introRunning)
            yield break;

        introRunning = true;

        // CameraFollow automatisch holen
        if (cameraFollow == null)
        {
            cameraFollow = GetComponent<CameraFollow>();
        }

        // Player automatisch holen
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        // Sicherheitschecks
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

        // =========================
        // MOVEMENT LOCK
        // =========================
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.canMove = false;
        }

        // =========================
        // CAMERA FOLLOW AUS
        // =========================
        if (cameraFollow != null)
        {
            cameraFollow.enabled = false;
        }

        // =========================
        // STARTPOSITION BEIM ENEMY
        // =========================
        transform.position = new Vector3(
            introPoint.position.x,
            introPoint.position.y,
            transform.position.z
        );

        // Kurz warten
        yield return new WaitForSeconds(waitAtIntroPoint);

        // =========================
        // STARTPOSITION
        // =========================
        Vector3 startPosition = transform.position;

        // =========================
        // NUR Y ACHSE BEWEGEN
        // =========================
        Camera cam = GetComponent<Camera>();

        float camHalfHeight = cam.orthographicSize;

        // X bleibt fix auf IntroPoint
        float fixedX = introPoint.position.x;

        // Y bewegt sich zum Player innerhalb Bounds
        float clampedY = Mathf.Clamp(
            player.position.y,
            cameraFollow.minY + camHalfHeight,
            cameraFollow.maxY - camHalfHeight
        );

        Vector3 targetPosition = new Vector3(
            fixedX,
            clampedY,
            transform.position.z
        );

        // =========================
        // SANFTER VERTIKALER SWIPE
        // =========================
        float elapsedTime = 0f;

        while (elapsedTime < panDuration)
        {
            if (player == null)
            {
                Debug.LogError("Player verloren!");
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

        // =========================
        // CAMERA FOLLOW WIEDER AN
        // =========================
        if (cameraFollow != null)
        {
            // Player wieder zuweisen
            cameraFollow.player = player;

            cameraFollow.enabled = true;
        }

        // =========================
        // MOVEMENT UNLOCK
        // =========================
        if (pm != null)
        {
            pm.canMove = true;
        }

        introRunning = false;
        }
}