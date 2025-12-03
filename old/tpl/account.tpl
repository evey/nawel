<div class='list-content'>
	<div class='user-form'>
		<form action='save.php' method='POST' enctype='multipart/form-data'>
			<input type='hidden' name='user_infos' id='user_infos' value='1' />
			<div class='log-infos'>
				<div class='log-infos-label'>
					<div class='user-row'>
						<label for='login'>Nom d'utilisateur :</label>
					</div>
					<div class='user-row'>
						<label for='pwd'>Mot de passe :</label>
					</div>
					<div class='user-row'>
						<label for="confirmation">Confirmer le mot de passe :</label>
					</div>
				</div>
				<div class='log-infos-input'>
					<div class='user-row'>
						<input class='rounded-input' type='text' name='login' id='login' value='{$res["login"]}' />
					</div>
					<div class='user-row'>
						<input class='rounded-input' type='password' name='pwd' id='pwd'/>
					</div>
					<div class='user-row'>
						<input class='rounded-input' type='password' name='confirmation' id='confirmation'/>
					</div>

				</div>
			</div>
			<div class='user-infos'>
				<div class='user-infos-label' style="width: 400px">
					<div class='user-row'>
						<label for='email'>Email :</label>
						<input class='rounded-input' type='text' style="float: right;" name='email' id='email' value='{$res["email"]}'/>
					</div>
					<div class='user-row'>
						<label for='firstname'>Prénom :</label>
						<input class='rounded-input' type='text'  style="float: right;" name='firstname' id='firstname' value='{$res["first_name"]}'/>
					</div>
					<div class='user-row'>
						<label for='lastname'>Nom :</label>
						<input class='rounded-input' type='text'  style="float: right;" name='lastname' id='lastname' value='{$res["last_name"]}'/>
					</div>
					<div class='user-row'>
						<label for='lastname'>pseudo :</label>
						<input class='rounded-input' type='text'  style="float: right;" name='pseudo' id='pseudo' value='{$res["pseudo"]}'/>
					</div>
					<div class='user-row'>
						<input type='checkbox' class='participant' name='notify_list_edit' id='notify_list_edit' {if $res["notify_list_edit"] == 1} checked='checked'{/if} /><label for="notify_list_edit"><span class="ui"></span>Notification lorsque quelqu&#39;un édite sa liste</label>
					</div>
					<div class='user-row'>
						<input type='checkbox' class='participant' name='notify_gift_taken' id='notify_gift_taken' {if $res["notify_gift_taken"] == 1} checked='checked'{/if} /><label for="notify_gift_taken"><span class="ui"></span>Notification lorsque quelqu&#39;un prend un objet</label>
					</div>
					<div class='user-row'>
						<img class='acc-user-icon' src='../img/avatar/{$res["avatar"]}' />
						<input type='hidden' name='MAX_FILE_SIZE' value='1048576' />
						<label for='avatar'>Avatar (JPG, PNG ou GIF | max. 1Mo) :</label>
						<input type='file' name='avatar' id='avatar'/>
						<input type='hidden' name='current-avatar' value='{$res["avatar"]}'/>
					</div>
				</div>
			</div>
			<div class='user-action-box'>
				<input type='submit' class='my-btn btn btn-hidden' id='submit' value='sauvegarder les changements' />
				<input type='button' class='my-btn btn btn-success' id='validation-form' value='sauvegarder les changements' />
			</div>
		</form>
	</div>
</div>