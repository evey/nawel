var lastTimeID = 0;

$(document).ready(function() {
	$('.item-check').change(function() {
		var ckID = $(this).attr('id');
		if ($(this).is(':checked')) {
			$('[name=' + ckID + ']').val('"' + sessUID + '"');
		}
		else {
			$('[name=' + ckID + ']').val('');
		}
	});

	$('.header-item-check').css('width', $('.item-check').css('width'));
	$('.header-item-name').css('width', $('.item-name').css('width'));
	$('.header-item-img').css('width', $('.item-img').css('width'));
	$('.header-item-desc').css('width', $('.item-desc').css('width'));
	$('.header-item-cost').css('width', $('.item-cost').css('width'));

	$('#validation-form').click(function() {
		var isValid = true;
		if ($('#login').val() == '') {
			$('#login').css('border', '1px solid red');
			isValid = false;
		}
		if ($('#firstname').val() == '') {
			$('#firstname').css('border', '1px solid red');
			isValid = false;
		}
		if ($('#pwd').val() != $('#confirmation').val()) {
			$('#pwd').css('border', '1px solid red');
			$('#confirmation').css('border', '1px solid red');
			isValid = false;
		}
		if (isValid)
		{
			$('#submit').click();
		}
	});

	$('#download').click(function() {
		window.location.replace("../files/" + $("#list_uid").val() + "/" + $("#file").val());
	});

	$('#back').click(function() {
		window.location.replace("home.php");
	});

	$('.nav li').click(function() {
		$('.nav li').removeClass('active');
		$(this).addClass('active');
	})

	$('#add').click(add_new_row);
	
	$('#import').click(import_data);
	
	$('#year-selector').change(function () {
		var listID = $('.hidden-list-id').val();
		var select = document.getElementById("year-selector");
		var year = select.options[select.selectedIndex].value;
		location.replace('./list.php?id=' + listID + '&year=' + year);
	});

	if ($('.index').length > 0) {
		for (var i = 0; i < $('.index').length; i++) {
			$('.index:nth('+i+')').val(i);
		}
	}

	$('.remove-btn').click(function() {
		$(this).parent().slideUp(500, function() {$(this).remove()});
	});

	$('.participant').click(function () {
		$(this).parent().parent().find('.hidden-participant').val($(this).is(':checked') ? 1 : 0);
	});

	$('#chat-message').keypress(function (e) {
  		if (e.which == 13) {
    		$('#chat-submit').click();
    		return false;
  		}
	});

	//lastTimeID = $('#lastTimeID').val();
	$('#chat-submit').click( function() {
    	sendChatText();
    	$('#chat-message').val("");
  	});
  	if (document.getElementById('chat-submit') != null) {
	  	startChat();
  	}

  	  // Get the modal
	var modal = document.getElementById('myModal');
	if (typeof(display_popup) !== 'undefined' && display_popup == 1) {
		modal.style.display = "block";
	}

	$('#news-button').click(function() {
		modal.style.display = "block";
	});
	// Get the <span> element that closes the modal
	var span = document.getElementsByClassName("close")[0];

	// When the user clicks on <span> (x), close the modal
	span.onclick = function() {
	    modal.style.display = "none";
	 	var userId = $('#sessId').val();
	    $.ajax({
	      		type: "GET",
	      		url: "news.php",
	      		data: "id=" + userId
	    	});
	}

	// When the user clicks anywhere outside of the modal, close it
	window.onclick = function(event) {
	    if (event.target == modal) {
	        modal.style.display = "none";
	 		var userId = $('#sessId').val();
	        $.ajax({
	      		type: "GET",
	      		url: "news.php",
	      		data: "id=" + userId
	    	});
	    }
	}

	$('#login-reset-button').click(function () {
		$('#reset-panel').slideDown(500);
	});

	$('#login-validate-reset').click(function () {
		$.ajax({
		    type: "GET",
		    url: "./reset.php?email="+encodeURIComponent($('#log-email').val()),
		    dataType: 'text'
		}).done( function( data ) {
				if (data == "success") {
					$('#success').slideDown(500);
				}
				else {
					$('#error').slideDown(500);
				}
		});
	});
});


function startChat(){
  setInterval( function() { getChatText(); }, 1000);
}

function getChatText() {
  $.ajax({
    type: "GET",
    url: "./dialog.php?lastTimeID=" + lastTimeID + "&chatId=" + $('#chat-id').val(),
    dataType: 'json'
  }).done( function( data )
  {
    var bot_scroll = false;
  	if($('#chat-display').scrollTop() + $('#chat-display').innerHeight() >= $('#chat-display')[0].scrollHeight) {
            bot_scroll = true;
    }
    var jsonData = data.results;
    var jsonLength = jsonData.length;//jsonData.results.length;
    var html = "";
    for (var i = 0; i < jsonLength; i++) {
      var result = jsonData[i];
      html += '<div><img class="mini-user-icon" src="../img/avatar/' + result.avatar + '" ><div class="triangle-right left">(' + result.date_sent + ') <b>' + result.pseudo +'</b>: ' + result.message + '</div></div>';
      lastTimeID = result.id;
    }
    $('#chat-display').append(html);
    if (bot_scroll) {
    	$('#chat-display').scrollTop($('#chat-display')[0].scrollHeight);
    }
  });
}

