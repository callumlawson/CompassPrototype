using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class TrailDrawer : MonoBehaviour
{
    public bool ShouldDrawLine;
    [Tooltip("The GameObject you want to draw the trail from")]
    public GameObject Target;
    [Tooltip("What offset to apply to the height of the trail")]
    public float VerticalOffset;
    [Range(1,5)]
    [Tooltip("How much to smooth the line. 1 is no smoothing")]
    public int CurveSmoothingFactor = 1;
    [Tooltip("Number of particles to create per second for each meter of path")]
    public float ParticlesPerSecondPerMeter = 1;
    
    private LineRenderer lineRenderer;
    private ParticleSystem ourParticleSystem;
    private NavMeshPath navMeshPath;

    private void Start () {
	    lineRenderer = GetComponent<LineRenderer>();
	    ourParticleSystem = GetComponent<ParticleSystem>();
	    navMeshPath = new NavMeshPath();
	}
	
	private void Update ()
    {
        var smoothedPath = GetSmoothedPathToTarget();
        if (smoothedPath.Length < 3)
        {
            return;
        }
        if (ShouldDrawLine)
        {
            DrawLine(smoothedPath);
        }
        else
        {
            lineRenderer.positionCount = 0;
        }
        CreateParticles(smoothedPath);
    }

    private Vector3[] GetSmoothedPathToTarget()
    {
        NavMesh.CalculatePath(transform.position, Target.transform.position, NavMesh.AllAreas, navMeshPath);
        var navPath = navMeshPath.corners;
        var positionCount = navPath.Length + 1;
        var trailPath = new Vector3[positionCount];
        for (var i = 0; i < navPath.Length; i++)
        {
            trailPath[i] = navPath[i] + Vector3.up * VerticalOffset;
        }

        trailPath[navPath.Length] = Target.transform.position + Vector3.up * VerticalOffset;
        var smoothedPath = MakeSmoothCurve(trailPath, CurveSmoothingFactor);
        return smoothedPath;
    }

    private void DrawLine(Vector3[] smoothedPath)
    {
        lineRenderer.positionCount = 0; //reset the points
        lineRenderer.positionCount = smoothedPath.Length;
        lineRenderer.SetPositions(smoothedPath);
    }

    private void CreateParticles(Vector3[] smoothedPath)
    {
        var smoothedPathSegmentLengths = new float[smoothedPath.Length - 1];
        for (var i = 0; i < smoothedPath.Length - 1; i++)
        {
            var thisElement = smoothedPath[i];
            var nextElement = smoothedPath[i + 1];
            smoothedPathSegmentLengths[i] = Vector3.Distance(thisElement, nextElement);
        }
        var smoothedPathTotalLength = smoothedPathSegmentLengths.Sum();

        for (var numParticle = 0; numParticle < Mathf.CeilToInt(ParticlesPerSecondPerMeter*Time.deltaTime*smoothedPathTotalLength); numParticle++)
        {
            var randomDistanceOnLine = Random.value * smoothedPathTotalLength;
            var sumDistance = 0.0f;
            var indexOfCurrentSegment = 0;
            for (var i = 0; i < smoothedPathSegmentLengths.Length; i++)
            {
                if (sumDistance + smoothedPathSegmentLengths[i] > randomDistanceOnLine)
                {
                    break;
                }
                indexOfCurrentSegment = i;
                sumDistance += smoothedPathSegmentLengths[i];
            }
            var startOfSegment = smoothedPath[indexOfCurrentSegment];
            var endOfSegment = smoothedPath[indexOfCurrentSegment + 1];
            var linearLerpValue = (randomDistanceOnLine - sumDistance) / smoothedPathSegmentLengths[indexOfCurrentSegment + 1];
            var resultingPosition = Vector3.Lerp(startOfSegment, endOfSegment, linearLerpValue);

            var emitParams = new ParticleSystem.EmitParams
            {
                position = resultingPosition
            };
            ourParticleSystem.Emit(emitParams, 1);
        }
    }

    private static Vector3[] MakeSmoothCurve(Vector3[] arrayToCurve, int smoothness)
    {
        if (smoothness == 1)
        {
            return arrayToCurve;
        }
        var pointsLength = arrayToCurve.Length;
        var curvedLength = pointsLength * smoothness - 1;
        var curvedPoints = new List<Vector3>(curvedLength);

        for (var pointInTimeOnCurve = 0; pointInTimeOnCurve < curvedLength + 1; pointInTimeOnCurve++)
        {
            var t = Mathf.InverseLerp(0, curvedLength, pointInTimeOnCurve);
            var points = new List<Vector3>(arrayToCurve);
            for (var j = pointsLength - 1; j > 0; j--)
            {
                for (var i = 0; i < j; i++)
                {
                    points[i] = (1 - t) * points[i] + t * points[i + 1];
                }
            }
            curvedPoints.Add(points[0]);
        }
        return curvedPoints.ToArray();
    }
}
