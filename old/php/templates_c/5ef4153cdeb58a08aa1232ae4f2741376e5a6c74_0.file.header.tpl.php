<?php
/* Smarty version 3.1.29, created on 2016-11-20 16:29:24
  from "/home/nironico/public_html/nawel/tpl/header.tpl" */

if ($_smarty_tpl->smarty->ext->_validateCompiled->decodeProperties($_smarty_tpl, array (
  'has_nocache_code' => false,
  'version' => '3.1.29',
  'unifunc' => 'content_583215b43786d5_57502486',
  'file_dependency' => 
  array (
    '5ef4153cdeb58a08aa1232ae4f2741376e5a6c74' => 
    array (
      0 => '/home/nironico/public_html/nawel/tpl/header.tpl',
      1 => 1479677363,
      2 => 'file',
    ),
  ),
  'includes' => 
  array (
  ),
),false)) {
function content_583215b43786d5_57502486 ($_smarty_tpl) {
?>
<div class='banner'>
</div>
<input type='button' id='news-button' class='btn btn-primary' value='News'>
<nav class="navbar navbar-default">
  <div class="header-container">
    <ul class="nav navbar-nav">
        <li class="bold <?php if ($_smarty_tpl->tpl_vars['currentPage']->value == 'home.php') {?>active<?php }?>""><a href="home.php">Accueil</a></li>
        <li class="bold <?php if ($_smarty_tpl->tpl_vars['currentPage']->value == 'mylist.php') {?>active<?php }?>""><a href="mylist.php">Ma liste</a></li>
        <li class="bold <?php if ($_smarty_tpl->tpl_vars['currentPage']->value == 'cart.php') {?>active<?php }?>""><a href="cart.php">Mon Panier</a></li>
        <li class="bold <?php if ($_smarty_tpl->tpl_vars['currentPage']->value == 'account.php') {?>active<?php }?>""><a href="account.php">Mon Compte</a></li>
        <img class='mini-user-icon' src='../img/avatar/<?php echo $_smarty_tpl->tpl_vars['myavatar']->value;?>
' />
        <li><a class="bold" href="logout.php">Se déconnecter</a></li>
        <!-- <li class="dropdown">
          <a href="#" class="dropdown-toggle" data-toggle="dropdown" role="button" aria-haspopup="true" aria-expanded="false">Listes <span class="caret"></span></a>
          <ul class="dropdown-menu">
            <li><a href="#">Action</a></li>
            <li><a href="#">Another action</a></li>
            <li><a href="#">Something else here</a></li>
            <li role="separator" class="divider"></li>
            <li><a href="#">Separated link</a></li>
            <li role="separator" class="divider"></li>
            <li><a href="#">One more separated link</a></li>
          </ul>
        </li> -->
      </ul>
  </div>
</nav>
<input type='hidden' id='sessId' value='<?php echo $_smarty_tpl->tpl_vars['sessId']->value;?>
' /><?php }
}
