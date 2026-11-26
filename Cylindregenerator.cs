using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CylindreGenerator : MonoBehaviour
{
    public float rayon = 1f;
    public float hauteur = 2f;
    [Range(3, 128)] public int nbMeridiens = 16;

    void Start()
    {
        GetComponent<MeshFilter>().mesh = Cylindre(rayon, hauteur, nbMeridiens);
    }

    
    public static Mesh Cylindre(float rayon, float hauteur, int nbMeridiens)
    {
        int n = Mathf.Max(3, nbMeridiens);
        float yBas = -hauteur / 2f;
        float yHaut = hauteur / 2f;

     
        int nbSommets = 2 * (n + 1) + 2 * (n + 1);
        Vector3[] sommets = new Vector3[nbSommets];
        Vector3[] normales = new Vector3[nbSommets];
        Vector2[] uvs = new Vector2[nbSommets];

        int[] triangles = new int[4 * n * 3]; 

      
        for (int i = 0; i <= n; i++)
        {
            float theta = 2f * Mathf.PI * i / n;
            float cos = Mathf.Cos(theta);
            float sin = Mathf.Sin(theta);
            Vector3 nor = new Vector3(cos, 0, sin);

            int bas = i;
            int haut = (n + 1) + i;

            sommets[bas] = new Vector3(rayon * cos, yBas, rayon * sin);
            sommets[haut] = new Vector3(rayon * cos, yHaut, rayon * sin);
            normales[bas] = nor;
            normales[haut] = nor;
            uvs[bas] = new Vector2((float)i / n, 0f);
            uvs[haut] = new Vector2((float)i / n, 1f);
        }

        int t = 0;
        for (int i = 0; i < n; i++)
        {
            int bi = i, bj = i + 1;
            int hi = (n + 1) + i, hj = (n + 1) + i + 1;

            
            triangles[t++] = bi; triangles[t++] = hi; triangles[t++] = hj;
            triangles[t++] = bi; triangles[t++] = hj; triangles[t++] = bj;
        }

        
        int centreHaut = 2 * (n + 1);
        sommets[centreHaut] = new Vector3(0, yHaut, 0);
        normales[centreHaut] = Vector3.up;
        uvs[centreHaut] = new Vector2(0.5f, 0.5f);

        for (int i = 0; i < n; i++)
        {
            float theta = 2f * Mathf.PI * i / n;
            int idx = centreHaut + 1 + i;
            sommets[idx] = new Vector3(rayon * Mathf.Cos(theta), yHaut, rayon * Mathf.Sin(theta));
            normales[idx] = Vector3.up;
            uvs[idx] = new Vector2(0.5f + 0.5f * Mathf.Cos(theta), 0.5f + 0.5f * Mathf.Sin(theta));
        }
        for (int i = 0; i < n; i++)
        {
            int a = centreHaut + 1 + i;
            int b = centreHaut + 1 + (i + 1) % n;
            
            triangles[t++] = centreHaut; triangles[t++] = b; triangles[t++] = a;
        }

        
        int centreBas = centreHaut + n + 1;
        sommets[centreBas] = new Vector3(0, yBas, 0);
        normales[centreBas] = Vector3.down;
        uvs[centreBas] = new Vector2(0.5f, 0.5f);

        for (int i = 0; i < n; i++)
        {
            float theta = 2f * Mathf.PI * i / n;
            int idx = centreBas + 1 + i;
            sommets[idx] = new Vector3(rayon * Mathf.Cos(theta), yBas, rayon * Mathf.Sin(theta));
            normales[idx] = Vector3.down;
            uvs[idx] = new Vector2(0.5f + 0.5f * Mathf.Cos(theta), 0.5f + 0.5f * Mathf.Sin(theta));
        }
        for (int i = 0; i < n; i++)
        {
            int a = centreBas + 1 + i;
            int b = centreBas + 1 + (i + 1) % n;
            
            triangles[t++] = centreBas; triangles[t++] = a; triangles[t++] = b;
        }

        Mesh mesh = new Mesh();
        mesh.name = "Cylindre";
        mesh.vertices = sommets;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        return mesh;
    }
}