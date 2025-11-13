# ⚡ Quick Start - Jelly Cube en 2 Minutes

## 🎯 Commencez Immédiatement

### Étape 1: Préparation (30 secondes)
```
1. Ouvrez votre scène Unity
2. Créez un nouveau GameObject vide (Ctrl+Shift+N)
3. Renommez-le "JellyCubeObject"
```

### Étape 2: Attachez le Script (30 secondes)
```
1. Sélectionnez "JellyCubeObject"
2. Dans l'Inspector, cliquez "Add Component"
3. Cherchez "JellyCubeSimulator"
4. Cliquez pour l'ajouter
```

### Étape 3: Paramètres Par Défaut (30 secondes)
Les valeurs par défaut fonctionnent! Vous pouvez directement jouer.

### Étape 4: Testez! (30 secondes)
```
Appuyez sur Play (Spacebar pour lancer le cube)
```

---

## 🎮 Contrôles de Base

| Touche | Effet |
|--------|-------|
| **W/A/S/D** | Déplacer le cube |
| **Space** | Lancer le cube (TEST!) |
| **R** | Réinitialiser |
| **P** | Pause |

---

## ✨ Customisations Rapides

### Le cube se déforme trop?
```
↓ Trouvez dans l'Inspector:
   - Spring Stiffness: 100 → 200
```

### Le cube est trop rigide?
```
↓ Trouvez dans l'Inspector:
   - Spring Stiffness: 100 → 50
```

### Le cube rebondit trop?
```
↓ Trouvez dans l'Inspector:
   - Collision Restitution: 0.3 → 0.1
```

### Vous voulez plus de fragments?
```
↓ Trouvez dans l'Inspector:
   - Grid Resolution: 4 → 5 (5×5×5 = 125 fragments)
```

---

## 🔬 Concepts Clés (Pour Comprendre)

### Pourquoi le cube tombe?
- **Gravité** fonctionne comme dans le monde réel
- Équation: **F = m × g** (9.81 m/s²)

### Pourquoi il se déforme?
- Les **ressorts** entre chaque point se compressent/étireent
- Équation: **F = -k × (distance - longueur_initiale)**

### Pourquoi il s'amortit?
- L'**amortissement** réduit les oscillations
- Sans amortissement → oscillation infinie
- Avec amortissement → repos stable

### Pourquoi il rebondit?
- **Collision avec le sol** inverse la vélocité
- Coefficient de restitution = **combien d'énergie est conservée**
- 1.0 = rebond parfait (Eternel)
- 0.0 = pas de rebond (Collant)

---

## 📊 Voir Les Stats

Appuyez sur **P** pour afficher:
- ✓ Nombre de fragments
- ✓ Nombre de ressorts actifs
- ✓ Ressorts cassés (en rouge)
- ✓ Énergie totale
- ✓ Temps de simulation

---

## 🚀 Prochaines Étapes

1. **Essayez les Présets** → JELLY_CUBE_USAGE.md
2. **Modifiez les Paramètres** → Experimentez en temps réel!
3. **Lisez les Équations** → Comprendre la physique
4. **Créez des Scénarios** → Lancez le cube sur des obstacles

---

## ❓ Problèmes Courants

| Problème | Solution |
|----------|----------|
| Cube invisible | Augmentez Cube Center.y |
| Pas de chute | Vérifiez Gravity > 0 |
| Pas de rebond | Augmentez Collision Restitution |
| Très lent | Réduisez Grid Resolution |
| Trop mou | Augmentez Spring Stiffness |

---

**Enjoy your Jelly Cube! 🎉**

Pour des détails complets → JELLY_CUBE_USAGE.md
