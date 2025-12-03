<?php
include('./layout.php');

$db = new mysqli(DB_HOST, DB_USER, DB_PASSWORD, DB_DATABASE);
$db->set_charset("utf8");

$sql = "SELECT id, first_name, email, notify_list_edit, notify_gift_taken FROM user WHERE 1";
$res = $db->query($sql);

$userToNotify = array();

while ($item = mysqli_fetch_array($res)) {
	$userToNotify[$item['id']] = $item;
}

$sql = "SELECT g.id as id, g.list_id, g.name, g.taken_by, u2.first_name as taken_by_name, u.id as uid, u.first_name, CONCAT_WS(';', g.id, g.name, IFNULL(g.description, ''), IFNULL(g.image, ''), IFNULL(g.link, ''), IFNULL(g.cost, ''), IFNULL(g.currency, '')) as data, CONCAT_WS(';', g.available, IFNULL(g.taken_by, ''), IFNULL(g.comment, '')) as taken FROM gifts g INNER JOIN lists l ON l.id = g.list_id INNER JOIN user u on u.id = l.user_id LEFT OUTER JOIN user u2 ON u2.id = g.taken_by WHERE 1";
$res = $db->query($sql);

$gifts = array();

while ($item = mysqli_fetch_array($res)) {
	$gifts[$item['id']] = $item;
}

