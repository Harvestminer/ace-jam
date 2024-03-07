using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Mathematics.math;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class Buoyancy : MonoBehaviour
{
    public WavesGenerator waterSource;

    [Header("Buoyancy Sim Settings")]
    [Space(5)]
    [Range(0.1f, 1.0f)]
    public float normalizedVoxelSize = 0.1f;

    public bool onlyReadbackBottomVoxels = false;

    [Header("Fake Sim Settings")]
    [Space(5)]
    [Range(0.01f, 10.0f)]
    public float stiffness = 1.0f;

    [Range(0.01f, 10.0f)]
    public float angularStiffness = 1.0f;

    [Range(0.01f, 10.0f)]
    public float directionalStrength = 1.0f;

    [Range(0.01f, 5.0f)]
    public float angleThreshold = 1.5f;

    private Rigidbody rigidBody;
    private Collider cachedCollider;

    private Voxel[,,] voxels;
    private List<Vector3> receiverVoxels;
    private Vector3 voxelSize;
    private int voxelsPerAxis = 0;

    private Vector3 origin, direction;

    List<Vector3> points;
    Queue<Vector3> cachedDirections;

    private float velocity = 0.0f;

    private struct Voxel
    {
        public Vector3 position { get; }
        private WavesGenerator waterSource;
        public bool isReceiver;

        private Vector3 displacement;

        public Voxel(Vector3 position, WavesGenerator waterSource, bool isReceiver)
        {
            this.position = position;
            this.waterSource = waterSource;
            this.isReceiver = isReceiver;

            displacement = waterSource.GetWaterDisplacement(position);
        }

        public float GetWaterHeight()
        {
            return waterSource.GetWaterHeight(position);
        }
    };

    // Largely referenced from https://github.com/dbrizov/NaughtyWaterBuoyancy/blob/master/Assets/NaughtyWaterBuoyancy/Scripts/Core/FloatingObject.cs
    private void CreateVoxels()
    {
        receiverVoxels = new List<Vector3>();

        Quaternion initialRotation = this.transform.rotation;
        this.transform.rotation = Quaternion.identity;

        Bounds bounds = this.cachedCollider.bounds;
        voxelSize = bounds.size * normalizedVoxelSize;
        voxelsPerAxis = Mathf.RoundToInt(1.0f / normalizedVoxelSize);
        voxels = new Voxel[voxelsPerAxis, voxelsPerAxis, voxelsPerAxis];

        for (int x = 0; x < voxelsPerAxis; ++x)
        {
            for (int y = 0; y < voxelsPerAxis; ++y)
            {
                for (int z = 0; z < voxelsPerAxis; ++z)
                {
                    Vector3 point = voxelSize;
                    point.Scale(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
                    point += bounds.min;

                    voxels[x, y, z] = new Voxel(this.transform.InverseTransformPoint(point), waterSource, onlyReadbackBottomVoxels ? y == 0 : true);

                    if (voxels[x, y, z].isReceiver)
                        receiverVoxels.Add(new Vector3(x, y, z));
                }
            }
        }
    }


    void OnEnable()
    {
        if (waterSource == null)
            return;

        rigidBody = GetComponent<Rigidbody>();
        cachedCollider = GetComponent<Collider>();
        cachedDirections = new Queue<Vector3>();

        //CreateVoxels();
    }

    // Taken from https://github.com/zalo/MathUtilities/blob/master/Assets/LeastSquares/LeastSquaresFitting.cs
    public static class Fit
    {
        public static void Line(List<Vector3> points, out Vector3 origin, ref Vector3 direction, int iters = 100, bool drawGizmos = false)
        {
            if (direction == Vector3.zero || float.IsNaN(direction.x) || float.IsInfinity(direction.x))
                direction = Vector3.up;

            //Calculate Average
            origin = Vector3.zero;
            for (int i = 0; i < points.Count; i++)
                origin += points[i];
            origin /= points.Count;

            // Step the optimal fitting line approximation:
            for (int iter = 0; iter < iters; iter++)
            {
                Vector3 newDirection = Vector3.zero;
                foreach (Vector3 worldSpacePoint in points)
                {
                    Vector3 point = worldSpacePoint - origin;
                    newDirection += Vector3.Dot(direction, point) * point;
                }
                direction = newDirection.normalized;
            }
        }

        public static void Plane(List<Vector3> points, out Vector3 position, out Vector3 normal, int iters = 200, bool drawGizmos = false)
        {
            //Find the primary principal axis
            Vector3 primaryDirection = Vector3.right;
            Line(points, out position, ref primaryDirection, iters / 2, false);

            //Flatten the points along that axis
            List<Vector3> flattenedPoints = new List<Vector3>(points);
            for (int i = 0; i < flattenedPoints.Count; i++)
                flattenedPoints[i] = Vector3.ProjectOnPlane(points[i] - position, primaryDirection) + position;

            //Find the secondary principal axis
            Vector3 secondaryDirection = Vector3.right;
            Line(flattenedPoints, out position, ref secondaryDirection, iters / 2, false);

            normal = Vector3.Cross(primaryDirection, secondaryDirection).normalized;

            if (drawGizmos)
            {
                Gizmos.color = Color.red;
                foreach (Vector3 point in points)
                    Gizmos.DrawLine(point, Vector3.ProjectOnPlane(point - position, normal) + position);
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(position, normal * 0.5f);
                Gizmos.DrawRay(position, -normal * 0.5f);
                Gizmos.matrix = Matrix4x4.TRS(position, Quaternion.LookRotation(normal, primaryDirection), new Vector3(1f, 1f, 0.001f));
                Gizmos.DrawSphere(Vector3.zero, 1f);
                Gizmos.matrix = Matrix4x4.identity;
            }
        }
    }

    private Vector3 GetAverageDirection()
    {
        if (cachedDirections.Count == 0)
            return Vector3.up;

        Vector3 directionSum = Vector3.zero;
        Vector3[] directionArray = cachedDirections.ToArray();
        for (int i = 0; i < cachedDirections.Count; ++i)
        {
            directionSum += directionArray[i];
        }

        directionSum /= cachedDirections.Count;
        directionSum.Normalize();

        return directionSum;
    }

    void Update()
    {
        if (waterSource == null)
            return;
        if (voxels == null)
            CreateVoxels();


        rigidBody.isKinematic = true;
        points = new List<Vector3>();

        for (int i = 0; i < receiverVoxels.Count; ++i)
        {
            Vector3 pos = this.transform.TransformPoint(voxels[(int)receiverVoxels[i].x, (int)receiverVoxels[i].y, (int)receiverVoxels[i].z].position);
            pos.y = voxels[(int)receiverVoxels[i].x, (int)receiverVoxels[i].y, (int)receiverVoxels[i].z].GetWaterHeight();
            points.Add(pos);
        }

        Fit.Plane(points, out origin, out direction, 100, false);
        direction.y = 1;
        direction.x *= directionalStrength;
        direction.z *= directionalStrength;
        direction.Normalize();

        cachedDirections.Enqueue(direction);
        if (cachedDirections.Count > 10)
            cachedDirections.Dequeue();

        Vector3 avgDirection = GetAverageDirection();
        Quaternion avgRotation = Quaternion.FromToRotation(Vector3.up, avgDirection);

        float Fspring = -stiffness * origin.y;
        float a = Fspring / rigidBody.mass;

        velocity += a * Time.deltaTime;

        Vector3 position = this.transform.position;
        position.y = origin.y - velocity * Time.deltaTime;
        this.transform.position = position;

        float angularDistance = Quaternion.Angle(Quaternion.identity, avgRotation);
        float FxAngle = -angularStiffness * angularDistance;
        float aa = FxAngle / rigidBody.mass;

        float localAngularDistance = Quaternion.Angle(this.transform.rotation, avgRotation);
        if (localAngularDistance > angleThreshold)
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, avgRotation, Mathf.Max(0.0f, localAngularDistance - aa * Time.deltaTime) * 0.01f);
    }

    void OnDisable()
    {
        voxels = null;
    }

    private void OnDrawGizmos()
    {
        if (this.voxels != null)
        {
            for (int x = 0; x < voxelsPerAxis; ++x)
            {
                for (int y = 0; y < voxelsPerAxis; ++y)
                {
                    for (int z = 0; z < voxelsPerAxis; ++z)
                    {
                        Gizmos.color = this.voxels[x, y, z].isReceiver ? Color.green : Color.red;
                        Gizmos.DrawCube(this.transform.TransformPoint(this.voxels[x, y, z].position), this.voxelSize * 0.8f);
                    }
                }
            }
        }
    }
}