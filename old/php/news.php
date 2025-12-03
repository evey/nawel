<?php

include("../config.php");

$db = new mysqli(DB_HOST, DB_USER, DB_PASSWORD, DB_DATABASE);
$db->set_charset("utf8");

$sql = 'UPDATE user SET display_popup = 0 WHERE id = '.(int)$_GET['id'];
$res = $db->query($sql);
var_dump($sql);