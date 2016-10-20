/**
 * Author : Warren
 */
var myTran = {
	/**
	 * 跳转方法
	 * name：跳转效果名称;
	 * reverse：效果是否倒转;
	 * to:目标页面;
	 * from:跳转页面;
	 */
	goTo: function(name, reverse, toPage, fromPage) {
		var to;
		// 判断是url还是pageId
		var myPattern = new RegExp("^(#)");
		if (myPattern.exec(toPage)) {
			// pageId
			to = toPage;
		} else {
			// url
			to = myTran.loadToPage(fromPage, toPage);
		}
		myTran.animationend(to);
		myTran.animationend(fromPage);
		myTran.transition(name, reverse, to, fromPage);
		$('html,body').scrollTop(0);
	},
	//添加转屏动画css
	transition: function(name, reverse, to, from) {
		reverseClass = reverse ? " reverse" : "";
		$(from).addClass(name + " out" + reverseClass);
		$(to).addClass(name + " in" + reverseClass);
		$(to).addClass("am-block");
		setTimeout(function() {
			$(to).removeClass("am-hide");
		}, 0)
	},
	// 加载页面
	loadToPage: function(page, url) {
		if (url) {
			var html = myTran.urlLoadContent(url);
			var toPageId = $(html).attr("id");
			$(page).after(html);
			$('#' + toPageId).addClass("am-hide");
			return '#' + toPageId;

		}
	},
	// 通过url加载html内容
	urlLoadContent: function(url) {
		var content = "";
		$.ajax({
			url: url,
			type: 'GET',
			dataType: "html",
			async: false,
			success: function(html, textStatus, xhr) {
				content = html;
			},
			error: function(xhr, textStatus, errorThrown) {
				content = "";
			}
		});
		return content;
	},
	// 绑定动画结束后事件
	animationend: function(page) {
		// 解绑事件
		$(page).off('animationend webkitAnimationEnd mozAnimationEnd');
		// 绑定事件
		$(page).on('animationend webkitAnimationEnd mozAnimationEnd', function() {
			if ($(this).attr('class').indexOf(" in") > 0) {
				$(this).removeClass();
				$(this).addClass("ui-mobile-viewport-transitioning ui-page");
			} else {
				$(this).removeClass();
				$(this).addClass("ui-mobile-viewport-transitioning ui-page am-hide");
			}
		});
	}
}