# 🧪 Guide de Test - Jelly Cube Réaliste

## Scénarios de Test Complets

---

## Test 1: ✅ Chute et Rebond Basique

### Objectif
Vérifier que le cube tombe et rebondit correctement sur le sol.

### Étapes
```
1. Appuyez Play
2. Observez le cube tomber de y=2 vers y=0
3. Cube devrait rebondir et s'amortir
4. Arrêtez après 5 secondes
```

### Résultat Attendu
```
✓ Cube tombe doucement
✓ Rebondit une fois au contact sol
✓ Progressivement immobile
✓ Pas de pénétration du sol
```

### Vérification Physique
```
Temps de chute: ~0.64s (h = 1/2·g·t² → t = √(2h/g))
Rebond: ~30% énergie (e=0.3)
Stabilisation: ~2-3 secondes
```

---

## Test 2: ✅ Déformation et Récupération

### Objectif
Vérifier que le cube se déforme et reprend sa forme.

### Étapes
```
1. Appuyez Play
2. Pendant la chute, regardez les arêtes
3. Observez compression au rebond
4. Regardez reprise de forme après
```

### Résultat Attendu
```
✓ Cube se compresse au contact sol
✓ Arêtes restent intactes
✓ Reprend forme originale
✓ Déformation proportionnelle à la force
```

### Paramètres Importants
```
Spring Stiffness: 100 (raideur ressorts)
Fragment Damping: 0.98 (amortissement)
Spring Damping: 10 (amortissement ressorts)
```

---

## Test 3: ✅ Déplacement WASD

### Objectif
Vérifier que les contrôles de mouvement fonctionnent.

### Étapes
```
1. Appuyez Play
2. Maintenez W → Cube se déplace forward
3. Maintenez S → Cube se déplace backward
4. Maintenez A → Cube se déplace left
5. Maintenez D → Cube se déplace right
```

### Résultat Attendu
```
✓ Déplacement fluide dans 4 directions
✓ Pas de rotation involontaire
✓ Cube reste cohésif pendant mouvement
```

### Vérification
```
Distance parcourue: d = v·t = (F/m)·t²·0.5
Avec F = impulseMagnitude × 0.3
```

---

## Test 4: ✅ Lancer du Cube (Space)

### Objectif
Vérifier le lancer du cube avec rebond.

### Étapes
```
1. Appuyez Play
2. Attendez 1 seconde (cube stabilisé)
3. Appuyez Space
4. Observez le lancer et la trajectoire
5. Observez le rebond
```

### Résultat Attendu
```
✓ Cube monte verticalement
✓ Se déplace aussi vers l'avant
✓ Atteint apogée puis redescend
✓ Rebondit et reprend repos
```

### Calcul de Trajectoire
```
Lancer: v = impulseMagnitude × 2 (vertical)
       v = impulseMagnitude × 1 (horizontal)
Hauteur max: h = v²/(2g) = (impulseMagnitude×2)²/(2×9.81)
Temps vol: t = 2v/g
```

---

## Test 5: ✅ Amortissement Progressif

### Objectif
Vérifier que l'énergie se dissipe correctement.

### Étapes
```
1. Appuyez Play
2. Space (lancer)
3. Lancez encore (Space)
4. Lancez une troisième fois
5. Observer hauteurs décroissantes
```

### Résultat Attendu
```
✓ Chaque rebond moins haut que le précédent
✓ Hauteur décroît exponentiellement
✓ Amplitude d'oscillation diminue
✓ Finalement au repos
```

### Vérification Physique
```
Avec e = 0.3 (coefficient restitution)
Hauteur N: h_n = h_0 × e^(2n)
h_1 ≈ 0.09 × h_0
h_2 ≈ 0.0081 × h_0
```

---

## Test 6: ✅ Paramètres Physiques Variables

### Test 6a: Augmenter Raideur

**Avant:**
```
Spring Stiffness: 100
Fragment Damping: 0.98
```

**Changement:**
```
Spring Stiffness: 300 (×3)
```

**Résultat Attendu:**
```
✓ Cube se déforme MOINS
✓ Rebond plus énergique
✓ Bruit de claquement visible
```

### Test 6b: Augmenter Amortissement

**Avant:**
```
Fragment Damping: 0.98
```

**Changement:**
```
Fragment Damping: 0.99 (moins d'amortissement)
```

**Résultat Attendu:**
```
✓ Cube oscille PLUS longtemps
✓ Rebonds plus nombreux
✓ Perte d'énergie moins rapide
```

### Test 6c: Réduire Raideur

**Avant:**
```
Spring Stiffness: 100
```

**Changement:**
```
Spring Stiffness: 50 (÷2)
```

**Résultat Attendu:**
```
✓ Cube très mou
✓ Déformation extrême
✓ Rebond faible
```

---

## Test 7: ✅ Rupture de Ressorts

### Objectif
Vérifier que les ressorts cassent au-delà du seuil.

### Configuration
```
Spring Break Threshold: 1.2 (20% extension)
Grid Resolution: 4
```

