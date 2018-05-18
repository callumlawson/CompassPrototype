using UnityEngine;
using UnityEngine.AI;

public class PathDrawer : MonoBehaviour
{
    public float VerticalOffset;
    private GameObject nurse;
    private LineRenderer lineRenderer;
    private NavMeshPath navMeshPath;
    private ParticleSystem particleSystem; 

	private void Start () {
		nurse = GameObject.FindWithTag("Nurse");
	    lineRenderer = GetComponent<LineRenderer>();
	    particleSystem = GetComponent<ParticleSystem>();
	    navMeshPath = new NavMeshPath();
	}
	
	private void Update ()
	{
	    NavMesh.CalculatePath(transform.position, nurse.transform.position, NavMesh.AllAreas, navMeshPath);
	    var path = navMeshPath.corners;

	    lineRenderer.positionCount = 0; //reset
        lineRenderer.positionCount = path.Length + 1;
	    for (var i = 0; i < path.Length; i++)
	    {
	        lineRenderer.SetPosition(i, path[i] + Vector3.up * VerticalOffset);
        }
        lineRenderer.SetPosition(path.Length, nurse.transform.position);
	}
}
