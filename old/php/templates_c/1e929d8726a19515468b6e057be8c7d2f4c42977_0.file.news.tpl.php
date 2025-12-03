<?php
/* Smarty version 3.1.29, created on 2017-09-15 19:24:36
  from "/home/nironico/public_html/nawel/tpl/news.tpl" */

if ($_smarty_tpl->smarty->ext->_validateCompiled->decodeProperties($_smarty_tpl, array (
  'has_nocache_code' => false,
  'version' => '3.1.29',
  'unifunc' => 'content_59bc61341d5ce2_58508741',
  'file_dependency' => 
  array (
    '1e929d8726a19515468b6e057be8c7d2f4c42977' => 
    array (
      0 => '/home/nironico/public_html/nawel/tpl/news.tpl',
      1 => 1505517745,
      2 => 'file',
    ),
  ),
  'includes' => 
  array (
  ),
),false)) {
function content_59bc61341d5ce2_58508741 ($_smarty_tpl) {
?>
<!-- The Modal -->
<div id="myModal" class="modal">

  <!-- Modal content -->
<div class="modal-content">
  <div class="modal-header">
    <span class="close">×</span>
    <h2>Quoi de neuf ?</h2>
  </div>
  <div class="modal-body">
    <p>Le site à été mis à jour pour cette année 2017 :</p>
    <p>- Un menu déroulant permet désormais de sélectionner unee année, celle-ci permet d'afficher la liste correspondant à l'année spécifiée</p>
    <p>- Il est possible d'importer les élements non sélectionnés de l'année précédente</p>
    <p>- Sur votre liste, le bouton Importer la dernière liste permet de le faire</p>
    <p>- Une fois importés, tous les éléments sont temporaires et doivent être sauvegardés</p>
    <p>- L'encodage du site a été revu afin de ne plus avoir de soucis avec les accents et caractères spéciaux</p>
  </div>
</div>
<input type='hidden' value='<?php echo $_smarty_tpl->tpl_vars['popup']->value;?>
' />

</div><?php }
}
