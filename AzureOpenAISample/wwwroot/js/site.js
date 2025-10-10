function initialize(partialUrl) {
	$("#form1").on("submit", function (event) {
		event.preventDefault();
		let formData = $(this).serialize();
		updateTalk(formData, partialUrl);
	});
	getTalk(partialUrl);
}

function getTalk(partialUrl) {
	$.ajax({
		type: "GET",
		url: partialUrl,

		success: function (data) {
			$("#talkResult").empty();
			$("#talkResult").append(data);
			window.scrollTo(0, document.body.scrollHeight);
		},

		error: function (jqXHR, status, error) {
			$('#talkResult').text('Status: ' + status + ', Error: ' + error);
		}
	});
}

function updateTalk(formData, partialUrl) {
	$("#waiting").show();
	$.ajax({
		type: "POST",
		url: partialUrl,

		data: formData,

		success: function (data) {
			$("#waiting").hide();
			$("#userMessage").val('');
			$("#talkResult").empty();
			$("#talkResult").append(data);
		},

		error: function (jqXHR, status, error) {
			$("#waiting").hide();
			$("#userMessage").val('');
			$('#talkResult').text('Status: ' + status + ', Error: ' + error);
		}
	});
}
