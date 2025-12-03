<?php

include('./layout.php');

$db = new mysqli(DB_HOST, DB_USER, DB_PASSWORD, DB_DATABASE);
$db->set_charset("utf8");

$sql = 'SELECT MAX(id) - 30 from chat_messages WHERE chat_id = 1';
$res = $db->query($sql)->fetch_array();

$smarty->assign('lastItemID', $res[0]);

$sql = 'SELECT * FROM family';
$res = $db->query($sql);
$famArr = array();
while ($item = mysqli_fetch_array($res)) {
	$famArr[] = $item;
}

$finalArr = array();

foreach ($famArr as $fam) {
	
	$sql = 'SELECT l.id, l.name, u.avatar, f.name FROM lists l INNER JOIN user u on u.id = l.user_id INNER JOIN family f on u.family_id = f.id WHERE f.id = '.$fam[0].' AND user_id NOT IN ('.$_SESSION['user_id'].',1) ORDER BY l.name ASC';

	$res = $db->query($sql);

	$arr = array();
	while ($item = mysqli_fetch_array($res)) {
		$arr[] = $item;
	}
	$finalArr[$fam[1]] = $arr;
}

$smarty->assign('res', $finalArr);
$smarty->assign('isActive', "1");
$smarty->assign('chatId', "9999");
$smarty->assign('userId', $_SESSION['user_id']);
$smarty->display('../tpl/layout.tpl');
$smarty->display('../tpl/header.tpl');
$smarty->display('../tpl/home.tpl');
$smarty->display('../tpl/chat.tpl');