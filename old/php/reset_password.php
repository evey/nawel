<?php
include('layout.php');

if (isset($_POST['pwd'])) {
    $db = new mysqli(DB_HOST, DB_USER, DB_PASSWORD, DB_DATABASE);
    $db->set_charset("utf8");
    
    $sql = 'SELECT id, reset_token FROM user WHERE email = "'.htmlspecialchars($_GET['email'], ENT_QUOTES).'"';
    $res = $db->query($sql)->fetch_array();
    $uid = (int)$res['id'];

    if ($uid != 0 && $res['reset_token'] == $_GET['token']) {
        $sql = "UPDATE user SET pwd='".MD5($_POST['pwd'])."', reset_token = NULL, token_expiry = NULL WHERE id=".$uid;
        $res = $db->query($sql);

        header('location: login.php');
        exit;
    }

    echo 'Invalid token';
}

$smarty->display('../tpl/layout.tpl');
$smarty->display('../tpl/reset_password.tpl');