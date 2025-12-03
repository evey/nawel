<?php
/* Smarty version 3.1.29, created on 2017-09-15 19:13:25
  from "/home/nironico/public_html/nawel/tpl/list.tpl" */

if ($_smarty_tpl->smarty->ext->_validateCompiled->decodeProperties($_smarty_tpl, array (
  'has_nocache_code' => false,
  'version' => '3.1.29',
  'unifunc' => 'content_59bc5e953d8658_62513558',
  'file_dependency' => 
  array (
    'd1c91c8e5c91bcfaaca5605ba73074b1674b5c11' => 
    array (
      0 => '/home/nironico/public_html/nawel/tpl/list.tpl',
      1 => 1505517194,
      2 => 'file',
    ),
  ),
  'includes' => 
  array (
  ),
),false)) {
function content_59bc5e953d8658_62513558 ($_smarty_tpl) {
?>
<div class='center'>
	<select id='year-selector' class='dropdown'>
		<?php
$_from = $_smarty_tpl->tpl_vars['resYear']->value;
if (!is_array($_from) && !is_object($_from)) {
settype($_from, 'array');
}
$__foreach_y_0_saved_item = isset($_smarty_tpl->tpl_vars['y']) ? $_smarty_tpl->tpl_vars['y'] : false;
$_smarty_tpl->tpl_vars['y'] = new Smarty_Variable();
$_smarty_tpl->tpl_vars['y']->_loop = false;
foreach ($_from as $_smarty_tpl->tpl_vars['y']->value) {
$_smarty_tpl->tpl_vars['y']->_loop = true;
$__foreach_y_0_saved_local_item = $_smarty_tpl->tpl_vars['y'];
?>
			<option value='<?php echo $_smarty_tpl->tpl_vars['y']->value[0];?>
' <?php if ($_smarty_tpl->tpl_vars['y']->value[0] == $_smarty_tpl->tpl_vars['selectedYear']->value) {?>selected='selected'<?php }?>><?php echo $_smarty_tpl->tpl_vars['y']->value[0];?>
</option>
		<?php
$_smarty_tpl->tpl_vars['y'] = $__foreach_y_0_saved_local_item;
}
if ($__foreach_y_0_saved_item) {
$_smarty_tpl->tpl_vars['y'] = $__foreach_y_0_saved_item;
}
?>
	</select>
</div>
<form action='save.php' method='POST'>
	<input type='hidden' name='list_infos' id='user_infos' class='hidden-list-id' value='<?php echo $_smarty_tpl->tpl_vars['listId']->value;?>
' />
	<div class='list-content'>
		<div class='item'>
			<div class="header header-item-check bold">Disponible ?</div>
			<div class="header header-item-name bold">Nom</div>
			<div class="header header-item-img bold">Image</div>
			<div class="header header-item-desc bold">Descriptrion</div>
			<div class="header header-item-cost bold">Prix</div>
			<div class="header header-item-taken bold">Pris par</div>
			<div class="header header-item-taken bold">Participants</div>
			<div class="header header-item-comment bold">Commentaires</div>
		</div>
		<?php
$_from = $_smarty_tpl->tpl_vars['res']->value;
if (!is_array($_from) && !is_object($_from)) {
settype($_from, 'array');
}
$__foreach_list_1_saved_item = isset($_smarty_tpl->tpl_vars['list']) ? $_smarty_tpl->tpl_vars['list'] : false;
$_smarty_tpl->tpl_vars['list'] = new Smarty_Variable();
$_smarty_tpl->tpl_vars['list']->_loop = false;
foreach ($_from as $_smarty_tpl->tpl_vars['list']->value) {
$_smarty_tpl->tpl_vars['list']->_loop = true;
$__foreach_list_1_saved_local_item = $_smarty_tpl->tpl_vars['list'];
?>
			<div class='item <?php if ($_smarty_tpl->tpl_vars['list']->value[8] == 0 && $_smarty_tpl->tpl_vars['list']->value[10] != $_smarty_tpl->tpl_vars['uid']->value) {?>item-disabled<?php }?>'>
				<input type="checkbox" class='item-check' name='ck_<?php echo $_smarty_tpl->tpl_vars['list']->value[0];?>
' id="<?php echo $_smarty_tpl->tpl_vars['list']->value[0];?>
" <?php if ($_smarty_tpl->tpl_vars['list']->value[8] == 0) {?> checked='checked' <?php if ($_smarty_tpl->tpl_vars['list']->value[10] != $_smarty_tpl->tpl_vars['uid']->value) {?> disabled<?php }?> <?php }?> /><label for="<?php echo $_smarty_tpl->tpl_vars['list']->value[0];?>
"><span class="ui"></span></label>
				<div class='item-name bold'>
					<?php echo $_smarty_tpl->tpl_vars['list']->value[2];?>

				</div>
				<div class='item-img'>
					<?php if ($_smarty_tpl->tpl_vars['list']->value[5] != null && $_smarty_tpl->tpl_vars['list']->value[5] != '') {?>
					<a href='<?php echo $_smarty_tpl->tpl_vars['list']->value[5];?>
' target="_blank">
					<?php }?>
						<img src='<?php if ($_smarty_tpl->tpl_vars['list']->value[4] == null) {?>http://www.diocese-djougou.org/images/actualitesdiocese/pas-d-image-dispo.jpg<?php } else {
echo $_smarty_tpl->tpl_vars['list']->value[4];
}?>' />
					<?php if ($_smarty_tpl->tpl_vars['list']->value[5] != null && $_smarty_tpl->tpl_vars['list']->value[5] != '') {?>
					</a>
					<?php }?>
				</div>
				<div class='item-desc'>
					<?php echo $_smarty_tpl->tpl_vars['list']->value[3];?>

				</div>
				<div class='item-cost'>
					<?php echo $_smarty_tpl->tpl_vars['list']->value[6];
echo $_smarty_tpl->tpl_vars['list']->value[7];?>

				</div>
				<div class='item-taken'>
					Pris par : <?php echo $_smarty_tpl->tpl_vars['list']->value[9];?>

				</div>
				<div class='item-taken'>
					<?php if ($_smarty_tpl->tpl_vars['list']->value[12]) {
echo $_smarty_tpl->tpl_vars['list']->value[12];?>
 participe(nt) au cadeau.<?php }?>
					<input type='checkbox' class='participant' name='ck_participant_<?php echo $_smarty_tpl->tpl_vars['list']->value[0];?>
' id='ck_participant_<?php echo $_smarty_tpl->tpl_vars['list']->value[0];?>
' <?php if ($_smarty_tpl->tpl_vars['list']->value[13] == 1) {?> checked='checked'<?php }?> /><label for="ck_participant_<?php echo $_smarty_tpl->tpl_vars['list']->value[0];?>
"><span class="ui"></span></label>
				</div>
				<div class='item-comment'>
					<textarea class='rounded-input' name='comment_<?php echo $_smarty_tpl->tpl_vars['list']->value[0];?>
' cols='30' rows='5'><?php echo $_smarty_tpl->tpl_vars['list']->value[14];?>
</textarea>
				</div>
				<input type='hidden' name='<?php echo $_smarty_tpl->tpl_vars['list']->value[0];?>
' value='<?php echo $_smarty_tpl->tpl_vars['list']->value[10];?>
' />
				<input type='hidden' class='hidden-participant' name='participant_<?php echo $_smarty_tpl->tpl_vars['list']->value[0];?>
' id='participant_<?php echo $_smarty_tpl->tpl_vars['list']->value[0];?>
' value='<?php echo $_smarty_tpl->tpl_vars['list']->value[13];?>
' />
				<input type='hidden' id='file' value='<?php echo $_smarty_tpl->tpl_vars['list']->value[11];?>
' />
				<input type='hidden' id='list_uid' value='<?php echo $_smarty_tpl->tpl_vars['list_uid']->value;?>
' />
			</div>
		<?php
$_smarty_tpl->tpl_vars['list'] = $__foreach_list_1_saved_local_item;
}
if ($__foreach_list_1_saved_item) {
$_smarty_tpl->tpl_vars['list'] = $__foreach_list_1_saved_item;
}
?>
	</div>
	<div class='action-box'>
		<input type='button' class='my-btn btn btn-primary' id='download' value='Télécharger la liste' />
		<input type='submit' class='my-btn btn btn-success' value='Sauvegarder les changements' />
		<input type='button' class='my-btn btn btn-danger' id='back' value='Retour' />
	</div>
</form><?php }
}
