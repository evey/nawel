<?php

include("../config.php");

if (isset($_GET['listID'])) {
    $listID = intval($_GET['listID']);
    $db = new mysqli(DB_HOST, DB_USER, DB_PASSWORD, DB_DATABASE);
    $db->set_charset("utf8");
    $sql = "SELECT list_id, name, description, image, link, cost, currency, available, YEAR(NOW()) FROM gifts g WHERE g.list_id = ".$listID." AND g.available = 1 AND year = YEAR(NOW()) - 1";
    $result = $db->query($sql);
	$jsonData = '{"results":[';
	$line = new stdClass;
    //echo 'test';
    //var_dump($result);
    while ($row = $result->fetch_array()) {
        $line->list_id = $row['list_id'];
        $line->name = $row['name'];
        $line->description = $row['description'];
        $line->image = $row['image'];
        $line->link = $row['link'];
		$line->cost = $row['cost'];
		$line->currency = $row['currency'];
        //$line->date_sent = getRelativeTime($row['date_sent'], $row['date_now']);
        $arr[] = json_encode($line);
    }
    //$statement->close();
    $db->close();
    $jsonData .= implode(",", $arr);
    $jsonData .= ']}';
    print $jsonData;
}