using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoolBeans
{
    public sealed class LineRendererColliderGenerator : MonoBehaviour
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

        public void Generate()
        {
            MeshCollider collider = GetComponent<MeshCollider>();

            if (collider == null)
            {
                collider = gameObject.AddComponent<MeshCollider>();
            }

            
            
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
