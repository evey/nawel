<?php
/* Smarty version 3.1.29, created on 2016-11-20 16:09:10
  from "/home/nironico/public_html/nawel/tpl/reset_password.tpl" */

if ($_smarty_tpl->smarty->ext->_validateCompiled->decodeProperties($_smarty_tpl, array (
  'has_nocache_code' => false,
  'version' => '3.1.29',
  'unifunc' => 'content_583210f675fa88_42922192',
  'file_dependency' => 
  array (
    'b70d3fcfb8e6cf58719a39851abebe1d41aac08c' => 
    array (
      0 => '/home/nironico/public_html/nawel/tpl/reset_password.tpl',
      1 => 1479676141,
      2 => 'file',
    ),
  ),
  'includes' => 
  array (
  ),
),false)) {
function content_583210f675fa88_42922192 ($_smarty_tpl) {
?>
<div id="log-main" class="log-main">
	<div id="log" class="log">
		<form action="" method="post">
			<label for="pwd">Mot de passe :</label>
			<input id="log-login" name="pwd" placeholder="**********" type="password">
			<label for="new-pwd">Confirmer mot de passe :</label>
			<input id="log-pwd" name="confirmation" placeholder="**********" type="password">
			<input id="log-submit" name="submit" type="submit" value=" Valider ">
		</form>
	</div>
</div><?php }
}
