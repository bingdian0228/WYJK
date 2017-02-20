//预先定义
var $IDcate = $('#idcate');
var $SBchk = $('#SBcheck');
var $Fundchk = $('#Fundcheck');
var $SBform = $('#SBform');
var $Fundform = $('#Fundform');
var $SBbaseTXT = $('#SBbase');
var $FundbaseTXT = $('#Fundbase');
var $schemeConfirm = $('#schemeConfirm');
var $mainForm = $('#main-form');
//获取memID
var id = Cookies.get("MemID");
isLogin(id, function() {
	$('#MemberID').val(id);
});
//预览图像
new previewImage('p-ID', 'p-ID-show');
new previewImage('o-ID', 'o-ID-show');
//后退按钮
goBack();
//获取户口性质
var IDcateTemplate = Handlebars.compile($("#IDcate-template").html());
$.ajax({
	type: "get",
	url: "/api/SocialSecurity/GetHouseholdPropertyList",
	async: false,
	success: function(d) {
		$IDcate.append(IDcateTemplate(d.Data));
	}
});
//选择方案转场动画
$('#chose_scheme').click(function() {
	var flag = $mainForm.validator('isFormValid');
	var ID = $('#IdentityCard').val();
	if (flag) {
		$.ajax({
			type: "get",
			url: "/api/SocialSecurity/SelectSocialSecurityScheme?IdentityCard=" + ID,
			async: true,
			success: function(d) {
				switch (d.Data.PayFlag) {
					case 1:
						$SBchk.uCheck("uncheck");
						$SBchk.uCheck('disable');
						$SBform.fadeOut();
						$Fundchk.uCheck("check");
						$Fundform.fadeIn();
						myTran.goTo('myslide', '', '#scheme-page', '#main-page');
						break;
					case 2:
						$Fundchk.uCheck('disable');
						myTran.goTo('myslide', '', '#scheme-page', '#main-page');
						break;
					case 3:
						alertFun("此身份证号已经填写参保方案！");
						break;
					default:
						$SBchk.uCheck('enable');
						$Fundchk.uCheck("enable");
						myTran.goTo('myslide', '', '#scheme-page', '#main-page');
						break;
				}
			}
		});
	}
});
//复选框选择
$SBchk.on('click', function(event) {
	var flag = $(this).is(':checked');
	if (flag) {
		$SBform.fadeIn();
	} else {
		$SBform.fadeOut();
	}
});
$Fundchk.on('click', function(event) {
	var flag = $(this).is(':checked');
	if (flag) {
		$Fundform.fadeIn();
	} else {
		$Fundform.fadeOut();
	}
});
//datepicker限定日期
var nowTemp = new Date();
var day = nowTemp.getDate();
var month = nowTemp.getMonth();
var nowMoth;
var nowDay;
if (day > 13) {
	nowDay = new Date(nowTemp.getFullYear(), month + 1, 1, 0, 0, 0, 0).valueOf();
	nowMoth = new Date(nowTemp.getFullYear(), month + 1, 1, 0, 0, 0, 0).valueOf();
} else {
	nowDay = new Date(nowTemp.getFullYear(), month, day, 0, 0, 0, 0).valueOf();
	nowMoth = new Date(nowTemp.getFullYear(), month, 1, 0, 0, 0, 0).valueOf();
}
var nowYear = new Date(nowTemp.getFullYear(), 0, 1, 0, 0, 0, 0).valueOf();
$('.payTime').datepicker({
	format: 'yyyy-mm',
	viewMode: 'years',
	onRender: function(date, viewMode) {
		// 默认 days 视图，与当前日期比较
		var viewDate = nowDay;

		switch (viewMode) {
			// moths 视图，与当前月份比较
			case 1:
				viewDate = nowMoth;
				break;
				// years 视图，与当前年份比较
			case 2:
				viewDate = nowYear;
				break;
		}

		return date.valueOf() < viewDate ? 'am-disabled' : '';
	}
});
//新开户/转移以及点击事件
var fundTypeTemplate = Handlebars.compile($("#fundType-template").html());
var transferInfoTemplate = Handlebars.compile($("#transferInfo-template").html());
var $typeBox = $('#fundTypeBox');
var $transferBox = $('#fundTransferBox');
$.ajax({
	type: "get",
	url: "/api/SocialSecurity/GetAccumulationFundTypeList",
	async: false,
	success: function(d) {
		$typeBox.append(fundTypeTemplate(d.Data));
		var $typeInput = $typeBox.find("input[type=radio]");
		$typeInput.uCheck();
		//点击事件
		$typeInput.click(function() {
			var flag = $(this).is("[value=2]:checked");
			var Area = $('#AccumulationFundArea').val();
			var houseHold = $IDcate.val();
			if (flag) {
				if (Area) {
					loadingFun(true);
					$.ajax({
						type: "get",
						url: "/api/SocialSecurity/AccumulationFundTransferShow?area=" + Area + "&HouseholdProperty=" + houseHold,
						async: true,
						success: function(d) {
							if (d.status) {
								loadingFun(false);
								$transferBox.html(transferInfoTemplate(d.Data));
								$transferBox.slideDown();
							} else {
								alertFun(d.Message);
							}
						}
					});
				} else {
					alertFun("请先选择参保省市！");
					$typeInput.uCheck("uncheck");
				}
			} else {
				$transferBox.slideUp();
			}
		});
	}
});

