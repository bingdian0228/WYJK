jQuery(function() {
				var oDate = new Date();
				var iHour = oDate.getHours();
				if(iHour > 8 && iHour < 22) {
					jQuery(".Service").addClass("startWork");
					jQuery(".Service").removeClass("Commuting");
				} else {
					jQuery(".Service").addClass("Commuting");
					jQuery(".Service").removeClass("startWork");
				}
			});

			jQuery(function() {
				jQuery('.listLeftMenu dl dt').click(function() {
					var but_list = jQuery(this).attr('rel');
					if(but_list == 'off') {
						jQuery(this).attr('rel', 'on');
						jQuery('.listLeftMenu dl').removeClass('off');
						jQuery(this).parent().addClass('on');
					} else {
						jQuery(this).attr('rel', 'off');
						jQuery(this).parent().removeClass('on');
						jQuery(this).parent().addClass('off');
					}
				});
			});

			/*动画轮播*/

			var FixBoxAuto = {
				FixBoxTimerOut: null, //周期定时器判断是否可以轮播
				FixBoxTimerInter: null, //一次性定时器执行切换动作
				FixBoxNum: 0, //动作序号得到1234
				WAIT: 5000, //周期切换时间
				canAuto: true, //轮播标记
				init: function() { //初始化轮播
					var me = this;
					document.getElementsByClassName("fixed-box")[0].addEventListener("mouseover", function(event) {
						me.canAuto = false;

					});
					document.getElementsByClassName("fixed-box")[0].addEventListener("mouseout", function(event) {
						me.canAuto = true;

					});
					this.startMove();
				},
				startMove: function() { //周期判断是否可执行
					var me = this;
					this.FixBoxTimerInter = setInterval(function() {
						if(me.canAuto) {

							me.move();
						} else {
							clearInterval(me.FixBoxTimerInter);
							clearTimeout(me.FixBoxTimerOut);
							me.startMove();
						}
					}, me.WAIT)
				},
				move: function() { //一次性执行
					var m = this;
					this.FixBoxTimer = setTimeout(function() {
						m.upDate();
						jQuery(document.getElementsByClassName("fix-box-li")[(m.FixBoxNum % 4) + 1]).find('span.fix-box-span').addClass("hover");
						jQuery(document.getElementsByClassName("fix-box-li")[(m.FixBoxNum % 4) + 1]).fadeIn().addClass("hover");
						m.FixBoxNum++;
					}, 100)
				},
				upDate: function() {
					jQuery(".fixed-box ul.fixed-box-list li.fix-box-li").find('span.fix-box-span').removeClass("hover");
					jQuery(".fixed-box ul.fixed-box-list li.fix-box-li").removeClass("hover");
				}
			}
			jQuery(function() {
				for(var i = 1; i < 4; i++) {
					document.getElementsByClassName("fix-box-li")[i].addEventListener("mouseover", function() {
						FixBoxAuto.upDate();
						jQuery(this).find('span.fix-box-span').addClass("hover");
						//jQuery(this).removeClass("hover_out");
						jQuery(this).addClass("hover");
					});
					document.getElementsByClassName("fix-box-li")[i].addEventListener("mouseout", function() {
						jQuery(this).find('span.fix-box-span').removeClass("hover");
						//jQuery(this).removeClass("hover"); 
						jQuery(this).addClass("hover_out");
					});

				}
				FixBoxAuto.init();
			})