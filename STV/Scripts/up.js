function saveVal(ele) {
	var valname = ele.getAttribute("edit");
	if (ele.tagName == "TD") {
		var val = ele.textContent;
	} else {
		if (ele.getAttribute("type") == "checkbox") {
			var val = ele.checked == true ? "1" : "0";
		} else {
			var val = ele.value;
		}
	}
	var obj = { uname: valname, ctx: getContextC(ele) };
	obj[valname] = val;
	postSave(obj);
	console.log(valname + " " + val);
}
function getContextC(e) {
	while (e.tagName != "TR") {
		e = e.parentElement;
	}
	return e.getAttribute("id");
}
function getEncryptStatus(e) {
	while (e.tagName != "TR") {
		e = e.parentElement;
	}
	var td = e.querySelector('[edit=encrypt]').checked;
	return td;
}
function setEncryptStatus(e, v) {
	while (e.tagName != "TR") {
		e = e.parentElement;
	}
	e.querySelector('[edit=encrypt]').checked = (v == "1") ? true : false;
}
function postSave(obj, calb) {
	ajax("ajax=bookmanager&action=save&table=" + window.table + "&obj=" + encodeURIComponent(JSON.stringify(obj)), function (d) {
		if (calb) {
			calb(d);
		}
		else if (d.length > 0) {
			ui.notif(d);
		}
	});
}
function postSaves(obj, calb) {
	ajax("ajax=bookmanager&action=saves&table=" + window.table + "&obj=" + encodeURIComponent(JSON.stringify(obj)), function (d) {
		if (calb) {
			calb(d);
		}
		else if (d.length > 0) {
			ui.notif(d);
		}
	});
}
q("[edit]").forEach(function (e) {
	if (e.tagName == "TD") {
		e.contentEditable = true;
		e.addEventListener('focusout', function () {
			saveVal(this);
		});
	} else {
		e.addEventListener('onchange', function () {
			saveVal(this);
		});
		if (e.getAttribute("type") == "checkbox")
			e.onclick = function () {
				saveVal(this);
			};
	}
});
q("[selector]").forEach(function (e) {
	e.onclick = function () {
		if (event.shiftKey && window.lastSel) {
			var list = Array.from(q("[selector]"));
			var idx = list.indexOf(this);
			var start = list.indexOf(window.lastSel);
			if (start < idx) {
				for (var i = start + 1; i < idx; i++) {
					list[i].checked = this.checked;
				}
			} else {
				for (var i = idx + 1; i < start; i++) {
					list[i].checked = this.checked;
				}
			}
		} else
			window.lastSel = e;
	}
});
function editChapter(chapid) {
	ajax("ajax=bookmanager&action=getchaptercontent&cid=" + chapid, function (d) {
		window.wd = ui.win.create("Chỉnh sửa");
		var bd = wd.body;
		var r = bd.row();
		r.innerHTML = "<textarea id='ccontent' style='min-height: 80vh;'>" + d + "</textarea>";
		r = bd.row();
		r.addButton("Lưu chỉnh sửa", "saveEditChapter(" + chapid + ")", "primary");
		r.addButton("Mở rộng", "openChapEdit(" + chapid + ")", "primary");
		r.style.display = "block";
		r.style.textAlign = 'center';
		wd.show();
		//ui.win.fullscreen(wd);
	});
}
function saveEditChapter(chapid) {
	postSave({ uname: "content", "content": g("ccontent").value.replace(/</g, ""), ctx: chapid }, function (d) {
		if (d == "") {
			window.wd.hide();
		} else {
			ui.alert(d);
		}
	});
}
function openChapEdit(chapid) {
	window.open("/uploader/view-chapter/" + chapid, "_blank");
}
function selOperation(ope) {
	q("[selector]:checked").forEach(ope);
}
function openVipSetter() {
	window.wd = ui.win.create("Thiết lập vip");
	var bd = wd.body;
	var r = bd.row();
	r.addText("Số lần mua để mở free: ");
	r.addPadder();
	var ip = r.addInput("unvipvalue", "");
	r = bd.row();
	r.addText("Điều chỉnh thành 0 để không mở vip tự động.");
	ip.value = 5;
	r = bd.row();
	r.addButton("Setvip", "setChapVip()", "primary");
	r.style.display = "block";
	r.style.textAlign = 'center';
	wd.show();
}
function setChapVip() {
	var list = [];
	var unvip = g("unvipvalue").value;
	if (unvip == "" || parseInt(unvip) < 0) {
		unvip = 0;
	}
	if (parseInt(unvip) > 50) {
		unvip = 50;
	}
	selOperation(function (e) {
		var ctx = getContextC(e);
		list.push({ uname: "isvip", "vip": "1", "ctx": ctx });
		list.push({ uname: "unvip", "unvip": unvip, "ctx": ctx });
	});
	postSaves(list);
	console.log(list);
}
function setChapUnVip() {
	var list = [];
	selOperation(function (e) {
		var ctx = getContextC(e);
		list.push({ uname: "isvip", "vip": "0", "ctx": ctx });
	});
	postSaves(list);
	console.log(list);
}
function setChapOpen() {
	var list = [];
	selOperation(function (e) {
		list.push({ uname: "open", "open": "1", "ctx": getContextC(e) });
	});
	postSaves(list);
	console.log(list);
}
function setChapEncrypt() {
	var list = [];
	var active = false;
	selOperation(function (e) {
		if (!active) {
			active = getEncryptStatus(e) ? "0" : "1";
		}
		list.push({ uname: "encrypt", "encrypt": active, "ctx": getContextC(e) });
		setEncryptStatus(e, active);
	});
	postSaves(list);
	console.log(list);
}
function addChapter() {
	window.open("/uploader/add-chapter/" + contextParent);
}
function saveNewChapter() {
	var num = val("num"), altnum = val("altnum"), content = val("content"), name = val("name");
	var issingle = val("issingle");
	if (issingle) {
		altnum = 999999;
	}
	ajax("ajax=bookmanager&action=createchap&ctx=" + num + "&pctx=" + contextParent, function (d) {
		if (d != "data broken") {
			var ctx = parseInt(d);
			postSaves([
				{ uname: "altnum", altnum: altnum, "ctx": ctx },
				{ uname: "name", name: name, "ctx": ctx },
				{ uname: "content", content: content.replace(/</g, ""), "ctx": ctx }
			], function () {
				location.href = "/uploader/list-chapter/" + contextParent;
			});
		} else {
			alert("Có lỗi xảy ra");
		}
	});
}
async function createAndSaveChapter(text, cb) {
	var num = val("num"), altnum = val("altnum"), content = text, name = await detectInfoInText(text);
	if (!name) {
		name = {
			altnum: 0,
			name: text.split("\n")[0].substr(0, 80) + ".."
		}
	}
	ajax("ajax=bookmanager&action=createchap&ctx=" + num + "&pctx=" + contextParent, function (d) {
		if (d != "data broken") {
			var ctx = parseInt(d);
			postSaves([
				{ uname: "altnum", altnum: name.altnum, "ctx": ctx },
				{ uname: "name", name: name.name, "ctx": ctx },
				{ uname: "content", content: content.replace(/</g, ""), "ctx": ctx }
			], function () {
				g("ip-num").value = parseInt(num) + 1;
				cb();
			});
		} else {
			alert("Có lỗi xảy ra");
		}
	});
}
function addAllDetectedChapter(btn, cont) {
	if (!cont && !confirm("Xác nhận thêm tất cả?")) {
		return;
	}
	var d = g("found");
	var e = d.children[0];
	var t = e.querySelector("button").stext;
	createAndSaveChapter(t, function () {
		d.children[0].remove();
		addAllDetectedChapter(t, true);
	});
}
function deletechapter(id) {
	var row = event.target.parentElement.parentElement;
	if (event.target.tagName == "I") {
		row = row.parentElement;
	}
	if (confirm("Xác nhận xóa chương [" + row.children[1].textContent + "] " + row.children[3].textContent + "? Nếu lỡ tay xóa có thể liên hệ khôi phục."))
		ajax("ajax=bookmanager&action=deletechapter&ctx=" + id + "&pctx=" + contextParent, function (d) {
			if (d == "not exist") {
				alert("Chương không tồn tại");
			} else {
				row.style.backgroundColor = "#ffc7c7";
				row.style.opacity = "0.5";
			}
		});
}
function deleteChapter() {
	var list = [];
	selOperation(function (e) {
		list.push({ "pctx": contextParent, "ctx": getContextC(e) });
	});
	if (confirm("Xác nhận xóa hàng loạt " + list.length + " chương?")) {
		for (var i = 0; i < list.length; i++) {
			var ind = list[i].ctx;
			var row = g(ind);
			ajax("ajax=bookmanager&action=deletechapter&ctx=" + list[i].ctx + "&pctx=" + contextParent, function (d) {
				if (d == "not exist") {
					alert("Chương không tồn tại");
				} else {
					row.style.backgroundColor = "#ffc7c7";
					row.style.opacity = "0.5";
				}
			});
		}
	}
}
function restoreChapter(id) {
	var row = event.target.parentElement.parentElement;
	if (event.target.tagName == "I") {
		row = row.parentElement;
	}
	if (confirm("Xác nhận khôi phục chương [" + row.children[1].textContent + "] " + row.children[3].textContent))
		ajax("ajax=bookmanager&action=restorechapter&ctx=" + id + "&pctx=" + contextParent, function (d) {
			if (d == "not exist") {
				alert("Chương không tồn tại");
			} else {
				row.style.backgroundColor = "#c6edc6";
				row.style.opacity = "0.5";
			}
		});
}
function restoreChapters() {
	var list = [];
	selOperation(function (e) {
		list.push({ "pctx": contextParent, "ctx": getContextC(e) });
	});
	if (confirm("Xác nhận khôi phục hàng loạt " + list.length + " chương?")) {
		for (var i = 0; i < list.length; i++) {
			var ind = list[i].ctx;
			var row = g(ind);
			ajax("ajax=bookmanager&action=restorechapter&ctx=" + list[i].ctx + "&pctx=" + contextParent, function (d) {
				if (d == "not exist") {
					alert("Chương không tồn tại");
				} else {
					row.style.backgroundColor = "#c6edc6";
					row.style.opacity = "0.5";
				}
			});
		}
	}
}
function validatecnum(num) {
	ajax("ajax=bookmanager&action=checkexist&type=chapnum&ctx=" + num + "&pctx=" + contextParent, function (d) {
		if (d == "true") {
			g("duplicatenotify").style.display = "block";
		} else {
			g("duplicatenotify").style.display = "none";
		}
	});
}
function rebuildchaplist() {
	ajax("minrt=langer&ajax=rebuildchaplist&bid=" + contextParent, function (down) {
		if (down) alert(down);
	});
}
function openTxtFile() {
	//return;
	window.wd = ui.win.create("Mở file txt");
	window.wd.id = "wdaddtxt";
	var bd = wd.body;
	var r = bd.row();
	r.addButton("Mở file", "loadTxtFile()", "primary");
	r.style.display = "block";
	r.style.textAlign = 'center';

	var sel = document.createElement("select");
	sel.id = "textencoding";
	sel.innerHTML = `
		<option value="utf-8">Chọn bảng mã</option>
		<option value="utf-8">UTF-8</option>
		<option value="gb18030">GBK</option>
	`;
	r.appendChild(sel);
	r = bd.row();
	r.addText("Phân tách: ");
	var ip = r.addInput("spl", "Phân tách chương");
	ip.style.height = "25px";
	ip.value = "auto";
	r.addButton("Có sẵn", "openSplitModeSelect()", "primary");
	r = bd.row();
	r.addText("Chương đã phát hiện: ");
	r.addPadder();
	var btn = r.addButton("Thêm tất cả", "addAllDetectedChapter(this)", "primary");
	btn.id = "btnsaveall";
	r = bd.row();
	r.id = "found";
	r.style.display = "block";
	wd.show();
}
function openSplitModeSelect() {
	var sel = ui.select("Lựa chọn");
	sel.proc = function (v) {
		g("spl").value = v;
		sel.hide();
	}
	sel.option("auto", "Phân tách tự động theo tên chương");
	sel.option("length=5000", "Phân tách theo độ dài");
	sel.option("-----", "-----------");
	sel.option("*****", "***********");
	sel.option("~~~~~", "~~~~~~~~~~~");
	sel.option("////", "/////");
	sel.show();
}
function loadTxtFile() {
	var enc = g("textencoding").value;
	ui.readfile(function (text) {
		var splitmethod = g("spl").value;
		var chapters = [];
		function addToList(e) {
			var n = document.createElement("div");
			var btn = document.createElement("button");
			btn.className = "primary";
			btn.innerHTML = "Thêm +";
			btn.style.float = "right";
			btn.type = "button";
			btn.stext = e;
			var d = document.createElement("div");
			d.style.clear = "both";
			btn.onclick = function () {
				event.preventDefault();
				createAndSaveChapter(e, function () {
					n.remove();
				});
			}
			n.innerHTML = e.split("\n")[0];
			n.appendChild(btn);
			n.appendChild(d);
			found.appendChild(n);
		}
		if (splitmethod == "auto") {
			var nametester = /^(?:thứ|chương|hồi|【|no\.?)? ?(?:[0-9]|một|hai|ba|bốn|năm|sáu|bảy|tám|chín|mười|lẻ|ngàn|trăm|mươi|linh)/i;
			var nametester2 = /^[ \t]*?(?:(?:第|【|no\.?)? ?(?:[0-9一二三四五六七八九十万千两百零]+) ?[章节集 ])|^[ \t]*?(?:(?:thứ|chương|hồi|【|no\.?)? ?(?:[0-9]|một|hai|ba|bốn|năm|sáu|bảy|tám|chín|mười|lẻ|ngàn|trăm|mươi|linh)).*?:/mgi;
			var m;//= nametester2.exec(text);
			var first = true;
			var lidx = 0;
			while ((m = nametester2.exec(text)) != null) {
				var idx = m.index;
				if (!first) {
					chapters.push(text.substr(lidx, idx - lidx));
				}
				first = false;
				lidx = idx;
				console.log(idx);
			}
			chapters.push(text.substr(lidx));
			var wd = g("wdaddtxt");
			var found = g("found");
			chapters.forEach(function (e) {
				addToList(e);
			});
		}
		else if (splitmethod.contain("length=")) {
			var len = + splitmethod.split("=")[1];
			if (len < 1200) {
				return alert("Độ dài quá ngắn.");
			}
			function chunkSubstr(str, size) { //stack overflow
				const numChunks = Math.ceil(str.length / size)
				const chunks = new Array(numChunks);
				for (let i = 0, o = 0; i < numChunks; ++i, o += size) {
					chunks[i] = str.substr(o, size);
				}
				return chunks;
			}
			chapters = chunkSubstr(text, len);
			var found = g("found");
			chapters.forEach(function (e, i) {
				addToList("Phần " + (i + 1) + ":\n" + e);
			});
		}
		else {
			chapters = text.split(new RegExp(`${splitmethod}+`, "")).map(function (e) {
				return e.trim();
			});
			var wd = g("wdaddtxt");
			var found = g("found");
			chapters.forEach(function (e) {
				if (e.length < 10) {
					return;
				}
				addToList(e);
			});
		}
		if (chapters.length > 0) {
			g("btnsaveall").chapters = chapters;
		}
	}, ".txt", enc == "gb18030" ? "gb18030" : null);
}
function contentChange(c) {
	var curcname = val("name").trim();
	var lines = c.split("\n");
	var nametester = /^(thứ|chương|hồi|【|no\.?)? ?([0-9]|một|hai|ba|bốn|năm|sáu|bảy|tám|chín|mười)/i;
	var nametester2 = /^(第|【|no\.?)? ?([0-9一二三四五六七八九十万千两百零])/i;
	for (var i = 0; i < lines.length; i++) {
		if (lines[i].trim() != "") {
			if (nametester2.test(lines[i].trim())) {
				var cname = lines[i].trim().split("章");
				if (cname.length > 1) {
					cname = cname[1];
				} else {
					cname = lines[i].trim().split("节");
					if (cname.length > 1) {
						cname = cname[1];
					} else {
						cname = lines[i].trim().split("集");
						if (cname.length > 1) {
							cname = cname[1];
						} else {
							cname = cname[0];
						}
					}
				}
				ajax("sajax=trans&content=" + encodeURIComponent(cname), function (down) {
					cname = down.trim();
					if (curcname == "" || cname.substr(0, curcname.length) == curcname) {
						g("ip-name").value = cname;
					}
				});
			} else
				if (nametester.test(lines[i].trim())) {
					var cname = lines[i].trim().split(":");
					if (cname.length > 1) {
						cname = cname[1];
					} else {
						cname = cname[0];
					}
					if (curcname == "" || cname.trim().substr(0, curcname.length) == curcname) {
						g("ip-name").value = cname;
					}
				}
			break;
		}
	}
}
async function detectInfoInText(t) {
	var lines = t.trim().split("\n");
	var nametester = /^(thứ|chương|hồi|phần|【|no\.?)? ?([0-9]|một|hai|ba|bốn|năm|sáu|bảy|tám|chín|mười)/i;
	var nametester2 = /^(第|【|no\.?)? ?([0-9一二三四五六七八九十万千两百零]+)/i;
	for (var i = 0; i < lines.length; i++) {
		if (lines[i].trim() != "") {
			if (nametester2.test(lines[i].trim())) {
				var ba = lines[i];
				var cname = lines[i].trim().split("章");
				if (cname.length > 1) {
					cname = cname[1];
				} else {
					cname = lines[i].trim().split("节");
					if (cname.length > 1) {
						cname = cname[1];
					} else {
						cname = lines[i].trim().split("集");
						if (cname.length > 1) {
							cname = cname[1];
						} else {
							cname = cname[0];
						}
					}
				}
				var pm = await new Promise(function (resolve, reject) {
					ajax("sajax=trans&content=" + encodeURIComponent(cname), function (down) {
						var altnum = new ChineseNumber(ba.match(/[0-9一二三四五六七八九十万千两百零]+/)[0]).toInteger();
						cname = down.trim();
						resolve({
							altnum: altnum,
							name: cname
						});
					});
				})
				return pm;
			} else
				if (nametester.test(lines[i].trim())) {
					var ba = lines[i];
					var cname = lines[i].trim().split(":");
					if (cname.length > 1) {
						cname = cname[1];
					} else {
						cname = cname[0];
					}
					return {
						altnum: ba.match(/[0-9]+/)[0],
						name: cname || "[Chưa đặt tên]"
					}
				}
			break;
		}
	}
	return false;
}
function uploadThumbImage() {
	ui.uploadimg(function (l) {
		setval("thumb", l);
	}, g("previewthumb"), "ajax=bookmanager&action=uploadthumb");
}