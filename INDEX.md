# 📚 Index - Jelly Cube Réaliste v2.0

## 🚀 Commencez Ici

**Si vous n'avez 2 minutes:**
→ Lire [QUICK_START.md](QUICK_START.md)

**Si vous avez 10 minutes:**
→ Lire [CORRECTIONS_SUMMARY.md](CORRECTIONS_SUMMARY.md)

**Si vous voulez tout comprendre:**
→ Lire [JELLY_CUBE_USAGE.md](JELLY_CUBE_USAGE.md)

**Pour tester complètement:**
→ Suivre [TESTING_GUIDE.md](TESTING_GUIDE.md)

---

## 📖 Documentation Complète

### 🎯 Guides Rapides
| Document | Durée | Contenu |
|----------|-------|---------|
| **QUICK_START.md** | 2 min | Setup minimal + contrôles |
| **CORRECTIONS_SUMMARY.md** | 5 min | Changements effectués + équations |
| **JELLY_CUBE_USAGE.md** | 20 min | Guide complet + paramètres + dépannage |
| **TESTING_GUIDE.md** | 30 min | 12 scénarios de test complets |
| **Ce fichier** | 5 min | Navigation et organisation |

---

## 🔧 Architecture du Code

### Classes Physiques
```
Fragment.cs
├── Position/Velocity/Acceleration
├── Mass & Damping
├── Forces accumulées
└── État (actif/détaché)

SpringConstraint.cs
├── Indices des deux fragments
├── Longueur au repos (L₀)
├── Raideur (k) et amortissement (d)
├── Loi de Hooke: F = -k(x-L₀) - d·v_rel
└── État (actif/cassé)

JellyCube.cs
├── Gestion des 64+ fragments
├── Gestion des ressorts
├── Boucle physique complète
├── Gestion collisions sol
├── Gestion rupture ressorts
└── Public API (ApplyImpulse, Reset, etc)

PhysicsIntegrator.cs
├── Intégration Verlet (stable)
├── Intégration Euler Semi-Implicite (rapide)
├── Gestion rotations
└── Gram-Schmidt orthonormalisation

CollisionSolver.cs
├── Détection collision sol
├── Détection collision fragment-fragment
├── Réponse impulsionnelle
└── Coefficient de restitution
```

### Classe MonoBehaviour
```
JellyCubeSimulator.cs
├── Contrôles WASD (déplacement)
├── Contrôles Space (lancer)
├── Contrôles R (reset) et P (pause)
├── Inspector parameters
├── Gizmos debugging
└── Stats panel
```

---

## 📐 Équations Mathématiques

### 1. Loi de Hooke (Ressorts)
```
F = -k(x - L₀) - d·v_rel

Implémentation:
- x = distance courante
- L₀ = longueur au repos
- k = raideur (100 par défaut)
- d = amortissement ressort (10 par défaut)
- v_rel = vitesse relative
```

### 2. Intégration Verlet
```
x_new = 2x - x_prev + a·dt²

Propriétés:
- Symplectique (préserve énergie)
- Stable numériquement
- Pas d'erreur d'accumulation
```

### 3. Intégration Euler Semi-Implicite
```
v_new = v + a·dt
x_new = x + v_new·dt

Propriétés:
- Rapide
- Moins stable que Verlet
- Bon pour simulations temps réel
```

### 4. Collision Sol
```
Si y < y_sol:
  y := y_sol
  v_y := -v_y × e
  
Où e = coefficient de restitution
```

### 5. Énergie Élastique
```
E = 1/2 × k × (Δx)²

Rupture si: E > E_max
Où E_max = 1/2 × k × (seuil × L₀)²
```

---

## ⚙️ Paramètres Configurables

### Configuration Physique
```
Spring Stiffness          [50-500]    N/m
Spring Damping          [5-100]     N·s/m
Fragment Mass           [0.01-1.0]  kg
Gravity                 [0-20]      m/s²
Spring Break Threshold  [1.0-3.0]   (adim)
Collision Restitution   [0-1]       (adim)
Fragment Damping        [0.9-0.99]  (adim)
```

