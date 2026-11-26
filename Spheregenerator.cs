using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SphereGenerator : MonoBehaviour
{
    public float rayon = 1f;
    [Range(1, 128)] public int nbParalleles = 12; 
    [Range(3, 128)] public int nbMeridiens = 24;

    void Start()
    {
        GetComponent<MeshFilter>().mesh = Sphere(rayon, nbParalleles, nbMeridiens);
    }

   
    public static Mesh Sphere(float rayon, int nbParalleles, int nbMeridiens)
    {
        int p = Mathf.Max(1, nbParalleles);
        int n = Mathf.Max(3, nbMeridiens);

       
        int nbSommets = 2 * n + p * (n + 1);
        Vector3[] sommets = new Vector3[nbSommets];
        Vector3[] normales = new Vector3[nbSommets];
        Vector2[] uvs = new Vector2[nbSommets];

        int baseNord = 0;
        int baseParallele1 = n;
        int baseSud = n + p * (n + 1);

        for (int i = 0; i < n; i++)
        {
            float u = (i + 0.5f) / n;

            sommets[baseNord + i] = new Vector3(0, rayon, 0);
            normales[baseNord + i] = Vector3.up;
            uvs[baseNord + i] = new Vector2(u, 1f);

            sommets[baseSud + i] = new Vector3(0, -rayon, 0);
            normales[baseSud + i] = Vector3.down;
            uvs[baseSud + i] = new Vector2(u, 0f);
        }

   
        for (int k = 1; k <= p; k++)
        {
            float phi = Mathf.PI * k / (p + 1);
            float y = rayon * Mathf.Cos(phi);
            float rho = rayon * Mathf.Sin(phi); 

            for (int i = 0; i <= n; i++)
            {
                float theta = 2f * Mathf.PI * i / n;
                Vector3 pos = new Vector3(rho * Mathf.Cos(theta), y, rho * Mathf.Sin(theta));

                int idx = baseParallele1 + (k - 1) * (n + 1) + i;
                sommets[idx] = pos;
                normales[idx] = pos.normalized;
                uvs[idx] = new Vector2((float)i / n, 1f - (float)k / (p + 1));
            }
        }

      
        int[] triangles = new int[2 * n * p * 3];
        int t = 0;

        
        for (int i = 0; i < n; i++)
        {
            int pole = baseNord + i;
            int gauche = baseParallele1 + i;
            int droite = baseParallele1 + i + 1;
            triangles[t++] = pole; triangles[t++] = droite; triangles[t++] = gauche;
        }

        
        for (int k = 1; k < p; k++)
        {
            int baseHaut = baseParallele1 + (k - 1) * (n + 1);
            int baseBas = baseParallele1 + k * (n + 1);

            for (int i = 0; i < n; i++)
            {
                int hi = baseHaut + i, hj = hi + 1;
                int bi = baseBas + i, bj = bi + 1;

                triangles[t++] = bi; triangles[t++] = hi; triangles[t++] = hj;
                triangles[t++] = bi; triangles[t++] = hj; triangles[t++] = bj;
            }
        }

        
        int baseDernier = baseParallele1 + (p - 1) * (n + 1);
        for (int i = 0; i < n; i++)
        {
            int pole = baseSud + i;
            int gauche = baseDernier + i;
            int droite = baseDernier + i + 1;
            triangles[t++] = gauche; triangles[t++] = droite; triangles[t++] = pole;
        }

        Mesh mesh = new Mesh();
        mesh.name = "Sphere";
        if (nbSommets > 65535)
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = sommets;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        return mesh;
    }
}