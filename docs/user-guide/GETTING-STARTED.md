# Guide de Démarrage - Nawel

## Bienvenue sur Nawel ! 🎄

Nawel est une application web qui permet de gérer vos listes de cadeaux de Noël en famille. Vous pouvez créer votre liste, consulter les listes des autres membres de la famille, et réserver des cadeaux.

---

## Première Connexion

### 1. Accéder à l'Application

Ouvrez votre navigateur web et accédez à l'adresse de l'application Nawel (fournie par votre administrateur).

**Navigateurs supportés** :
- Google Chrome (recommandé)
- Mozilla Firefox
- Safari
- Microsoft Edge

### 2. Se Connecter

Sur la page de connexion, entrez vos identifiants :

- **Login** : Votre nom d'utilisateur (fourni par l'administrateur)
- **Mot de passe** : Votre mot de passe

Cliquez sur **"Se connecter"**.

**Première connexion ?**
Si c'est votre première connexion, vous recevrez vos identifiants par email. Il est recommandé de changer votre mot de passe après la première connexion (voir section Profil).

---

## Page d'Accueil

Après connexion, vous arrivez sur la **page d'accueil** qui affiche :

### Familles et Membres

L'écran affiche toutes les familles avec leurs membres. Chaque membre est représenté par :
- **Avatar** : Photo de profil ou initiales
- **Nom** : Prénom et nom de famille
- **Badges** :
  - 🏅 **Admin** : Administrateur de l'application
  - 👶 **Enfant** : Compte enfant (géré par les parents)

### Actions Disponibles

