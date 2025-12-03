<?php
/* Smarty version 3.1.29, created on 2025-10-13 17:28:45
  from "/home/nironico/public_html/nawel/tpl/home.tpl" */

if ($_smarty_tpl->smarty->ext->_validateCompiled->decodeProperties($_smarty_tpl, array (
  'has_nocache_code' => false,
  'version' => '3.1.29',
  'unifunc' => 'content_68ed6f0d4bcab1_37360203',
  'file_dependency' => 
  array (
    'd6566d22fdb33626c33cf3e02b3a941f3ed555dc' => 
    array (
      0 => '/home/nironico/public_html/nawel/tpl/home.tpl',
      1 => 1760390922,
      2 => 'file',
    ),
  ),
  'includes' => 
  array (
  ),
),false)) {
function content_68ed6f0d4bcab1_37360203 ($_smarty_tpl) {
?>
<input type='hidden' id='lastItemID' value = '<?php echo $_smarty_tpl->tpl_vars['lastItemID']->value;?>
' />
<div class='list-content'>
	<?php
$_from = $_smarty_tpl->tpl_vars['res']->value;
if (!is_array($_from) && !is_object($_from)) {
settype($_from, 'array');
}
$__foreach_family_0_saved_item = isset($_smarty_tpl->tpl_vars['family']) ? $_smarty_tpl->tpl_vars['family'] : false;
$__foreach_family_0_saved_key = isset($_smarty_tpl->tpl_vars['famName']) ? $_smarty_tpl->tpl_vars['famName'] : false;
$_smarty_tpl->tpl_vars['family'] = new Smarty_Variable();
$_smarty_tpl->tpl_vars['famName'] = new Smarty_Variable();
$_smarty_tpl->tpl_vars['family']->_loop = false;
foreach ($_from as $_smarty_tpl->tpl_vars['famName']->value => $_smarty_tpl->tpl_vars['family']->value) {
$_smarty_tpl->tpl_vars['family']->_loop = true;
$__foreach_family_0_saved_local_item = $_smarty_tpl->tpl_vars['family'];
?>
		<div class='list-family'>
		<p class='list-family-name'>Famille <?php echo $_smarty_tpl->tpl_vars['famName']->value;?>
</p>
			<?php
$_from = $_smarty_tpl->tpl_vars['family']->value;
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
				<div class='list-item'>
					<a href='./list.php?id=<?php echo $_smarty_tpl->tpl_vars['list']->value[0];?>
'>
						<div class='list-body'>
							<div class='list-name bold'>
								<?php echo $_smarty_tpl->tpl_vars['list']->value[1];?>

							</div>
							<img class='user-icon' src='../img/avatar/<?php echo $_smarty_tpl->tpl_vars['list']->value[2];?>
' />
						</div>
					</a>
				</div>
			<?php
$_smarty_tpl->tpl_vars['list'] = $__foreach_list_1_saved_local_item;
}
if ($__foreach_list_1_saved_item) {
$_smarty_tpl->tpl_vars['list'] = $__foreach_list_1_saved_item;
}
?>
		</div>
	<?php
$_smarty_tpl->tpl_vars['family'] = $__foreach_family_0_saved_local_item;
}
if ($__foreach_family_0_saved_item) {
$_smarty_tpl->tpl_vars['family'] = $__foreach_family_0_saved_item;
}
if ($__foreach_family_0_saved_key) {
$_smarty_tpl->tpl_vars['famName'] = $__foreach_family_0_saved_key;
}
?>
</div><?php }
}
