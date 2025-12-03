<?php
session_start();
require_once(__DIR__ . '/functions.php');

/**
 * On ne traite pas les super globales provenant de l'utilisateur directement,
 * ces données doivent être testées et vérifiées.
 */
$postData = $_POST;

// Validation du formulaire
if (isset($postData['login']) && isset($postData['password'])) {
    try {
        $mysqlClient = new PDO('mysql:host=localhost;dbname=nawel;charset=utf8', 'root', '',[PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION]);
        $sqlQuery = 'SELECT * FROM user WHERE login = :login';
        $userStatement = $mysqlClient->prepare($sqlQuery);
        $userStatement->execute(['login' => $postData['login']]);

        $users = $userStatement->fetchAll();
        
        foreach ($users as $u){
            if ($u['pwd'] === md5($postData['password'])) {
                $_SESSION['LOGGED_USER'] = [
                    'login' => $u['login'],
                    'user_id' => $u['id'],
                    'email' => $u['email'],
                    'pseudo' => $u['pseudo'],
                    'avatar' => $u['avatar'],
                ];
            }
        } 
        

    } catch (Exception $e) {
        die('Erreur : ' . $e->getMessage());
    }

    if (!isset($_SESSION['LOGGED_USER'])) {
        $_SESSION['LOGIN_ERROR_MESSAGE'] = sprintf(
            'Les informations envoyées ne permettent pas de vous identifier : (%s/%s)',
            $postData['login'],
            strip_tags($postData['password'])
        );
    }

    redirectToUrl('index.php');
}