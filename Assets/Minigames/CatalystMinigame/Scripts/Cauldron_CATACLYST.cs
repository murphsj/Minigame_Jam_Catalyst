using System;
using UnityEngine;

public class Cauldron_CATACLYST : MonoBehaviour
{
    public Vector2 localEndPoint;
    public float moveSpeed;


    Vector2 startPoint;
    Vector2 endPoint;
    enum Target
    {
        Start,
        End
    }

    Target target;

    void Start()
    {
        startPoint = transform.position;
        endPoint = (Vector2)transform.position + localEndPoint;
        target = Target.End;
    }

    void FixedUpdate()
    {
        float maxDistance = Time.deltaTime * moveSpeed;
        if (target == Target.Start)
        {
            transform.position = Vector2.MoveTowards(transform.position, startPoint, maxDistance);
            if ((Vector2)transform.position == startPoint) target = Target.End;
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, endPoint, maxDistance);
            if ((Vector2)transform.position == endPoint) target = Target.Start;
        }
    }

    private void DrawCoordMarkerGizmo(Vector2 pos)
    {
        Gizmos.color = Color.red;
        float size = .3f;
        Gizmos.DrawLine(new Vector2(pos.x - size, pos.y), new Vector2(pos.x + size, pos.y));
        Gizmos.DrawLine(new Vector2(pos.x, pos.y - size), new Vector2(pos.x, pos.y + size));
    }

    void OnDrawGizmos()
    {
        if (localEndPoint != null) DrawCoordMarkerGizmo((Vector2)transform.position + localEndPoint);
    }
}