### Étapes
```
1. Space (lancer normal)
2. Space (relancer directement après)
3. Space (lancer très violent)
4. Observer si ressorts cassent (en rouge)
```

### Résultat Attendu
```
✓ Ressorts cassent après trop d'étirement
✓ Ressorts cassés affichés en rouge
✓ Cube se désagrège progressivement
✓ Fragments libres oscillent
```

### Calcul de Seuil
```
Rupture si: (distance - L₀) / L₀ > 1.2
Donc si distance > 1.2 × L₀
Ou extension > 20%
```

---

## Test 8: ✅ Collision Restitution

### Objectif
Vérifier l'effet du coefficient de restitution.

### Test 8a: Faible Restitution

**Configuration:**
```
Collision Restitution: 0.1 (10% énergie)
```

**Résultat Attendu:**
```
✓ Rebond très faible
✓ Cube s'arrête rapidement
✓ Colle au sol presque
```

### Test 8b: Haute Restitution

**Configuration:**
```
Collision Restitution: 0.7 (70% énergie)
```

**Résultat Attendu:**
```
✓ Rebond très énergique
✓ Cube remonte presque à la hauteur initiale
✓ Oscillations plus longtemps
```

---

## Test 9: ✅ Résolution de Grille

### Objectif
Tester l'impact de la résolution sur le comportement.

### Test 9a: Faible Résolution

**Configuration:**
```
Grid Resolution: 3 (3×3×3 = 27 fragments)
```

**Résultat Attendu:**
```
✓ Cube plus grand et moins détaillé
✓ Simulation plus rapide
✓ Moins de déformation fine
```

### Test 9b: Haute Résolution

**Configuration:**
```
Grid Resolution: 6 (6×6×6 = 216 fragments)
```

**Résultat Attendu:**
```
✓ Cube plus détaillé
✓ Déformation très fine
✓ Simulation plus lente (CPU)
```

---

## Test 10: ✅ Pause et Reprise

### Objectif
Vérifier que la pause fonctionne correctement.

### Étapes
```
1. Play et Space (lancer)
2. Cube en vol → Appuyez P
3. Cube devrait s'arrêter DANS LES AIRS
4. Appuyez P à nouveau
5. Cube continue sa trajectoire
6. Cube tombe et rebondit
```

### Résultat Attendu
```
✓ Pause arrête complètement la simulation
✓ Tous les mouvements figés
✓ Reprise continue exactement où arrêté
✓ Pas de saut ou instabilité
```

---

## Test 11: ✅ Réinitialisation (R)

### Objectif
Vérifier que la réinitialisation ramène à l'état initial.

### Étapes
```
1. Play et Space plusieurs fois
2. Cube défiguré et mobil
3. Appuyez R
4. Cube devrait revenir position initiale
5. Tous ressorts redeviennent actifs
```

### Résultat Attendu
```
✓ Cube repositionné exactement y=2
✓ Tous ressorts réactivés
✓ Tous fragments au repos
✓ Vitesses = 0
```

---

## Test 12: ✅ Performance et Timing

### Objectif
Vérifier que la simulation tourne à bonne vitesse.

### Mesure
```
1. Ouvrez Console (Ctrl+Shift+C)
2. Lancez la simulation
3. Observez le framerate (Affichage)
4. Devrait être ~60 FPS
```

### Résultat Attendu
```
Avec Grid Resolution = 4:
  ✓ FPS > 60 (très bon)
  
Avec Grid Resolution = 5:
  ✓ FPS > 30 (acceptable)
  
Avec Grid Resolution = 6:
  ✓ FPS > 15-20 (ralenti)
```

---

## 📊 Tableau de Vérification

| Test | Résultat | Notes |
|------|----------|-------|
| Chute/Rebond | ✓ | Cube tombe et rebondit |
| Déformation | ✓ | Reprend forme |
| Mouvement WASD | ✓ | Déplacement 4 directions |
| Lancer Space | ✓ | Monte et avant |
| Amortissement | ✓ | Énergie dissipée |
| Raideur | ✓ | Paramètre fonctionnel |
| Amortissement param | ✓ | Paramètre fonctionnel |
| Rupture | ✓ | Ressorts cassent |
| Restitution | ✓ | Rebond paramétrable |
| Résolution | ✓ | Impact visible |
| Pause/Reprise | ✓ | P-key fonctionne |
| Réinitialisation | ✓ | R-key remet état initial |

---

## 🎯 Critères de Succès

Pour valider que le Jelly Cube fonctionne correctement:

✅ **Physique:**
- Chute due à la gravité
- Rebond réaliste avec coefficient e
- Amortissement progressif

✅ **Élasticité:**
- Déformation proportionnelle à force
- Récupération de forme
- Ressorts actifs

✅ **Contrôles:**
- WASD déplace le cube
- Space lance le cube
- R réinitialise, P pause

✅ **Paramètres:**
- Modification en temps réel efficace
- Chaque paramètre change le comportement
- Pas d'instabilité numérique

✅ **Performance:**
- Stable à 60 FPS (resolution 4)
- Pas de crash
- Pas de dérive

---

**Tout test réussi? Félicitations! Le Jelly Cube est operationnel! 🎉**
