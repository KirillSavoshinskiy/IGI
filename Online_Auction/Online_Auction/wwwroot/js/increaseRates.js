"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/Home/ProfileLot").build();

 
connection.on("ReceiveRate", function (user, rate) { 
    document.getElementById("price").textContent = rate + " " + "BYN"; 
});

connection.on("Alert", function (alert) {
    document.getElementById("Alert").textContent = alert; 
});

connection.on("AlertOwner", function (alert) {
    document.getElementById("Alert").textContent = alert;
});

connection.start().then(function(){
}).catch(function (err) {
    return console.error(err.toString());
}); 

document.getElementById("submitRate").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var rate = document.getElementById("inputRate").value;
    var lotId = document.getElementById("lotId").value;
    document.getElementById("Alert").textContent = "";
    connection.invoke("SendRate", user, rate, lotId).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});