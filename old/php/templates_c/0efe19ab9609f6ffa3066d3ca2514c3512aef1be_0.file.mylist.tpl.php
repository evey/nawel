<?php
/* Smarty version 3.1.29, created on 2017-09-15 19:13:22
  from "/home/nironico/public_html/nawel/tpl/mylist.tpl" */

if ($_smarty_tpl->smarty->ext->_validateCompiled->decodeProperties($_smarty_tpl, array (
  'has_nocache_code' => false,
  'version' => '3.1.29',
  'unifunc' => 'content_59bc5e92780ae7_96218325',
  'file_dependency' => 
  array (
    '0efe19ab9609f6ffa3066d3ca2514c3512aef1be' => 
    array (
      0 => '/home/nironico/public_html/nawel/tpl/mylist.tpl',
      1 => 1505517193,
      2 => 'file',
    ),
  ),
  'includes' => 
  array (
  ),
),false)) {
function content_59bc5e92780ae7_96218325 ($_smarty_tpl) {
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
<form action='save.php' method='POST' accept-charset="UTF-8">
	<input type='hidden' id='myListID' class='hidden-list-id' name='mylist' value='<?php echo $_smarty_tpl->tpl_vars['listId']->value;?>
' />
	<div id='list'>
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
		<div class='item'>
			<input type='hidden' id='id' name='id[]' value='<?php echo $_smarty_tpl->tpl_vars['list']->value[0];?>
' />
			<input type='hidden' class='index' name='index[]' value='' />
			<div class='infos-block'>
				<div class='my-item-name'>
					<label>Nom : </label>
					<input class='rounded-input long-input' name='name[]' type='text' value='<?php echo htmlspecialchars($_smarty_tpl->tpl_vars['list']->value[2], ENT_QUOTES, 'UTF-8', true);?>
' />
				</div>
				<div class='my-item-img'>
					<label>Image : </label>
					<input class='rounded-input long-input' name='image[]' type='text' value='<?php echo $_smarty_tpl->tpl_vars['list']->value[4];?>
' />
				</div>
				<div class='my-item-link'>
					<label>Lien : </label>
					<input class='rounded-input long-input' name='link[]' type='text' value='<?php echo $_smarty_tpl->tpl_vars['list']->value[5];?>
' />
				</div>
			</div>
			<div class='my-item-img-preview'>
				<img src='<?php if ($_smarty_tpl->tpl_vars['list']->value[4] == null) {?>http://www.diocese-djougou.org/images/actualitesdiocese/pas-d-image-dispo.jpg<?php } else {
echo $_smarty_tpl->tpl_vars['list']->value[4];
}?>' >
			</div>
			<div class='my-item-desc'>
				<textarea class='rounded-input' name='description[]' cols='50' rows='5'><?php echo $_smarty_tpl->tpl_vars['list']->value[3];?>
</textarea>
			</div>
			<div class='my-item-cost'>
				<label>Prix :</label>
				<input class='rounded-input' name='price[]' type='text' value='<?php echo $_smarty_tpl->tpl_vars['list']->value[6];?>
' />
				<select class='rounded-input' name='currency[]'>
					<option value='EUR'>€</option>
					<option value='USD'>$</option>
				</select>
			</div>
			<input type='button' class='btn btn-danger remove-btn hidden-btn' value="Supprimer" />
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
		<input type='button' class='my-btn btn btn-primary' id='add' value='Ajouter un élément' />
		<input type='button' class='my-btn btn btn-primary' id='import' value='Importer la dernière liste' />
		<input type='submit' class='my-btn btn btn-success' value='Sauvegarder les changements' />
		<input type='button' class='my-btn btn btn-danger' id='back' value='Retour' />
	</div>
</form><?php }
}