function sendChatText(){
  var chatInput = $('#chat-message').val();
  var userId = $('#chat-uid').val();
  var chatId = $('#chat-id').val();
  if(chatInput != ""){
    $.ajax({
      type: "GET",
      url: "dialog.php",
      data: "chattext=" + encodeURIComponent(chatInput) + "&chatId=" + chatId + "&userId=" + userId
    });
  }
}

function add_new_row(row) {
	if ($('.index:last').length == 0) {
		$item = "<div class='item'>";
		$item += "<input type='hidden' id='id' name='id[]' value='0' />";
		$item += "<input type='hidden' class='index' name='index[]' value='0' />";
		$item += "<div class='infos-block'>";
		$item += "<div class='my-item-name'>";
		$item += "<label>Nom : </label>";
		if (row !== 'undefined')
			$item += "<input class='rounded-input long-input' name='name[]' type='text' value='" + row.name + "' />";
		else
			$item += "<input class='rounded-input long-input' name='name[]' type='text' value='' />";
		$item += "</div>";
		$item += "<div class='my-item-img'>";
		$item += "<label>Image : </label>";
		if (row !== 'undefined')
			$item += "<input class='rounded-input long-input' name='image[]' type='text' value='" + row.image + "' />";
		else
			$item += "<input class='rounded-input long-input' name='image[]' type='text' value='' />";
		$item += "</div>";
		$item += "<div class='my-item-link'>";
		$item += "<label>Lien : </label>";
		if (row !== 'undefined')
			$item += "<input class='rounded-input long-input' name='link[]' type='text' value='" + row.link + "' />";
		else
			$item += "<input class='rounded-input long-input' name='link[]' type='text' value='' />";
		$item += "</div>";
		$item += "</div>";
		$item += "<div class='my-item-img-preview'>";
		if (row !== 'undefined')
			$item += "<img src='" + row.image + "' >";
		else
			$item += "<img src='http://www.diocese-djougou.org/images/actualitesdiocese/pas-d-image-dispo.jpg' >";
		$item += "</div>";
		$item += "<div class='my-item-desc'>";
		if (row !== 'undefined')
			$item += "<textarea class='rounded-input' name='description[]' cols='80' rows='5'>" + row.description + "</textarea>";
		else
			$item += "<textarea class='rounded-input' name='description[]' cols='80' rows='5'></textarea>";
		$item += "</div>";
		$item += "<div class='my-item-cost'>";
		$item += "<label>Prix :</label>";
		if (row !== 'undefined')
			$item += "<input class='rounded-input' name='price[]' type='text' value='" + row.cost + "' />";
		else
			$item += "<input class='rounded-input' name='price[]' type='text' value='' />";
		$item += "<select class='rounded-input' name='currency[]'>";
		if (row !== 'undefined') {
			$item += "<option " + (row.currency == "EUR" ? "selected" : "") + " value='EUR'>€</option>";
			$item += "<option " + (row.currency == "USD" ? "selected" : "") + "value='USD'>$</option>";
		} else {
			$item += "<option value='EUR'>€</option>";
			$item += "<option value='USD'>$</option>";
		}
		$item += "</select>";
		$item += "</div>";
		$item += "<input type='button' class='btn btn-danger remove-btn' value='Supprimer' />";
		$item += "</div>";
		$('#list').append($item).hide().slideDown(500);
	}
	else {
		var index = parseInt($('.index:last').val());
		if (row)
			$('.item:last').clone(true).insertAfter('.item:last');
		else
			$('.item:last').clone(true).insertAfter('.item:last').hide().slideDown(500);
		$('.item:last input').each(function() {
			if ($(this).val() != 'Supprimer' && row === 'undefined') {
				$(this).val('');
			}
			else if ($(this).val() != 'Supprimer' && row !== 'undefined') {
				if ($(this)[0].name == "name[]")
					$(this).val(row.name);
				else if ($(this)[0].name == "image[]")
					$(this).val(row.image);
				else if ($(this)[0].name == "link[]")
					$(this).val(row.link);
				else if ($(this)[0].name == "price[]")
					$(this).val(row.cost);
				else
					$(this).val('');
			}
		});
		$('.item:last textarea').each(function() {
			if ($(this)[0].name == "description[]" && row !== 'undefined')
				$(this).val(row.description);
			else
				$(this).val('');
		});
		$('.item #id:last').val('0');
		$('.item img:last').attr('src', (row !== 'undefined' ? row.image : 'http://www.diocese-djougou.org/images/actualitesdiocese/pas-d-image-dispo.jpg'));
		$('.index:last').val(++index);
		$('.remove-btn:last').removeClass('hidden-btn');
	}
	$('.remove-btn').off("click");
	$('.remove-btn').click(function() {
		$(this).parent().slideUp(500, function() {$(this).remove()});
	});
}

function import_data() {
	if (confirm("Vous êtes sur le point d'importer les cadeaux encore disponible de l'année dernière, voulez vous continuer ?")) {
		var listID = $('#myListID').val();
		$.ajax({
			type: "GET",
			url: "./import.php?listID=" + listID,
			dataType: 'json',
			success: function(data) {
				var jsonData = data.results;
				var jsonLength = jsonData.length;//jsonData.results.length;
				for (var i = 0; i < jsonLength; i++) {
				  var result = jsonData[i];
				  add_new_row(result);
				}
			},
			error: function (error) {
				alert(error);
			}
		});
	}
}