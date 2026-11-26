using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ObjetsTronquesGenerator : MonoBehaviour
{
    public enum Forme { Plan, Cylindre, Sphere, Cone }

    public Forme forme = Forme.Sphere;

    [Tooltip("Angle du secteur retiré, en degrés (0 = objet complet). La bouche est centrée sur +X.")]
    [Range(0f, 350f)] public float angleOuverture = 60f;

    public float rayon = 1f;
    public float hauteur = 2f;           // cylindre et cône
    public float hauteurTronque = 0f;    // cône : hauteur de coupe (0 = cône complet)

    [Range(3, 128)] public int nbMeridiens = 32;
    [Range(1, 128)] public int nbParalleles = 16;  // sphère (pôles exclus)

    public int Nb_Lignes = 20;       // plan
    public int Nb_Colonnes = 20;     // plan

    void Start()
    {
        Mesh mesh = null;
        switch (forme)
        {
            case Forme.Plan: mesh = Plan(rayon, Nb_Lignes, Nb_Colonnes, angleOuverture); break;
            case Forme.Cylindre: mesh = Cylindre(rayon, hauteur, nbMeridiens, angleOuverture); break;
            case Forme.Sphere: mesh = Sphere(rayon, nbParalleles, nbMeridiens, angleOuverture); break;
            case Forme.Cone: mesh = Cone(rayon, hauteur, hauteurTronque, nbMeridiens, angleOuverture); break;
        }
        GetComponent<MeshFilter>().mesh = mesh;
    }

    // =====================================================================
    //  Outils
    // =====================================================================

    class MeshData
    {
        public List<Vector3> sommets = new List<Vector3>();
        public List<Vector3> normales = new List<Vector3>();
        public List<Vector2> uvs = new List<Vector2>();
        public List<int> triangles = new List<int>();

        public int Compte { get { return sommets.Count; } }

        public int Ajouter(Vector3 p, Vector3 n, Vector2 uv)
        {
            sommets.Add(p); normales.Add(n); uvs.Add(uv);
            return sommets.Count - 1;
        }

        public void Triangle(int a, int b, int c)
        {
            triangles.Add(a); triangles.Add(b); triangles.Add(c);
        }

        // Ajoute le triangle dans le bon sens : sa normale géométrique
        // doit pointer du même côté que 'normaleSortante'.
        public void TriangleOriente(int a, int b, int c, Vector3 normaleSortante)
        {
            Vector3 n = Vector3.Cross(sommets[b] - sommets[a], sommets[c] - sommets[a]);
            if (Vector3.Dot(n, normaleSortante) < 0f) { int tmp = b; b = c; c = tmp; }
            Triangle(a, b, c);
        }

        public Mesh Construire(string nom)
        {
            Mesh mesh = new Mesh();
            mesh.name = nom;
            if (sommets.Count > 65535)
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(sommets);
            mesh.SetNormals(normales);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }
    }

    // Point de coordonnées cylindriques (rho, y, theta)
    static Vector3 Point(float rho, float y, float theta)
    {
        return new Vector3(rho * Mathf.Cos(theta), y, rho * Mathf.Sin(theta));
    }

    // Disque (ou secteur de disque) en éventail autour d'un sommet central.
    // Les n+1 sommets du bord parcourent l'arc [theta0, theta0 + arc].
    static void AjouterDisque(MeshData m, float y, float r, int n, float theta0, float arc, bool versHaut)
    {
        Vector3 nor = versHaut ? Vector3.up : Vector3.down;
        int centre = m.Ajouter(new Vector3(0, y, 0), nor, new Vector2(0.5f, 0.5f));
        int premier = m.Compte;
        for (int i = 0; i <= n; i++)
        {
            float th = theta0 + arc * i / n;
            m.Ajouter(Point(r, y, th), nor,
                      new Vector2(0.5f + 0.5f * Mathf.Cos(th), 0.5f + 0.5f * Mathf.Sin(th)));
        }
        for (int i = 0; i < n; i++)
        {
            int a = premier + i, b = premier + i + 1;
            if (versHaut) m.Triangle(centre, b, a);
            else m.Triangle(centre, a, b);
        }
    }

    // Face plane qui ferme l'objet à l'endroit de la coupe.
    // 'profil' : polygone convexe dans le plan (rho, y), un bord sur l'axe Y.
    // 'signe' = -1 pour la face de départ (theta0), +1 pour la face d'arrivée.
    static void AjouterFaceCoupe(MeshData m, List<Vector2> profil, float theta, float signe)
    {
        Vector3 tangente = new Vector3(-Mathf.Sin(theta), 0f, Mathf.Cos(theta));
        Vector3 nor = signe * tangente; // normale sortante

        int premier = m.Compte;
        foreach (Vector2 p in profil)
            m.Ajouter(Point(p.x, p.y, theta), nor, p);

        // éventail à partir du 1er point du profil
        for (int k = 1; k < profil.Count - 1; k++)
            m.TriangleOriente(premier, premier + k, premier + k + 1, nor);
    }

    static void AjouterFacesCoupe(MeshData m, List<Vector2> profil, float theta0, float arc)
    {
        AjouterFaceCoupe(m, profil, theta0, -1f);
        AjouterFaceCoupe(m, profil, theta0 + arc, +1f);
    }

    // =====================================================================
    //  Exercice 1 : plan (grille) avec un secteur retiré
    // =====================================================================
    public static Mesh Plan(float rayon, int nbLignes, int nbColonnes, float ouvertureDeg)
    {
        int L = Mathf.Max(1, nbLignes), C = Mathf.Max(1, nbColonnes);
        float ouv = ouvertureDeg * Mathf.Deg2Rad;
        MeshData m = new MeshData();

        for (int l = 0; l <= L; l++)
            for (int c = 0; c <= C; c++)
            {
                float u = (float)c / C, v = (float)l / L;
                m.Ajouter(new Vector3(-rayon + 2f * rayon * u, 0f, -rayon + 2f * rayon * v),
                          Vector3.up, new Vector2(u, v));
            }

        for (int l = 0; l < L; l++)
            for (int c = 0; c < C; c++)
            {
                // centre de la case, par rapport au centre du plan
                float cx = -rayon + 2f * rayon * (c + 0.5f) / C;
                float cz = -rayon + 2f * rayon * (l + 0.5f) / L;
                float angle = Mathf.Atan2(cz, cx);

                // case dans le secteur retiré : on ne crée pas ses triangles
                if (ouv > 0f && Mathf.Abs(angle) < ouv / 2f) continue;

                int i0 = l * (C + 1) + c, i1 = i0 + 1;
                int i2 = i0 + (C + 1), i3 = i2 + 1;
                m.Triangle(i0, i2, i1);
                m.Triangle(i1, i2, i3);
            }

        return m.Construire("Plan tronque");
    }

    // =====================================================================
    //  Exercice 2 : cylindre
    // =====================================================================
    public static Mesh Cylindre(float rayon, float hauteur, int nbMeridiens, float ouvertureDeg)
    {
        int n = Mathf.Max(3, nbMeridiens);
        float ouv = ouvertureDeg * Mathf.Deg2Rad;
        float theta0 = ouv / 2f;
        float arc = 2f * Mathf.PI - ouv;
        float yBas = -hauteur / 2f, yHaut = hauteur / 2f;

        MeshData m = new MeshData();

        // Corps : deux anneaux de n+1 sommets (arc ouvert ou couture si ouv = 0)
        int baseBas = m.Compte;
        for (int i = 0; i <= n; i++)
        {
            float th = theta0 + arc * i / n;
            m.Ajouter(Point(rayon, yBas, th), new Vector3(Mathf.Cos(th), 0, Mathf.Sin(th)),
                      new Vector2((float)i / n, 0f));
        }
        int baseHaut = m.Compte;
        for (int i = 0; i <= n; i++)
        {
            float th = theta0 + arc * i / n;
            m.Ajouter(Point(rayon, yHaut, th), new Vector3(Mathf.Cos(th), 0, Mathf.Sin(th)),
                      new Vector2((float)i / n, 1f));
        }
        for (int i = 0; i < n; i++)
        {
            int bi = baseBas + i, bj = bi + 1;
            int hi = baseHaut + i, hj = hi + 1;
            m.Triangle(bi, hi, hj);
            m.Triangle(bi, hj, bj);
        }

        // Disques (ou secteurs de disque)
        AjouterDisque(m, yHaut, rayon, n, theta0, arc, true);
        AjouterDisque(m, yBas, rayon, n, theta0, arc, false);

        // Faces de coupe : rectangle entre l'axe et la paroi
        if (ouv > 0.0001f)
        {
            List<Vector2> profil = new List<Vector2>
            {
                new Vector2(0, yBas), new Vector2(rayon, yBas),
                new Vector2(rayon, yHaut), new Vector2(0, yHaut)
            };
            AjouterFacesCoupe(m, profil, theta0, arc);
        }

        return m.Construire("Cylindre tronque");
    }

    // =====================================================================
    //  Exercice 3 : sphère (Pac-Man)
    // =====================================================================
    public static Mesh Sphere(float rayon, int nbParalleles, int nbMeridiens, float ouvertureDeg)
    {
        int p = Mathf.Max(1, nbParalleles);
        int n = Mathf.Max(3, nbMeridiens);
        float ouv = ouvertureDeg * Mathf.Deg2Rad;
        float theta0 = ouv / 2f;
        float arc = 2f * Mathf.PI - ouv;

        MeshData m = new MeshData();

        // Pôle Nord : un sommet par fuseau
        int baseNord = m.Compte;
        for (int i = 0; i < n; i++)
            m.Ajouter(new Vector3(0, rayon, 0), Vector3.up, new Vector2((i + 0.5f) / n, 1f));

        // Parallèles
        int baseRings = m.Compte;
        List<Vector2> profil = new List<Vector2>();
        profil.Add(new Vector2(0, rayon)); // pôle Nord, début du profil de coupe
        for (int k = 1; k <= p; k++)
        {
            float phi = Mathf.PI * k / (p + 1);
            float y = rayon * Mathf.Cos(phi);
            float rho = rayon * Mathf.Sin(phi);
            profil.Add(new Vector2(rho, y));

            for (int i = 0; i <= n; i++)
            {
                float th = theta0 + arc * i / n;
                Vector3 pos = Point(rho, y, th);
                m.Ajouter(pos, pos.normalized, new Vector2((float)i / n, 1f - (float)k / (p + 1)));
            }
        }
        profil.Add(new Vector2(0, -rayon)); // pôle Sud

        // Pôle Sud
        int baseSud = m.Compte;
        for (int i = 0; i < n; i++)
            m.Ajouter(new Vector3(0, -rayon, 0), Vector3.down, new Vector2((i + 0.5f) / n, 0f));

        // Éventail Nord
        for (int i = 0; i < n; i++)
            m.Triangle(baseNord + i, baseRings + i + 1, baseRings + i);

        // Bandes
        for (int k = 1; k < p; k++)
        {
            int baseHaut = baseRings + (k - 1) * (n + 1);
            int baseBas = baseRings + k * (n + 1);
            for (int i = 0; i < n; i++)
            {
                int hi = baseHaut + i, hj = hi + 1;
                int bi = baseBas + i, bj = bi + 1;
                m.Triangle(bi, hi, hj);
                m.Triangle(bi, hj, bj);
            }
        }

        // Éventail Sud
        int dernier = baseRings + (p - 1) * (n + 1);
        for (int i = 0; i < n; i++)
            m.Triangle(dernier + i, dernier + i + 1, baseSud + i);

        // Faces de coupe : deux demi-disques (la "bouche")
        if (ouv > 0.0001f)
            AjouterFacesCoupe(m, profil, theta0, arc);

        return m.Construire("Sphere tronquee");
    }

    // =====================================================================
    //  Exercice 4 : cône (tronqué en hauteur et/ou en angle)
    // =====================================================================
    public static Mesh Cone(float rayon, float hauteur, float hauteurTronque, int nbMeridiens, float ouvertureDeg)
    {
        int n = Mathf.Max(3, nbMeridiens);
        hauteur = Mathf.Max(hauteur, 0.0001f);
        float ouv = ouvertureDeg * Mathf.Deg2Rad;
        float theta0 = ouv / 2f;
        float arc = 2f * Mathf.PI - ouv;

        bool tronque = hauteurTronque > 0f && hauteurTronque < hauteur;
        float hHaut = tronque ? hauteurTronque : hauteur;
        float rHaut = tronque ? rayon * (1f - hauteurTronque / hauteur) : 0f;

        MeshData m = new MeshData();

        int baseBas = m.Compte;
        for (int i = 0; i <= n; i++)
        {
            float th = theta0 + arc * i / n;
            m.Ajouter(Point(rayon, 0f, th), NormaleLaterale(th, rayon, hauteur),
                      new Vector2((float)i / n, 0f));
        }

        int baseHaut = m.Compte;
        if (tronque)
        {
            for (int i = 0; i <= n; i++)
            {
                float th = theta0 + arc * i / n;
                m.Ajouter(Point(rHaut, hHaut, th), NormaleLaterale(th, rayon, hauteur),
                          new Vector2((float)i / n, 1f));
            }
            for (int i = 0; i < n; i++)
            {
                int bi = baseBas + i, bj = bi + 1;
                int hi = baseHaut + i, hj = hi + 1;
                m.Triangle(bi, hi, hj);
                m.Triangle(bi, hj, bj);
            }
        }
        else
        {
            // pointe : un sommet par fuseau
            for (int i = 0; i < n; i++)
            {
                float thMilieu = theta0 + arc * (i + 0.5f) / n;
                m.Ajouter(new Vector3(0, hauteur, 0), NormaleLaterale(thMilieu, rayon, hauteur),
                          new Vector2((i + 0.5f) / n, 1f));
            }
            for (int i = 0; i < n; i++)
                m.Triangle(baseBas + i, baseHaut + i, baseBas + i + 1);
        }

        // Disques
        AjouterDisque(m, 0f, rayon, n, theta0, arc, false);
        if (tronque)
            AjouterDisque(m, hHaut, rHaut, n, theta0, arc, true);

        // Faces de coupe
        if (ouv > 0.0001f)
        {
            List<Vector2> profil = new List<Vector2>();
            profil.Add(new Vector2(0, 0));
            profil.Add(new Vector2(rayon, 0));
            if (tronque) profil.Add(new Vector2(rHaut, hHaut));
            profil.Add(new Vector2(0, hHaut));
            AjouterFacesCoupe(m, profil, theta0, arc);
        }

        return m.Construire(tronque ? "Tronc de cone tronque" : "Cone tronque");
    }

    static Vector3 NormaleLaterale(float theta, float rayon, float hauteur)
    {
        return new Vector3(hauteur * Mathf.Cos(theta), rayon, hauteur * Mathf.Sin(theta)).normalized;
    }
}   