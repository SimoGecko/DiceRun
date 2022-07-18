// (c) Simone Guggiari 2022

using System.Collections.Generic;
using UnityEngine;
using System.Linq;

////////// PURPOSE:  //////////

namespace sxg
{
    public class LevelGenerator : MonoBehaviour
    {
        // -------------------- VARIABLES --------------------

        // public
        [Header("AddPiece")]
        public float length = 10f;
        public float curvature = 0f;
        public float angle = 0f;
        public float radius = 40f;

        [Header("AddRandomPiece")]
        public Vector2 randomLength = new Vector2(10f, 100f);
        public Vector2 randomCurvature = new Vector2(0, 30f);
        public Vector2 randomAngle = new Vector2(0f, 360f);
        public Vector2 randomRadius = new Vector2(10f, 50f);
        public Vector2Int randomRingOffset = new Vector2Int(1, 20);



        [Header("Setup")]
        public int initialPieces = 10;
        public int segmentsDensity = 10;
        public int steps = 32;
        private int nextIndex = 0;
        public Vector3 startCirclePosition = new Vector3(0f, 0f, -2f);

        public List<Circle> circles = new();

        // private
        float ringRadius = 1f;
        int lastRingCircleIndex = 0;
        List<GameObject> chunks = new();
        List<GameObject> chunkRings = new();
        public List<int> chunksCount = new();
        int lastChunkDeletedIndex = -1;


        // references
        [Header("References")]
        public MeshFilter mfPrefab;
        public Transform piecesParent;
        public Transform dummy;
        public Ring ringPrefab;
        public Transform ringParent;


        [System.Serializable]
        public struct Circle
        {
            public Vector3 center;
            public Quaternion orientation;
            public float radius;

            public Circle(Vector3 center, Quaternion orientation, float radius)
            {
                this.center = center;
                this.orientation = orientation;
                this.radius = radius;
            }
        }



        // -------------------- BASE METHODS --------------------

        void Awake ()
        {
            ringRadius = ringPrefab.transform.localScale.x;
            // create pieces
            for (int i = 0; i < initialPieces; i++)
            {
                Add1PiecesAll();
            }

            GameManager.Instance.OnModeChanged += OnModeChanged;
        }
        
        void Update ()
        {
            
        }

        // -------------------- CUSTOM METHODS --------------------


        // commands
        void Add1PiecesAll()
        {
            int circleFrom = circles.Count;
            AddRandomPieceCircles();
            int circleTo = circles.Count - 1;
            chunksCount.Add(circleTo);

            AddPieceMesh(Mathf.Max(0, circleFrom - 1), circleTo);
            AddRings(circleFrom, circleTo);
            //Debug.Log("Added chunk " + chunksCount.Count);
        }

        void AddRandomPieceCircles()
        {
            AddPieceCircles(randomLength.GetRandom(), randomCurvature.GetRandom(), randomAngle.GetRandom(), randomRadius.GetRandom(), segmentsDensity);
        }

        void AddPieceCircles(float length, float curvature, float angle, float radius, int segmentsDensity)
        {
            int segments = Mathf.RoundToInt(length / segmentsDensity);

            if (circles.IsNullOrEmpty())
            {
                circles.Add(new Circle(startCirclePosition, Quaternion.identity, radius));
            }
            Circle last = circles[^1];
            if (curvature == 0f)
            {
                Vector3 lastNormal = last.orientation * Vector3.forward;
                for (int i = 0; i < segments; i++)
                {
                    float t = (float)(i + 1) / (segments);
                    Vector3 center = last.center + lastNormal * length * t;
                    float newRadius = Mathf.Lerp(last.radius, radius, t);
                    circles.Add(new Circle(center, last.orientation, newRadius));
                }
            }
            else
            {
                float R = length * 360f / (curvature * Mathf.PI * 2f);

                Quaternion nn = last.orientation;
                nn = Quaternion.AngleAxis(angle, nn * Vector3.forward) * nn;

                Vector3 C = last.center - (nn * Vector3.up * R);
                Vector3 right = nn * Vector3.right;

                dummy.position = last.center;
                dummy.rotation = last.orientation;
                float step = 1f / segments * curvature;
                for (int i = 0; i < segments; i++)
                {
                    dummy.RotateAround(C, right, step);
                    float t = (float)(i + 1) / (segments);
                    float newRadius = Mathf.Lerp(last.radius, radius, t);
                    circles.Add(new Circle(dummy.position, dummy.rotation, newRadius));
                }
            }
        }

