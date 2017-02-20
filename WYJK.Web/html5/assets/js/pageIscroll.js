var EventsList = function(element, options) {
	var $main = $('#wrapper');
	var $list = $main.find('#events-list');
	var $pullDown = $main.find('#pull-down');
	var $pullDownLabel = $main.find('#pull-down-label');
	var $pullUp = $main.find('#pull-up');
	var topOffset = -$pullDown.outerHeight();
	this.newParams = options.params;
	this.compiler = Handlebars.compile($('#list-item').html());
	this.prev = this.next = this.start = options.params.PageIndex;
	this.total = null;
	this.getURL = function(params) {
		var queries = [];
		for(var key in params) {
			if(key !== 'PageIndex') {
				queries.push(key + '=' + params[key]);
			}
		}
		queries.push('PageIndex=');
		return options.api + '?' + queries.join('&');
	};
	this.renderList = function(start, type) {
		var _this = this;
		var $el = $pullDown;
		if(type === 'load') {
			$el = $pullUp;
		}
		options.params.PageIndex = start;
		$.getJSON(this.URL + start).then(function(d) {
			_this.total = d.Data.total;
			console.log(d.Data.length);
			console.log(d.Data.length != 0);
			if((d.Data.length != 0)) {
				if($.isFunction(options.ajaxCallback)) {
					options.ajaxCallback(d);
				}
				var html = _this.compiler(d.Data);
				if(type === 'refresh') {
					$list.html(html);
					if($.isFunction(options.templateCallback)) {
						options.templateCallback(d);
					}
				} else if(type === 'load') {
					$list.append(html);
					if($.isFunction(options.templateCallback)) {
						options.templateCallback(d);
					}
				} else {
					$list.html(html);
					if($.isFunction(options.templateCallback)) {
						options.templateCallback(d);
					}
				}
			} else {
				LiHtml = '<li class="am-margin-top-lg"><img src="./img/no-data.png" alt="" width="100%"/></li>';
				$list.html(LiHtml);
			}
			// refresh iScroll
			setTimeout(function() {
				_this.iScroll.refresh();
			}, 100);
		}, function() {
			console.log('Error...')
		}).always(function() {
			_this.resetLoading($el);
			if(type !== 'load') {
				_this.iScroll.scrollTo(0, topOffset, 800, $.AMUI.iScroll.utils.circular);
			}
		});
	};
	this.setLoading = function($el) {
		$el.addClass('loading');
	};
	this.resetLoading = function($el) {
		$el.removeClass('loading');
	};
	this.init = function() {
		var myScroll = this.iScroll = new $.AMUI.iScroll('#wrapper', {
			click: true
		});
		// myScroll.scrollTo(0, topOffset);
		var _this = this;
		var pullFormTop = false;
		var pullStart;
		this.URL = this.getURL(options.params);
		this.renderList(options.params.PageIndex);
		myScroll.on('scrollStart', function() {
			if(this.y >= topOffset) {
				pullFormTop = true;
			}
			pullStart = this.y;
			// console.log(this);
		});
		myScroll.on('scrollEnd', function() {
			if(pullFormTop && this.directionY === -1) {
				_this.handlePullDown();
			}
			pullFormTop = false;
			// pull up to load more
			if(pullStart === this.y && (this.directionY === 1)) {
				_this.handlePullUp();
			}
		});
	};
	this.sortInit = function() {
		this.URL = this.getURL(this.newParams);
		this.renderList(this.newParams.PageIndex);
	};
	this.handlePullDown = function() {
		console.log('handle pull down');
		if(this.prev > 0) {
			this.setLoading($pullDown);
			this.renderList(1, 'refresh');
			//							this.prev -= options.params.pageSize;
			//							this.renderList(this.prev, 'refresh');
			this.next = 1;
			$('#pull-up-label').text(' 上拉加载更多');
		} else {
			console.log('别刷了，没有了');
		}
	};
	this.handlePullUp = function() {
		console.log('handle pull up');
		if(this.next * options.params.PageSize < this.total) {
			this.setLoading($pullUp);
			this.next++;
			this.renderList(this.next, 'load');
		} else {
			console.log(this.next);
			$('#pull-up-label').text(' 别刷了，没有了');
			// this.iScroll.scrollTo(0, topOffset);
		}
	}
};
document.addEventListener('touchmove', function(e) {
	e.preventDefault();
}, false);