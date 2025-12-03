<?php
/* Smarty version 3.1.29, created on 2025-08-28 15:33:46
  from "/home/nironico/public_html/nawel/tpl/login.tpl" */

if ($_smarty_tpl->smarty->ext->_validateCompiled->decodeProperties($_smarty_tpl, array (
  'has_nocache_code' => false,
  'version' => '3.1.29',
  'unifunc' => 'content_68b0af1a330aa0_51895869',
  'file_dependency' => 
  array (
    '03668195fc6f3140213f0501ed86ef586b2c5a51' => 
    array (
      0 => '/home/nironico/public_html/nawel/tpl/login.tpl',
      1 => 1756409584,
      2 => 'file',
    ),
  ),
  'includes' => 
  array (
  ),
),false)) {
function content_68b0af1a330aa0_51895869 ($_smarty_tpl) {
?>
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
</div><?php }
}
