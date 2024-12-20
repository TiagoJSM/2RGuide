using Assets._2RGuide.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Assets._2RGuide.Runtime.Helpers;

namespace Assets._2RGuide.Editor
{
    public static class ColliderGeneration
    {
        private static readonly string RootCompositeName = "[NAV CLONE]";
        public static CompositeCollider2D GenerateComposite(GameObject root, LayerMask oneWayPlatformer)
        {
            var rootComposite = GetRootComposiveGameObject();
            var composite = rootComposite.AddComponent<CompositeCollider2D>();
            composite.geometryType = CompositeCollider2D.GeometryType.Outlines;
            composite.generationType = CompositeCollider2D.GenerationType.Synchronous;
            var colliders = GetColliders(root, oneWayPlatformer);
            
            foreach(var collider in colliders)
            {
                var go = new GameObject();
                go.layer = collider.gameObject.layer;
                go.transform.parent = rootComposite.transform;
                go.transform.SetPositionAndRotation(collider.transform.position, collider.transform.rotation);
                go.transform.localScale = collider.transform.lossyScale;
                AddCollider(go, collider, !oneWayPlatformer.Includes(go));
            }

            return composite;
        }

        private static void AddCollider(GameObject go, Collider2D collider, bool usedByComposite)
        {
            // BoxCollider2D can be copied to the GameObject without any additional processing
            if (collider is BoxCollider2D)
            {
                var addedComponent = go.AddComponent(collider.GetType()) as Collider2D;
                EditorUtility.CopySerialized(collider, addedComponent);
                addedComponent.usedByComposite = usedByComposite;
            }
            // PolygonCollider2D needs to be separated in different colliders per path, the reason is that 
            else if (collider is PolygonCollider2D polygonCollider)
            {
                AddPolygonCollider(go, polygonCollider, usedByComposite);
            }
        }

        private static void AddPolygonCollider(GameObject go, PolygonCollider2D polygonCollider, bool usedByComposite)
        {
            for (var pathIndex = 0; pathIndex < polygonCollider.pathCount; pathIndex++)
            {
                var path = polygonCollider.GetPath(pathIndex);
                var addedComponent = go.AddComponent(polygonCollider.GetType()) as PolygonCollider2D;
                EditorUtility.CopySerialized(polygonCollider, addedComponent);
                addedComponent.pathCount = 1;
                addedComponent.SetPath(0, path);
                addedComponent.usedByComposite = usedByComposite;
            }
        }

        public static void DestroyRootComposiveGameObject()
        {
            var go = GameObject.Find(RootCompositeName);
            if(go != null)
            {
                GameObject.DestroyImmediate(go);
            }
        }

        private static GameObject GetRootComposiveGameObject()
        {
            DestroyRootComposiveGameObject();
            var go = new GameObject(RootCompositeName);
            go.hideFlags = HideFlags.HideAndDontSave;
            return go;
        }

        private static Collider2D[] GetColliders(GameObject root, LayerMask oneWayPlatformer)
        {
            return 
                root
                    .GetComponentsInChildren<Collider2D>(false)
                    .Where(c => IsColliderValidForPathfinding(c) && c.GetComponent<NavTagBounds>() == null)
                    .ToArray();
        }

        private static bool IsColliderValidForPathfinding(Collider2D collider)
        {
            if (collider is BoxCollider2D)
            {
                return true;
            }
            else if (collider is PolygonCollider2D)
            {
                return true;
            }
            return false;
        }
    }
}
