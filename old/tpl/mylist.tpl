<div class='center'>
	<select id='year-selector' class='dropdown'>
		{foreach from=$resYear item=y}
			<option value='{$y[0]}' {if $y[0] == $selectedYear}selected='selected'{/if}>{$y[0]}</option>
		{/foreach}
	</select>
</div>
<form action='save.php' method='POST' accept-charset="UTF-8">
	<input type='hidden' id='myListID' class='hidden-list-id' name='mylist' value='{$listId}' />
	<div id='list'>
	{foreach from=$res item=list}
		<div class='item'>
			<input type='hidden' id='id' name='id[]' value='{$list[0]}' />
			<input type='hidden' class='index' name='index[]' value='' />
			<div class='infos-block'>
				<div class='my-item-name'>
					<label>Nom : </label>
					<input class='rounded-input long-input' name='name[]' type='text' value='{$list[2]|escape}' />
				</div>
				<div class='my-item-img'>
					<label>Image : </label>
					<input class='rounded-input long-input' name='image[]' type='text' value='{$list[4]}' />
				</div>
				<div class='my-item-link'>
					<label>Lien : </label>
					<input class='rounded-input long-input' name='link[]' type='text' value='{$list[5]}' />
				</div>
			</div>
			<div class='my-item-img-preview'>
				<img src='{if $list[4] == null}http://www.diocese-djougou.org/images/actualitesdiocese/pas-d-image-dispo.jpg{else}{$list[4]}{/if}' >
			</div>
			<div class='my-item-desc'>
				<textarea class='rounded-input' name='description[]' cols='50' rows='5'>{$list[3]}</textarea>
			</div>
			<div class='my-item-cost'>
				<label>Prix :</label>
				<input class='rounded-input' name='price[]' type='text' value='{$list[6]}' />
				<select class='rounded-input' name='currency[]'>
					<option value='EUR'>€</option>
					<option value='USD'>$</option>
				</select>
			</div>
			<input type='button' class='btn btn-danger remove-btn hidden-btn' value="Supprimer" />
		</div>
	{/foreach}
	</div>
	<div class='action-box'>
		<input type='button' class='my-btn btn btn-primary' id='add' value='Ajouter un élément' />
		<input type='button' class='my-btn btn btn-primary' id='import' value='Importer la dernière liste' />
		<input type='submit' class='my-btn btn btn-success' value='Sauvegarder les changements' />
		<input type='button' class='my-btn btn btn-danger' id='back' value='Retour' />
	</div>
</form>