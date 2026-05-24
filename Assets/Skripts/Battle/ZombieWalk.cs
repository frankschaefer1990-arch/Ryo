using UnityEngine;

public class ZombieWalk : MonoBehaviour
{
    public float walkSpeed = 1.0f;
    public float stepFrequency = 2.0f;
    public float tiltAmount = 5.0f;
    public float verticalBob = 0.05f;

    private Transform visualChild;
    private float timer;

    void Start()
    {
        // Similar to ReaperFollow, we find or create a VisualRoot
        Transform existing = transform.Find("VisualRoot");
        if (existing != null)
        {
            visualChild = existing;
        }
        else
        {
            // If no VisualRoot, we might be the sprite renderer ourselves
            // But usually we want a child to bob independently of the collider
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null && sr.transform == transform)
            {
                GameObject root = new GameObject("VisualRoot");
                root.transform.SetParent(transform);
                root.transform.localPosition = Vector3.zero;
                
                SpriteRenderer newSr = root.AddComponent<SpriteRenderer>();
                newSr.sprite = sr.sprite;
                newSr.color = sr.color;
                newSr.sortingOrder = sr.sortingOrder;
                
                Destroy(sr);
                visualChild = root.transform;
            }
        }
    }

    void Update()
    {
        if (visualChild == null) return;

        timer += Time.deltaTime * stepFrequency;
        
        // Asymmetric wobble for a "limp"
        float tilt = Mathf.Sin(timer) * tiltAmount;
        float bob = Mathf.Abs(Mathf.Cos(timer)) * verticalBob;
        
        visualChild.localRotation = Quaternion.Euler(0, 0, tilt);
        visualChild.localPosition = new Vector3(0, bob, 0);
    }
}
