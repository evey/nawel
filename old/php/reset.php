<?php

include("../config.php");
include("./mail.php");

if (isset($_GET['email'])) {
	$db = new mysqli(DB_HOST, DB_USER, DB_PASSWORD, DB_DATABASE);
	$db->set_charset("utf8");

	$sql = "SELECT id, email FROM user WHERE email = '".htmlspecialchars($_GET['email'], ENT_QUOTES)."'";
	$res = $db->query($sql)->fetch_array();
	if ((int)$res['id'] != 0) {
		$token = MD5(rand());
		$mail = $res['email'];

		$sql = "UPDATE user SET reset_token = '".$token."', token_expiry = DATE_ADD(NOW(), INTERVAL 1 DAY) WHERE email = '".htmlspecialchars($_GET['email'], ENT_QUOTES)."'";
		$res = $db->query($sql);
		$pwrurl = "http://nironi.com/nawel/php/reset_password.php?email=".$_GET['email']."&token=".$token;
		$msg = "<p>Dear user,</p><p>If this e-mail does not apply to you please ignore it. It appears that you have requested a password reset at our website.</p><p>To reset your password, please click the link below. If you cannot click it, please paste it into your web browser's address bar.</p><p>" . $pwrurl . "</p><p>Thanks,\nThe Administration</p>";
		notify_mail($mail, "[NAWEL] Password reset", $msg);
		echo 'success';
	}
	else {
		echo 'failed';
	}
}
else {
	echo 'failed';
}