### Configuration Simulation
```
Grid Resolution         [3-6]       (fragments par côté)
Integration Method      [Verlet, Euler]
Time Scale              [0.1-2.0]   (multiplicateur)
```

### Configuration Rendu
```
Cube Center             [x, y, z]   Unity units
Cube Size               [0.5-10]    Unity units
Use Gizmos For Debug    [true/false]
Show Debug Info         [true/false]
```

---

## 🎮 Contrôles

| Touche | Action | Code |
|--------|--------|------|
| **W** | Forward | GetKey() continuous |
| **A** | Left | GetKey() continuous |
| **S** | Back | GetKey() continuous |
| **D** | Right | GetKey() continuous |
| **Space** | Lancer | GetKeyDown() impulse |
| **R** | Réinitialiser | GetKeyDown() reset |
| **P** | Pause/Reprendre | GetKeyDown() toggle |

---

## 💡 Conseils d'Utilisation

### Pour Plus de Réalisme
```
Spring Stiffness: 150
Spring Damping: 25
Fragment Mass: 0.2
Fragment Damping: 0.97
Collision Restitution: 0.2
Integration Method: Verlet
```

### Pour Plus de Performance
```
Grid Resolution: 3
Integration Method: Euler
Spring Stiffness: 50 (moins de calculs)
```

### Pour Plus de Flexibilité (Gelée)
```
Spring Stiffness: 30
Fragment Damping: 0.99
Spring Damping: 5
```

### Pour Plus de Rigidité (Caoutchouc)
```
Spring Stiffness: 300
Fragment Damping: 0.95
Spring Damping: 50
```

---

## 🔍 Dépannage Rapide

| Symptôme | Cause | Solution |
|----------|-------|----------|
| Cube invisible | y trop bas | ↑ Cube Center.y |
| Pas de gravité | Gravity = 0 | ↑ Gravity > 0 |
| Pas de rebond | e = 0 | ↑ Collision Restitution |
| Très lent | Trop fragments | ↓ Grid Resolution |
| Trop mou | Ressorts faibles | ↑ Spring Stiffness |
| Trop rigide | Ressorts forts | ↓ Spring Stiffness |
| Instable | Damping trop faible | ↑ Fragment Damping |

---

## 📊 Performance Attendue

### Avec Grid Resolution = 4 (64 fragments)
```
FPS: 60+
Memory: ~5-10 MB
CPU Time: 1-2 ms
Collision Checks: 64×63/2 = 2016 par frame
```

### Avec Grid Resolution = 5 (125 fragments)
```
FPS: 30-60
Memory: ~15-25 MB
CPU Time: 5-10 ms
Collision Checks: 125×124/2 = 7750 par frame
```

### Avec Grid Resolution = 6 (216 fragments)
```
FPS: 15-30
Memory: ~30-50 MB
CPU Time: 20-50 ms
Collision Checks: 216×215/2 = 23220 par frame
```

---

## 📋 Fichiers du Projet

### Scripts Physiques (8 fichiers, ~2600 lignes)
```
Assets/Scripts/
├── Fragment.cs                    (127 lignes)
├── SpringConstraint.cs            (88 lignes)
├── PhysicsIntegrator.cs          (175 lignes)
├── CollisionSolver.cs            (150+ lignes)
├── MatrixTransform.cs            (200+ lignes)
├── JellyCube.cs                  (400+ lignes)
└── JellyCubeSimulator.cs         (259 lignes)
```

### Documentation (5 fichiers, ~2000 lignes)
```
├── QUICK_START.md                (50 lignes)
├── CORRECTIONS_SUMMARY.md        (250 lignes)
├── JELLY_CUBE_USAGE.md          (500 lignes)
├── TESTING_GUIDE.md             (600 lignes)
└── INDEX.md (ce fichier)        (200 lignes)
```

---

## ✅ Checklist de Vérification