if (isset($_POST['user_infos'])) {
	$pwd = '';
	$avatar = secure_string($_POST['current-avatar']);
	if (isset($_POST['pwd']) && $_POST['pwd'] != '') {
		$pwd = 'pwd = "'.MD5($_POST['pwd']).'",';
	}
	if (isset($_FILES['avatar']) && $_FILES['avatar']['error'] == UPLOAD_ERR_OK) {
		$extension = strtolower(substr(strrchr($_FILES['avatar']['name'], '.'), 1));
		$avatar = ''.(int)$_SESSION['user_id'].'.'.$extension.'';
		$filename = '../img/avatar/'.$avatar;
		move_uploaded_file($_FILES['avatar']['tmp_name'], $filename);
	}
	var_dump($_POST);
	$sql = 'UPDATE user SET login = "'.$_POST['login'].'", '.$pwd.' email = '.isnull($_POST['email']).', first_name = '.isnull($_POST['firstname']).', last_name = '.isnull($_POST['lastname']).', avatar = '.isnull($avatar).', pseudo = IFNULL('.isnull($_POST['pseudo']).', '.isnull($_POST['firstname']).'), notify_list_edit = '.($_POST['notify_list_edit'] == 'on' ? 1 : 0).', notify_gift_taken = '.($_POST['notify_gift_taken'] == 'on' ? 1 : 0).' WHERE id='.(int)$_SESSION['user_id'].'';
	$db->query($sql);
	header('location: home.php', true);
	exit;
}
else if (isset($_POST['list_infos'])) {
	$add = array();
	$del = array();
	$user = $userToNotify[$_SESSION['user_id']]['first_name'];
	$target = '';
	$tid = 0;
	foreach ($_POST as $k => $v) {
		if ((int)$k)
		{
			$comment = $_POST['comment_'.(int)$k];
			$taken = ($v ? 0 : 1).';'.$v.';'.$comment;
			if ($taken != $gifts[$k]['taken']) {
				$cmt = ', comment = NULL';
				if (strlen($comment) > 0) {
					$cmt = ', comment = \''.$comment.'\'';
				}
				$sql = 'UPDATE gifts SET available = '.($v ? 0 : 1).', taken_by = '.($v ? $v : 'NULL').$cmt.' WHERE id='.$k.'';
				$db->query($sql);
				if ($v != $gifts[$k]['taken_by']) {
					$target = $userToNotify[$gifts[$k]['uid']]['first_name'];
					$tid = $gifts[$k]['uid'];
					if ($v) {
						$add[] = $gifts[$k]['name'];
					}
					else {
						$del[] = $gifts[$k]['name'];
					}
				}
			}
		}
		else if (substr($k, 0, 11) == 'participant') {
			$giftId = substr($k, 12);
			$sql = 'DELETE from gift_participation WHERE gift_id = '.(int)$giftId.' AND user_id = '.(int)$_SESSION['user_id'].'';
			$db->query($sql);
			if ($v == 1)
			{
				$sql = 'INSERT INTO gift_participation (gift_id, user_id) VALUES ('.(int)$giftId.', '.(int)$_SESSION['user_id'].')';
				$db->query($sql);
			}
		}
	}
	if (count($add) > 0 || count($del) > 0) {
		$sub = "[NAWEL] : Quelqu'un a modifié son choix de cadeaux";
		$msg = "<p>".$user." a édité la liste de ".$target."</p>";
		if (count($add) > 0) {
			$msg .= "<p>Les objets suivants ont été pris :<br />";
			foreach ($add as $key => $value) {
				$msg .= "- ".$value."<br />";
			}
			$msg .= "</p>";
		}
		else {
			$msg .= "<p>aucun objet n'a été pris</p>";
		}
		if (count($del) > 0) {
			$msg .= "<p>Les objets suivants ont été libérés :<br />";
			foreach ($del as $key => $value) {
				$msg .= "- ".$value."<br />";
			}
			$msg .= "</p>";
		}
		else {
			$msg .= "<p>aucun objet n'a été libéré</p>";
		}
		send_notification($sub, $msg, $tid, 'notify_gift_taken', $userToNotify);
	}

	header('Location: home.php', true);
	exit;
}
else if (isset($_POST['mylist'])) {
	$ids = $_POST['id'];
	$name = $_POST['name'];
	$img = $_POST['image'];
	$link = $_POST['link'];
	$desc = $_POST['description'];
	$price = $_POST['price'];
	$curr = $_POST['currency'];
	$upd = array();
	$add = array();
	$uid = $gifts[$ids[0]]['uid'];
	$lid = $gifts[$ids[0]]['list_id'];
	$user = $gifts[$ids[0]]['first_name'];

	foreach ($ids as $key => $value) {
		if ($value == 0 && isnull($name[$key]) != 'NULL') {
			$sql = 'INSERT INTO gifts (list_id, name, description, image, link, cost, currency, year) VALUES ("'.(int)$_POST['mylist'].'", '.isnull($name[$key]).', '.isnull($desc[$key]).', '.isnull($img[$key]).', '.isnull($link[$key]).', '.isnull($price[$key]).', '.isnull($curr[$key]).', '.date("Y").')';
			$db->query($sql);
			$add[] = $name[$key];
		}
		else {
			$data = $value.';'.$name[$key].';'.$desc[$key].';'.$img[$key].';'.$link[$key].';'.$price[$key].';'.$curr[$key];
			if ($data != $gifts[$value]['data']) {
				$sql = 'UPDATE gifts SET name = '.isnull($name[$key]).', description = '.isnull($desc[$key]).', image = '.isnull($img[$key]).', link = '.isnull($link[$key]).', cost = '.isnull($price[$key]).', currency = '.isnull($curr[$key]).', year = '.date("Y").' WHERE id = '.(int)$value;
				$db->query($sql);
				$upd[] = $name[$key];
			}
		}
	}
	if (count($add) > 0 || count($upd) > 0) {
		$sub = "[NAWEL] : Une liste a été éditée";
		$msg = "<p>".$user." a édité sa liste</p>";
		if (count($add) > 0) {
			$msg .= "<p>Les objets suivants ont été ajoutés :<br />";
			foreach ($add as $key => $value) {
				$msg .= "- ".$value."<br />";
			}
			$msg .= "</p>";
		}
		else {
			$msg .= "<p>aucun objet n'a été ajouté</p>";
		}
		if (count($upd) > 0) {
			$msg .= "<p>Les objets suivants ont été modifiés :<br />";
			foreach ($upd as $key => $value) {
				$msg .= "- ".$value."<br />";
			}
			$msg .= "</p>";
		}
		else {
			$msg .= "<p>aucun objet n'a été modifié</p>";
		}
		$msg .= "<p>Pour voir la liste en entier, <a href=http://nironi.com/nawel/php/list.php?id=".$lid.">cliquez ici</a></p>";
		send_notification($sub, $msg, $uid, 'notify_list_edit', $userToNotify);
	}
	header('Location: mylist.php', true);
	exit;
}

function send_notification($sub, $msg, $exclude, $notif, $userToNotify) {
	$userNotified = array();
	foreach ($userToNotify as $key => $value) {
		if ($value['id'] != $exclude && $value['email'] != '' && $value[$notif] == 1) {
			echo '$sub = '.$sub.'<br />$msg = '.$msg.'<br />$exclude = '.$exclude.'<br />notif = '.$notif.'<br />';
			var_dump($value);
			echo '<br />'.$value['id'].'<br />'.$value['email'].'<br />'.$value[$notif];
			$userNotified[] = $value['email'];
		}
	}
	if (count($userNotified) > 0) {
		notify_mail(implode(', ', $userNotified), $sub, $msg);
	}
	//notify_mail('evey.hyuu@gmail.com, uqcvl3nv@yopmail.com', 'user notified', "sujet : ".$sub."<br />Message : ".$msg."<br />Excluded : ".$exclude."<br />Notif Type : ".$notif."<br />User Notified : ");
}

$smarty->display('../tpl/layout.tpl');
$smarty->display('../tpl/header.tpl');
$smarty->display('../tpl/save.tpl');