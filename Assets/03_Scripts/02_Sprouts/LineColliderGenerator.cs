using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

using I32 = System.Int32;

namespace CoolBeans
{
    public sealed class LineColliderGenerator : MonoBehaviour
    {
        [SerializeField, HideInInspector] private LineRenderer lineRenderer;
        
        [SerializeField, HideInInspector] private PolygonCollider2D polygonCollider2D;

        [SerializeField, HideInInspector] private EdgeCollider2D edgeCollider2D;

        private void Reset()
        {
            lineRenderer = GetComponent<LineRenderer>();
            
            polygonCollider2D = transform.GetComponent<PolygonCollider2D>();
            edgeCollider2D    = transform.GetComponent<EdgeCollider2D>();
            
        }
        private void OnValidate()
        {
            lineRenderer = GetComponent<LineRenderer>();
            
            polygonCollider2D = transform.GetComponent<PolygonCollider2D>();
            edgeCollider2D    = transform.GetComponent<EdgeCollider2D>();
        }

        #if ODIN_INSPECTOR
        [Button]
        #endif
        public void Generate()
        {
            //MeshCollider collider = GetComponent<MeshCollider>();
            // if (collider == null)
            // {
            //     collider = gameObject.AddComponent<MeshCollider>();
            // }
            
            Vector3[] __points3D = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(__points3D);

            Matrix4x4 __worldToLocalMatrix = transform.worldToLocalMatrix;
            
            //Convert 3D points to 2D points
            Vector2[] __points2D = new Vector2[lineRenderer.positionCount];
            for (I32 __index = 0; __index < lineRenderer.positionCount; __index += 1)
            {
                Vector3 __point3D = __points3D[__index];
                __point3D = __worldToLocalMatrix.MultiplyPoint3x4(__point3D);
                
                __points2D[__index] = new(__point3D.x, __point3D.y);
            }

            edgeCollider2D.points = __points2D;



            // Mesh mesh = new Mesh();
            // lineRenderer.BakeMesh(mesh, true);
            //
            // // if you need collisions on both sides of the line, simply duplicate & flip facing the other direction!
            // // This can be optimized to improve performance ;)
            // int[] meshIndices = mesh.GetIndices(0);
            // int[] newIndices = new int[meshIndices.Length * 2];
            //
            // int j = meshIndices.Length - 1;
            // for (int i = 0; i < meshIndices.Length; i++)
            // {
            //     newIndices[i] = meshIndices[i];
            //     newIndices[meshIndices.Length + i] = meshIndices[j];
            // }
            // mesh.SetIndices(newIndices, MeshTopology.Triangles, 0);
            //
            // collider.sharedMesh = mesh;
        }
    }
}
