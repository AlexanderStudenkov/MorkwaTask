using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radar : MonoBehaviour
{
    [SerializeField] private int sideRayCounts;
    [SerializeField] private float rayRadius;
    [SerializeField] private float sectorAngle;

    private int rayCount;
    private float angleStep;
    private List<Vector3> points;

    [SerializeField]
    private GameObject enemy;
    LineRenderer _lineRenderer;
    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }
    
    void Start()
    {
        rayCount = sideRayCounts * 2 + 1;
        angleStep = sectorAngle / rayCount;

        _lineRenderer.positionCount = rayCount + 1;

        points = new List<Vector3>();
        
        for(int i=0; i<rayCount; i++)
        {
            points.Add(new Vector3(0, 0, 0));
        }
    }

    
    void Update()
    {
        _lineRenderer.SetPosition(0, transform.position);

        RaycastHit hit = new RaycastHit();
        Vector3 newDirection = new Vector3(0, 0, 0);

        for(int i=0;i<rayCount; i++)
        {
            newDirection = Quaternion.Euler(0, angleStep*i - sectorAngle/2, 0) * transform.forward;
            Ray ray = new Ray(transform.position, newDirection);   
            points[i] = transform.position + newDirection * rayRadius;

            if (Physics.Raycast(ray, out hit, rayRadius))
            {
                _lineRenderer.SetPosition(i+1, hit.point);

                if (hit.collider.gameObject.CompareTag("Player"))
                {
                    var enemyComp = enemy.GetComponent<Enemy>();
                    enemyComp.SetTargetAndStartFollow(hit.collider.gameObject.transform);
                }
            }
            else
            {
                _lineRenderer.SetPosition(i+1, points[i]);
            }
        }
    }
}
