using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PlanGenerator : MonoBehaviour
{
    public enum Mode { Triangle, DeuxTriangles, Grille }

    public Mode mode = Mode.Grille;

    [Header("Paramètres de la grille")]
    public int Nb_Lignes = 10;
    public int Nb_Colonnes = 10;
    public float largeur = 10f;   // taille selon X
    public float profondeur = 10f; // taille selon Z

    void Start()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Plan procedural";

        switch (mode)
        {
            case Mode.Triangle: CreerTriangle(mesh); break;
            case Mode.DeuxTriangles: CreerPlan(mesh); break;
            case Mode.Grille: CreerGrille(mesh); break;
        }

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().mesh = mesh;
    }


    void CreerTriangle(Mesh mesh)
    {
        mesh.vertices = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 0)
        };

        mesh.triangles = new int[] { 0, 1, 2 };
        mesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0) };
    }


    void CreerPlan(Mesh mesh)
    {
        mesh.vertices = new Vector3[]
        {
            new Vector3(0, 0, 0), 
            new Vector3(1, 0, 0), 
            new Vector3(0, 0, 1), 
            new Vector3(1, 0, 1)  
        };
        mesh.triangles = new int[]
        {
            0, 2, 1,   
            1, 2, 3    
        };
        mesh.uv = new Vector2[]
        {
            new Vector2(0, 0), new Vector2(1, 0),
            new Vector2(0, 1), new Vector2(1, 1)
        };
    }


    void CreerGrille(Mesh mesh)
    {
        int nbSommetsParLigne = Nb_Colonnes + 1;
        Vector3[] sommets = new Vector3[(Nb_Lignes + 1) * nbSommetsParLigne];
        Vector2[] uvs = new Vector2[sommets.Length];

        for (int l = 0; l <= Nb_Lignes; l++)
        {
            for (int c = 0; c <= Nb_Colonnes; c++)
            {
                float u = (float)c / Nb_Colonnes;
                float v = (float)l / Nb_Lignes;
                int i = l * nbSommetsParLigne + c;
                sommets[i] = new Vector3(u * largeur, 0, v * profondeur);
                uvs[i] = new Vector2(u, v);
            }
        }

        int[] triangles = new int[Nb_Lignes * Nb_Colonnes * 6];
        int t = 0;
        for (int l = 0; l < Nb_Lignes; l++)
        {
            for (int c = 0; c < Nb_Colonnes; c++)
            {
                int i0 = l * nbSommetsParLigne + c;   
                int i1 = i0 + 1;                       
                int i2 = i0 + nbSommetsParLigne;       
                int i3 = i2 + 1;                       

                triangles[t++] = i0; triangles[t++] = i2; triangles[t++] = i1;
                triangles[t++] = i1; triangles[t++] = i2; triangles[t++] = i3;
            }
        }

       
        if (sommets.Length > 65535)
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.vertices = sommets;
        mesh.triangles = triangles;
        mesh.uv = uvs;
    }
}