        void AddPieceMesh(int circleFrom, int circleTo)
        {
            Mesh mesh = MakeMesh(circleFrom, circleTo);
            MeshFilter mf = Instantiate(mfPrefab, piecesParent) as MeshFilter;
            chunks.Add(mf.gameObject);
            mf.mesh = mesh;
        }

        void AddRings(int circleFrom, int circleTo)
        {
            // create rings
            //int i = lastRingCircleIndex;
            //i += CircleOffset(i);
            //while (circleFrom <= i && i <= circleTo)
            //for (int i = CircleOffset(0); i < circles.Count;)
            Transform ringChunkParent = new GameObject("chunkparent").transform;
            ringChunkParent.parent = ringParent;
            chunkRings.Add(ringChunkParent.gameObject);

            for (int i = circleFrom; i <= circleTo; i+= CircleOffset(i))
            {
                // place circle
                Circle c = circles[i];
                Ring newRing = Instantiate(ringPrefab, ringChunkParent);
                newRing.transform.rotation = c.orientation;
                newRing.transform.position = c.center + (c.orientation * (Vector3)Random.insideUnitCircle) * (c.radius - ringRadius);
                newRing.SetRandom();

                lastRingCircleIndex = i;
                i += CircleOffset(i);
            }
        }

        void OnModeChanged(GameManager.Mode mode)
        {
            ringParent.gameObject.SetActive(mode == GameManager.Mode.Game);
        }

        // queries
        Mesh MakeMesh(int first, int last)
        {
            List<Vector3> verts = new();
            List<Vector2> uvs = new();
            List<int> indices = new();
            int vi = 0;
            Vector2 uv = Vector2.zero;
            for (int i = first; i <= last; i++)
            {
                Circle c = circles[i];
                float step = 1f / steps * Mathf.PI * 2f;
                for (int s = 0; s <= steps; s++)
                {
                    float a = s * step;
                    verts.Add(c.center + Mathf.Cos(a) * (c.orientation * Vector3.right) * c.radius + Mathf.Sin(a) * (c.orientation * Vector3.up) * c.radius);
                    uvs.Add(uv);
                    uv.x += 0.5f;
                }
                if (i > first)
                {
                    for (int s = 0; s < steps; s++)
                    {

                        int a = vi + s;//(vi + s%steps);
                        int b = vi + s + 1;//(vi + (s+1)%steps);
                        indices.AddRange(new int[] { a, b, b+steps+1, a + steps+1 });
                    }
                    vi += steps+1;
                }
                uv.y += 0.5f;
                uv.x = 0f;
            }
            Mesh mesh = new ();// Utility.MakeMesh(verts, indices)
            mesh.SetVertices(verts);
            mesh.SetIndices(indices.ToArray(), MeshTopology.Quads, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.triangles = mesh.triangles.Reverse().ToArray();
        
            mesh.RecalculateTangents();
            mesh.Optimize();
            mesh.OptimizeIndexBuffers();
            mesh.OptimizeReorderVertexBuffer();
            return mesh;
        }

        public Circle GetNextCircle()
        {
            CheckDeletion(nextIndex - 1);
            // notify we got over nextIndex
            return circles[nextIndex++];
        }

        int CircleOffset(int i)
        {
            return Random.Range(randomRingOffset.x, randomRingOffset.y);
        }

        void CheckDeletion(int index)
        {
            if (index <= 2) return;
            if (lastChunkDeletedIndex + 3 >= chunksCount.Count) return;
            if (index > chunksCount[lastChunkDeletedIndex+3])
            {
                // delete last chunk
                ++lastChunkDeletedIndex;
                //Debug.Log("Destroyed chunk " + lastChunkDeletedIndex);
                Destroy(chunkRings[lastChunkDeletedIndex]);
                chunkRings[lastChunkDeletedIndex] = null;
                Destroy(chunks[lastChunkDeletedIndex]);
                chunks[lastChunkDeletedIndex] = null;

                // create new chunk
                Add1PiecesAll();
            }
        }



        // other
        [EditorButton]
        [LayoutBeginHorizontal]
        void EDITOR_AddPiece()
        {
            AddPieceCircles(length, curvature, angle, radius, segmentsDensity);
        }
        [EditorButton]
        void EDITOR_AddRandomPiece()
        {
            AddRandomPieceCircles();
        }
        [EditorButton]
        [LayoutEndHorizontal]
        void EDITOR_Clear()
        {
            circles = new();
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
            //Gizmos.color = Color.green;
            //foreach(Circle circle in circles)
            //{
            //    Gizmos2.DrawWireCircle(circle.center, circle.radius, circle.orientation * Vector3.forward);
            //}
        }
    }
}