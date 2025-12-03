<?php
/* Smarty version 3.1.29, created on 2016-11-20 17:50:34
  from "/home/nironico/public_html/nawel/tpl/cart.tpl" */

if ($_smarty_tpl->smarty->ext->_validateCompiled->decodeProperties($_smarty_tpl, array (
  'has_nocache_code' => false,
  'version' => '3.1.29',
  'unifunc' => 'content_583228bab577e6_36056244',
  'file_dependency' => 
  array (
    '0ffd761ffe48498a0cf77a15b468331341ee75a4' => 
    array (
      0 => '/home/nironico/public_html/nawel/tpl/cart.tpl',
      1 => 1479670194,
      2 => 'file',
    ),
  ),
  'includes' => 
  array (
  ),
),false)) {
function content_583228bab577e6_36056244 ($_smarty_tpl) {
?>
<div class='list-content'>
	<div class='item'>
		<div class="header header-item-name bold">Nom</div>
		<div class="header header-item-img bold">Image</div>
		<div class="header header-item-desc bold">Description</div>
		<div class="header header-item-cost bold">Prix</div>
		<div class="header header-item-taken bold">Pour</div>
	</div>
	<?php
$_from = $_smarty_tpl->tpl_vars['res']->value;
if (!is_array($_from) && !is_object($_from)) {
settype($_from, 'array');
}
$__foreach_gift_0_saved_item = isset($_smarty_tpl->tpl_vars['gift']) ? $_smarty_tpl->tpl_vars['gift'] : false;
$_smarty_tpl->tpl_vars['gift'] = new Smarty_Variable();
$_smarty_tpl->tpl_vars['gift']->_loop = false;
foreach ($_from as $_smarty_tpl->tpl_vars['gift']->value) {
$_smarty_tpl->tpl_vars['gift']->_loop = true;
$__foreach_gift_0_saved_local_item = $_smarty_tpl->tpl_vars['gift'];
?>
		<div class='item'>
			<div class='item-name bold'>
				<?php echo $_smarty_tpl->tpl_vars['gift']->value[1];?>

			</div>
			<div class='item-img'>
				<?php if ($_smarty_tpl->tpl_vars['gift']->value[3] != null && $_smarty_tpl->tpl_vars['gift']->value[3] != '') {?>
				<a href='<?php echo $_smarty_tpl->tpl_vars['gift']->value[3];?>
' target="_blank">
				<?php }?>
					<img src='<?php if ($_smarty_tpl->tpl_vars['gift']->value[2] == null) {?>http://www.diocese-djougou.org/images/actualitesdiocese/pas-d-image-dispo.jpg<?php } else {
echo $_smarty_tpl->tpl_vars['gift']->value[2];
}?>' />
				<?php if ($_smarty_tpl->tpl_vars['gift']->value[3] != null && $_smarty_tpl->tpl_vars['gift']->value[3] != '') {?>
				</a>
				<?php }?>
			</div>
			<div class='item-desc'>
				<?php echo $_smarty_tpl->tpl_vars['gift']->value[4];?>

			</div>
			<div class='item-cost'>
				<?php echo $_smarty_tpl->tpl_vars['gift']->value[5];
echo $_smarty_tpl->tpl_vars['gift']->value[6];?>

			</div>
			<div class='item-taken'>
				Pour : <?php echo $_smarty_tpl->tpl_vars['gift']->value[7];?>

			</div>
		</div>
	<?php
$_smarty_tpl->tpl_vars['gift'] = $__foreach_gift_0_saved_local_item;
}
if ($__foreach_gift_0_saved_item) {
$_smarty_tpl->tpl_vars['gift'] = $__foreach_gift_0_saved_item;
}
?>
</div>
<?php }
}
