# Modélisation procédurale de maillages sous Unity

Série d'exercices de géométrie 3D en C# : construction « à la main » de maillages (`Mesh`) à partir de sommets, de normales, de coordonnées de texture (UV) et de triangles. Aucun objet primitif d'Unity n'est utilisé.

## Contenu

| Fichier | Exercice | Description |
|---|---|---|
| `PlanGenerator.cs` | 1 – Plan | Un triangle, un plan de 2 triangles, ou une grille `Nb_Lignes × Nb_Colonnes` |
| `CylindreGenerator.cs` | 2 – Cylindre | Cylindre fermé par deux disques, paramétré par le rayon, la hauteur et le nombre de méridiens |
| `SphereGenerator.cs` | 3 – Sphère | Sphère UV paramétrée par le rayon, le nombre de parallèles et de méridiens |
| `ConeGenerator.cs` | 4 – Cône | Cône complet ou tronqué en hauteur |
| `ObjetsTronquesGenerator.cs` | 5 – Objets tronqués | Plan, cylindre, sphère et cône avec un secteur angulaire retiré (sphère « Pac-Man ») |

## Mise en place

1. Créer un projet Unity en **Universal 3D** (URP).
2. Copier les scripts `.cs` dans le dossier `Assets`.
3. Créer un GameObject vide (`GameObject > Create Empty`) et lui ajouter le script voulu. Les composants `MeshFilter` et `MeshRenderer` sont ajoutés automatiquement.
4. Affecter au `MeshRenderer` un matériau utilisant le shader **Universal Render Pipeline/Lit**, sinon l'objet apparaît rose.
5. Lancer la scène (Play). Le maillage est généré dans `Start()`.

Pour visualiser les triangles, activer le mode **Wireframe** dans le menu « Shading Mode » de la vue Scene.

## Paramètres

| Script | Paramètres |
|---|---|
| `PlanGenerator` | `mode` (Triangle, DeuxTriangles, Grille), `Nb_Lignes`, `Nb_Colonnes`, `largeur`, `profondeur` |
| `CylindreGenerator` | `rayon`, `hauteur`, `nbMeridiens` |
| `SphereGenerator` | `rayon`, `nbParalleles` (pôles exclus), `nbMeridiens` |
| `ConeGenerator` | `rayon`, `hauteur`, `hauteurTronque` (0 ou ≥ `hauteur` : cône complet), `nbMeridiens` |
| `ObjetsTronquesGenerator` | `forme`, `angleOuverture` (degrés, 0 = objet complet), plus les paramètres de la forme choisie |

Chaque générateur est aussi disponible sous forme de méthode statique (`Cylindre`, `Sphere`, `Cone`, `Plan`) qui renvoie un `Mesh`, réutilisable depuis un autre script.

## Principes de construction

**Conventions**
- Les objets sont d'axe Y.
- Unity affiche la face avant d'un triangle quand ses sommets sont dans le **sens horaire** vu de la caméra. Si une face est invisible, il suffit de permuter deux indices.
- Un point d'un cercle de rayon `ρ` à l'angle `θ` se place en `(ρ·cosθ, y, ρ·sinθ)`.

**Plan.** Une grille de `(Nb_Lignes + 1) × (Nb_Colonnes + 1)` sommets. Chaque case est découpée en 2 triangles. Au-delà de 65 535 sommets, le format d'indices 32 bits est activé.

**Cylindre.** Deux anneaux de `n + 1` sommets (le dernier duplique le premier pour la couture UV), deux triangles par facette. Les disques sont des éventails autour d'un sommet central. Les sommets des disques et ceux du corps sont dupliqués, car leurs normales diffèrent : l'arête reste ainsi nette.

**Sphère.** Les parallèles sont des cercles de latitude (`φ` de 0 au pôle Nord à π au pôle Sud), les pôles étant exclus du décompte. Chaque pôle est relié au parallèle voisin par un éventail de triangles, ce qui évite les triangles dégénérés. Chaque pôle a un sommet par méridien, pour obtenir des UV correctes. La normale d'un sommet est sa position normalisée.

**Cône.** Le rayon diminue linéairement de `rayon` à 0 sur la hauteur : à la hauteur de coupe `hc`, il vaut `rayon · (1 − hc / hauteur)`. Un cône complet se termine par une pointe (un sommet par fuseau, un triangle par facette). Un cône tronqué a deux anneaux et deux disques. Les normales latérales sont inclinées : `(h·cosθ, r, h·sinθ)` normalisé.

**Objets tronqués.** Les anneaux ne parcourent plus un tour complet, mais l'arc `[θ₀, θ₀ + arc]` avec `θ₀ = ouverture / 2` et `arc = 2π − ouverture`. La bouche est donc centrée sur l'axe +X. Quand l'ouverture est nulle, on retrouve l'objet complet. Dans le cas contraire :
- les disques deviennent des secteurs de disque ;
- deux faces de coupe planes ferment l'objet : un rectangle pour le cylindre, un triangle ou un trapèze pour le cône, un demi-disque pour la sphère ;
- ces faces ont leurs propres sommets, avec une normale plate perpendiculaire à la coupe ;
- le sens des triangles de ces faces est corrigé automatiquement en comparant la normale calculée à la normale sortante attendue.

Pour le plan, les cases dont le centre est dans le secteur retiré ne reçoivent pas de triangles. Le bord de la découpe est donc en escalier, d'autant plus fin que la grille est dense.

## Vérification par comptage de triangles

| Objet | Complet | Tronqué en angle |
|---|---|---|
| Grille | `2 · Nb_Lignes · Nb_Colonnes` | variable (cases retirées) |
| Cylindre | `4n` | `4n + 4` |
| Sphère (`p` parallèles) | `2·n·p` | `2·n·p + 2·p` |
| Cône complet | `2n` | `2n + 2` |
| Tronc de cône | `4n` | `4n + 4` |

où `n` est le nombre de méridiens. Ces valeurs se comparent avec les statistiques de la vue Game (« Tris »).

## Dépannage

- **Objet rose** : le matériau n'utilise pas un shader URP (voir « Mise en place »).
- **Face invisible ou intérieur visible** : le sens horaire des indices est inversé ; permuter deux indices du triangle concerné.
- **Ombrage étrange sur une arête** : vérifier que les sommets des surfaces de normales différentes sont bien dupliqués.
