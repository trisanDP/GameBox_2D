// ===== InteractableEntity.cs =====
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class InteractableEntity : MonoBehaviour {
    [Header("Movement")]
    [Tooltip("Speed in units per second")]
    public float speed = 2f;

    [Header("Visual Feedback")]
    [Tooltip("SpriteRenderer used to flash color on feed/miss")]
    public SpriteRenderer whiteSprite; // Assign in Inspector for each prefab variation

    private Rect movementBounds;
    private Vector2 direction;
    private bool isPaused = false;
    private Rigidbody2D rb;
    private bool hasBeenFed = false;

    public bool HasBeenFed => hasBeenFed;

    void Awake() {
        PickRandomDirection();
    }

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Ensure we have a SpriteRenderer reference
        if(whiteSprite == null) {
            whiteSprite = GetComponentInChildren<SpriteRenderer>();
            if(whiteSprite == null)
                Debug.LogError($"[{name}] No SpriteRenderer found for visual feedback.");
        }

        // Calculate movement bounds inset by half sprite size
        var boundary = FindFirstObjectByType<MovementBoundary>();
        if(boundary != null && whiteSprite != null) {
            Rect raw = boundary.GetWorldBounds();
            Vector2 half = whiteSprite.bounds.extents;
            movementBounds = new Rect(
                raw.xMin + half.x,
                raw.yMin + half.y,
                raw.width - half.x * 2f,
                raw.height - half.y * 2f
            );
        } else {
            Debug.LogWarning($"[{name}] MovementBoundary or SpriteRenderer missing.");
        }
    }

    void FixedUpdate() {
        if(!isPaused) Move();
    }

    private void Move() {
        Vector2 next = rb.position + direction * speed * Time.fixedDeltaTime;
        // Bounce X
        if(next.x < movementBounds.xMin || next.x > movementBounds.xMax) {
            direction.x = -direction.x;
            next.x = Mathf.Clamp(next.x, movementBounds.xMin, movementBounds.xMax);
        }
        // Bounce Y
        if(next.y < movementBounds.yMin || next.y > movementBounds.yMax) {
            direction.y = -direction.y;
            next.y = Mathf.Clamp(next.y, movementBounds.yMin, movementBounds.yMax);
        }
        rb.MovePosition(next);
    }

    private void PickRandomDirection() {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
    }

    void OnMouseDown() {
        if(!KoiGameManager.Instance.CanSelect) return;
        isPaused = true;
        KoiGameManager.Instance.OnEntitySelected(this);
    }

    public void Feed() {
        if(!hasBeenFed) {
            hasBeenFed = true;
            StartCoroutine(Flash(Color.blue));
            KoiGameManager.Instance.OnEntityFed();
        } else {
            StartCoroutine(Flash(Color.red));
            KoiGameManager.Instance.OnWrongFeed();
        }
        isPaused = false;
    }

    private IEnumerator Flash(Color flashColor) {
        if(whiteSprite == null) yield break;
        Color original = whiteSprite.color;
        whiteSprite.color = flashColor;
        yield return new WaitForSeconds(0.3f);
        whiteSprite.color = original;
    }
}
