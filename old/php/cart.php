<?php

include('./layout.php');

$db = new mysqli(DB_HOST, DB_USER, DB_PASSWORD, DB_DATABASE);
$db->set_charset("utf8");

$sql = "SELECT g.id, g.name, IFNULL(g.image, '') as image, IFNULL(g.link, '') as link, IFNULL(g.description, '') as description, IFNULL(g.cost, '') as cost, g.currency, u.first_name from gifts g INNER JOIN lists l on l.id = g.list_id INNER JOIN user u on u.id = l.user_id WHERE g.id IN (SELECT sub_g.id FROM gifts sub_g LEFT OUTER JOIN gift_participation sub_gp on sub_gp.gift_id = sub_g.id WHERE (sub_g.taken_by = ".(int)$_SESSION['user_id']." OR sub_gp.user_id = ".(int)$_SESSION['user_id'].") AND sub_g.year = ".date("Y").") ORDER BY u.first_name ASC";

$res = $db->query($sql);

$arr = array();

while ($item = mysqli_fetch_array($res)) {
	$arr[] = $item;
}

$smarty->assign('res', $arr);
$smarty->assign('isActive', "1");
$smarty->display('../tpl/layout.tpl');
$smarty->display('../tpl/header.tpl');
$smarty->display('../tpl/cart.tpl');