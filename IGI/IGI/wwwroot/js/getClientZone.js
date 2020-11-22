let d = new Date();
document.cookie = "ClientZone=" + d.getTimezoneOffset().toString(); 
console.log(document.cookie)
 