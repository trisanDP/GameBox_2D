// ===== MovementBoundary.cs =====
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(BoxCollider2D))]
public class MovementBoundary : MonoBehaviour {
    [Tooltip("Defines where entities can move")]
    public BoxCollider2D boundaryCollider;

    void OnValidate() {
        if(boundaryCollider == null)
            boundaryCollider = GetComponent<BoxCollider2D>();
    }

    public Rect GetWorldBounds() {
        if(boundaryCollider == null)
            return new Rect();

        Bounds b = boundaryCollider.bounds;
        return new Rect(
            new Vector2(b.min.x, b.min.y),
            new Vector2(b.size.x, b.size.y)
        );
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {
        if(boundaryCollider == null) return;
        Gizmos.color = Color.cyan;
        Rect r = GetWorldBounds();
        Gizmos.DrawWireCube(r.center, r.size);
    }
#endif
}

