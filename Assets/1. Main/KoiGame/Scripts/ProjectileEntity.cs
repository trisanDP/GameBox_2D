// ProjectileEntity.cs
using UnityEngine;

public class ProjectileEntity : MonoBehaviour {
    #region Inspector Settings
    [Tooltip("Speed at which the projectile moves toward its target")]
    public float speed = 5f;
    #endregion

    private InteractableEntity target;

    /// <summary>
    /// Initialize this projectile with the entity it should feed.
    /// </summary>
    public void Initialize(InteractableEntity entity) {
        target = entity;
    }

    void Update() {
        if(target == null) {
            Destroy(gameObject);
            return;
        }

        // Move toward the target each frame
        Vector2 newPos = Vector2.MoveTowards(transform.position,
                                             target.transform.position,
                                             speed * Time.deltaTime);
        transform.position = newPos;

        // Check if we’ve reached (or are very close to) the target
        if(Vector2.Distance(newPos, target.transform.position) < 0.1f) {
            target.Feed();
            Destroy(gameObject);
        }
    }
}
