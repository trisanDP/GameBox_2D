using UnityEngine;

[ExecuteAlways]
public class MovementBoundary : MonoBehaviour {
    [Tooltip("Area where entities can move (based on this BoxCollider2D)")]
    public BoxCollider2D boundaryCollider;

    public Rect GetWorldBounds() {
        if(boundaryCollider == null)
            return new Rect();

        Vector2 size = boundaryCollider.size;
        Vector2 center = (Vector2)boundaryCollider.transform.position + boundaryCollider.offset;
        return new Rect(center - size / 2f, size);
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
