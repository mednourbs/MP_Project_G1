# Présentation du Projet - Moteur Physique Personnalisé

## 🎯 Objectifs du Projet

Ce projet répond aux exigences du cours de **Moteur Physique** en implémentant:

### 1. ✅ Objets 3D Pré-Fracturés
- **Mur fracturé** en grille paramétrique (4×3 fragments par défaut)
- Chaque fragment = objet indépendant avec:
  - Masse individuelle calculée
  - Centre d'inertie au centre géométrique
  - Tenseur d'inertie: `I_xx = (1/12) * m * (h² + d²)`
  - Corps rigide complet (position, rotation, vélocité)

### 2. ✅ Transformations Manuelles
- **Matrices 4×4** pour toutes les transformations
- Rotation: matrices construites manuellement (pas de Unity Transform)
- Translation: intégrée dans la 4ème colonne de la matrice
- Composition: multiplication manuelle de matrices
- Exemple de mouvement complet: sphère roule avec rotation réaliste

### 3. ✅ Contraintes Entre Fragments
- **Modèle de ressort rigide** entre fragments adjacents:
  ```
  Force: F = k * (longueur_actuelle - longueur_repos)
  ```
- **Mesure de déformation**:
  ```
  déformation = |longueur_actuelle - longueur_repos|
  ```
- **Seuil de rupture configurable**:
  ```
  rupture si déformation > seuil
  ```
- Contraintes horizontales et verticales entre tous les fragments adjacents

### 4. ✅ Énergie Stockée comme Impulsion
- **Calcul de l'énergie potentielle**:
  ```
  E = (1/2) * k * x²
  où k = rigidité du ressort
      x = déformation
  ```
- **Direction de l'impulsion**:
  ```
  direction = normalize(position_fragment2 - position_fragment1)
  ```
- **Application de l'impulsion** au moment de la rupture:
  ```
  ΔV = √(2E / m)
  impulse1 = direction * ΔV1
  impulse2 = -direction * ΔV2
  ```

## 🔬 Concepts Scientifiques Implémentés

### Physique des Corps Rigides
```
Position:     x(t+dt) = x(t) + v·dt + (1/6)·(k₁ + 2k₂ + 2k₃ + k₄)
Vélocité:     v(t+dt) = v(t) + a·dt + (1/6)·(j₁ + 2j₂ + 2j₃ + j₄)
Momentum:     P = m·v
              L = I·ω
Rotation:     R' = (I + Ω·dt)·R
              Ω = matrice skew-symmetric de ω
```

### Détection de Collision
```
Sphère-Plan:  d = |n·(p - p₀)|
              collision si d < rayon

Sphère-Box:   closest_point = clamp(sphere_pos, box_min, box_max)
              distance = |sphere_pos - closest_point|
              collision si distance < rayon

Projection:   proj_v(u) = (u·v / |v|²)·v
```

### Système de Contraintes
```
Ressort:      F = -k·x                    (Loi de Hooke)
Énergie:      E = (1/2)·k·x²              (Énergie potentielle)
Impulsion:    I = ∫F·dt = √(2mE)          (Conservation d'énergie)
```

## 📊 Architecture du Code

```
PhysicsSimulationController (Contrôleur principal)
    │
    ├── PhysicsSphere (Sphère avec physique RK4)
    │   └── CustomRigidBody
    │       ├── Masse, vélocité, momentum
    │       ├── Tenseur d'inertie
    │       └── Matrices de transformation
    │
    ├── SkateRamp (Rampe procédurale)
    │   ├── Mesh courbe généré
    │   └── Détection de collision
    │
    └── FracturedWall (Mur pré-fracturé)
        ├── FracturedFragment[] (12 fragments)
        │   └── CustomRigidBody (chacun)
        └── FragmentConstraint[] (contraintes)
            ├── Calcul de déformation
            ├── Énergie potentielle
            └── Rupture + impulsion

CollisionDetection (Système de collision)
    ├── SphereToPlane
    ├── SphereToBox
    ├── SphereToMesh
    └── ResolveCollision
```

## 📐 Formules Mathématiques Clés

### Tenseurs d'Inertie
| Objet   | Formule                                    | Code |
|---------|-----------------------------------------------|------|
| Sphère  | `I = (2/5)·m·r²`                             | `(2f/5f) * mass * radius * radius` |
| Box     | `Iₓₓ = (1/12)·m·(h²+d²)`                     | `(1f/12f) * mass * (h*h + d*d)` |

