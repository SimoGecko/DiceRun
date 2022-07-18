// (c) Simone Guggiari 2022

using System.Collections.Generic;
using UnityEngine;
using System.Linq;

////////// PURPOSE:  //////////

namespace sxg
{
    [System.Serializable]
    public struct OCircle
    {
        public Vector3 center;
        public Quaternion orientation;
        public float radius;

        public OCircle(Vector3 center, Quaternion orientation, float radius)
        {
            this.center = center;
            this.orientation = orientation;
            this.radius = radius;
        }
    }

    public class LevelGenerator : MonoBehaviour
    {
        // -------------------- VARIABLES --------------------

        // public
        [Header("AddPiece")]
        public float length = 200f;
        public float curvature = 0f;
        public float angle = 0f;
        public float radius = 40f;

        [Header("AddRandomPiece")]
        public Vector2 randomLength = new Vector2(60f, 400f);
        public Vector2 randomCurvature = new Vector2(0, 90f);
        public Vector2 randomAngle = new Vector2(0f, 360f);
        public Vector2 randomRadius = new Vector2(20f, 60f);
        public Vector2Int randomRingOffset = new Vector2Int(1, 20);

        [Header("Setup")]
        public int initialPieces = 10;
        public int segmentsDensity = 25;
        public int steps = 32;
        public Vector3 startCirclePosition = new Vector3(0f, 0f, -2f);
        public bool EDITOR_drawGizmos = false;

        // private
        private int nextIndex = 0;
        float ringRadius = 1f;
        int lastChunkDeletedIndex = -1;

        List<OCircle> circles = new();
        List<GameObject> piecesChunks = new();
        List<GameObject> ringsChunks = new();
        List<int> chunksCumsum = new();


        // references
        [Header("References")]
        public MeshFilter mfPrefab;
        public Transform piecesParent;
        public Ring ringPrefab;
        public Transform ringParent;

        // -------------------- BASE METHODS --------------------

        void Awake ()
        {
            ringRadius = ringPrefab.transform.localScale.x;

            for (int i = 0; i < initialPieces; i++)
            {
                Add1Chunk();
            }

            GameManager.Instance.OnModeChanged += mode => ringParent.gameObject.SetActive(mode == GameManager.Mode.Game); // TODO: move
        }
        
        void Update ()
        {
            
        }

        // -------------------- CUSTOM METHODS --------------------


        // commands
        void Add1Chunk()
        {
            int circleFrom = circles.Count;
            AddRandomCircles();
            int circleTo = circles.Count - 1;
            chunksCumsum.Add(circleTo);

            AddChunkMesh(Mathf.Max(0, circleFrom - 1), circleTo);
            AddChunkRings(circleFrom, circleTo);
            //Debug.Log("Added chunk " + chunksCount.Count);
        }

        void AddRandomCircles()
        {
            AddCircles(randomLength.GetRandom(), randomCurvature.GetRandom(), randomAngle.GetRandom(), randomRadius.GetRandom(), segmentsDensity);
        }

        void AddCircles(float length, float curvature, float angle, float radius, int segmentsDensity)
        {
            int segments = Mathf.RoundToInt(length / segmentsDensity);

            if (circles.IsNullOrEmpty())
            {
                circles.Add(new OCircle(startCirclePosition, Quaternion.identity, radius));
            }
            OCircle last = circles[^1];
            if (curvature == 0f)
            {
                Vector3 lastNormal = last.orientation * Vector3.forward;
                for (int i = 0; i < segments; i++)
                {
                    float t = (float)(i + 1) / (segments);
                    Vector3 newCenter = last.center + lastNormal * length * t;
                    float newRadius = Mathf.Lerp(last.radius, radius, t);
                    circles.Add(new OCircle(newCenter, last.orientation, newRadius));
                }
            }
            else
            {
                float R = length * 360f / (curvature * Mathf.PI * 2f); // radius of curvature circle
                Quaternion newOrientation = Quaternion.AngleAxis(angle, last.orientation * Vector3.forward) * last.orientation; // roll last
                Vector3 C = last.center - (newOrientation * Vector3.up * R); // center of curvature circle
                Vector3 right = newOrientation * Vector3.right;

                // use transform for temporary computation
                transform.position = last.center;
                transform.rotation = last.orientation;
                float step = 1f / segments * curvature;
                for (int i = 0; i < segments; i++)
                {
                    float t = (float)(i + 1) / (segments);
                    transform.RotateAround(C, right, step);
                    float newRadius = Mathf.Lerp(last.radius, radius, t);
                    circles.Add(new OCircle(transform.position, transform.rotation, newRadius));
                }
                transform.Reset();
            }
        }

