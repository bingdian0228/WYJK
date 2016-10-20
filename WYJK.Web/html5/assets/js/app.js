//增加表单验证
(function($) {
	if ($.AMUI && $.AMUI.validator) {
		// 增加多个正则
		$.AMUI.validator.patterns = $.extend($.AMUI.validator.patterns, {
			password: /^[0-9a-zA-Z_]{6,12}$/,
			mobile: /(13\d|14[57]|15[^4,\D]|17[678]|18\d)\d{8}|170[059]\d{7}/,
			IDnum: /^(\d{15}$|^\d{18}$|^\d{17}(\d|X|x))$/,
			intNum: /^[1-9]\d*$/,
			ZeroIntNum: /^[0-9]\d*$/
		});
		// 增加单个正则
		//身份证号码正则表达式备用:/^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$|^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}([0-9]|X)$/; 
	}
})(window.jQuery);
//获取url参数
function GetRequest() {
	var url = location.search; //获取url中"?"符后的字串
	var theRequest = new Object();
	if (url.indexOf("?") != -1) {
		var str = url.substr(1);
		strs = str.split("&");
		for (var i = 0; i < strs.length; i++) {
			theRequest[strs[i].split("=")[0]] = (strs[i].split("=")[1]);
		}
	}
	return theRequest;
}
//loading
function loadingFun(boolen) {
	var $modal_loading = $('#modal_loading');
	if (!$modal_loading.length) {
		$('body').append('<div class="am-modal am-modal-loading am-modal-no-btn" tabindex="-1" id="modal_loading"><div class="am-modal-dialog"><div class="am-modal-hd">正在载入...</div><div class="am-modal-bd"><span class="am-icon-spinner am-icon-spin"></span></div></div></div>');
		$modal_loading = $('#modal_loading');
	}
	if (boolen) {
		$modal_loading.modal('open');
	} else {
		$modal_loading.modal('close');
	}
}

//提示信息
function alertFun(str, callback) {
	var $alert = $('#modal_alert'); //提示框
	if (!$alert.length) {
		$('body').append('<div class="am-modal am-modal-no-btn" tabindex="-1" id="modal_alert"><div class="am-modal-dialog"><div class="am-modal-hd">提示信息<a href="javascript: void(0)" class="am-close am-close-spin" data-am-modal-close>&times;</a></div><div class="am-modal-bd">GG，请重新加载</div></div></div>');
		$alert = $('#modal_alert');
	}
	var $alert_txt = $alert.find('.am-modal-bd'); //提示信息
	var str = str ? str : 'GG，请重新加载';
	$alert_txt.text(str);
	$alert.modal('open');
	$alert.on('closed.modal.amui', function() {
		if (typeof(callback) == "function") {
			callback();
		}
	});
}

//表单序列化为对象

function serializeObject(form) {
	var o = {};
	$.each(form.serializeArray(), function(index) {
		if (o[this['name']]) {
			o[this['name']] = o[this['name']] + ";" + this['value'];
		} else {
			o[this['name']] = this['value'];
		}
	});
	return o;
}

//判断是否已经登录
function isLogin(id, callback) {
	if (!id) {
		window.location.href = "login.html";
	} else {
		var flag = $.isFunction(callback);
		if (flag) {
			callback();
		}
		return true;
	}
}
//后退按钮
function goBack() {
	var $arrow = $('header .am-header-left.am-header-nav a');
	if ($arrow.length) {
		$arrow.click(function(e) {
			var flag = $(this).attr('onclick');
			if (!flag) {
				e.preventDefault();
				//				window.location.href = document.referrer;
				window.history.back();
			}
		});
	}
	//取消按钮
	$('#cancel').click(function() {
		if ($arrow.length) {
			$arrow.trigger("click");
		}
	});
}