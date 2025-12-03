<?php
echo date('U');
echo'<br />';

$UTC = new DateTimeZone("UTC");
$newTZ = new DateTimeZone("Europe/Paris");
$date = new DateTime();
echo $date->format('Y-m-d H:i:s');
echo'<br />';
$date->setTimezone( $UTC );
echo $date->format('Y-m-d H:i:s');
echo'<br />';
$date->setTimezone( $newTZ );
echo $date->format('Y-m-d H:i:s');