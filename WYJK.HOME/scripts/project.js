var cateMenu = function() {
	var cateLiNum = $("#cateMenu li").length;
	var Mh = $('#cateMenu').offset().top;
	$("#cateMenu li").each(function(index, element) {
		if (index < cateLiNum) {
			$(this).mouseenter(function() {
				var $sub = $(this).find(".list-item-w");
				if ($sub.length) {
					var ty = $(this).offset().top - Mh;
					var sh = $(window).scrollTop();
					var oy = ty + $sub.height() + Mh - sh;
					var dest = oy - $(window).height();
					if (oy > $(window).height()) {
						ty = ty - dest;
					}
					if (ty < 0) ty = 0;
					$sub.show();
					$("#cateMenu li").find(".list-item-w").stop().animate({
						"top": ty
					});
					$sub.stop().animate({
						"top": ty
					});
				}
			});
			$(this).mouseleave(function() {
				$(this).find(".list-item-w").hide();
			});
		}
	});
}();