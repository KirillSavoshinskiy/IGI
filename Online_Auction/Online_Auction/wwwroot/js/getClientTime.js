"use strict";

document.getElementById("createSubmit").addEventListener("click", function (event) {
    let d = new Date();
     
    console.log(d.toString());
    document.cookie = "hour="+d.getHours().toString();
    var cookies = document.cookie;
    console.log(cookies);
});