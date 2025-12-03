<?php
/* Smarty version 3.1.29, created on 2016-11-18 15:32:02
  from "/home/nironico/public_html/nawel/tpl/chat.tpl" */

if ($_smarty_tpl->smarty->ext->_validateCompiled->decodeProperties($_smarty_tpl, array (
  'has_nocache_code' => false,
  'version' => '3.1.29',
  'unifunc' => 'content_582f6542d96eb7_01381378',
  'file_dependency' => 
  array (
    '79f4a932bbe69d0d745e158601767129e1c77868' => 
    array (
      0 => '/home/nironico/public_html/nawel/tpl/chat.tpl',
      1 => 1479501108,
      2 => 'file',
    ),
  ),
  'includes' => 
  array (
  ),
),false)) {
function content_582f6542d96eb7_01381378 ($_smarty_tpl) {
?>
<table id="chat-body">
    <tr >
        <td style="height:500px">
            <div id="chat-display"></div>
        </td>
    </tr>
    <tr >
        <td id="chat-form" valign="top">
            <table id="chat-form-table">
                <tr>
                    <td style="width:100%">
                        <label for="chat-message" >Message</label>
                    </td>
                    <td></td>
                </tr>
                <tr>
                    <td>
                        <input type='hidden' id='chat-id' value='<?php echo $_smarty_tpl->tpl_vars['chatId']->value;?>
' />
                        <input type='hidden' id='chat-uid' value='<?php echo $_smarty_tpl->tpl_vars['userId']->value;?>
' />
                        <input type="text" class='rounded-input' id="chat-message" maxlength="250"  />
                    </td>
                    <td>
                        <button id="chat-submit">Envoyer</button>
                    </td>
                </tr>
            </table>
        </td>
    </tr>
</table><?php }
}
