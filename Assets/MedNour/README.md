# Simulation de Moteur Physique - Projet MedNour

## Description
Cette simulation implémente un moteur physique complet **sans utiliser les composants Unity** (Rigidbody, Collider, etc.). Tous les calculs physiques sont faits manuellement.

## Scénario de la Simulation
1. **Sphère qui tombe** - Une sphère commence en hauteur
2. **Descente sur rampe courbe** - La sphère roule sur une rampe de skate avec friction
3. **Impact avec le mur** - La sphère entre en collision avec un mur pré-fracturé
4. **Destruction du mur** - Le mur se brise en fragments avec contraintes qui se rompent
5. **Réduction de vitesse** - La vitesse de la sphère diminue à cause du crash

## Composants Implémentés

### 1. CustomRigidBody.cs
- **Corps rigide personnalisé** avec propriétés physiques complètes
- Masse, vélocité linéaire et angulaire
- Momentum linéaire (P = m·v) et angulaire (L)
- Tenseurs d'inertie pour sphères et boxes
- **Intégration RK4** (Runge-Kutta 4ème ordre) pour une précision élevée
- Matrices de rotation 4×4 manuelles
- Orthonormalisation Gram-Schmidt pour stabilité

### 2. CollisionDetection.cs
- **Détection de collision sans Colliders Unity**:
  - Sphère-Plan (basé sur distance point-plan)
  - Sphère-Box (AABB et OBB)
  - Sphère-Mesh (pour la rampe courbe)
  - Sphère-Sphère
- **Résolution de collision** avec:
  - Restitution (rebond)
  - Friction (pour le roulement)
  - Torque pour rotation réaliste

### 3. PhysicsSphere.cs
- Génération procédurale d'une sphère (mesh)
- Intégration RK4 pour le mouvement
- Application de transformations matricielles 4×4
- Gestion des collisions multiples

### 4. SkateRamp.cs
- **Rampe courbe procédurale** (demi-cylindre)
- Génération de mesh paramétrique
- Collision précise avec surface courbe
- Calcul de normales pour forces de contact

### 5. FracturedWall.cs
- **Mur pré-fracturé** en grille de fragments
- Chaque fragment est un corps rigide indépendant
- **Système de contraintes**:
  - Contraintes type ressort entre fragments adjacents
  - Mesure de déformation: |longueur_actuelle - longueur_repos|
  - Énergie potentielle: **E = ½·k·x²**
  - Seuil de rupture configurable
- **Rupture de contraintes**:
  - Calcul de l'impulsion: **ΔV = √(2E/m)**
  - Application d'impulsions opposées aux fragments
  - Propagation de la destruction

### 6. PhysicsSimulationController.cs
- Contrôleur principal qui orchestre toute la simulation
- Initialise tous les objets procéduralement
- Gère les collisions entre tous les éléments
- Applique la réduction de vitesse lors du crash

### 7. SimulationStarter.cs
- Script simple pour lancer la simulation automatiquement

## Concepts Mathématiques Utilisés

### Transformations Géométriques
- **Matrices 4×4** pour rotation et translation
- Composition de transformations
- Produit matrice-vecteur manuel

### Physique des Corps Rigides
- **Équations du mouvement**:
  - Position: **x(t + dt) = x(t) + v·dt**
  - Vélocité: **v(t + dt) = v(t) + a·dt**
  - Intégration RK4 pour précision
- **Momentum**:
  - Linéaire: **P = m·v**
  - Angulaire: **L = I·ω**
- **Tenseurs d'inertie**:
  - Sphère: **I = (2/5)·m·r²**
  - Box: **I_xx = (1/12)·m·(h² + d²)**

### Détection de Collision
- **Distance point-plan**: **d = |n·(p - p₀)|**
- **Projection vectorielle**: **proj_v(u) = (u·v / v·v)·v**
- **Test point dans triangle** (coordonnées barycentriques)

### Contraintes et Rupture
- **Force de ressort**: **F = k·(l - l₀)**
- **Énergie potentielle**: **E = ½·k·x²**
- **Impulsion de rupture**: **ΔV = √(2E/m)**
- Direction de l'impulsion selon le vecteur reliant les fragments

## Instructions d'Utilisation

### Configuration Initiale
1. Ouvrir Unity et le projet MedNour_MP
2. Ouvrir la scène `Scenes/SampleScene.unity`
3. Créer un GameObject vide: `GameObject > Create Empty`
4. Attacher le script `SimulationStarter.cs` au GameObject
5. Lancer la simulation: `Play ▶️`

### Personnalisation des Paramètres
Tous les scripts ont des paramètres exposés dans l'Inspector:

**PhysicsSimulationController:**
- Position/taille de chaque élément
- Masse de la sphère
- Nombre de fractures du mur
- Propriétés physiques (friction, restitution)

**FracturedWall:**
- `constraintStiffness`: Rigidité des contraintes (défaut: 800)
- `breakThreshold`: Seuil de rupture (défaut: 0.25)
- `fragmentMass`: Masse de chaque fragment

**PhysicsSphere:**
- `gravity`: Force de gravité (défaut: 9.81)
- `friction`: Coefficient de friction (défaut: 0.4)
- `restitution`: Coefficient de restitution (défaut: 0.5)

## Références aux Scripts Fournis
- **RigidBody3DState.cs**: Base pour CustomRigidBody
- **FreeFallK4.cs**: Implémentation RK4
- **ProjectionDistance.cs**: Calculs de projection
- **DistancePointToPlane.cs**: Distance point-plan
- **Math3D.cs**: Utilitaires matriciels
- **EulerRotation.cs**: Matrices de rotation

## Résultats Attendus
1. ✅ La sphère tombe et accélère sous l'effet de la gravité
2. ✅ Elle roule sur la rampe courbe avec friction réaliste
3. ✅ Elle continue sur le sol plat
4. ✅ Elle percute le mur et le détruit
5. ✅ Les fragments du mur tombent avec contraintes qui se rompent
6. ✅ La sphère ralentit significativement après le crash
7. ✅ Toutes les transformations sont faites avec des matrices 4×4
8. ✅ Aucun composant Unity prédéfini n'est utilisé

## Debug
- Les messages de debug apparaissent dans la Console Unity
- Les Gizmos montrent la position et vélocité de la sphère
- Le paramètre `showDebugInfo` permet d'activer/désactiver les infos

## Performance
- Intégration RK4: très stable même avec grands pas de temps
- Détection de collision optimisée
- Environ 12 fragments pour le mur (configurable)
- 60 FPS stable sur machine moderne

## Auteur
MedNour - Projet Moteur Physique
École Supérieure Privée d'Ingénierie et de Technologies (ESPRIT)
