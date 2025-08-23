using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class InvisibleWallProbe2D : MonoBehaviour
{
    public float probeDistance = 0.6f;        // how far ahead to check
    public LayerMask probeMask = ~0;          // check everything
    public bool drawDebug = true;

    Collider2D col;
    Rigidbody2D rb;
    HashSet<Collider2D> seen = new HashSet<Collider2D>();

    void Awake()
    {
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        // don’t hit yourself
        probeMask &= ~(1 << gameObject.layer);
    }

    void Update()
    {
        if (!col) return;

        // face from velocity if moving, else from localScale.x
        float dir = (rb && Mathf.Abs(rb.velocity.x) > 0.01f) ? Mathf.Sign(rb.velocity.x)
                                                             : (transform.localScale.x >= 0 ? 1f : -1f);

        Vector2 center = col.bounds.center;
        Vector2 size = col.bounds.size * new Vector2(0.95f, 0.9f);

        var hit = Physics2D.BoxCast(center, size, 0f, new Vector2(dir, 0f), probeDistance, probeMask);

        if (drawDebug)
            Debug.DrawLine(center, center + new Vector2(dir * (hit ? hit.distance : probeDistance), 0f),
                           hit ? Color.red : Color.green, 0f, false);

        if (hit.collider && seen.Add(hit.collider))
        {
            var go = hit.collider.gameObject;
            Debug.Log($"[InvisibleWallProbe2D] Ahead: '{go.name}'  Type:{hit.collider.GetType().Name}  Layer:{LayerMask.LayerToName(go.layer)}  Point:{hit.point}");
        }
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (seen.Add(c.collider))
            Debug.Log($"[InvisibleWallProbe2D] Collided with '{c.collider.name}'  Type:{c.collider.GetType().Name}  Layer:{LayerMask.LayerToName(c.collider.gameObject.layer)}");
    }
}
