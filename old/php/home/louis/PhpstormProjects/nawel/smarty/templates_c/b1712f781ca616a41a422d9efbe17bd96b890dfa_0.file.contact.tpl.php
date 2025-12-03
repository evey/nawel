<?php
/* Smarty version 3.1.29, created on 2016-11-10 22:32:43
  from "C:\wamp64\www\nawel\tpl\contact.tpl" */

if ($_smarty_tpl->smarty->ext->_validateCompiled->decodeProperties($_smarty_tpl, array (
  'has_nocache_code' => false,
  'version' => '3.1.29',
  'unifunc' => 'content_5824f58b3a3ae4_57936855',
  'file_dependency' => 
  array (
    'b1712f781ca616a41a422d9efbe17bd96b890dfa' => 
    array (
      0 => 'C:\\wamp64\\www\\nawel\\tpl\\contact.tpl',
      1 => 1462288097,
      2 => 'file',
    ),
  ),
  'includes' => 
  array (
  ),
),false)) {
function content_5824f58b3a3ae4_57936855 ($_smarty_tpl) {
?>
<div class="form">
    <form method="post" action="../php/contact.php" role="form">
        <div id="form" class="contact">
            <input type="hidden" name="send" value="1"/>
            <div class="form-group">
                <label for="name">Nom</label>
                <input type="text" id="name" name="name"/><br>
            </div>
            <div class="form-group">
                <label for="firstname">Prénom</label>
                <input type="text" id="firstname" name="firstname"/><br>
            </div>
            <div class="form-group">
                <label for="email">Email</label>
                <input type="text" id="email" name="email"/><br>
            </div>
            <div class="form-group">
                <label for="address_1">Addresse</label>
                <input type="text" id="address_1" name="address_1"/><br>
            </div>
            <div class="form-group">
                <label for="address_2"></label>
                <input type="text" id="address_2" name="address_2"/><br>
            </div>
            <div class="form-group">
                <label for="zip">Code postal</label>
                <input type="text" id="zip" name="zip"/><br>
            </div>
            <div class="form-group">
                <label for="city">Ville</label>
                <input type="text" id="city" name="city"/><br>
            </div>
            <div class="form-group">
                <label for="phone">Téléphone</label>
                <input type="text" id="phone" name="phone"/><br>
            </div>
            <div class="form-group">
                <label for="message">Message</label>
                <textarea id="message" rows="4" cols="50" name="message"></textarea><br>
            </div>
            <div class="form-group">
                <label></label>
                <input type="submit" class="btn btn-info" value="Envoyer"/>
            </div>
        </div>
    </form>
</div><?php }
}
