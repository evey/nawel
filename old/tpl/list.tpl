<div class='center'>
	<select id='year-selector' class='dropdown'>
		{foreach from=$resYear item=y}
			<option value='{$y[0]}' {if $y[0] == $selectedYear}selected='selected'{/if}>{$y[0]}</option>
		{/foreach}
	</select>
</div>
<form action='save.php' method='POST'>
	<input type='hidden' name='list_infos' id='user_infos' class='hidden-list-id' value='{$listId}' />
	<div class='list-content'>
		<div class='item'>
			<div class="header header-item-check bold">Disponible ?</div>
			<div class="header header-item-name bold">Nom</div>
			<div class="header header-item-img bold">Image</div>
			<div class="header header-item-desc bold">Descriptrion</div>
			<div class="header header-item-cost bold">Prix</div>
			<div class="header header-item-taken bold">Pris par</div>
			<div class="header header-item-taken bold">Participants</div>
			<div class="header header-item-comment bold">Commentaires</div>
		</div>
		{foreach from=$res item=list}
			<div class='item {if $list[8] == 0 && $list[10] != $uid}item-disabled{/if}'>
				<input type="checkbox" class='item-check' name='ck_{$list[0]}' id="{$list[0]}" {if $list[8] == 0} checked='checked' {if $list[10] != $uid} disabled{/if} {/if} /><label for="{$list[0]}"><span class="ui"></span></label>
				<div class='item-name bold'>
					{$list[2]}
				</div>
				<div class='item-img'>
					{if $list[5] != null and $list[5]  != ''}
					<a href='{$list[5]}' target="_blank">
					{/if}
						<img src='{if $list[4] == null}http://www.diocese-djougou.org/images/actualitesdiocese/pas-d-image-dispo.jpg{else}{$list[4]}{/if}' />
					{if $list[5] != null and $list[5]  != ''}
					</a>
					{/if}
				</div>
				<div class='item-desc'>
					{$list[3]}
				</div>
				<div class='item-cost'>
					{$list[6]}{$list[7]}
				</div>
				<div class='item-taken'>
					Pris par : {$list[9]}
				</div>
				<div class='item-taken'>
					{if $list[12]}{$list[12]} participe(nt) au cadeau.{/if}
					<input type='checkbox' class='participant' name='ck_participant_{$list[0]}' id='ck_participant_{$list[0]}' {if $list[13] == 1} checked='checked'{/if} /><label for="ck_participant_{$list[0]}"><span class="ui"></span></label>
				</div>
				<div class='item-comment'>
					<textarea class='rounded-input' name='comment_{$list[0]}' cols='30' rows='5'>{$list[14]}</textarea>
				</div>
				<input type='hidden' name='{$list[0]}' value='{$list[10]}' />
				<input type='hidden' class='hidden-participant' name='participant_{$list[0]}' id='participant_{$list[0]}' value='{$list[13]}' />
				<input type='hidden' id='file' value='{$list[11]}' />
				<input type='hidden' id='list_uid' value='{$list_uid}' />
			</div>
		{/foreach}
	</div>
	<div class='action-box'>
		<input type='button' class='my-btn btn btn-primary' id='download' value='Télécharger la liste' />
		<input type='submit' class='my-btn btn btn-success' value='Sauvegarder les changements' />
		<input type='button' class='my-btn btn btn-danger' id='back' value='Retour' />
	</div>
</form>