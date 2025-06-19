// InteractableEntity.cs
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class InteractableEntity : MonoBehaviour {

    #region Inspector Settings
    [Tooltip("Movement speed in units per second")]
    public float speed = 2f;
    [Tooltip("Rect area (in world units) within which this entity may move")]
    private Rect movementBounds = new Rect();
    #endregion

    #region Runtime State
    private Vector2 direction;
    private SpriteRenderer spriteRenderer;
    public bool HasBeenFed { get; private set; }
    #endregion

    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        PickRandomDirection();
    }
    void Start() {
        var boundary = FindFirstObjectByType<MovementBoundary>();
        if(boundary != null)
            movementBounds = boundary.GetWorldBounds();
        else
            Debug.LogWarning("No MovementBoundary found in scene.");
    }

    void Update() {
        Move();
    }

    private void Move() {
        Vector2 pos = (Vector2)transform.position + direction * speed * Time.deltaTime;

        // Bounce off bounds
        if(pos.x < movementBounds.xMin || pos.x > movementBounds.xMax) {
            direction.x = -direction.x;
            pos.x = Mathf.Clamp(pos.x, movementBounds.xMin, movementBounds.xMax);
        }
        if(pos.y < movementBounds.yMin || pos.y > movementBounds.yMax) {
            direction.y = -direction.y;
            pos.y = Mathf.Clamp(pos.y, movementBounds.yMin, movementBounds.yMax);
        }

        transform.position = pos;
    }

    private void PickRandomDirection() {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
    }

    void OnMouseDown() {
        // Called on both mouse click (Editor) and touch (mobile)
        if(KoiGameManager.Instance.CanSelect)
            KoiGameManager.Instance.OnEntitySelected(this);
    }

    /// <summary>
    /// Called by the projectile when it reaches this entity.
    /// </summary>
    public void Feed() {
        if(!HasBeenFed) {
            HasBeenFed = true;
            StartCoroutine(Flash(Color.blue));
            KoiGameManager.Instance.OnEntityFed(this);
        } else {
            StartCoroutine(Flash(Color.red));
            KoiGameManager.Instance.OnWrongFeed();
        }
    }

    private IEnumerator Flash(Color flashColor) {
        Color original = spriteRenderer.color;
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(0.3f);
        spriteRenderer.color = original;
    }   

}
