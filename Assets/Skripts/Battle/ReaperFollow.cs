using UnityEngine;

public class ReaperFollow : MonoBehaviour
{
    public float detectionRange = 1.8f; // Even smaller range for "close quarters"
    public float moveSpeed = 1.2f; // Slightly slower for better control
public float floatAmplitude = 0.3f;
    public float floatFrequency = 1.2f;

    private Transform player;
    private Vector3 startPos;
    private float floatOffset;

    void Start()
    {
        startPos = transform.position;
        floatOffset = Random.Range(0f, 2f * Mathf.PI);
        FindPlayer();
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
        }

        float distance = Vector2.Distance(transform.position, player.position);
        
        // Floating animation (bobbing)
        float verticalOffset = Mathf.Sin(Time.time * floatFrequency + floatOffset) * floatAmplitude;
        
        if (distance <= detectionRange)
        {
            // Chase logic
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            
            // Apply floating to the current position
            float verticalOffsetMod = Mathf.Sin(Time.time * floatFrequency + floatOffset) * floatAmplitude * 0.01f;
            transform.position = new Vector3(transform.position.x, transform.position.y + verticalOffsetMod, transform.position.z);
        }
        else
        {
            // Just float at current position
            float verticalOffsetMod = Mathf.Sin(Time.time * floatFrequency + floatOffset) * floatAmplitude * 0.005f;
            transform.position = new Vector3(transform.position.x, transform.position.y + verticalOffsetMod, transform.position.z);
        }
    }

    private void FindPlayer()
    {
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            player = GameManager.Instance.player.transform;
        }
        else
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }
}