# Guide d'Installation et de Lancement

## Installation Rapide

### Étape 1: Vérifier le Projet
1. Ouvrir Unity Hub
2. Ouvrir le projet `MedNour_MP`
3. Vérifier que tous les scripts sont dans `Assets/MedNour/`

### Étape 2: Configuration de la Scène

**Option A - Automatique (Recommandé):**
1. Dans Unity, aller dans le menu: `Tools > Setup Physics Simulation Scene`
2. Cliquer sur "OK" dans la boîte de dialogue
3. Appuyer sur Play ▶️

**Option B - Manuelle:**
1. Ouvrir la scène `Assets/Scenes/SampleScene.unity`
2. Clic droit dans la Hierarchy: `Create Empty`
3. Nommer le GameObject: "SimulationStarter"
4. Dans l'Inspector, cliquer `Add Component`
5. Chercher et ajouter: `SimulationStarter`
6. Appuyer sur Play ▶️

### Étape 3: Lancer la Simulation
- Cliquer sur le bouton Play ▶️ en haut de l'éditeur
- Observer la console pour les messages de debug
- La simulation démarre automatiquement

## Ce Qui Va Se Passer

```
1. [T=0s]     La sphère bleue apparaît en hauteur (position: -4, 4, 0)
              
2. [T=0-1s]   La sphère tombe sous l'effet de la gravité
              Accélération: 9.81 m/s²
              
3. [T=1-2s]   La sphère entre en contact avec la rampe grise
              Elle commence à rouler avec friction
              Rotation visible due au torque
              
4. [T=2-3s]   La sphère continue sur le sol plat
              Maintient sa vitesse avec légère décélération
              
5. [T=3-4s]   IMPACT! La sphère percute le mur orange
              Le mur se fracture en 12 fragments (4×3)
              
6. [T=4-5s]   Les fragments tombent avec contraintes actives
              Les contraintes se rompent progressivement
              Impulsions appliquées selon E = 1/2·k·x²
              
7. [T=5-10s]  Les fragments continuent de tomber
              La sphère ralentit (perd 40% de vitesse)
              Tous les objets atteignent le sol
```

## Paramètres Ajustables

### Dans PhysicsSimulationController:
- **sphereStartPosition**: Position initiale de la sphère
- **sphereMass**: Masse de la sphère (affecte le momentum)
- **rampRadius**: Courbure de la rampe
- **wallFracturesX/Y**: Nombre de fragments du mur

### Dans l'Inspector (si créé manuellement):
1. Sélectionner "PhysicsSimulationController" dans la Hierarchy
2. Modifier les paramètres dans l'Inspector
3. Relancer la simulation

## Tests

### Tester les Calculs Physiques:
1. Créer un GameObject vide
2. Attacher le script `PhysicsTests`
3. Lancer Play
4. Consulter la Console pour les résultats

## Débogage

### Messages de Console Importants:
```
✅ "Simulation initialisée!" → Tout est prêt
✅ "Mur détruit!" → Impact réussi
✅ "Contrainte rompue! Énergie: X" → Physique fonctionne
```

### Problèmes Communs:

**La sphère traverse le sol:**
- Vérifier que `groundHeight = 0` dans le contrôleur
- Augmenter le coefficient de restitution

**Le mur ne se casse pas:**
- Vérifier la position du mur (doit être sur le chemin)
- Réduire `breakThreshold` dans FracturedWall

**La simulation est trop rapide/lente:**
- Ajuster `Time.fixedDeltaTime` (Edit > Project Settings > Time)
- Valeur recommandée: 0.02 (50 FPS physique)

## Visualisation

### Vue de la Caméra:
- Position recommandée: (0, 3, -10)
- Rotation: (15°, 0°, 0°)
- Permet de voir toute la scène

### Gizmos (Mode Scene):
- Sphère cyan: Position actuelle
- Ligne rouge: Vecteur vélocité
- Ligne verte: Niveau du sol
- Cube jaune: Zone du mur

## Performance

**Configuration Minimale:**
- Unity 2020.3 ou supérieur
- Processeur: Intel i5 ou équivalent
- RAM: 4 GB
- GPU: Intégré suffit

**Performance Attendue:**
- 60 FPS en mode Play
- ~12 fragments de mur
- Intégration RK4 stable

## Captures d'Écran Recommandées

1. **T=0s**: Configuration initiale
2. **T=1.5s**: Sphère sur la rampe
3. **T=3s**: Impact avec le mur
4. **T=4s**: Fragments en vol
5. **T=6s**: État final

## Support

Si vous rencontrez des problèmes:
1. Vérifier la Console pour les erreurs
2. Relire le README.md complet
3. Vérifier que tous les scripts sont présents dans Assets/MedNour/
4. Réimporter les scripts si nécessaire

## Checklist Finale

- [ ] Tous les scripts sont dans Assets/MedNour/
- [ ] La scène est configurée (manuellement ou via le menu)
- [ ] SimulationStarter est dans la Hierarchy
- [ ] La caméra est bien positionnée
- [ ] Aucune erreur dans la Console
- [ ] Play ▶️ lancé
- [ ] La simulation démarre automatiquement

**Bon succès avec votre simulation de moteur physique! 🎯**
