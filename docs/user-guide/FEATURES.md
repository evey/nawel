# Guide Complet des Fonctionnalités - Nawel

## Vue d'Ensemble

Ce guide détaille toutes les fonctionnalités de l'application Nawel avec des cas d'usage avancés, des exemples concrets, et des bonnes pratiques.

---

## Table des Matières

1. [Gestion des Listes](#gestion-des-listes)
2. [Types de Cadeaux](#types-de-cadeaux)
3. [Réservations](#réservations)
4. [Extraction Automatique](#extraction-automatique)
5. [Gestion Multi-Années](#gestion-multi-années)
6. [Mode Gestion Enfant](#mode-gestion-enfant)
7. [Notifications](#notifications)
8. [Panier et Suivi](#panier-et-suivi)
9. [Profil Utilisateur](#profil-utilisateur)
10. [Cas d'Usage Avancés](#cas-dusage-avancés)

---

## Gestion des Listes

### Principe

Chaque utilisateur possède **une seule liste de cadeaux**, mais peut créer des cadeaux pour **différentes années**.

### Structure d'une Liste

Une liste contient :
- **Nom de la liste** : Automatiquement généré ("Liste de [Prénom]")
- **Cadeaux** : Illimités
- **Années** : Historique complet

### Création de Cadeaux

#### Formulaire Complet

| Champ | Type | Obligatoire | Description | Exemple |
|-------|------|-------------|-------------|---------|
| **Nom** | Texte | ✅ Oui | Nom du cadeau | "Nintendo Switch" |
| **Description** | Texte long | ❌ Non | Détails, préférences | "Console de jeu portable, version OLED de préférence" |
| **Lien** | URL | ❌ Non | Lien vers le produit | "https://www.amazon.fr/..." |
| **Prix** | Nombre | ❌ Non | Coût estimé | 299.99 |
| **Devise** | Liste | ❌ Non | EUR, USD, GBP, etc. | EUR (par défaut) |
| **Image** | URL | ❌ Non | URL de l'image | "https://m.media-amazon.com/..." |
| **Cadeau groupé** | Case à cocher | ❌ Non | Achat à plusieurs | Décoché par défaut |

#### Bonnes Pratiques

**Nom** :
- ✅ "MacBook Air 13 pouces M2"
- ❌ "Ordinateur" (trop vague)

**Description** :
- Ajoutez des préférences (couleur, taille, modèle)
- Mentionnez si vous avez déjà certaines choses
- Exemple : "Taille 42, couleur bleu si possible"

**Lien** :
- Collez l'URL complète du produit
- Utilisez Amazon, Fnac, ou tout site e-commerce
- Facilite la recherche pour celui qui offre

**Prix** :
- Estimez au mieux
- Aide les autres à budgétiser
- Plusieurs devises supportées

**Image** :
- URL directe vers l'image
- Ou laissez vide, l'extraction automatique la trouvera

### Modification de Cadeaux

Vous pouvez modifier un cadeau à tout moment **tant qu'il n'est pas réservé** pour l'année courante.

**Si le cadeau est réservé** :
- Modification possible (le réserveur ne verra pas les changements instantanément)
- Une notification peut être envoyée si activée

**Années passées** :
- Les cadeaux des années passées sont en **lecture seule**
- Impossible de modifier/supprimer

### Suppression de Cadeaux

**Suppression simple** :
1. Click sur 🗑️ Supprimer
2. Confirmation
3. Suppression définitive

**Si le cadeau est réservé** :
- La réservation est annulée automatiquement
- Le réserveur en est informé (son panier est mis à jour)

---

## Types de Cadeaux

### Cadeaux Classiques (Par Défaut)

**Fonctionnement** :
- Une seule personne peut réserver
- Passe en statut "Réservé" dès la première réservation
- Masqué pour les autres (mais visible qu'il est réservé)

**Cas d'usage** :
- Tous les cadeaux normaux
- Livres, vêtements, jeux, etc.
- Tout ce qui coûte < 200€ généralement

**Exemple** :
```
Nom : "Livre : Le Seigneur des Anneaux - Édition Collector"
Prix : 45€
Type : Cadeau classique
```

### Cadeaux Groupés

**Fonctionnement** :
- Plusieurs personnes peuvent participer
- Reste "Disponible" même avec des participants
- Affiche la liste des participants
- Chacun peut ajouter un commentaire

**Activation** :
☑️ Cochez "Cadeau groupé" lors de la création

**Cas d'usage** :
- Cadeaux coûteux (> 200€)
- Vélo, ordinateur, télévision, voyage
- Contributions à un gros achat

**Exemple** :
```
Nom : "Vélo électrique VTT"
Prix : 1500€
Type : Cadeau groupé ✅
Description : "VTT électrique pour les balades en montagne"

Participants :
- Jean (200€) : "Je participe pour les roues"
- Marie (300€) : "Participation"
- Pierre (150€) : "Avec plaisir !"
```

**Coordination** :
- Les participants voient les autres participants
- Coordination à faire en dehors de l'app (qui achète, où, etc.)
- Commentaire utile pour indiquer la contribution

---

## Réservations

### Réserver un Cadeau Classique

**Processus** :
1. Accéder à la liste d'un utilisateur
2. Trouver un cadeau "Disponible" 🟢
3. Click sur **"Réserver"**
4. Ajouter un commentaire (optionnel)
   - Ex: "Avec joie !", "Je l'achèterai chez Fnac"
5. Valider

**Effet** :
- Le cadeau passe en "Réservé" immédiatement
- Il disparaît des cadeaux disponibles pour les autres
- Il apparaît dans votre panier
- Une notification est envoyée au propriétaire (après 2 min)

**Commentaire** :
- Visible seulement par le propriétaire du cadeau
- Utile pour ajouter un message personnel
- Exemple : "Hâte de te l'offrir !", "Je l'ai vu en promo !"

### Participer à un Cadeau Groupé

**Processus** :
1. Accéder à la liste d'un utilisateur
2. Trouver un cadeau "Cadeau groupé" 🔷
3. Click sur **"Participer"**
4. Ajouter un commentaire (recommandé)
   - Ex: "Je participe à hauteur de 200€"
5. Valider

**Effet** :
- Vous êtes ajouté à la liste des participants
- Le cadeau reste disponible pour d'autres
- Il apparaît dans votre panier
- Notification au propriétaire

**Voir les Participants** :
- Nombre de participants affiché
- Liste des prénoms (pas les montants)
- Vos propres commentaires visibles

### Annuler une Réservation

**Depuis le Panier** :
1. Accéder à votre panier 🛒
2. Trouver le cadeau
3. Click sur **"Annuler"**
4. Confirmer

**Depuis la Liste** :
1. Accéder à la liste du propriétaire
2. Trouver le cadeau "Réservé par vous" 🔵
3. Click sur **"Annuler"**
4. Confirmer

**Effet** :
- Cadeau classique → Redevient disponible pour tous
- Cadeau groupé → Vous êtes retiré de la liste, reste disponible
- Disparaît de votre panier

**Notification** :
- Pas de notification d'annulation envoyée
- Le propriétaire verra simplement que c'est de nouveau disponible

---

## Extraction Automatique

### Fonctionnement

L'extraction automatique utilise le service **OpenGraph** pour récupérer les métadonnées d'une page web (titre, description, prix, image).

### Sites Supportés

**Bien supportés** :
- ✅ Amazon (.fr, .com, .uk, .de, etc.)
- ✅ Fnac
- ✅ Cultura
- ✅ Cdiscount
- ✅ Boulanger
- ✅ Darty
- ✅ Decathlon
- ✅ La plupart des sites e-commerce

**Partiellement supportés** :
- ⚠️ Sites sans métadonnées OpenGraph
- ⚠️ Sites avec protection anti-scraping
- ⚠️ URLs raccourcies (bit.ly, etc.)

### Utilisation

**Étape par étape** :

1. **Copier l'URL** depuis votre navigateur
   ```
   Exemple : https://www.amazon.fr/Nintendo-Switch-console-Joy-Con-n%C3%A9on/dp/B07WKNQ8K2
   ```

2. **Ouvrir le formulaire** "Ajouter un cadeau"

3. **Coller l'URL** dans le champ "Lien"

4. **Click sur "Extraire les informations"**
   - Un loader s'affiche (quelques secondes)

5. **Vérifier les champs** automatiquement remplis :
   - ✅ Nom : "Console Nintendo Switch avec paire de Joy-Con..."
   - ✅ Description : "Découvrez Nintendo Switch, la..."
   - ✅ Prix : 299.99
   - ✅ Devise : EUR
   - ✅ Image : URL de l'image produit

6. **Ajuster si nécessaire** (le nom est parfois trop long)

7. **Sauvegarder**

### Exemples de Résultats

**Amazon** :
```
URL : https://www.amazon.fr/dp/B08H93ZRK9
↓
Nom : "PlayStation 5"
Prix : 499.99 EUR
Image : https://m.media-amazon.com/images/...jpg
Description : "Vivez une nouvelle génération de jeux PlayStation..."
```

**Fnac** :
```
URL : https://www.fnac.com/livre-xyz
↓
Nom : "Harry Potter à l'école des sorciers"
Prix : 8.90 EUR
Image : https://static.fnac-static.com/...jpg
Description : "Harry Potter, un jeune orphelin..."
```

### Limitations

**Si l'extraction échoue** :
- Message : "Impossible d'extraire les informations"
- Raisons possibles :
  - Site non supporté
  - URL incorrecte
  - Service temporairement indisponible
  - Protection anti-bot

**Solution** :
- Remplissez les champs manuellement
- Copiez-collez depuis le site

**Données manquantes** :
- Certains sites ne fournissent pas le prix
- Certaines images sont en basse résolution
- Ajustez manuellement après extraction

---

## Gestion Multi-Années

### Principe

Nawel gère un **historique complet** de vos listes par année. Vous pouvez consulter, mais pas modifier, les années passées.

### Sélection d'Année

**Dropdown en haut à droite** :
- Années disponibles : Toutes les années où vous avez des cadeaux
- Année courante : Modifiable
- Années passées : Lecture seule

**Exemple** :
```
Dropdown :
- 2025 (année courante)
- 2024
- 2023
- 2022
```

### Année Courante

**Fonctionnalités actives** :
- ✅ Ajouter des cadeaux
- ✅ Modifier des cadeaux
- ✅ Supprimer des cadeaux
- ✅ Importer depuis année précédente
- ✅ Réserver des cadeaux des autres

**Détection automatique** :
- L'année courante est détectée automatiquement
- Basée sur l'année système de votre ordinateur

### Années Passées

**Mode lecture seule** :
- 👁️ Consulter vos anciennes listes
- 👁️ Voir ce que vous aviez demandé
- ❌ Pas de modification possible
- ❌ Pas de suppression possible

**Réservations passées** :
- Visibles dans le panier (changez l'année)
- Historique de ce que vous avez offert

**Cas d'usage** :
- Se rappeler ce qu'on avait demandé
- Éviter de redemander la même chose
- Nostalgie 😊

### Import de Cadeaux

**Principe** :
Copier les cadeaux **non réservés** d'une année précédente vers l'année courante.

**Utilisation** :

1. Click sur **"Importer"**
2. Une modal s'ouvre avec la liste des années disponibles
3. Sélectionnez l'année source (ex: 2024)
4. Click sur **"Importer"**
5. Patientez (quelques secondes)
6. Message de confirmation : "X cadeaux importés"

**Logique** :
- Seuls les cadeaux **disponibles** (non réservés) sont importés
- Les cadeaux réservés ne sont pas copiés
- Les cadeaux importés ont l'année courante

**Exemple** :
```
Liste 2024 :
- Cadeau A (disponible) → ✅ Sera importé
- Cadeau B (réservé) → ❌ Ne sera pas importé
- Cadeau C (disponible) → ✅ Sera importé

Résultat dans liste 2025 :
- Cadeau A (copie)
- Cadeau C (copie)
```

**Cas d'usage** :
- Vous aviez demandé un vélo en 2024, pas eu → Importez en 2025
- Économise du temps (pas besoin de re-saisir)

---

## Mode Gestion Enfant

### Concept

Les **parents** peuvent gérer les listes de leurs **enfants** (comptes enfants de la même famille).

### Activation

**Conditions** :
- Vous êtes dans la même famille que l'enfant
- L'enfant a un compte avec badge 👶 **Enfant**
- Vous n'êtes pas vous-même un enfant

**Processus** :

1. Page d'accueil
2. Repérer votre enfant (badge 👶)
3. Click sur **"Gérer"**
4. Confirmation → Mode activé

**Indicateur visuel** :
- Banner jaune en haut : 🟡 "Vous gérez actuellement la liste de [Nom]"
- Bouton **"Revenir à mon compte"**

### Fonctionnalités en Mode Gestion

**Ce que vous pouvez faire** :
- ✅ Voir la liste de l'enfant
- ✅ Ajouter des cadeaux pour l'enfant
- ✅ Modifier des cadeaux de l'enfant
- ✅ Supprimer des cadeaux de l'enfant
- ✅ Importer des cadeaux

**Ce que vous ne pouvez pas faire** :
- ❌ Réserver des cadeaux de l'enfant (ce serait vous-même)
- ❌ Modifier le profil de l'enfant

**Notifications** :
- Si l'enfant a activé "Notification si ma liste est modifiée"
- Il recevra un email indiquant qu'un parent a modifié sa liste

### Cas d'Usage

**Enfant en bas âge** :
- Parents gèrent la liste entièrement
- L'enfant ne se connecte pas

**Enfant plus âgé** :
- L'enfant crée sa liste
- Parents ajustent/complètent si besoin
- Collaboration

**Exemple concret** :
```
Maman gère la liste de Léo (7 ans) :
1. Click "Gérer" sur Léo
2. Banner : "Vous gérez la liste de Léo"
3. Ajoute 3 cadeaux :
   - Lego Star Wars
   - Livre : Harry Potter Tome 1
   - Jeu de société Dobble
4. Click "Revenir à mon compte"
5. Retour à sa propre liste
```

### Désactivation

**Manuellement** :
- Click sur **"Revenir à mon compte"** dans le banner

**Automatiquement** :
- Si vous naviguez vers une autre page (ex: Panier, Profil)
- Le mode se désactive et vous revenez à votre contexte

---

## Notifications

### Types de Notifications

Nawel envoie des **emails** pour différents événements.

#### 1. Cadeau Réservé

**Trigger** :
- Quelqu'un réserve un de vos cadeaux (classique ou groupé)

**Contenu** :
- "Un cadeau de votre liste a été réservé"
- Nom du cadeau
- Année
- Pas le nom du réserveur (surprise !)

**Délai** :
- **2 minutes après la réservation**
- Permet de regrouper plusieurs réservations en un seul email

**Exemple** :
```
Sujet : 🎁 Réservation sur votre liste Nawel

Bonjour Marie,

Un cadeau de votre liste 2025 a été réservé :
- Nintendo Switch

Joyeux Noël ! 🎄

L'équipe Nawel
```

#### 2. Liste Modifiée (Enfants)

**Trigger** :
- Un parent modifie votre liste (si vous êtes enfant)

**Contenu** :
- "Votre liste a été modifiée"
- Actions effectuées (ajout, modification, suppression)
- Nom des cadeaux concernés

**Exemple** :
```
Sujet : ✏️ Votre liste Nawel a été modifiée

Bonjour Léo,

Un parent a modifié votre liste 2025 :
- Ajout : Lego Star Wars
- Ajout : Livre Harry Potter
- Suppression : Ancien jouet cassé

L'équipe Nawel
```

#### 3. Migration Mot de Passe MD5

**Trigger** :
- Tentative de connexion avec ancien mot de passe MD5

**Contenu** :
- Lien de réinitialisation sécurisé
- Expiration : 24 heures

**Exemple** :
```
Sujet : 🔐 Réinitialisation de votre mot de passe Nawel

Bonjour Jean,

Pour des raisons de sécurité, votre mot de passe doit être réinitialisé.

Cliquez sur le lien ci-dessous pour créer un nouveau mot de passe :
[Lien de réinitialisation]

Ce lien expire dans 24 heures.

L'équipe Nawel
```

### Configuration des Notifications

**Dans votre profil** :

☑️ **Notification si ma liste est modifiée**
- Active/désactive les emails de modification de liste
- Utile pour les comptes enfants

☑️ **Notification si un cadeau est réservé**
- Active/désactive les emails de réservation
- Recommandé : Activé (pour suivre vos réservations)

### Délai de Groupement (Debouncing)

**Principe** :
Pour éviter le spam d'emails, les notifications de réservation sont regroupées.

**Fonctionnement** :
```
T+0s : Marie réserve "Livre A" → Timer démarre (2 min)
T+30s : Pierre réserve "Livre B" → Timer reset (2 min)
T+1m : Jean réserve "Jeu C" → Timer reset (2 min)
T+3m : Aucune autre réservation → Email envoyé

Email reçu :
"3 cadeaux de votre liste ont été réservés :
- Livre A
- Livre B
- Jeu C"
```

**Avantage** :
- Un seul email au lieu de 3
- Moins de spam
- Information groupée

---

## Panier et Suivi

### Accès au Panier

Click sur 🛒 dans la barre de navigation.

### Vue d'Ensemble

Le panier affiche **tous les cadeaux que vous avez réservés** pour l'année sélectionnée.

**Organisation** :
- Groupés par propriétaire
- Affichage par carte

**Informations par Cadeau** :
- Nom du cadeau
- Prix (si renseigné)
- Pour qui (propriétaire)
- Votre commentaire
- Badge "Groupe" si cadeau groupé

### Calcul des Totaux

**Par Devise** :
En bas de page, vous voyez les totaux par devise :

```
Total EUR : 450,00€
Total USD : 120,00$
```

**Logique** :
- Seuls les cadeaux avec prix sont comptabilisés
- Groupés par devise
- Cadeaux sans prix : Ignorés dans le total

**Cas d'usage** :
- Budgétiser vos achats de Noël
- Voir combien vous allez dépenser
- Comparer les devises

### Filtrage par Année

**Dropdown "Année"** :
- Voir vos réservations de 2024, 2023, etc.
- Historique complet de vos cadeaux offerts

**Exemple** :
```
2025 : 5 cadeaux réservés (450€)
2024 : 7 cadeaux réservés (680€)
2023 : 4 cadeaux réservés (320€)
```

### Actions dans le Panier

**Annuler une Réservation** :
1. Click sur **"Annuler"** sur le cadeau
2. Confirmer
3. Le cadeau disparaît du panier
4. Il redevient disponible dans la liste du propriétaire

**Navigation** :
- Click sur le nom du propriétaire → Voir sa liste complète

---

## Profil Utilisateur

### Informations Personnelles

**Champs modifiables** :
- **Prénom** : Affiché partout dans l'app
- **Nom** : Affiché avec le prénom
- **Email** : Pour les notifications
- **Pseudo** : Surnom optionnel (affiché à la place du prénom si renseigné)

**Affichage** :
```
Prénom : Marie
Nom : Dupont
Pseudo : Mimi

→ Affiché comme "Mimi" (si pseudo renseigné)
→ Sinon "Marie Dupont"
```

### Gestion de l'Avatar

**Avatar par Défaut** :
- Initiales du prénom + nom
- Exemple : "MD" pour Marie Dupont
- Cercle coloré

**Upload d'Avatar** :
1. Click "Changer l'avatar"
2. Sélectionner un fichier depuis votre ordinateur
3. Formats : JPEG, PNG, GIF, WebP
4. Taille max : 5 MB
5. Upload instantané

**Suppression** :
1. Click "Supprimer l'avatar"
2. Retour aux initiales

**Bonnes Pratiques** :
- Photo de profil claire
- Format carré de préférence
- Éviter les images trop grandes (optimiser avant)

### Préférences

#### Notifications Email

**notifyListEdit** :
- ☑️ Activé : Recevoir email si un parent modifie votre liste
- Utile pour : Comptes enfants, savoir ce qui a été ajouté/supprimé

**notifyGiftTaken** :
- ☑️ Activé : Recevoir email si quelqu'un réserve votre cadeau
- Recommandé : Toujours activé (suivre vos réservations)

**displayPopup** :
- ☑️ Activé : Afficher les popups d'information dans l'app
- Désactivez si vous connaissez bien l'application

#### Sécurité - Mot de Passe

**Changer le Mot de Passe** :
1. Click "Changer le mot de passe"
2. Modal avec 3 champs :
   - Ancien mot de passe
   - Nouveau mot de passe (min 6 caractères)
   - Confirmation
3. Validation

**Règles** :
- Minimum 6 caractères
- Pas de caractères spéciaux obligatoires (mais recommandés)
- Confirmation doit correspondre

**Sécurité** :
- Mot de passe hashé avec BCrypt (très sécurisé)
- Jamais stocké en clair
- Impossible de récupérer (seulement réinitialiser)

---

## Cas d'Usage Avancés

### Scénario 1 : Famille avec Jeunes Enfants

**Contexte** :
- Parents : Marc et Julie
- Enfants : Léo (7 ans), Emma (5 ans)

**Setup** :
1. Admin crée 4 comptes :
   - Marc (parent)
   - Julie (parent)
   - Léo (enfant ☑️)
   - Emma (enfant ☑️)

**Workflow** :

**Étape 1 - Parents gèrent les listes** :
- Marc se connecte
- Click "Gérer" sur Léo
- Ajoute 5 cadeaux pour Léo
- Click "Revenir à mon compte"
- Click "Gérer" sur Emma
- Ajoute 5 cadeaux pour Emma

**Étape 2 - Réservations** :
- Marc réserve 2 cadeaux de Julie
- Julie réserve 1 cadeau de Marc
- Grands-parents réservent des cadeaux pour Léo et Emma

**Résultat** :
- Listes complètes pour tous
- Enfants ne se connectent pas
- Parents coordonnent entre eux

### Scénario 2 : Cadeau Groupé Coûteux

**Contexte** :
- Pierre veut un MacBook Pro (2500€)

**Setup** :
1. Pierre crée le cadeau :
   - Nom : "MacBook Pro 14 pouces M3"
   - Prix : 2500€
   - ☑️ Cadeau groupé

**Workflow** :

**Participants** :
- Maman : "Je participe à hauteur de 800€"
- Papa : "Je participe pour 800€"
- Grand-mère : "500€ de ma part"
- Tante Sophie : "400€ pour toi !"

**Coordination** :
- Maman contacte les participants par phone
- Décision : Papa achète le MacBook
- Chacun rembourse Papa selon sa participation

**Résultat** :
- 4 participants
- Total : 2500€ couvert
- Pierre a son MacBook 🎉

### Scénario 3 : Import Multi-Années

**Contexte** :
- Jean avait demandé des livres en 2023 et 2024, pas tous reçus

**Workflow** :

**Année 2023** :
- Livre A (réservé, reçu)
- Livre B (non réservé)
- Livre C (réservé, reçu)

**Année 2024** :
- Livre D (non réservé)
- Livre E (réservé, reçu)

**Import en 2025** :
1. Jean accède à "Ma liste" (année 2025)
2. Click "Importer"
3. Sélectionne 2024 → Livre D importé
4. Click "Importer" à nouveau
5. Sélectionne 2023 → Livre B importé

**Résultat 2025** :
- Livre B (copié de 2023)
- Livre D (copié de 2024)
- + Nouveaux cadeaux de 2025

### Scénario 4 : Extraction Amazon Massive

**Contexte** :
- Sophie veut ajouter 10 livres depuis Amazon

**Workflow** :

**Pour chaque livre** :
1. Ouvrir Amazon, chercher le livre
2. Copier l'URL (ex: `https://www.amazon.fr/dp/B08H93ZRK9`)
3. Dans Nawel : "Ajouter un cadeau"
4. Coller l'URL dans "Lien"
5. Click "Extraire les informations"
6. Vérifier les champs auto-remplis
7. Ajuster le nom si trop long
8. Sauvegarder

**Temps gagné** :
- Sans extraction : ~3 min par livre = 30 min
- Avec extraction : ~30 sec par livre = 5 min
- Gain : 25 minutes ! ⚡

### Scénario 5 : Notifications Groupées

**Contexte** :
- Liste de Marie avec 5 cadeaux

**Timeline** :
```
14h00 : Jean réserve "Livre A"
        → Timer démarre (2 min)

14h01 : Pierre réserve "Livre B"
        → Timer reset (2 min)

14h02 : Sophie réserve "Jeu C"
        → Timer reset (2 min)

14h04 : Aucune autre réservation pendant 2 min
        → Email envoyé à Marie

Email :
"3 cadeaux de votre liste ont été réservés :
- Livre A
- Livre B
- Jeu C"
```

**Avantage** :
- Marie reçoit 1 email au lieu de 3
- Information groupée et claire

---

## Statistiques et Limites

### Limites Techniques

| Ressource | Limite | Notes |
|-----------|--------|-------|
| Cadeaux par liste | Illimité | Pas de limite technique |
| Années disponibles | Illimité | Historique complet |
| Taille avatar | 5 MB | JPEG, PNG, GIF, WebP |
| Longueur nom cadeau | 200 caractères | Recommandé : 50-100 |
| Longueur description | 1000 caractères | Optionnel |
| Participants cadeau groupé | Illimité | Pas de limite technique |
| Réservations par utilisateur | Illimité | Tous les cadeaux possibles |

### Performance

**Temps de Réponse** :
- Chargement page : < 1 seconde
- Extraction automatique : 2-5 secondes
- Upload avatar : 1-3 secondes (selon taille)

**Rate Limiting** :
- Protection anti-spam
- Pas de limite pour usage normal
- Si trop de requêtes : Message "Trop de requêtes, réessayez dans 1 minute"

---

## Conseils et Astuces

### 💡 Productivité

1. **Utilisez l'extraction automatique** systématiquement
2. **Ajoutez des descriptions** détaillées (aide celui qui offre)
3. **Mettez des prix** (aide à budgétiser)
4. **Commentez vos réservations** (message personnel)

### 🎁 Cadeaux Groupés

1. **Coordonnez-vous** en dehors de l'app (phone, email)
2. **Indiquez votre contribution** dans le commentaire
3. **Désignez un responsable** pour l'achat

### 👨‍👩‍👧‍👦 Gestion Enfants

1. **Créez les listes tôt** (novembre)
2. **Impliquez les enfants** (plus âgés) dans leur liste
3. **Complétez discrètement** si besoin

### 📅 Multi-Années

1. **Importez systématiquement** les cadeaux non reçus
2. **Consultez l'historique** pour éviter les doublons
3. **Gardez l'historique** (souvenirs)

### 🔔 Notifications

1. **Activez les notifications** de réservation
2. **Désactivez** les popups si vous connaissez l'app
3. **Vérifiez vos spams** si vous ne recevez pas d'emails

---

## Raccourcis Clavier (Futur)

_Fonctionnalité à venir_

```
Ctrl+N : Nouveau cadeau
Ctrl+S : Sauvegarder
Esc : Fermer modal
```

---

## Changelog des Fonctionnalités

### Version 2.0.0 (Décembre 2024)

**Nouvelles fonctionnalités** :
- ✨ Extraction automatique de produits (OpenGraph)
- ✨ Cadeaux groupés
- ✨ Mode gestion enfant
- ✨ Notifications par email avec debouncing
- ✨ Historique multi-années
- ✨ Import de cadeaux

**Améliorations** :
- 🎨 Nouveau design Material-UI
- ⚡ Performance améliorée
- 🔒 Sécurité renforcée (BCrypt, JWT, rate limiting)

---

## Support

Pour toute question ou problème, consultez :
- [Guide de Démarrage](GETTING-STARTED.md)
- [Guide de Dépannage](../TROUBLESHOOTING.md)
- Contactez votre administrateur

**Joyeuses fêtes avec Nawel ! 🎄🎁**
