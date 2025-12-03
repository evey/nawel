<?php
include('layout.php');

if (!empty($_POST)) {
    $db = new mysqli(DB_HOST, DB_USER, DB_PASSWORD, DB_DATABASE);
    $db->set_charset("utf8");
    
    $sql = 'SELECT id, pwd, avatar FROM user WHERE login = "'.$_POST['login'].'"';
    $res = $db->query($sql)->fetch_array();

    if ($res['pwd'] === MD5($_POST['pwd'])) {
        $_SESSION['connected'] = true;
        $_SESSION['user_id'] = $res['id'];
        $_SESSION['avatar'] = $res['avatar'];

        if ($_POST['login'] == $_POST['pwd']) {
            header('location: account.php');
            exit;
        }
        else {
            header('Location: home.php');
            exit;
        }
    }

    echo 'you fail to connect';
}

$smarty->display('../tpl/layout.tpl');
$smarty->display('../tpl/login.tpl');