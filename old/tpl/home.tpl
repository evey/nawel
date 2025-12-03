<input type='hidden' id='lastItemID' value = '{$lastItemID}' />
<div class='list-content'>
	{foreach from=$res item=family key=famName}
		<div class='list-family'>
		<p class='list-family-name'>Famille {$famName}</p>
			{foreach from=$family item=list}
				<div class='list-item'>
					<a href='./list.php?id={$list[0]}'>
						<div class='list-body'>
							<div class='list-name bold'>
								{$list[1]}
							</div>
							<img class='user-icon' src='../img/avatar/{$list[2]}' />
						</div>
					</a>
				</div>
			{/foreach}
		</div>
	{/foreach}
</div>