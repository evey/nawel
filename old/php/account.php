<?php

include('./layout.php');

$db = new mysqli(DB_HOST, DB_USER, DB_PASSWORD, DB_DATABASE);

$sql = 'SELECT * FROM user WHERE id = '.$_SESSION['user_id'].'';
$res = $db->query($sql)->fetch_array();

$smarty->assign('res', $res);
$smarty->assign('isActive', "1");
$smarty->display('../tpl/layout.tpl');
$smarty->display('../tpl/header.tpl');
$smarty->display('../tpl/account.tpl');