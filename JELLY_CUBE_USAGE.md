# Guide Complet du Jelly Cube

## 📋 Table des Matières
1. [Installation](#installation)
2. [Contrôles](#contrôles)
3. [Paramètres Physiques](#paramètres-physiques)
4. [Équations Mathématiques](#équations-mathématiques)
5. [Dépannage](#dépannage)

---

## 🚀 Installation

### Étapes:
1. Créez une scène vide dans Unity
2. Créez un GameObject vide (GameObject > Create Empty)
3. Attachez le script `JellyCubeSimulator` au GameObject
4. Pressez Play

## 🎮 Contrôles

| Touche | Action |
|--------|--------|
| **W** | Déplacer le cube vers l'avant |
| **S** | Déplacer le cube vers l'arrière |
| **A** | Déplacer le cube vers la gauche |
| **D** | Déplacer le cube vers la droite |
| **Space** | Lancer le cube vers le haut et l'avant |
| **R** | Réinitialiser la simulation |
| **P** | Pause/Reprendre |

## ⚙️ Paramètres Physiques

### Configuration du Cube
- **Cube Center**: Position initiale du cube (défaut: 0, 2, 0)
- **Cube Size**: Taille totale du cube en unités (défaut: 2)
- **Grid Resolution**: Nombre de divisions par côté (défaut: 4 → 4×4×4 = 64 fragments)

### Paramètres de Physique
| Paramètre | Signification | Valeur Recommandée | Unité |
|-----------|---------------|-------------------|-------|
| **Spring Stiffness** | Raideur du ressort (k) | 100 | N/m |
| **Spring Damping** | Amortissement du ressort (d) | 10 | N·s/m |
| **Fragment Mass** | Masse de chaque fragment | 0.1 | kg |
| **Gravity** | Accélération gravitationnelle | 9.81 | m/s² |
| **Spring Break Threshold** | Seuil de rupture (extension relative) | 1.5 | (adimensionné) |
| **Collision Restitution** | Coefficient de restitution (rebond) | 0.3 | (0-1) |
| **Fragment Damping** | Amortissement du fragment | 0.98 | (0-1) |

### Intégration Numérique
- **Integration Method**: 
  - Verlet (stable, recommandé)
  - Semi-Implicit Euler (plus rapide)
- **Time Scale**: Facteur multiplicateur du temps (défaut: 1.0)

### Rendu
- **Use Gizmos For Debug**: Affiche les fragments et ressorts
- **Gizmo Size**: Taille des sphères gizmo
- **Fragment Color**: Couleur des fragments actifs
- **Fragment Broken Color**: Couleur des ressorts cassés
- **Spring Color**: Couleur des ressorts actifs
- **Spring Broken Color**: Couleur des ressorts cassés

## 📐 Équations Mathématiques

### 1. Loi de Hooke (Ressort)
```
F = -k(x - L₀) - d·v_rel
```
Où:
- F : Force totale
- k : Raideur du ressort
- x : Distance courante
- L₀ : Longueur au repos
- d : Coefficient d'amortissement
- v_rel : Vitesse relative entre fragments

### 2. Intégration Verlet
```
x_new = 2x - x_prev + a·dt²
v = (x_new - x_prev) / (2·dt)
```
Propriétés:
- Stable numériquement
- Préserve l'énergie
- Pas besoin de stocker la vitesse explicitement

### 3. Énergie Élastique
```
E_elastic = 1/2 · k · (Δx)²
```
Où Δx = x - L₀

### 4. Rupture du Ressort
```
Rupture si: (x - L₀) / L₀ > Seuil_rupture
```

### 5. Collision avec le Sol
```
Si y < y_sol:
  - y := y_sol (collision)
  - v_y := -v_y · e (rebond)
```
Où e = coefficient de restitution

### 6. Intégration Semi-Implicite Euler
```
v_new = v + a·dt
x_new = x + v_new·dt
```

## 🔧 Conseils pour l'Utilisation

### Pour un Cube Plus "Gelé" (Moins de Déformation)
- Augmenter **Spring Stiffness** (ex: 200)
- Réduire **Fragment Damping** (ex: 0.95)
- Augmenter **Fragment Mass** (ex: 0.5)

### Pour un Cube Plus "Fluidique" (Très Gélatineux)
- Réduire **Spring Stiffness** (ex: 50)
- Augmenter **Fragment Damping** (ex: 0.99)
- Réduire **Fragment Mass** (ex: 0.05)

### Pour un Cube Plus Rapide
- Augmenter **Time Scale** (ex: 2.0)
- Réduire **Integration Method** à Semi-Implicit Euler
- Augmenter **Grid Resolution** (moins de fragments = plus rapide)

### Pour une Simulation Plus Réaliste
- Utiliser **Integration Method** = Verlet
- Augmenter **Spring Damping** (ex: 20-30)
- Réduire **Collision Restitution** (ex: 0.1-0.2)
- Augmenter **Fragment Mass** (ex: 0.2-0.3)

## 🐛 Dépannage

### Le cube disparaît immédiatement
- ✅ Augmentez **Cube Center.y** (ex: 5)
- ✅ Vérifiez que **Gravity** est positif

### Le cube ne rebondit pas
- ✅ Augmentez **Collision Restitution** (ex: 0.5)
- ✅ Vérifiez que Ground Level est à y = 0

### Le cube se déforme trop
- ✅ Augmentez **Spring Stiffness**
- ✅ Augmentez **Spring Damping**
- ✅ Réduisez **Fragment Damping**

### Simulation trop lente
- ✅ Réduisez **Grid Resolution** (ex: 3 pour 3×3×3)
- ✅ Utilisez Semi-Implicit Euler
- ✅ Augmentez **Time Scale**

### Le cube se déforme bizarrement
- ✅ Assurez-vous que **Spring Damping** > 0
- ✅ Réduisez **Spring Break Threshold**
- ✅ Vérifiez les valeurs de masse (toutes identiques)

## 📊 Informations de Debug

Appuyez sur P pour afficher le panneau d'informations qui montre:
- Nombre total de fragments
- Nombre de ressorts actifs
- Nombre de ressorts cassés
- Énergie totale du système
- Temps de simulation

---

## 🎯 Paramètres de Préset Recommandés

### Préset: Jelly Cube Réaliste
```
Spring Stiffness: 150
Spring Damping: 25
Fragment Mass: 0.2
Gravity: 9.81
Spring Break Threshold: 1.2
Collision Restitution: 0.2
Fragment Damping: 0.97
Grid Resolution: 4
Integration Method: Verlet
```

### Préset: Jelly Cube Très Mou
```
Spring Stiffness: 30
Spring Damping: 5
Fragment Mass: 0.05
Gravity: 9.81
Spring Break Threshold: 2.5
Collision Restitution: 0.1
Fragment Damping: 0.99
Grid Resolution: 5
Integration Method: Verlet
```

### Préset: Cube Rigide
```
Spring Stiffness: 500
Spring Damping: 100
Fragment Mass: 1.0
Gravity: 9.81
Spring Break Threshold: 3.0
Collision Restitution: 0.5
Fragment Damping: 0.95
Grid Resolution: 3
Integration Method: Verlet
```

---

## 📚 Architecture du Code

### Classes Principales
1. **Fragment.cs**: Représente un point de masse unique
2. **SpringConstraint.cs**: Lien élastique entre deux fragments
3. **JellyCube.cs**: Orchestrateur de simulation
4. **JellyCubeSimulator.cs**: Intégration MonoBehaviour
5. **PhysicsIntegrator.cs**: Méthodes d'intégration numérique
6. **CollisionSolver.cs**: Détection et résolution de collisions

### Flux d'Exécution
```
Update() → 
  → ResetForces() 
  → ApplyGravity() 
  → ApplySprings() 
  → IntegrateMotion() 
  → SolveCollisions() 
  → CheckBrokenSprings()
```

---

**Dernier Update**: 10 Novembre 2025  
**Version**: 2.0 Jelly Cube Réaliste
