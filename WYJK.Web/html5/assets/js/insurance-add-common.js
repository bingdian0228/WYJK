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
//转场动画
$('#chose_scheme').click(function() {
	var flag = $IDcate.val();
	if (flag) {
		myTran.goTo('myslide', '', '#scheme-page', '#main-page');
	} else {
		alertFun('请先选择户籍性质！');
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
			HouseholdProperty: $IDcate.val(),
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