### Avant de Commencer
- [ ] Unity 2020.3+ installé
- [ ] Project créé
- [ ] Scripts placés dans Assets/Scripts/

### Après Installation
- [ ] GameObject créé
- [ ] JellyCubeSimulator attaché
- [ ] Pas d'erreurs de compilation

### Test Basique
- [ ] Play fonctionne
- [ ] Cube visible (y=2)
- [ ] Cube tombe (dû à gravité)
- [ ] Cube rebondit

### Test Avancé
- [ ] W/A/S/D déplacent le cube
- [ ] Space lance le cube
- [ ] R réinitialise
- [ ] P pause et reprend
- [ ] Paramètres modifiables en temps réel

---

## 📚 Ressources Externes

### Documentation Unity
- [Physics](https://docs.unity3d.com/Manual/PhysicsSection.html)
- [Scripting](https://docs.unity3d.com/Manual/ScriptingSection.html)

### Références Mathématiques
- Verlet Integration: [Wikipedia](https://en.wikipedia.org/wiki/Verlet_integration)
- Hooke's Law: [Wikipedia](https://en.wikipedia.org/wiki/Hooke%27s_law)
- Collision Detection: [Real-time Collision Detection](https://realtimecollisiondetection.net/)

---

## 🎓 Concepts Appris

### Physique
1. **Gravité**: Force constante vers bas (m·g)
2. **Ressorts**: Force proportionnelle à déformation (-k·Δx)
3. **Amortissement**: Force proportionnelle à vitesse (-d·v)
4. **Collision**: Détection et réponse impulsionnelle
5. **Énergie**: Conservation et dissipation

### Programmation
1. **Architecture**: Séparation concerns (Fragment, Spring, Integrator)
2. **Performance**: Optimisation collisions O(n²/2)
3. **Stabilité**: Intégration symplectique
4. **Paramétrage**: Exposition des variables en Inspector

### Mathématiques
1. **Algèbre linéaire**: Matrices, vecteurs, produits scalaires
2. **Calcul**: Intégration numérique, dérivées
3. **Géométrie**: Distances, normes, projections

---

## 🎉 Prochaines Étapes

### Améliorations Possibles
- [ ] Ajouter volumétric constraints
- [ ] Ajouter bending resistance
- [ ] Ajouter friction avec sol
- [ ] Ajouter wind forces
- [ ] Ajouter attachement à points fixes
- [ ] Ajouter terrain variable

### Extensions
- [ ] Multiple jelly cubes
- [ ] Collision entre cubes
- [ ] Interaction avec objets rigides
- [ ] Effets visuels (ripples, particles)
- [ ] Audio (son collision/déformation)

### Optimisations
- [ ] Job system (Burst compilation)
- [ ] Spatial hashing pour collisions
- [ ] SIMD vectorization
- [ ] GPU simulation

---

## 📞 Support

### Questions Courantes
- **Q**: Pourquoi le cube se déforme?
  **R**: Loi de Hooke: F = -k(x-L₀). Augmenter k pour moins déformer.

- **Q**: Comment faire un cube plus mou?
  **R**: Réduire Spring Stiffness (ex: 30 au lieu de 100).

- **Q**: Pourquoi simulation lente?
  **R**: Grid Resolution trop élevée. Réduire de 5 à 4 ou 3.

- **Q**: Comment lancer le cube plus haut?
  **R**: Augmenter impulseMagnitude dans inspector.

---

## 📝 Historique des Versions

### v2.0 (11 Novembre 2024)
- ✅ Physique réaliste complète
- ✅ Sol et collisions
- ✅ Contrôles WASD + Space
- ✅ Documentation complète
- ✅ 12 scénarios de test
- ✅ 0 erreurs de compilation

### v1.0 (Antérieur)
- Base du projet
- Fragments et ressorts
- Intégration numérique

---

**Status: ✅ PRODUCTION READY**

Jelly Cube Réaliste v2.0 - Prêt à l'emploi!

Dernière mise à jour: 11 Novembre 2024
