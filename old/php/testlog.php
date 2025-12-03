<!--
   Si utilisateur/trice est non identifié(e), on affiche le formulaire
-->
<?php session_start(); ?>
<?php if (!isset($_SESSION['LOGGED_USER'])) : ?>
    <form action="submit_login.php" method="POST">
        <!-- si message d'erreur on l'affiche -->
        <?php if (isset($_SESSION['LOGIN_ERROR_MESSAGE'])) : ?>
        <div class="alert alert-danger" role="alert">
            <?php echo $_SESSION['LOGIN_ERROR_MESSAGE'];
            ($_SESSION['LOGIN_ERROR_MESSAGE']); ?>
        </div>
        <?php endif; ?>
        <div class="mb-3">
            <label for="login" class="form-label">Login</label>
            <input type="login" class="form-control" id="login" name="login">
        </div>
        <div class="mb-3">
            <label for="password" class="form-label">Mot de passe</label>
            <input type="password" class="form-control" id="password" name="password">
        </div>
        <button type="submit" class="btn btn-primary">Envoyer</button>
    </form>

    <!-- Si utilisateur/trice bien connectée on affiche un message de succès -->
<?php else : ?>
    <div class="alert alert-success" role="alert">
        Bonjour <?php echo $_SESSION['LOGGED_USER']['pseudo']; ?> et bienvenue sur le site !
    </div>
<?php endif; ?>