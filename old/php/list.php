<?php

include('./layout.php');

$db = new mysqli(DB_HOST, DB_USER, DB_PASSWORD, DB_DATABASE);
$db->set_charset("utf8");

$sql = 'SELECT isChildren FROM user WHERE id = '.(int)$_SESSION['user_id'].'';

$res = $db->query($sql)->fetch_array();

$isChildren = $res[0];

$sql = 'SELECT user_id FROM lists WHERE id = '.(int)$_GET['id'].'';

$res = $db->query($sql)->fetch_array();

if (($isChildren == 1 || $_SESSION['user_id'] == 6 || $_SESSION['user_id'] == 14 || $res[0] == $_SESSION['user_id']) && (!isset($_GET['year']) || $_GET['year'] == date("Y"))) {
	header('Location: mylist.php');
    exit;
}
else {
	$smarty->assign('list_uid', $res[0]);
}



$sql = 'SELECT DISTINCT year FROM (SELECT DISTINCT year FROM gifts UNION ALL SELECT YEAR(NOW())) as R ORDER BY year DESC';

$res = $db->query($sql);

$arrY = array();

while ($item = mysqli_fetch_array($res)) {
	$arrY[] = $item;
}

if (isset($_GET['year']))
	$year = (int)$_GET['year'];
else
	$year = date("Y");

$sql = 'SELECT g.id, g.list_id, g.name, g.description, g.image, g.link, g.cost, g.currency, g.available, u.first_name, g.taken_by, l.filename, GROUP_CONCAT(u2.first_name SEPARATOR \', \') as participants, MAX(CASE WHEN u2.id = '.(int)$_SESSION['user_id'].' THEN 1 ELSE 0 END) AS myparticipation, g.comment FROM gifts g LEFT OUTER JOIN user u ON u.id = g.taken_by INNER JOIN lists l on l.id = g.list_id LEFT OUTER JOIN gift_participation gp on gp.gift_id = g.id LEFT OUTER JOIN user u2 on u2.id = gp.user_id WHERE list_id = '.(int)$_GET['id'].' AND g.year = '.$year.' GROUP BY g.id ORDER BY g.id ASC';

$res = $db->query($sql);

$arr = array();

while ($item = mysqli_fetch_array($res)) {
	$arr[] = $item;
}

$smarty->assign('selectedYear', $year);
$smarty->assign('resYear', $arrY);
$smarty->assign('res', $arr);
$smarty->assign('uid', $_SESSION['user_id']);
$smarty->assign('listId', (int)$_GET['id']);
$smarty->assign('chatId', (int)$_GET['id']);
$smarty->assign('userId', $_SESSION['user_id']);
$smarty->assign('isActive', "1");
$smarty->display('../tpl/layout.tpl');
$smarty->display('../tpl/header.tpl');
$smarty->display('../tpl/list.tpl');
$smarty->display('../tpl/chat.tpl');
