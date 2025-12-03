<?php

include('./layout.php');

$db = new mysqli(DB_HOST, DB_USER, DB_PASSWORD, DB_DATABASE);
$db->set_charset("utf8");

$sql = 'SELECT id FROM lists WHERE user_id = '.(int)$_SESSION['user_id'].'';

$res = $db->query($sql)->fetch_array();

$smarty->assign('listId', $res['id']);

$sql = 'SELECT DISTINCT year FROM (SELECT DISTINCT year FROM gifts UNION ALL SELECT YEAR(NOW())) as R ORDER BY year DESC';

$res2 = $db->query($sql);

$arrY = array();

while ($item = mysqli_fetch_array($res2)) {
	$arrY[] = $item;
}

if (isset($_GET['year']))
	$year = (int)$_GET['year'];
else
	$year = date("Y");

$sql = 'SELECT g.id, g.list_id, g.name, g.description, g.image, g.link, g.cost, g.currency, g.available, u.first_name, g.taken_by, l.filename FROM gifts g LEFT OUTER JOIN user u ON u.id = g.taken_by INNER JOIN lists l on l.id = g.list_id WHERE list_id = '.$res['id'].' AND g.year = '.date("Y").'';

$res = $db->query($sql);

$arr = array();

while ($item = mysqli_fetch_array($res)) {
	$arr[] = $item;
}

$smarty->assign('selectedYear', $year);
$smarty->assign('resYear', $arrY);
$smarty->assign('res', $arr);
$smarty->assign('uid', $_SESSION['user_id']);
$smarty->assign('isActive', "1");
$smarty->display('../tpl/layout.tpl');
$smarty->display('../tpl/header.tpl');
$smarty->display('../tpl/mylist.tpl');