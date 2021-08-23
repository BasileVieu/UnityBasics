using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;
using Random = UnityEngine.Random;

public class FractalJobs : MonoBehaviour
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct UpdateFractalLevelJob : IJobFor
    {
        public float scale;
        public float deltaTime;

        [ReadOnly] public NativeArray<FractalJobsPart> parents;
        public NativeArray<FractalJobsPart> parts;

        [WriteOnly] public NativeArray<float3x4> matrices;

        public void Execute(int _i)
        {
            FractalJobsPart parent = parents[_i / 5];
            FractalJobsPart part = parts[_i];
            part.spinAngle += part.spinVelocity * deltaTime;

            float3 upAxis = mul(mul(parent.worldRotation, part.rotation), up());
            float3 sagAxis = cross(up(), upAxis);

            float sagMagnitude = length(sagAxis);

            quaternion baseRotation;

            if (sagMagnitude > 0.0f)
            {
                sagAxis /= sagMagnitude;
                quaternion sagRotation = quaternion.AxisAngle(sagAxis, part.maxSagAngle * sagMagnitude);
                baseRotation = mul(sagRotation, parent.worldRotation);
            }
            else
            {
                baseRotation = parent.worldRotation;
            }

            part.worldRotation = mul(baseRotation, mul(part.rotation, quaternion.RotateY(part.spinAngle)));
            part.worldPosition = parent.worldPosition + mul(part.worldRotation, float3(0.0f, 1.5f * scale, 0.0f));

            parts[_i] = part;

            float3x3 r = float3x3(part.worldRotation) * scale;

            matrices[_i] = float3x4(r.c0, r.c1, r.c2, part.worldPosition);
        }
    }

    struct FractalJobsPart
    {
        public float3 worldPosition;
        public quaternion rotation;
        public quaternion worldRotation;
        public float maxSagAngle;
        public float spinAngle;
        public float spinVelocity;
    }

    [SerializeField] [Range(3, 8)] private int depth = 4;

    [SerializeField] private Mesh mesh;
    [SerializeField] private Mesh leafMesh;

    [SerializeField] private Material material;

    [SerializeField] private Gradient gradientA;
    [SerializeField] private Gradient gradientB;

    [SerializeField] private Color leafColorA;
    [SerializeField] private Color leafColorB;

    [SerializeField] [Range(0.0f, 90.0f)] private float maxSagAngleA = 15.0f;
    [SerializeField] [Range(0.0f, 90.0f)] private float maxSagAngleB = 25.0f;

    [SerializeField] [Range(0.0f, 90.0f)] private float spinSpeedA = 20.0f;
    [SerializeField] [Range(0.0f, 90.0f)] private float spinSpeedB = 25.0f;

    [SerializeField] [Range(0.1f, 1.0f)] private float reverseSpinChance = 0.25f;

    private Vector4[] sequenceNumbers;

    private NativeArray<FractalJobsPart>[] parts;

    private NativeArray<float3x4>[] matrices;

    private ComputeBuffer[] matricesBuffers;

    private static quaternion[] rotations =
    {
            quaternion.identity,
            quaternion.RotateZ(-0.5f * PI),
            quaternion.RotateZ(0.5f * PI),
            quaternion.RotateX(0.5f * PI),
            quaternion.RotateX(-0.5f * PI)
    };

    private static readonly int colorAId = Shader.PropertyToID("_ColorA");
    private static readonly int colorBId = Shader.PropertyToID("_ColorB");
    private static readonly int matricesId = Shader.PropertyToID("_Matrices");
    private static readonly int sequenceNumbersId = Shader.PropertyToID("_SequenceNumbers");

    private static MaterialPropertyBlock propertyBlock;

    void OnEnable()
    {
        parts = new NativeArray<FractalJobsPart>[depth];
        matrices = new NativeArray<float3x4>[depth];
        matricesBuffers = new ComputeBuffer[depth];
        sequenceNumbers = new Vector4[depth];

        int stride = 12 * 4;

        for (int i = 0, length = 1; i < parts.Length; i++, length *= 5)
        {
            parts[i] = new NativeArray<FractalJobsPart>(length, Allocator.Persistent);
            matrices[i] = new NativeArray<float3x4>(length, Allocator.Persistent);
            matricesBuffers[i] = new ComputeBuffer(length, stride);
            sequenceNumbers[i] = new Vector4(Random.value, Random.value, Random.value, Random.value);
        }

        parts[0][0] = CreatePart(0);

        for (var li = 1; li < parts.Length; li++)
        {
            NativeArray<FractalJobsPart> levelParts = parts[li];

            for (var fpi = 0; fpi < levelParts.Length; fpi += 5)
            {
                for (var ci = 0; ci < 5; ci++)
                {
                    levelParts[fpi + ci] = CreatePart(ci);
                }
            }
        }

        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
    }

    void OnDisable()
    {
        for (var i = 0; i < matricesBuffers.Length; i++)
        {
            matricesBuffers[i].Release();
            parts[i].Dispose();
            matrices[i].Dispose();
        }

        parts = null;
        matrices = null;
        matricesBuffers = null;
        sequenceNumbers = null;
    }

    void OnValidate()
    {
        if (parts != null
            && enabled)
        {
            OnDisable();
            OnEnable();
        }
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;

        FractalJobsPart rootPart = parts[0][0];
        rootPart.spinAngle += rootPart.spinVelocity * deltaTime;
        rootPart.worldRotation =
                mul(transform.rotation, mul(rootPart.rotation, quaternion.RotateY(rootPart.spinAngle)));
        rootPart.worldPosition = transform.position;
        parts[0][0] = rootPart;

        float objectScale = transform.lossyScale.x;

        float3x3 r = float3x3(rootPart.worldRotation) * objectScale;

        matrices[0][0] = float3x4(r.c0, r.c1, r.c2, rootPart.worldPosition);

        float scale = objectScale;

        JobHandle jobHandle = default;

        for (var li = 1; li < parts.Length; li++)
        {
            scale *= 0.5f;

            jobHandle = new UpdateFractalLevelJob
            {
                    deltaTime = deltaTime,
                    scale = scale,
                    parents = parts[li - 1],
                    parts = parts[li],
                    matrices = matrices[li]
            }.ScheduleParallel(parts[li].Length, 5, jobHandle);
        }

        jobHandle.Complete();

        var bounds = new Bounds(rootPart.worldPosition, 3.0f * objectScale * Vector3.one)
        {
                extents = Vector3.one * 0.5f
        };

        int leafIndex = matricesBuffers.Length - 1;

        for (var i = 0; i < matricesBuffers.Length; i++)
        {
            ComputeBuffer buffer = matricesBuffers[i];
            buffer.SetData(matrices[i]);

            Color colorA;
            Color colorB;

            Mesh instanceMesh;

            if (i == leafIndex)
            {
                colorA = leafColorA;
                colorB = leafColorB;

                instanceMesh = leafMesh;
            }
            else
            {
                float gradientInterpolator = i / (matricesBuffers.Length - 2.0f);

                colorA = gradientA.Evaluate(gradientInterpolator);
                colorB = gradientB.Evaluate(gradientInterpolator);

                instanceMesh = mesh;
            }

            propertyBlock.SetColor(colorAId, colorA);
            propertyBlock.SetColor(colorBId, colorB);

            propertyBlock.SetBuffer(matricesId, buffer);
            
            propertyBlock.SetVector(sequenceNumbersId, sequenceNumbers[i]);

            Graphics.DrawMeshInstancedProcedural(instanceMesh, 0, material, bounds, buffer.count, propertyBlock);
        }
    }

    FractalJobsPart CreatePart(int _childIndex) => new FractalJobsPart
    {
            maxSagAngle = radians(Random.Range(maxSagAngleA, maxSagAngleB)),
            rotation = rotations[_childIndex],
            spinVelocity = (Random.value < reverseSpinChance ? -1.0f : 1.0f) * radians(Random.Range(spinSpeedA, spinSpeedB)),
    };
}