<?php

include("../config.php");
//require_once( "./chat_model.php" );

if (isset($_GET['chattext']) AND isset($_GET['userId']) AND isset($_GET['chatId'])) {
    $chattext = htmlspecialchars($_GET['chattext'], ENT_QUOTES);
    $userId = intval($_GET['userId']);
    $chatId = intval($_GET['chatId']);
    $db = new mysqli(DB_HOST, DB_USER, DB_PASSWORD, DB_DATABASE);
    $db->set_charset("utf8");
    $sql = "INSERT INTO chat_messages (chat_id, user_id, message, date_sent) VALUES (".$chatId.", ".$userId.", '".$chattext."', NOW())";
    $db->query($sql);
}
else {
    $id = intval($_GET['lastTimeID']);
    $chatId = intval($_GET['chatId']);
    $arr = array();
    $jsonData = '{"results":[';
    $db = new mysqli(DB_HOST, DB_USER, DB_PASSWORD, DB_DATABASE);
    $db->set_charset("utf8");
    if ($id == 0) {
        $sql = "SELECT m.id, u.pseudo, m.message, DATE_ADD(m.date_sent, INTERVAL 6 HOUR) as date_sent, u.avatar, NOW() as date_now FROM chat_messages m INNER JOIN user u on u.id = m.user_id WHERE m.chat_id = ".$chatId." and m.id > (SELECT MIN(id) from chat_messages where chat_id = ".$chatId." order by id DESC LIMIT 0, 30) ORDER BY date_sent ASC"; 
    }
    else {
        $sql = "SELECT m.id, u.pseudo, m.message, DATE_ADD(m.date_sent, INTERVAL 6 HOUR) as date_sent, u.avatar, NOW() as date_now FROM chat_messages m INNER JOIN user u on u.id = m.user_id WHERE m.chat_id = ".$chatId." and m.id > ".$id." and m.date_sent >= DATE_SUB(NOW(), INTERVAL 1 HOUR) ORDER BY date_sent ASC"; 
    }
    //echo $sql;
    //$statement = $db->prepare("SELECT m.id, u.pseudo, m.message, m.date_sent FROM chat_messages m INNER JOIN user u on u.id = m.user_id WHERE m.chat_id = ? and m.id > ? and m.date_sent >= DATE_SUB(NOW(), INTERVAL 1 HOUR)");
    //$statement->bind_param($chatId, $id);
    //$statement->execute();
    $result = $db->query($sql);//->fetch_array();
    //$statement->bind_result($id, $pseudo, $message, $date_sent);
    //var_dump($result);
    $line = new stdClass;
    //echo 'test';
    //var_dump($result);
    while ($row = $result->fetch_array()) {
        $line->id = $row['id'];
        $line->pseudo = $row['pseudo'];
        $line->message = $row['message'];
        $line->date_sent = date('H:i:s', strtotime($row['date_sent']));
        $line->avatar = $row['avatar'];
        //$line->date_sent = getRelativeTime($row['date_sent'], $row['date_now']);
        $arr[] = json_encode($line);
    }
    //$statement->close();
    $db->close();
    $jsonData .= implode(",", $arr);
    $jsonData .= ']}';
    print $jsonData;
}

function getRelativeTime($date, $now) {
    // Déduction de la date donnée à la date actuelle
    $time = strtotime($now) - strtotime($date); 
 
    // Calcule si le temps est passé ou à venir
    if ($time > 0) {
        $when = "il y a";
    } else if ($time < 0) {
        $when = "dans environ";
    } else {
        return "il y a 1 seconde";
    }
    $time = abs($time); 
 
    // Tableau des unités et de leurs valeurs en secondes
    $times = array( 31104000 =>  'an{s}',       // 12 * 30 * 24 * 60 * 60 secondes
                    2592000  =>  'mois',        // 30 * 24 * 60 * 60 secondes
                    86400    =>  'jour{s}',     // 24 * 60 * 60 secondes
                    3600     =>  'heure{s}',    // 60 * 60 secondes
                    60       =>  'minute{s}',   // 60 secondes
                    1        =>  'seconde{s}'); // 1 seconde         
 
    foreach ($times as $seconds => $unit) {
        // Calcule le delta entre le temps et l'unité donnée
        $delta = round($time / $seconds); 
 
        // Si le delta est supérieur à 1
        if ($delta >= 1) {
            // L'unité est au singulier ou au pluriel ?
            if ($delta == 1) {
                $unit = str_replace('{s}', '', $unit);
            } else {
                $unit = str_replace('{s}', 's', $unit);
            }
            // Retourne la chaine adéquate
            return $when." ".$delta." ".$unit;
        }
    }
}