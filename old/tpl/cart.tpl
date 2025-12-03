<div class='list-content'>
	<div class='item'>
		<div class="header header-item-name bold">Nom</div>
		<div class="header header-item-img bold">Image</div>
		<div class="header header-item-desc bold">Description</div>
		<div class="header header-item-cost bold">Prix</div>
		<div class="header header-item-taken bold">Pour</div>
	</div>
	{foreach from=$res item=gift}
		<div class='item'>
			<div class='item-name bold'>
				{$gift[1]}
			</div>
			<div class='item-img'>
				{if $gift[3] != null and $gift[3]  != ''}
				<a href='{$gift[3]}' target="_blank">
				{/if}
					<img src='{if $gift[2] == null}http://www.diocese-djougou.org/images/actualitesdiocese/pas-d-image-dispo.jpg{else}{$gift[2]}{/if}' />
				{if $gift[3] != null and $gift[3]  != ''}
				</a>
				{/if}
			</div>
			<div class='item-desc'>
				{$gift[4]}
			</div>
			<div class='item-cost'>
				{$gift[5]}{$gift[6]}
			</div>
			<div class='item-taken'>
				Pour : {$gift[7]}
			</div>
		</div>
	{/foreach}
</div>
