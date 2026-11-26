using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ConeGenerator : MonoBehaviour
{
    public float rayon = 1f;            
    public float hauteur = 2f;          
    public float hauteurTronque = 0f;   
    [Range(3, 128)] public int nbMeridiens = 24;

    void Start()
    {
        GetComponent<MeshFilter>().mesh = Cone(rayon, hauteur, hauteurTronque, nbMeridiens);
    }

    
    public static Mesh Cone(float rayon, float hauteur, float hauteurTronque, int nbMeridiens)
    {
        int n = Mathf.Max(3, nbMeridiens);
        hauteur = Mathf.Max(hauteur, 0.0001f);

        bool tronque = hauteurTronque > 0f && hauteurTronque < hauteur;
        float hHaut = tronque ? hauteurTronque : hauteur;
        
        float rHaut = tronque ? rayon * (1f - hauteurTronque / hauteur) : 0f;

        List<Vector3> sommets = new List<Vector3>();
        List<Vector3> normales = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

       
        for (int i = 0; i <= n; i++)
        {
            float theta = 2f * Mathf.PI * i / n;
            float cos = Mathf.Cos(theta), sin = Mathf.Sin(theta);
            sommets.Add(new Vector3(rayon * cos, 0f, rayon * sin));
            normales.Add(NormaleLaterale(cos, sin, rayon, hauteur));
            uvs.Add(new Vector2((float)i / n, 0f));
        }

        int baseHaut = sommets.Count; 

        if (tronque)
        {
           
            for (int i = 0; i <= n; i++)
            {
                float theta = 2f * Mathf.PI * i / n;
                float cos = Mathf.Cos(theta), sin = Mathf.Sin(theta);
                sommets.Add(new Vector3(rHaut * cos, hHaut, rHaut * sin));
                normales.Add(NormaleLaterale(cos, sin, rayon, hauteur));
                uvs.Add(new Vector2((float)i / n, 1f));
            }

            
            for (int i = 0; i < n; i++)
            {
                int bi = i, bj = i + 1;
                int hi = baseHaut + i, hj = baseHaut + i + 1;
                triangles.Add(bi); triangles.Add(hi); triangles.Add(hj);
                triangles.Add(bi); triangles.Add(hj); triangles.Add(bj);
            }
        }
        else
        {
           
            for (int i = 0; i < n; i++)
            {
                float thetaMilieu = 2f * Mathf.PI * (i + 0.5f) / n;
                sommets.Add(new Vector3(0f, hauteur, 0f));
                normales.Add(NormaleLaterale(Mathf.Cos(thetaMilieu), Mathf.Sin(thetaMilieu), rayon, hauteur));
                uvs.Add(new Vector2((i + 0.5f) / n, 1f));
            }

            
            for (int i = 0; i < n; i++)
            {
                triangles.Add(i);
                triangles.Add(baseHaut + i);
                triangles.Add(i + 1);
            }
        }

       
        AjouterDisque(sommets, normales, uvs, triangles, 0f, rayon, n, false);

       
        if (tronque)
            AjouterDisque(sommets, normales, uvs, triangles, hHaut, rHaut, n, true);

        Mesh mesh = new Mesh();
        mesh.name = tronque ? "Tronc de cone" : "Cone";
        if (sommets.Count > 65535)
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(sommets);
        mesh.SetNormals(normales);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    
    static Vector3 NormaleLaterale(float cos, float sin, float rayon, float hauteur)
    {
        return new Vector3(hauteur * cos, rayon, hauteur * sin).normalized;
    }

    
    static void AjouterDisque(List<Vector3> sommets, List<Vector3> normales, List<Vector2> uvs,
                              List<int> triangles, float y, float rayon, int n, bool versHaut)
    {
        Vector3 nor = versHaut ? Vector3.up : Vector3.down;

        int centre = sommets.Count;
        sommets.Add(new Vector3(0f, y, 0f));
        normales.Add(nor);
        uvs.Add(new Vector2(0.5f, 0.5f));

        for (int i = 0; i < n; i++)
        {
            float theta = 2f * Mathf.PI * i / n;
            sommets.Add(new Vector3(rayon * Mathf.Cos(theta), y, rayon * Mathf.Sin(theta)));
            normales.Add(nor);
            uvs.Add(new Vector2(0.5f + 0.5f * Mathf.Cos(theta), 0.5f + 0.5f * Mathf.Sin(theta)));
        }

        for (int i = 0; i < n; i++)
        {
            int a = centre + 1 + i;
            int b = centre + 1 + (i + 1) % n;
            if (versHaut) { triangles.Add(centre); triangles.Add(b); triangles.Add(a); }
            else { triangles.Add(centre); triangles.Add(a); triangles.Add(b); }
        }
    }
}