        void AddChunkMesh(int circleFrom, int circleTo)
        {
            MeshFilter mf = Instantiate(mfPrefab, piecesParent) as MeshFilter;
            mf.mesh = MakeMesh(circleFrom, circleTo);
            piecesChunks.Add(mf.gameObject);
        }

        void AddChunkRings(int circleFrom, int circleTo)
        {
            // create rings
            Transform ringChunkParent = new GameObject("chunkparent").transform;
            ringChunkParent.parent = ringParent;
            ringsChunks.Add(ringChunkParent.gameObject);

            for (int i = circleFrom; i <= circleTo; i+= CircleOffset(i))
            {
                // place ring
                OCircle c = circles[i];
                Ring newRing = Instantiate(ringPrefab, ringChunkParent);
                newRing.transform.rotation = c.orientation;
                newRing.transform.position = c.center + (c.orientation * (Vector3)Random.insideUnitCircle) * (c.radius - ringRadius);
                newRing.SetRandom();
            }
        }

        // queries
        private Mesh MakeMesh(int first, int last)
        {
            List<Vector3> verts = new();
            List<Vector2> uvs = new();
            List<int> indices = new();
            int vi = 0;
            Vector2 uv = Vector2.zero;
            float stepAngle = 1f / steps * Mathf.PI * 2f;
            for (int i = first; i <= last; ++i)
            {
                OCircle c = circles[i];
                for (int step = 0; step <= steps; ++step)
                {
                    float angle = step * stepAngle;
                    Vector3 circleCorner = Mathf.Cos(angle) * (c.orientation * Vector3.right) + Mathf.Sin(angle) * (c.orientation * Vector3.up);
                    verts.Add(c.center + circleCorner * c.radius);
                    uvs.Add(uv);
                    uv.x += 0.5f;
                }
                if (i > first)
                {
                    for (int step = 0; step < steps; step++)
                    {

                        int a = vi + step;
                        int b = vi + step + 1;
                        indices.AddRange(new int[] { a, b, b+steps+1, a+steps+1 });
                    }
                    vi += steps+1;
                }
                uv.y += 0.5f;
                uv.x = 0f;
            }

            Mesh mesh = new ();
            mesh.SetVertices(verts);
            mesh.SetIndices(indices.ToArray(), MeshTopology.Quads, 0);
            mesh.SetUVs(0, uvs);

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.triangles = mesh.triangles.Reverse().ToArray(); // flip normals
            mesh.RecalculateTangents();
            mesh.Optimize();
            mesh.OptimizeIndexBuffers();
            mesh.OptimizeReorderVertexBuffer();
            return mesh;
        }

        public OCircle GetNextCircle()
        {
            CheckDeletion(nextIndex - 1);
            // notify we got over nextIndex
            return circles[nextIndex++];
        }

        private int CircleOffset(int i)
        {
            return Random.Range(randomRingOffset.x, randomRingOffset.y);
        }

        private void CheckDeletion(int circleIndex)
        {
            if (circleIndex < 0) return;
            if (lastChunkDeletedIndex+3 >= chunksCumsum.Count) return;
            if (circleIndex > chunksCumsum[lastChunkDeletedIndex+3])
            {
                // delete last chunk
                ++lastChunkDeletedIndex;
                //Debug.Log("Destroyed chunk " + lastChunkDeletedIndex);
                Destroy(ringsChunks[lastChunkDeletedIndex]);
                ringsChunks[lastChunkDeletedIndex] = null;
                Destroy(piecesChunks[lastChunkDeletedIndex]);
                piecesChunks[lastChunkDeletedIndex] = null;

                // create new chunk
                Add1Chunk();
            }
        }



        // other
        [EditorButton]
        [LayoutBeginHorizontal]
        void EDITOR_AddPiece()
        {
            AddCircles(length, curvature, angle, radius, segmentsDensity);
        }
        [EditorButton]
        void EDITOR_AddRandomPiece()
        {
            AddRandomCircles();
        }
        [EditorButton]
        [LayoutEndHorizontal]
        void EDITOR_Clear()
        {
            circles = new();
            foreach (GameObject go in piecesChunks) Destroy(go);
            foreach (GameObject go in ringsChunks) Destroy(go);
            piecesChunks = new();
            ringsChunks = new();
            chunksCumsum = new();
        }

        private static LevelGenerator instance;
        public static LevelGenerator Instance
        {
            get
            {
                if (instance == null) instance = FindObjectOfType<LevelGenerator>();
                return instance;
            }
        }

        private void OnDrawGizmos()
        {
            if (!EDITOR_drawGizmos) return;
            Gizmos.color = Color.green;
            foreach (OCircle circle in circles)
            {
                Gizmos2.DrawWireCircle(circle.center, circle.radius, circle.orientation * Vector3.forward);
            }
        }
    }
}