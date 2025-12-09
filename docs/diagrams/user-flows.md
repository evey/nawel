# Flux Utilisateurs - Nawel

## Vue d'Ensemble

Ce document contient les diagrammes de flux utilisateurs pour les parcours principaux de l'application Nawel.

---

## Table des Matières

1. [Connexion et Navigation](#connexion-et-navigation)
2. [Création de Cadeau](#création-de-cadeau)
3. [Réservation de Cadeau](#réservation-de-cadeau)
4. [Cadeau Groupé](#cadeau-groupé)
5. [Gestion Enfant](#gestion-enfant)
6. [Import de Cadeaux](#import-de-cadeaux)
7. [Gestion du Profil](#gestion-du-profil)

---

## Connexion et Navigation

### Flux de Connexion

```mermaid
flowchart TD
    Start([Utilisateur accède à l'app]) --> Login[Page de Connexion]
    Login --> EnterCred[Entre login + password]
    EnterCred --> Submit{Valide ?}

    Submit -->|Oui| CheckMD5{Mot de passe MD5 ?}
    Submit -->|Non| Error[Affiche erreur<br/>'Identifiants incorrects']
    Error --> Login

    CheckMD5 -->|Oui| MD5Modal[Modal Migration MD5<br/>'Votre mot de passe doit être réinitialisé']
    CheckMD5 -->|Non| Home[Redirect vers Home]

    MD5Modal --> ResetEmail[Utilisateur entre son login]
    ResetEmail --> SendEmail[Email de reset envoyé]
    SendEmail --> CheckEmail[Utilisateur vérifie ses emails]
    CheckEmail --> ClickLink[Click sur lien dans email]
    ClickLink --> NewPassword[Entre nouveau mot de passe]
    NewPassword --> PasswordUpdated[Mot de passe mis à jour]
    PasswordUpdated --> Login

    Home --> Nav{Navigation}

    Nav -->|Ma liste| MyList[Page Ma Liste]
    Nav -->|Liste user| UserList[Page Liste Utilisateur]
    Nav -->|Panier| Cart[Page Panier]
    Nav -->|Profil| Profile[Page Profil]
    Nav -->|Admin<br/>si admin| Admin[Page Admin]
    Nav -->|Déconnexion| Logout[Déconnexion]
    Logout --> Login

    style Start fill:#e1f5ff
    style Home fill:#c8e6c9
    style Error fill:#ffcdd2
    style MD5Modal fill:#fff9c4
```

---

## Création de Cadeau

### Flux Complet de Création

```mermaid
flowchart TD
    Start([Utilisateur sur Ma Liste]) --> ClickAdd[Click 'Ajouter un cadeau']
    ClickAdd --> Modal[Modal GiftForm s'ouvre]

    Modal --> HasLink{A une URL<br/>produit ?}

    HasLink -->|Oui| PasteURL[Colle l'URL dans 'Lien']
    PasteURL --> Extract[Click 'Extraire les informations']
    Extract --> Wait[Loader affiché<br/>2-5 secondes]
    Wait --> ExtractSuccess{Extraction<br/>réussie ?}

    ExtractSuccess -->|Oui| AutoFill[Champs auto-remplis:<br/>Nom, Description,<br/>Prix, Image]
    ExtractSuccess -->|Non| ErrorMsg[Message d'erreur<br/>'Impossible d'extraire']
    ErrorMsg --> ManualFill

    HasLink -->|Non| ManualFill[Remplit manuellement<br/>les champs]

    AutoFill --> AdjustName{Nom trop<br/>long ?}
    AdjustName -->|Oui| ShortenName[Raccourcit le nom]
    AdjustName -->|Non| FillOther
    ShortenName --> FillOther

    ManualFill --> FillOther[Remplit autres champs]

    FillOther --> GroupGift{Cadeau<br/>groupé ?}
    GroupGift -->|Oui| CheckGroup[Coche 'Cadeau groupé']
    GroupGift -->|Non| FillPrice
    CheckGroup --> FillPrice

    FillPrice[Entre prix optionnel] --> Validate{Champ 'Nom'<br/>rempli ?}

    Validate -->|Non| ErrorValidation[Erreur validation<br/>'Nom requis']
    ErrorValidation --> FillOther

    Validate -->|Oui| ClickSave[Click 'Sauvegarder']
    ClickSave --> Saving[Envoi API<br/>POST /gifts]
    Saving --> Success{Succès ?}

    Success -->|Oui| GiftAdded[Cadeau ajouté<br/>à la liste]
    Success -->|Non| APIError[Erreur serveur]

    GiftAdded --> RefreshList[Liste rafraîchie]
    RefreshList --> End([Fin])

    APIError --> FillOther

    style Start fill:#e1f5ff
    style GiftAdded fill:#c8e6c9
    style ErrorMsg fill:#ffcdd2
    style ErrorValidation fill:#ffcdd2
    style APIError fill:#ffcdd2
    style Modal fill:#fff9c4
```

---

## Réservation de Cadeau

### Flux de Réservation Classique

```mermaid
flowchart TD
    Start([Utilisateur sur Liste d'un user]) --> Browse[Parcourt les cadeaux]
    Browse --> Find{Trouve un<br/>cadeau ?}

    Find -->|Non| Browse
    Find -->|Oui| CheckStatus{Statut du<br/>cadeau ?}

    CheckStatus -->|Disponible| CanReserve[Peut réserver]
    CheckStatus -->|Réservé| Already[Déjà réservé<br/>par quelqu'un]
    CheckStatus -->|Réservé par moi| MyReserve[Déjà réservé<br/>par moi]
    CheckStatus -->|Cadeau groupé| GroupFlow[Voir flux<br/>cadeau groupé]

    Already --> Browse

    CanReserve --> ClickReserve[Click 'Réserver']
    ClickReserve --> Dialog[ReserveDialog s'ouvre]
    Dialog --> AddComment{Ajouter<br/>commentaire ?}

    AddComment -->|Oui| TypeComment[Entre commentaire<br/>Ex: 'Avec plaisir !']
    AddComment -->|Non| Confirm
    TypeComment --> Confirm

    Confirm[Click 'Réserver'] --> Reserving[Envoi API<br/>POST /gifts/:id/reserve]
    Reserving --> ReserveSuccess{Succès ?}

    ReserveSuccess -->|Oui| Reserved[Cadeau réservé]
    ReserveSuccess -->|Non| ErrorReserve[Erreur<br/>Peut-être déjà réservé]

    Reserved --> UpdateUI[UI mise à jour<br/>Statut: 'Réservé par vous']
    UpdateUI --> AddToCart[Ajouté au panier]
    AddToCart --> Notify[Notification envoyée<br/>au propriétaire<br/>après 2 min]
    Notify --> End([Fin])

    ErrorReserve --> Refresh[Rafraîchit la page]
    Refresh --> Browse

    MyReserve --> ViewCart{Voir dans<br/>panier ?}
    ViewCart -->|Oui| GoCart[Redirect vers Panier]
    ViewCart -->|Non| Browse

    style Start fill:#e1f5ff
    style Reserved fill:#c8e6c9
    style ErrorReserve fill:#ffcdd2
    style Already fill:#ffe0b2
    style MyReserve fill:#c5e1a5
```

---

## Cadeau Groupé

### Flux de Participation

```mermaid
flowchart TD
    Start([Utilisateur voit cadeau groupé]) --> ViewGroup[Cadeau avec badge<br/>'Cadeau groupé']
    ViewGroup --> CheckParticipants[Voir nombre de participants]

    CheckParticipants --> AmIParticipant{Déjà<br/>participant ?}

    AmIParticipant -->|Oui| ShowMyParticipation[Affiche 'Vous participez']
    ShowMyParticipation --> ViewOthers[Voir les autres participants]
    ViewOthers --> Decide1{Annuler<br/>participation ?}

    Decide1 -->|Oui| ClickCancel[Click 'Annuler']
    Decide1 -->|Non| Stay[Reste participant]

    ClickCancel --> Confirm2[Confirme l'annulation]
    Confirm2 --> Unreserve[API POST /unreserve]
    Unreserve --> Removed[Retiré de la liste]
    Removed --> UpdateGroup[Cadeau mis à jour]
    UpdateGroup --> End1([Fin])

    AmIParticipant -->|Non| CanParticipate[Peut participer]
    CanParticipate --> ClickParticipate[Click 'Participer']
    ClickParticipate --> Dialog[ReserveDialog s'ouvre]
    Dialog --> AddComment[Ajoute commentaire<br/>Ex: 'Je participe<br/>à hauteur de 200€']
    AddComment --> ConfirmParticipate[Click 'Participer']

    ConfirmParticipate --> Participating[API POST /reserve]
    Participating --> Success{Succès ?}

    Success -->|Oui| Added[Ajouté aux participants]
    Success -->|Non| Error[Erreur serveur]

    Added --> UpdateUI[UI mise à jour<br/>Affiche 'Vous participez']
    UpdateUI --> AddToCart[Ajouté au panier]
    AddToCart --> Notify[Notification au<br/>propriétaire<br/>après 2 min]
    Notify --> ViewAllParticipants[Voir tous les participants]
    ViewAllParticipants --> Coordinate[Coordonner entre<br/>participants<br/>hors app]
    Coordinate --> End2([Fin])

    Error --> Retry[Réessayer]
    Retry --> ClickParticipate

    style Start fill:#e1f5ff
    style Added fill:#c8e6c9
    style Error fill:#ffcdd2
    style ViewGroup fill:#bbdefb
    style Coordinate fill:#fff9c4
```

---

## Gestion Enfant

### Flux de Gestion par Parent

```mermaid
flowchart TD
    Start([Parent sur page Home]) --> ViewFamily[Voit liste des familles]
    ViewFamily --> FindChild{Trouve son<br/>enfant ?}

    FindChild -->|Non| NoChild[Pas d'enfant<br/>dans la famille]
    FindChild -->|Oui| SeeChild[Enfant avec badge<br/>'Enfant']

    SeeChild --> ClickManage[Click 'Gérer']
    ClickManage --> Confirm[Confirmation]
    Confirm --> ModeActivated[Mode gestion activé]

    ModeActivated --> Banner[Banner jaune affiché<br/>'Vous gérez la liste de [Nom]']
    Banner --> Redirect[Redirect vers Ma Liste]
    Redirect --> ShowChildList[Liste de l'enfant affichée]

    ShowChildList --> Actions{Action<br/>souhaitée ?}

    Actions -->|Ajouter| AddGift[Click 'Ajouter un cadeau']
    Actions -->|Modifier| EditGift[Click 'Modifier' sur cadeau]
    Actions -->|Supprimer| DeleteGift[Click 'Supprimer' sur cadeau]
    Actions -->|Importer| ImportGift[Click 'Importer']
    Actions -->|Terminer| ExitMode

    AddGift --> GiftForm1[Formulaire création]
    EditGift --> GiftForm2[Formulaire édition]
    DeleteGift --> ConfirmDelete[Confirmation suppression]
    ImportGift --> ImportDialog[Dialog import année]

    GiftForm1 --> SaveGift1[Sauvegarde]
    GiftForm2 --> SaveGift2[Sauvegarde]
    ConfirmDelete --> DeleteAction[Suppression]
    ImportDialog --> ImportAction[Import]

    SaveGift1 --> NotifyChild1{Notif<br/>activée ?}
    SaveGift2 --> NotifyChild2{Notif<br/>activée ?}
    DeleteAction --> NotifyChild3{Notif<br/>activée ?}

    NotifyChild1 -->|Oui| EmailChild1[Email à l'enfant<br/>'Liste modifiée']
    NotifyChild1 -->|Non| UpdateList1
    NotifyChild2 -->|Oui| EmailChild2[Email à l'enfant<br/>'Liste modifiée']
    NotifyChild2 -->|Non| UpdateList1
    NotifyChild3 -->|Oui| EmailChild3[Email à l'enfant<br/>'Liste modifiée']
    NotifyChild3 -->|Non| UpdateList1

    EmailChild1 --> UpdateList1[Liste enfant mise à jour]
    EmailChild2 --> UpdateList1
    EmailChild3 --> UpdateList1
    ImportAction --> UpdateList1

    UpdateList1 --> ShowChildList

    ExitMode[Click 'Revenir à mon compte'] --> ModeDeactivated[Mode gestion désactivé]
    ModeDeactivated --> BannerHidden[Banner disparaît]
    BannerHidden --> RedirectMyList[Redirect vers Ma Liste]
    RedirectMyList --> ShowMyList[Ma propre liste affichée]
    ShowMyList --> End([Fin])

    NoChild --> End

    style Start fill:#e1f5ff
    style ModeActivated fill:#fff9c4
    style Banner fill:#fff59d
    style EmailChild1 fill:#e1bee7
    style EmailChild2 fill:#e1bee7
    style EmailChild3 fill:#e1bee7
    style ShowMyList fill:#c8e6c9
```

---

## Import de Cadeaux

### Flux d'Import depuis Année Précédente

```mermaid
flowchart TD
    Start([Utilisateur sur Ma Liste<br/>Année courante: 2025]) --> ClickImport[Click 'Importer']

    ClickImport --> ImportDialog[ImportDialog s'ouvre]
    ImportDialog --> ShowYears[Affiche années disponibles<br/>Ex: 2024, 2023, 2022]

    ShowYears --> HasYears{Années<br/>disponibles ?}

    HasYears -->|Non| NoHistory[Pas d'historique<br/>Rien à importer]
    NoHistory --> CloseDialog1[Ferme dialog]
    CloseDialog1 --> End1([Fin])

    HasYears -->|Oui| SelectYear[Sélectionne année source<br/>Ex: 2024]
    SelectYear --> ConfirmImport[Click 'Importer']

    ConfirmImport --> APICall[API POST /gifts/import<br/>fromYear: 2024<br/>toYear: 2025]
    APICall --> Processing[Traitement côté serveur]

    Processing --> Filter[Filtre: Cadeaux disponibles<br/>non réservés en 2024]
    Filter --> Copy[Copie vers 2025<br/>avec nouvelle année]
    Copy --> Count[Compte nombre<br/>de cadeaux importés]

    Count --> ImportSuccess{Succès ?}

    ImportSuccess -->|Oui| ShowCount[Message: 'X cadeaux importés']
    ImportSuccess -->|Non| ErrorImport[Erreur serveur]

    ShowCount --> HasImported{Cadeaux<br/>importés > 0 ?}

    HasImported -->|Oui| RefreshList[Rafraîchit la liste 2025]
    HasImported -->|Non| NoImport[Message: 'Aucun cadeau à importer']

    RefreshList --> ShowNewGifts[Affiche nouveaux cadeaux<br/>dans liste 2025]
    ShowNewGifts --> Success[Import réussi]
    Success --> CloseDialog2[Ferme dialog]
    CloseDialog2 --> End2([Fin])

    NoImport --> CloseDialog2

    ErrorImport --> RetryOption{Réessayer ?}
    RetryOption -->|Oui| ConfirmImport
    RetryOption -->|Non| CloseDialog2

    style Start fill:#e1f5ff
    style Success fill:#c8e6c9
    style ShowNewGifts fill:#c8e6c9
    style ErrorImport fill:#ffcdd2
    style NoHistory fill:#ffe0b2
    style NoImport fill:#ffe0b2
```

---

## Gestion du Profil

### Flux de Modification du Profil

```mermaid
flowchart TD
    Start([Utilisateur sur page Profil]) --> ViewProfile[Affiche profil actuel]

    ViewProfile --> WhatToChange{Que<br/>modifier ?}

    WhatToChange -->|Infos perso| EditInfo[Modifie Prénom, Nom,<br/>Email, Pseudo]
    WhatToChange -->|Avatar| ChangeAvatar
    WhatToChange -->|Notifications| ChangeNotif
    WhatToChange -->|Mot de passe| ChangePassword

    %% --- Infos personnelles ---
    EditInfo --> EditFields[Modifie les champs]
    EditFields --> ClickSaveInfo[Click 'Sauvegarder']
    ClickSaveInfo --> APISaveInfo[API PUT /users/me]
    APISaveInfo --> SuccessInfo{Succès ?}

    SuccessInfo -->|Oui| UpdatedInfo[Message: 'Profil mis à jour']
    SuccessInfo -->|Non| ErrorInfo[Erreur sauvegarde]

    UpdatedInfo --> UpdateContext[AuthContext mis à jour]
    UpdateContext --> UIRefresh1[UI rafraîchie]
    UIRefresh1 --> End1([Fin])

    ErrorInfo --> EditFields

    %% --- Avatar ---
    ChangeAvatar{Action<br/>avatar ?} -->|Upload| UploadFlow
    ChangeAvatar -->|Supprimer| DeleteFlow

    UploadFlow[Click 'Changer l'avatar'] --> FileSelect[Sélectionne fichier]
    FileSelect --> Validate{Validation<br/>fichier ?}

    Validate -->|Format invalide| ErrorFormat[Erreur: Format<br/>non supporté]
    Validate -->|Trop gros > 5MB| ErrorSize[Erreur: Fichier<br/>trop volumineux]
    Validate -->|OK| PreviewAvatar[Preview affiché]

    ErrorFormat --> FileSelect
    ErrorSize --> FileSelect

    PreviewAvatar --> UploadFile[API POST /users/me/avatar<br/>multipart/form-data]
    UploadFile --> UploadSuccess{Succès ?}

    UploadSuccess -->|Oui| AvatarUploaded[Avatar uploadé]
    UploadSuccess -->|Non| ErrorUpload[Erreur upload]

    AvatarUploaded --> UpdateAvatar[AuthContext mis à jour]
    UpdateAvatar --> ShowNewAvatar[Nouvel avatar affiché]
    ShowNewAvatar --> End2([Fin])

    ErrorUpload --> FileSelect

    DeleteFlow[Click 'Supprimer l'avatar'] --> ConfirmDelete[Confirmation]
    ConfirmDelete --> APIDelete[API DELETE /users/me/avatar]
    APIDelete --> DeleteSuccess{Succès ?}

    DeleteSuccess -->|Oui| AvatarDeleted[Avatar supprimé]
    DeleteSuccess -->|Non| ErrorDelete[Erreur suppression]

    AvatarDeleted --> BackToInitials[Retour aux initiales]
    BackToInitials --> End3([Fin])

    ErrorDelete --> ViewProfile

    %% --- Notifications ---
    ChangeNotif[Toggle checkboxes] --> NotifChanged[Préférences modifiées]
    NotifChanged --> ClickSaveNotif[Click 'Sauvegarder']
    ClickSaveNotif --> APISaveNotif[API PUT /users/me]
    APISaveNotif --> NotifSaved[Préférences sauvegardées]
    NotifSaved --> End4([Fin])

    %% --- Mot de passe ---
    ChangePassword[Click 'Changer le mot de passe'] --> PasswordDialog[Dialog s'ouvre]
    PasswordDialog --> EnterPasswords[Entre:<br/>- Ancien MDP<br/>- Nouveau MDP<br/>- Confirmation]

    EnterPasswords --> ValidatePassword{Validation ?}

    ValidatePassword -->|Nouveau < 6 chars| ErrorShort[Erreur: Minimum<br/>6 caractères]
    ValidatePassword -->|Confirmation ≠ Nouveau| ErrorMatch[Erreur: Mots de passe<br/>ne correspondent pas]
    ValidatePassword -->|OK| SubmitPassword[Click 'Changer']

    ErrorShort --> EnterPasswords
    ErrorMatch --> EnterPasswords

    SubmitPassword --> APIPassword[API POST /users/me/change-password]
    APIPassword --> PasswordSuccess{Succès ?}

    PasswordSuccess -->|Oui| PasswordChanged[Mot de passe changé]
    PasswordSuccess -->|Non| ErrorOldPassword[Erreur: Ancien mot<br/>de passe incorrect]

    PasswordChanged --> CloseDialog[Ferme dialog]
    CloseDialog --> End5([Fin])

    ErrorOldPassword --> EnterPasswords

    style Start fill:#e1f5ff
    style UpdatedInfo fill:#c8e6c9
    style AvatarUploaded fill:#c8e6c9
    style AvatarDeleted fill:#c8e6c9
    style PasswordChanged fill:#c8e6c9
    style NotifSaved fill:#c8e6c9
    style ErrorInfo fill:#ffcdd2
    style ErrorFormat fill:#ffcdd2
    style ErrorSize fill:#ffcdd2
    style ErrorUpload fill:#ffcdd2
    style ErrorDelete fill:#ffcdd2
    style ErrorShort fill:#ffcdd2
    style ErrorMatch fill:#ffcdd2
    style ErrorOldPassword fill:#ffcdd2
```

---

## Annulation de Réservation

### Flux d'Annulation depuis Panier

```mermaid
flowchart TD
    Start([Utilisateur dans Panier]) --> ViewCart[Voit tous ses cadeaux réservés]
    ViewCart --> SelectYear{Changer<br/>année ?}

    SelectYear -->|Oui| ChangeYear[Sélectionne année<br/>dans dropdown]
    ChangeYear --> RefreshCart[Panier rafraîchi<br/>pour cette année]
    RefreshCart --> ViewCart

    SelectYear -->|Non| BrowseCart[Parcourt les cadeaux]
    BrowseCart --> FindGift{Trouve cadeau<br/>à annuler ?}

    FindGift -->|Non| KeepAll[Garde toutes<br/>les réservations]
    KeepAll --> End1([Fin])

    FindGift -->|Oui| CheckType{Type de<br/>cadeau ?}

    CheckType -->|Classique| ClassicCancel[Cadeau réservé<br/>par moi seul]
    CheckType -->|Groupe| GroupCancel[Participation<br/>cadeau groupé]

    ClassicCancel --> ClickCancel1[Click 'Annuler']
    GroupCancel --> ClickCancel2[Click 'Annuler ma participation']

    ClickCancel1 --> Confirm1[Confirmation]
    ClickCancel2 --> Confirm2[Confirmation]

    Confirm1 --> APIUnreserve1[API POST /unreserve]
    Confirm2 --> APIUnreserve2[API POST /unreserve]

    APIUnreserve1 --> UnreserveSuccess1{Succès ?}
    APIUnreserve2 --> UnreserveSuccess2{Succès ?}

    UnreserveSuccess1 -->|Oui| Released[Cadeau libéré]
    UnreserveSuccess1 -->|Non| Error1[Erreur serveur]

    UnreserveSuccess2 -->|Oui| RemovedFromGroup[Retiré des participants]
    UnreserveSuccess2 -->|Non| Error2[Erreur serveur]

    Released --> UpdateOwnerList1[Liste du propriétaire<br/>mise à jour:<br/>Cadeau redevient disponible]
    RemovedFromGroup --> UpdateOwnerList2[Liste du propriétaire<br/>mise à jour:<br/>Reste cadeau groupé]

    UpdateOwnerList1 --> RemoveFromCart1[Supprimé du panier]
    UpdateOwnerList2 --> RemoveFromCart2[Supprimé du panier]

    RemoveFromCart1 --> RecalcTotal1[Totaux recalculés]
    RemoveFromCart2 --> RecalcTotal2[Totaux recalculés]

    RecalcTotal1 --> Success[Annulation réussie]
    RecalcTotal2 --> Success

    Success --> End2([Fin])

    Error1 --> BrowseCart
    Error2 --> BrowseCart

    style Start fill:#e1f5ff
    style Success fill:#c8e6c9
    style Error1 fill:#ffcdd2
    style Error2 fill:#ffcdd2
    style Released fill:#c5e1a5
    style RemovedFromGroup fill:#c5e1a5
```

---

## Récapitulatif des Flux

| Flux | Complexité | Pages Impliquées | Durée Estimée |
|------|------------|------------------|---------------|
| **Connexion** | Simple | Login, Home | 30 sec - 2 min |
| **Création Cadeau** | Moyenne | MyList | 1-3 min |
| **Réservation** | Simple | UserList, Cart | 30 sec - 1 min |
| **Cadeau Groupé** | Moyenne | UserList, Cart | 1-2 min |
| **Gestion Enfant** | Complexe | Home, MyList | 5-10 min |
| **Import Cadeaux** | Simple | MyList | 30 sec - 1 min |
| **Profil** | Moyenne | Profile | 2-5 min |
| **Annulation** | Simple | Cart, UserList | 30 sec |

---

## Légende des Diagrammes

### Formes

- **Rectangle arrondi** (Start/End) : Point d'entrée/sortie
- **Rectangle** : Action/Processus
- **Losange** : Décision/Condition
- **Parallélogramme** : Données/Input

### Couleurs

- 🔵 **Bleu clair** (`#e1f5ff`) : Points de départ
- 🟢 **Vert** (`#c8e6c9`) : Succès/Complétion
- 🔴 **Rouge** (`#ffcdd2`) : Erreurs
- 🟡 **Jaune** (`#fff9c4`) : Avertissements/States spéciaux
- 🟠 **Orange** (`#ffe0b2`) : États neutres

---

## Notes d'Implémentation

Ces diagrammes de flux représentent le comportement idéal de l'application. En cas de divergence entre le comportement réel et ces diagrammes, considérez :

1. **Bugs potentiels** : Si l'app se comporte différemment, c'est peut-être un bug
2. **Évolutions** : Ces flux peuvent être mis à jour lors de nouvelles fonctionnalités
3. **Cas d'usage non couverts** : Certains cas limites peuvent ne pas être représentés

---

## Références

- [Guide de Démarrage](../user-guide/GETTING-STARTED.md)
- [Guide des Fonctionnalités](../user-guide/FEATURES.md)
- [Architecture Système](system-architecture.md)
- [Mermaid Documentation](https://mermaid.js.org/syntax/flowchart.html)
