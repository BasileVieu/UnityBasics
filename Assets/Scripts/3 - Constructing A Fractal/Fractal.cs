using System.Collections;
using UnityEngine;

public class Fractal : MonoBehaviour
{
    public Mesh[] meshes;
    public Material material;

    public int maxDepth;

    private int depth;

    public float childScale;

    private Material[,] materials;

    public float spawnProbability;

    public float maxRotationSpeed;

    private float rotationSpeed;

    public float maxTwist;

    private static Vector3[] childDirections = {
        Vector3.up,
        Vector3.right,
        Vector3.left,
        Vector3.forward,
        Vector3.back };

    private static Quaternion[] childOrientations = {
        Quaternion.identity,
        Quaternion.Euler(0f, 0f, -90f),
        Quaternion.Euler(0f, 0f, 90f),
        Quaternion.Euler(90f, 0f, 0f),
        Quaternion.Euler(-90f, 0f, 0f) };

    void Start()
    {
        if (materials == null)
        {
            InitializeMaterials();
        }
        
        gameObject.AddComponent<MeshFilter>().mesh = meshes[Random.Range(0, meshes.Length)];
        gameObject.AddComponent<MeshRenderer>().material = materials[depth, Random.Range(0, 2)];

        if (depth < maxDepth)
        {
            StartCoroutine(CreateChildren());
        }

        rotationSpeed = Random.Range(-maxRotationSpeed, maxRotationSpeed);
        transform.Rotate(Random.Range(-maxTwist, maxTwist), 0f, 0f);
    }

    private void InitializeMaterials()
    {
        materials = new Material[maxDepth + 1, 2];

        for (int i = 0; i <= maxDepth; i++)
        {
            float t = i / (maxDepth - 1f);
            t *= t;
            
            materials[i, 0] = new Material(material);
            materials[i, 0].color = Color.Lerp(Color.white, Color.yellow, t);

            materials[i, 1] = new Material(material);
            materials[i, 1].color = Color.Lerp(Color.white, Color.cyan, t);
        }

        materials[maxDepth, 0].color = Color.magenta;
        materials[maxDepth, 1].color = Color.red;
    }

    private IEnumerator CreateChildren()
    {
        for (int i = 0; i < childDirections.Length; i++)
        {
            if (Random.value < spawnProbability)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
                new GameObject("FractalChild").AddComponent<Fractal>().Initialize(this, i);
            }
        }
    }

    private void Initialize(Fractal _parent, int _childIndex)
    {
        meshes = _parent.meshes;
        materials = _parent.materials;
        maxDepth = _parent.maxDepth;
        depth = _parent.depth + 1;
        childScale = _parent.childScale;
        spawnProbability = _parent.spawnProbability;
        maxRotationSpeed = _parent.maxRotationSpeed;
        maxTwist = _parent.maxTwist;
        transform.parent = _parent.transform;
        transform.localScale = Vector3.one * childScale;
        transform.localPosition = childDirections[_childIndex] * (0.5f + 0.5f * childScale);
        transform.localRotation = childOrientations[_childIndex];
    }

    private void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
}