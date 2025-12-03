<div id="log-main" class="log-main">
	<div id="log" class="log">
		<form action="" method="post">
			<label for="login">Nom d'utilisateur :</label>
			<input id="log-login" name="login" placeholder="Nom d'utilisateur" type="text">
			<label for="pwd">Mot de passe :</label>
			<input id="log-pwd" name="pwd" placeholder="**********" type="password">
			<input id="log-submit" name="submit" type="submit" value=" Login ">
			<input type="button" name="reset_button" value="Reset mot de passe" id="login-reset-button">
		</form>
	</div>
	<div id='reset-panel' class='log'>
		<label for="log-email">Email :</label>
		<input id="log-email" name="email" placeholder="Email" type="text">
		<input type="button" name="validate_reset" value="Envoyer" id="login-validate-reset" class='btn btn-success'>
		<div id="error">Une erreur s'est produite</div>
		<div id="success">email de récupération envoyé</div>
	</div>
</div>