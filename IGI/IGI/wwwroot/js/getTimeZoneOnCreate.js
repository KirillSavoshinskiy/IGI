document.getElementById("createSubmit").addEventListener("click", function (event) {
    let d = new Date(); 
    document.cookie = "timeZone="+d.getTimezoneOffset().toString();
    let cookies = document.cookie; 
});
