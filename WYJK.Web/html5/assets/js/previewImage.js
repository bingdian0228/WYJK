var previewImage = function(oid, sid, callback) { //上传预览函数
	var that = this;
	var init = function(oid, sid) {
		that.oid = document.getElementById(oid);
		that.sid = document.getElementById(sid);
	};
	var readFile = function() {
		var file = this.files[0];
		if (!file.type.match(/image.*/)) {
			alert("请选择正确格式的图片");
			return false;
		}
		//		if (!/image\/\w+/.test(file.type)) { //这里我们判断下类型如果不是图片就返回 去掉就可以上传任意文件   
		//			alert("请选择正确格式的图片");
		//			return false;
		//		}
		loadingFun(true);
		var reader = new FileReader();
		reader.readAsDataURL(file);
		reader.onload = function(e) {
			that.sid.src = this.result;
			if (callback) {
				callback();
			}
		}
		uploadFile(file, function(result) {
			$(that.sid).siblings("input").val(result.Data[0]);
			loadingFun(false);
		});
	};
	init(oid, sid);
	that.oid.addEventListener('change', readFile, false);
};

/**
 * Upload a file
 * @param file
 */

function uploadFile(file, callback) {
	var url = "/api/Upload/MultiUpload";
	var xhr = new XMLHttpRequest();
	var fd = new FormData();
	xhr.open("POST", url, true);
	//	xhr.setRequestHeader("Client-Agent", "xxxx");
	xhr.onreadystatechange = function() {
		if (xhr.readyState == 4 && xhr.status == 200) {
			// Every thing ok, file uploaded
			var data = $.parseJSON(xhr.responseText);
			if (typeof callback == "function") {
				callback(data);
			}
		}
	};
	fd.append('uploaded_file', file);
	xhr.send(fd);
}