<div class='banner'>
</div>
<input type='button' id='news-button' class='btn btn-primary' value='News'>
<nav class="navbar navbar-default">
  <div class="header-container">
    <ul class="nav navbar-nav">
        <li class="bold {if $currentPage == 'home.php'}active{/if}""><a href="home.php">Accueil</a></li>
        <li class="bold {if $currentPage == 'mylist.php'}active{/if}""><a href="mylist.php">Ma liste</a></li>
        <li class="bold {if $currentPage == 'cart.php'}active{/if}""><a href="cart.php">Mon Panier</a></li>
        <li class="bold {if $currentPage == 'account.php'}active{/if}""><a href="account.php">Mon Compte</a></li>
        <img class='mini-user-icon' src='../img/avatar/{$myavatar}' />
        <li><a class="bold" href="logout.php">Se déconnecter</a></li>
        <!-- <li class="dropdown">
          <a href="#" class="dropdown-toggle" data-toggle="dropdown" role="button" aria-haspopup="true" aria-expanded="false">Listes <span class="caret"></span></a>
          <ul class="dropdown-menu">
            <li><a href="#">Action</a></li>
            <li><a href="#">Another action</a></li>
            <li><a href="#">Something else here</a></li>
            <li role="separator" class="divider"></li>
            <li><a href="#">Separated link</a></li>
            <li role="separator" class="divider"></li>
            <li><a href="#">One more separated link</a></li>
          </ul>
        </li> -->
      </ul>
  </div>
</nav>
<input type='hidden' id='sessId' value='{$sessId}' />