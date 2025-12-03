<?php
/* Smarty version 3.1.29, created on 2016-11-10 22:36:26
  from "C:\wamp64\www\nawel\tpl\layout.tpl" */

if ($_smarty_tpl->smarty->ext->_validateCompiled->decodeProperties($_smarty_tpl, array (
  'has_nocache_code' => false,
  'version' => '3.1.29',
  'unifunc' => 'content_5824f66a42bad4_58051553',
  'file_dependency' => 
  array (
    '95677d729f0c548be398dc02ec7ef374247336d7' => 
    array (
      0 => 'C:\\wamp64\\www\\nawel\\tpl\\layout.tpl',
      1 => 1478817385,
      2 => 'file',
    ),
  ),
  'includes' => 
  array (
  ),
),false)) {
function content_5824f66a42bad4_58051553 ($_smarty_tpl) {
?>
<head>
    <title>Swag</title>
    <meta http-equiv="content-type" content="text/html; charset=utf-8" />
    <link href='../css/bootstrap-theme.css' rel='stylesheet' type='text/css' />
    <link href='../css/bootstrap.min.css' rel='stylesheet' type='text/css' />
    <link href='../css/comon.css' rel='stylesheet' type='text/css' />
    <!--<link rel="shortcut icon" type="image/x-icon" href="images/favicon.ico" />-->
    <?php echo '<script'; ?>
 src="../js/jquery.min.js"><?php echo '</script'; ?>
>
    <?php echo '<script'; ?>
 type="text/javascript" src="../js/bootstrap.min.js"><?php echo '</script'; ?>
>
</head>

<div class="header">
    <div class="menu">
        <nav class="navbar navbar-default">
            <div class="container-fluid">
                <div class="collapse navbar-collapse" id="bs-example-navbar-collapse-1">
                    <ul class="nav navbar-nav">
                        <li class="<?php if ($_smarty_tpl->tpl_vars['isActive']->value == 1) {?>active<?php }?>"><a href="../php/contact.php">Construction</a></li>
                        <li class="<?php if ($_smarty_tpl->tpl_vars['isActive']->value == 2) {?>active<?php }?>"><a href="../php/contact.php">Maisons</a></li>
                        <li class="<?php if ($_smarty_tpl->tpl_vars['isActive']->value == 3) {?>active<?php }?>"><a href="../php/contact.php">Services</a></li>
                        <li class="<?php if ($_smarty_tpl->tpl_vars['isActive']->value == 4) {?>active<?php }?>"><a href="../php/contact.php">Tarifs</a></li>
                        <li class="<?php if ($_smarty_tpl->tpl_vars['isActive']->value == 5) {?>active<?php }?>"><a href="../php/contact.php">Contact</a></li>
                    </ul>
                </div>
            </div>
        </nav>
    </div>
</div><?php }
}
