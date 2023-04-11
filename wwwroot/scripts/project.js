/*
 * Get Elements Position
 */
function getElementPosition(id) {
    var ele = document.getElementById(id);
    return ele.getBoundingClientRect();
}
function scrollToBottom(id) {
    var ele = document.getElementById(id);
    ele.scrollTop = ele.scrollHeight;
}
function getEleValue(id) {
    var ele = document.getElementById(id);
    return ele.value;
}
function setMouseEventHandler(ele,event,helper,func) {
    ele.addEventListener(
        event,
        function(e) {
            helper.invokeMethodAsync(func,e.offsetX,e.offsetY);
        },
        false
    );
}
function setEventHandler(ele,event,helper,func) {
    ele.addEventListener(
        event,
        function(e) {
            helper.invokeMethodAsync(func,e);
        },
        false
    );
}
function saveImageToFile(filename,canvas) {
    var dataurl = canvas.toDataURL();
    var lnk = document.createElement("a");
    lnk.href = dataurl;
    lnk.download = filename;
    lnk.click();
}

function LoadAudio(ele) {
    ele.load();
}
function StartAudio(ele) {
    ele.currentTime = 0;
    ele.play();
}
function StopAudio(ele) {
    ele.pause();
}

//
// Canvas Helper
//

// Fill Rectangle
function FillRect(canvas, left,top,width,height,color) {
    var ctx = canvas.getContext('2d');
    ctx.fillStyle=color;
    ctx.fillRect(left,top,width,height);
}
// Fill Ellipse
function FillEllipse(canvas,left,top,r,color) {
    var ctx = canvas.getContext('2d');
    ctx.beginPath();
    ctx.fillStyle=color;
    ctx.arc(left,top,r,0,2*Math.PI);
    ctx.fill();
    ctx.closePath();
}

// Fill Traiangle
function FillTraiangle(canvas,left,top,len,color) {
    var ctx = canvas.getContext('2d');
    ctx.beginPath();
    ctx.fillStyle = color;
    ctx.moveTo(left,top);
    ctx.lineTo(left-len/2,len);
    ctx.lineTo(left-len/2+len,len);
    ctx.lineTo(left,top);
    ctx.fill();
    ctx.closePath();
}

// Clear Canvas
function ClearCanvas(canvas) {
    var ctx = canvas.getContext('2d');
    ctx.clearRect(0,0,canvas.width,canvas.height);
}

// Get Specified Elements Rectangle
function GetElementRect(canvas) {
    var rect = canvas.getBoundingClientRect();
    return rect;
}

// moveTo Helper
function MoveToCanvas(canvas,x,y) {
    var ctx = canvas.getContext('2d');
    ctx.beginPath();
    ctx.moveTo(x,y);
}

// lineTo Helper
function LineToCanvas(canvas,x,y,color,width) {
    var ctx = canvas.getContext('2d');
    ctx.lineWidth = width;
    ctx.strokeStyle = color;
    ctx.fillStyle = color;
    ctx.lineTo(x,y);
    ctx.stroke();
}

// closePath Helper
function ClosePath(canvas,fillflag) {
    var ctx = canvas.getContext('2d');
    if (fillflag) {
        ctx.fill();
    }
    ctx.closePath();
}

// Get Image Data
function GetImageData(canvas,x,y) {
    var ctx = canvas.getContext('2d');
    return ctx.getImageData(x,y,1,1);
}

// Save to Blob
function SaveImageData(canvas,helper,callback) {
    var img;
    var url = canvas.toDataURL();
    img = helper.invokeMethod(callback,url);
    return img;
}