### Intégration RK4
```
k₁ = f(t, y)
k₂ = f(t + dt/2, y + k₁·dt/2)
k₃ = f(t + dt/2, y + k₂·dt/2)
k₄ = f(t + dt, y + k₃·dt)
y(t+dt) = y(t) + (dt/6)·(k₁ + 2k₂ + 2k₃ + k₄)
```

### Résolution de Collision
```
Vélocité relative:     vᵣₑₗ = v₂ - v₁
Vélocité normale:      vₙ = vᵣₑₗ · n
Impulsion normale:     j = -(1 + e)·vₙ / (1/m₁ + 1/m₂)
Impulsion friction:    jₜ = μ·j
```

## 🎮 Scénario de Simulation

| Temps | Événement | Physique Appliquée |
|-------|-----------|-------------------|
| 0.0s  | Initialisation | Création procédurale de tous les objets |
| 0.0-1.0s | Chute libre | Gravité: F = m·g, Intégration RK4 |
| 1.0-2.0s | Rampe | Collision courbe, Force normale, Friction → Torque |
| 2.0-3.0s | Sol plat | Friction cinétique, Roulement |
| 3.0s | IMPACT | Détection collision sphère-mur |
| 3.0-3.5s | Rupture | Contraintes vérifient déformation > seuil |
| 3.5-5.0s | Propagation | Ruptures en cascade, Impulsions E=½kx² |
| 5.0-10.0s | Chute fragments | Chaque fragment = corps rigide indépendant |

## 🔧 Technologies Utilisées

### Unity Engine
- Version: 2020.3 LTS ou supérieur
- Rendering Pipeline: Universal RP
- Langage: C# 8.0

### Bibliothèques
- **Aucune bibliothèque externe!**
- Toutes les mathématiques implémentées manuellement
- Pas de PhysX, Rigidbody, Collider, etc.

### Algorithmes
- **RK4**: Intégration numérique de 4ème ordre
- **Gram-Schmidt**: Orthonormalisation de matrices
- **SAT** (implicite): Separation Axis Theorem pour collisions
- **Barycentric**: Coordonnées pour point-dans-triangle

## 📈 Performances

### Complexité
- Intégration physique: **O(n)** où n = nombre de corps rigides
- Détection collision: **O(n·m)** où m = nombre de surfaces
- Contraintes: **O(c)** où c = nombre de contraintes

### Optimisations
- RK4 permet des pas de temps plus grands (stabilité)
- Détection de collision avec early-exit
- Fragments désactivés après stabilisation
- Contraintes rompues ne sont plus calculées

### Métriques
- FPS cible: 60
- Temps physique: 0.02s (50Hz)
- Fragments max: 20 (configurable)
- Contraintes max: ~40 (2×fragments)

## 📝 Validation des Exigences

| Exigence | Status | Implémentation |
|----------|--------|----------------|
| Objets 3D pré-fracturés | ✅ | `FracturedWall.cs` - 12 fragments avec masse et inertie |
| Transformations manuelles | ✅ | Matrices 4×4 dans `CustomRigidBody.cs` |
| Rotation et translation | ✅ | Composition matricielle manuelle |
| Contraintes = ressort | ✅ | `FragmentConstraint` avec F = k·x |
| Mesure déformation | ✅ | `GetDeformation()` = |l - l₀| |
| Seuil de rupture | ✅ | `breakThreshold` paramétrable |
| Énergie E = ½kx² | ✅ | `GetStoredEnergy()` |
| Direction impulsion | ✅ | Vecteur entre fragments |
| Impulsion ΔV = √(2E/m) | ✅ | `Break()` applique la formule |

## 🎓 Concepts Pédagogiques

Ce projet illustre:
1. **Dynamique des corps rigides** (undergraduate physics)
2. **Algèbre linéaire** (matrices, vecteurs, transformations)
3. **Analyse numérique** (méthodes d'intégration)
4. **Géométrie computationnelle** (détection de collision)
5. **Mécanique des contraintes** (ressorts, rupture)
6. **Conservation de l'énergie** (transfert potentiel → cinétique)
7. **Programmation orientée objet** (architecture logicielle)

## 👨‍💻 Auteur

**MedNour**  
Projet de Moteur Physique  
ESPRIT - École Supérieure Privée d'Ingénierie et de Technologies

---

*Tous les calculs physiques sont implémentés from scratch, sans utiliser les composants Unity prédéfinis (Rigidbody, Collider, Joints, etc.)*
