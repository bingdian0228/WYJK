$(document).ready(function() {
	var id = Cookies.get("MemID");
	var Logined = isLogin(id);
	var $settleBtn = $('#settleBtn');
	var $chkAllBtn = $('#chkAllBtn');
	var $listBox = $('#list_box');
	//是否登录
	if (Logined) {
		loadingFun(true);
		//获取列表信息
		var Template = Handlebars.compile($("#list-template").html());
		$.ajax({
			type: "get",
			url: "/api/SocialSecurity/GetUnInsuredPeopleList?memberID=" + id,
			success: function(d) {
				if (d.status) {
					if (d.Data.length) {
						$.each(d.Data, function(i, item) {
							item.insuranceFee = item.SocialSecurityAmount + item.AccumulationFundAmount;
							item.serviceCost = item.AccumulationFundFirstBacklogCost + item.socialSecurityFirstBacklogCost;
							item.totalAmount = item.insuranceFee + item.serviceCost;
						});
						$listBox.html(Template(d.Data));
					} else {
						$listBox.html('<div class="am-padding-sm am-text-center">暂无信息</div>');
					}
					$('input[type="checkbox"]').uCheck();
					loadingFun(false);
				} else {
					alertFun(d.Message);
				}
			}
		});
	}
	//复选按钮
	$listBox.on('click', '.l-li-check', calculatePrice);
	//全选按钮
	$chkAllBtn.click(function() {
		var isChk = $(this).is(":checked");
		if (isChk) {
			$listBox.find('.l-li-check').uCheck("check");
			calculatePrice();
		} else {
			$listBox.find('.l-li-check').uCheck("uncheck");
			calculatePrice();
		}
	});
	//删除按钮
	$listBox.on('click', '.l-deleteBtn', function() {
		var id = $(this).data("id");
		$('#delete-confirm').modal({
			relatedTarget: this,
			closeOnConfirm: false,
			onConfirm: function(options) {
				var $li = $(this.relatedTarget).parents("li");
				$.ajax({
					type: "get",
					url: "/api/SocialSecurity/DeleteUninsuredPeople?SocialSecurityPeopleID=" + id,
					async: true,
					success: function(d) {
						$('#delete-confirm').modal('close');
						if (d.status) {
							$li.remove();
						} else {
							alertFun(d.Message);
						}
					}
				});
			}
		});
	});
	//结算按钮
	$settleBtn.click(function() {
		var flag = $('.l-li-check:checked').length;
		if (flag) {
			var idAr = $(this).data("idAr");
			var dataModel = {
				SocialSecurityPeopleIDS: idAr,
				MemberID: id
			};
			//判断是否可以自动扣款
			$.ajax({
				type: "post",
				url: "/api/Order/IsCanAutoPayment",
				async: true,
				data: dataModel,
				success: function(d) {
					if (d.status) {
						//弹出confirm框
						$('#settle-confirm').modal({
							relatedTarget: this,
//							closeOnConfirm: false,
							onConfirm: function(options) {
								//自动扣款
								$.ajax({
									type: "post",
									url: "/api/Order/AutoPayment",
									async: true,
									data: dataModel,
									success: function(d) {
										if (d.status) {
											alertFun(d.Message,function () {
												window.location.reload();
											});
										} else {
											alertFun(d.Message);
										}
									}
								});
							},
							onCancel: function() {
								if (id && idAr) {
									//生成订单
									$.ajax({
										type: "post",
										url: "/api/Order/GenerateOrder",
										async: true,
										data: dataModel,
										success: function(d) {
											if (d.status) {
												window.location.href = "insurance-submit.html?OrderCode=" + d.Data;
											} else {
												alertFun(d.Message);
											}
										}
									});
								}
							}
						});
					} else {
						//生成订单
						$.ajax({
							type: "post",
							url: "/api/Order/GenerateOrder",
							async: true,
							data: dataModel,
							success: function(d) {
								if (d.status) {
									window.location.href = "insurance-submit.html?OrderCode=" + d.Data;
								} else {
									alertFun(d.Message);
								}
							}
						});
					}
				}
			});

		} else {
			alertFun('请选择一个参保人！');
		}
	});

	function calculatePrice() {
		var $allChk = $('.l-li-check');
		var $chk = $('.l-li-check:checked');
		if ($allChk.length == $chk.length) {
			$chkAllBtn.uCheck('check');
		} else {
			$chkAllBtn.uCheck('uncheck');
		}
		var f = new Number();
		var s = new Number();
		var t = new Number();
		var ar = [];
		$.each($chk, function(i, item) {
			f = f + parseFloat($(this).data("fee"));
			s = s + parseFloat($(this).data("svc"));
			t = t + parseFloat($(this).data("total"));
			ar.push($(this).data("id"));
		});
		$('#fee').text(f);
		$('#svc').text(s);
		$('#total').text(t);
		$settleBtn.data('idAr', ar);
	}
});