Pour chaque membre, vous pouvez :
- **Cliquer sur leur nom** → Voir leur liste de cadeaux
- **"Ma liste"** (si c'est vous) → Gérer votre propre liste
- **"Gérer"** (pour les parents d'enfants) → Gérer la liste de votre enfant

---

## Ma Liste de Cadeaux

### Accéder à Ma Liste

Depuis la page d'accueil, cliquez sur **"Ma liste"** ou sur votre propre nom.

### Vue d'Ensemble

Votre liste affiche :
- **Année sélectionnée** : Dropdown en haut à droite pour changer d'année
- **Tous vos cadeaux** : Affichés sous forme de cartes
- **Boutons d'action** :
  - ➕ **Ajouter un cadeau**
  - 📥 **Importer** (importer des cadeaux de l'année précédente)

### Ajouter un Cadeau

1. Cliquez sur **"Ajouter un cadeau"**
2. Une fenêtre s'ouvre avec le formulaire :

**Champs disponibles** :
- **Nom*** (obligatoire) : Le nom du cadeau
- **Description** : Détails supplémentaires
- **Lien** : URL du produit (Amazon, site web, etc.)
- **Prix** : Coût estimé
- **Devise** : EUR, USD, etc. (par défaut EUR)
- **Image** : URL de l'image du produit
- **Cadeau groupé** : Cochez si c'est un cadeau qui peut être acheté à plusieurs

**🪄 Astuce - Extraction Automatique** :
Si vous collez une URL dans le champ "Lien", cliquez sur **"Extraire les informations"**. L'application va automatiquement remplir les champs (nom, description, prix, image) depuis le site web !

3. Cliquez sur **"Sauvegarder"**

### Modifier un Cadeau

1. Sur la carte d'un cadeau, cliquez sur l'icône **✏️ Modifier**
2. Le formulaire s'ouvre avec les informations pré-remplies
3. Modifiez les champs souhaités
4. Cliquez sur **"Sauvegarder"**

### Supprimer un Cadeau

1. Sur la carte d'un cadeau, cliquez sur l'icône **🗑️ Supprimer**
2. Confirmez la suppression dans la fenêtre qui s'ouvre
3. Le cadeau est supprimé définitivement

⚠️ **Attention** : Si le cadeau était déjà réservé par quelqu'un, la réservation sera annulée automatiquement.

### Importer des Cadeaux

Vous pouvez importer les cadeaux **non réservés** d'une année précédente :

1. Cliquez sur **"Importer"**
2. Sélectionnez l'année source (ex: 2024)
3. Cliquez sur **"Importer"**
4. Les cadeaux non pris de l'année sélectionnée sont copiés dans l'année courante

**Cas d'usage** :
Vous aviez des cadeaux en 2024 qui n'ont pas été offerts ? Importez-les dans votre liste 2025 !

### Changer d'Année

En haut à droite, utilisez le **dropdown** pour sélectionner une année différente :
- Consultez vos anciennes listes (historique)
- Seule l'année courante est modifiable
- Les années passées sont en **lecture seule**

---

## Consulter une Liste

### Accéder à la Liste d'un Membre

Depuis la page d'accueil, cliquez sur le nom d'un membre de la famille.

### Vue d'Ensemble

La liste affiche :
- **Avatar et nom** du propriétaire de la liste
- **Année sélectionnée** : Dropdown pour voir les années précédentes
- **Tous les cadeaux** avec leur statut :
  - 🟢 **Disponible** : Cadeau libre, vous pouvez le réserver
  - 🔵 **Réservé par vous** : Vous avez réservé ce cadeau
  - ⚪ **Déjà réservé** : Un autre membre l'a réservé
  - 🔷 **Cadeau groupé** : Plusieurs personnes peuvent participer
  - ✅ **Vous participez** : Vous participez à ce cadeau groupé

### Réserver un Cadeau (Classique)

1. Sur un cadeau **Disponible**, cliquez sur **"Réserver"**
2. Une fenêtre s'ouvre pour ajouter un commentaire (optionnel)
   - Ex: "Avec plaisir !"
3. Cliquez sur **"Réserver"**
4. Le cadeau passe en statut **"Réservé par vous"**

**✉️ Notification** :
Le propriétaire de la liste recevra un email pour le prévenir que vous avez réservé son cadeau (email envoyé après 2 minutes, pour regrouper plusieurs réservations).

### Participer à un Cadeau Groupé

Les cadeaux groupés permettent à plusieurs personnes de contribuer ensemble :

1. Sur un cadeau **Cadeau groupé**, cliquez sur **"Participer"**
2. Ajoutez un commentaire (optionnel)
   - Ex: "Je participe à hauteur de 200€"
3. Cliquez sur **"Participer"**
4. Le cadeau affiche **"Vous participez"**
5. Vous voyez la liste des autres participants

**Fonctionnement** :
- Le cadeau reste disponible pour d'autres participants
- Pas de limite de participants
- Coordination à faire entre participants (qui achète, etc.)

### Annuler une Réservation

Si vous changez d'avis :

1. Sur un cadeau **"Réservé par vous"** ou **"Vous participez"**, cliquez sur **"Annuler"**
2. Confirmez l'annulation
3. Le cadeau redevient disponible (ou reste ouvert si cadeau groupé)

---

## Mon Panier

### Accéder au Panier

Cliquez sur l'icône **🛒 Panier** dans la barre de navigation.

### Vue d'Ensemble

Le panier affiche tous les cadeaux que vous avez réservés :
- **Groupés par propriétaire** : Les cadeaux sont organisés par personne
- **Informations par cadeau** :
  - Nom du cadeau
  - Prix (si renseigné)
  - Propriétaire (pour qui c'est)
  - Votre commentaire
  - Badge "Groupe" si c'est un cadeau groupé

### Total des Achats

En bas de page, vous voyez le **total par devise** :
- Total EUR : 450,00€
- Total USD : 120,00$

**Note** : Les cadeaux sans prix ne sont pas comptabilisés dans le total.

### Actions dans le Panier

- **Annuler une réservation** : Cliquez sur "Annuler" pour libérer le cadeau
- **Changer d'année** : Dropdown pour voir vos réservations des années précédentes

---

## Mon Profil

### Accéder au Profil

Cliquez sur votre **avatar** ou l'icône **👤** dans la barre de navigation.

### Informations Personnelles

Vous pouvez modifier :
- **Prénom**
- **Nom**
- **Email** (pour recevoir les notifications)
- **Pseudo** (surnom affiché)

### Avatar

**Changer votre avatar** :
1. Cliquez sur **"Changer l'avatar"**
2. Sélectionnez une image depuis votre ordinateur
   - Formats supportés : JPEG, PNG, GIF, WebP
   - Taille maximum : 5 MB
3. L'image est uploadée et s'affiche immédiatement

**Supprimer votre avatar** :
1. Cliquez sur **"Supprimer l'avatar"**
2. Votre avatar revient aux initiales par défaut

### Préférences de Notifications

Cochez/décochez les options :
- ✅ **Notification si ma liste est modifiée** : Recevoir un email si quelqu'un modifie votre liste (parents gérant un enfant)
- ✅ **Notification si un cadeau est réservé** : Recevoir un email quand quelqu'un réserve un de vos cadeaux
- ✅ **Afficher les popups d'information** : Afficher les messages d'aide dans l'application

### Changer le Mot de Passe

1. Cliquez sur **"Changer le mot de passe"**
2. Une fenêtre s'ouvre avec 3 champs :
   - **Ancien mot de passe**
   - **Nouveau mot de passe** (minimum 6 caractères)
   - **Confirmer le nouveau mot de passe**
3. Cliquez sur **"Changer"**
4. Votre mot de passe est mis à jour

**Mot de passe oublié ?**
Contactez votre administrateur pour réinitialiser votre mot de passe.

### Sauvegarder les Modifications

Après avoir modifié vos informations, cliquez sur **"Sauvegarder"** en bas du formulaire.

Un message de confirmation s'affiche : ✅ "Profil mis à jour !"

---

## Fonctionnalités Parents

### Gérer la Liste de Votre Enfant

Si vous avez un compte enfant dans votre famille :

1. Sur la page d'accueil, repérez votre enfant (badge 👶 **Enfant**)
2. Cliquez sur **"Gérer"**
3. Un **banner jaune** apparaît en haut : "Vous gérez actuellement la liste de [Nom de l'enfant]"
4. Vous êtes redirigé vers "Ma liste" qui affiche maintenant la liste de votre enfant
5. Vous pouvez ajouter, modifier, supprimer des cadeaux pour votre enfant

**Revenir à votre compte** :
Cliquez sur **"Revenir à mon compte"** dans le banner jaune.

**Note** : Les actions effectuées sur la liste de l'enfant envoient des notifications au propriétaire (l'enfant), si les notifications sont activées.

---

## Navigation

### Barre de Navigation

En haut de l'application, la barre de navigation affiche :
- 🏠 **Accueil** : Retour à la page d'accueil
- 📝 **Ma liste** : Accéder à votre liste
- 🛒 **Panier** : Voir vos réservations
- 👤 **Profil** : Gérer votre profil
- 🔧 **Admin** (si vous êtes admin) : Accéder au panneau d'administration
- 🚪 **Déconnexion** : Se déconnecter de l'application

### Version Mobile

Sur mobile/tablette, la navigation se fait via un **menu burger** (☰) qui ouvre un menu latéral.

---

## Conseils et Astuces

### 💡 Extraction Automatique de Produits

Gagnez du temps en copiant l'URL d'un produit depuis Amazon, Fnac, ou n'importe quel site :
1. Copiez l'URL du produit
2. Collez-la dans le champ "Lien"
3. Cliquez sur "Extraire les informations"
4. Les champs se remplissent automatiquement !

### 🎁 Cadeaux Groupés

Pour les cadeaux coûteux (vélo, ordinateur, etc.) :
- Cochez "Cadeau groupé" lors de la création
- Plusieurs personnes peuvent participer
- Utilisez le commentaire pour indiquer votre contribution

### 📅 Historique des Années

Consultez vos anciennes listes pour :
- Voir ce que vous aviez demandé les années précédentes
- Éviter de redemander le même cadeau
- Importer des cadeaux non offerts

### 🔔 Notifications

Activez les notifications email pour :
- Être prévenu quand quelqu'un réserve votre cadeau
- Savoir si un parent modifie la liste de votre enfant
- Ne jamais manquer une mise à jour

---

## Résolution de Problèmes

### Je ne peux pas me connecter

**Vérifiez** :
- Votre login et mot de passe (attention aux majuscules/minuscules)
- Votre compte n'est pas désactivé (contactez l'admin)

**Si mot de passe oublié** :
- Contactez votre administrateur pour le réinitialiser

### Je ne vois pas certaines familles

**Raison** :
- Vous ne voyez que votre propre famille
- Les autres familles sont masquées pour des raisons de confidentialité

### Mon avatar ne s'affiche pas

**Vérifiez** :
- Le fichier fait moins de 5 MB
- Le format est supporté (JPEG, PNG, GIF, WebP)
- Votre connexion internet est stable

### Je ne reçois pas les emails

**Vérifiez** :
- Votre adresse email est correcte dans votre profil
- Vos notifications sont activées
- Les emails ne sont pas dans vos spams/indésirables

**Délai** :
- Les emails de réservation sont envoyés après 2 minutes (regroupement)

### L'extraction automatique ne fonctionne pas

**Raisons possibles** :
- Le site web ne supporte pas l'extraction
- L'URL est incorrecte
- Le service externe est temporairement indisponible

**Solution** :
- Remplissez les champs manuellement

---

## Questions Fréquentes (FAQ)

### Puis-je voir qui a réservé un cadeau sur ma liste ?

**Non**, pour préserver la surprise ! Vous recevez seulement une notification qu'un cadeau a été réservé, mais pas par qui.

### Puis-je réserver plusieurs cadeaux pour la même personne ?

**Oui**, vous pouvez réserver autant de cadeaux que vous voulez.

### Puis-je annuler une réservation ?

**Oui**, à tout moment depuis le panier ou depuis la liste de l'utilisateur.

### Puis-je voir les anciennes listes ?

**Oui**, utilisez le dropdown "Année" pour consulter l'historique.

### Puis-je modifier la liste de quelqu'un d'autre ?

**Non**, sauf si :
- C'est votre enfant (compte enfant) et vous êtes le parent
- Vous êtes administrateur

### Combien de cadeaux puis-je ajouter ?

**Illimité** ! Ajoutez autant de cadeaux que vous voulez.

### Les données sont-elles sécurisées ?

**Oui** :
- Connexion sécurisée avec mot de passe hashé (BCrypt)
- Token JWT pour l'authentification
- Protection contre les attaques (rate limiting)

---

## Contacter l'Administrateur

Pour toute question, problème technique, ou demande de création de compte, contactez votre administrateur de l'application.

**Informations généralement fournies par l'admin** :
- URL de l'application
- Identifiants de connexion
- Support technique

---

## Prochaines Étapes

Maintenant que vous connaissez les bases, vous pouvez :
1. ✅ Créer votre liste de cadeaux pour cette année
2. ✅ Consulter les listes des autres membres
3. ✅ Réserver des cadeaux pour vos proches
4. ✅ Personnaliser votre profil et avatar

**Joyeux Noël avec Nawel ! 🎄🎁**