//提交参保方案
$('#schemeSubmit').click(function() {
	var fSB = $SBchk.is(":checked");
	var fFund = $Fundchk.is(":checked");
	var houseHold = $IDcate.val();
	var SBData = {};
	var FundData = {};
	var isReadySB = false;
	var isReadyF = false;
	if (fSB) {
		var baseSB = $SBbaseTXT.is(":disabled");
		if (baseSB) {
			alertFun("请先选择城市再输入基数！");
		} else {
			var vSB = $SBform.validator('isFormValid');
			if (vSB) {
				SBData = serializeObject($SBform);
				SBData.HouseholdProperty = houseHold;
				isReadySB = true;
			}
		}
	}
	if (fFund) {
		var baseFund = $FundbaseTXT.is(":disabled");
		if (baseFund) {
			alertFun("请先选择城市再输入基数！");
		} else {
			var vFund = $Fundform.validator('isFormValid');
			if (vFund) {
				FundData = serializeObject($Fundform);
				FundData.HouseholdProperty = houseHold;
				isReadyF = true;
			}
		}
	}
	if (isReady()) {
		var TotalData = {
			HouseholdProperty: houseHold,
			socialSecurity: SBData,
			accumulationFund: FundData
		};
		$schemeConfirm.data('SBData', SBData);
		$schemeConfirm.data('FundData', FundData);
		//模板
		var resultTemplate = Handlebars.compile($("#result-template").html());
		loadingFun(true);
		$.ajax({
			type: "post",
			url: "/api/SocialSecurity/ConfirmSocialSecurityScheme",
			data: TotalData,
			success: function(d) {
				if (d.status) {
					if (d.Data.IsExistSocialSecurityCase) {
						d.Data.SocialSecurityAmount = Number((d.Data.SocialSecurityAmount * d.Data.socialSecuritypayMonth).toFixed(2));
					}
					if (d.Data.IsExistaAccumulationFundCase) {
						d.Data.AccumulationFundAmount = Number((d.Data.AccumulationFundAmount * d.Data.AccumulationFundpayMonth).toFixed(2));
					}
					d.Data.total = d.Data.SocialSecurityAmount + d.Data.AccumulationFundAmount + d.Data.socialSecurityFirstBacklogCost + d.Data.AccumulationFundFirstBacklogCost + d.Data.FreezingCharge;
					d.Data.total = d.Data.total.toFixed(2);
					$('#result-box').html(resultTemplate(d.Data));
					loadingFun(false);
					myTran.goTo('myslide', '', '#confirm-page', '#scheme-page');
				} else {
					alertFun(d.Message);
				}
			}
		});
	}
	//判断是否填写规范
	function isReady() {
		if (fSB == isReadySB) {
			if (fFund) {
				return isReadyF;
			} else {
				return fSB;
			}
		}
		if (fFund == isReadyF) {
			if (fSB) {
				return isReadySB;
			} else {
				return fFund;
			}
		}
	}
});

//社保基数查询
function SBbaseFun() {
	var p = $('#SBprovince').val();
	var c = $('#SBcity').val();
	var $baseTXT = $SBbaseTXT;
	var houseHold = $IDcate.val();
	if (p) {
		var Area = p;
		if (c != '0') {
			Area = Area + '|' + c;
		}
		$('#InsuranceArea').val(Area);
		$.getJSON('/api/SocialSecurity/GetSocialSecurityBase?area=' + Area + '&HouseholdProperty=' + houseHold, function(d) {
			if (d.status) {
				$baseTXT.attr('disabled', false);
				$baseTXT.attr('placeholder', '输入社保基数（' + d.Data.minBase + '-' + d.Data.maxBase + '）');
				$baseTXT.attr('min', d.Data.minBase);
				$baseTXT.attr('max', d.Data.maxBase);
			} else {
				$baseTXT.val('');
				$baseTXT.attr('placeholder', '请先选择城市再输入');
				alertFun(d.Message);
				$baseTXT.attr('disabled', true);
			}
		});
	}
}
//公积金基数查询
function FundbaseFun() {
	var p = $('#Fundprovince').val();
	var c = $('#Fundcity').val();
	var $baseTXT = $FundbaseTXT;
	var houseHold = $IDcate.val();
	if (p) {
		var Area = p;
		if (c != '0') {
			Area = Area + '|' + c;
		}
		$('#AccumulationFundArea').val(Area);
		$.getJSON('/api/SocialSecurity/GetAccumulationFundBase?area=' + Area + '&HouseholdProperty=' + houseHold, function(d) {
			if (d.status) {
				$baseTXT.attr('disabled', false);
				$baseTXT.attr('placeholder', '输入公积金基数（' + d.Data.minBase + '-' + d.Data.maxBase + '）');
				$baseTXT.attr('min', d.Data.minBase);
				$baseTXT.attr('max', d.Data.maxBase);
			} else {
				$baseTXT.val('');
				$baseTXT.attr('placeholder', '请先选择城市再输入');
				alertFun(d.Message);
				$baseTXT.attr('disabled', true);
			}
		});
	}
}