﻿"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/Home/ProfileLot").build();

//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;
 
connection.on("ReceiveMessage", function (user, message) {
 
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = user + ":" + msg;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
});

connection.start().then(function(){
    let lotId = document.getElementById("lotId").value; 
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
}); 

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    var lotId = document.getElementById("lotId").value;
     
    
    connection.invoke("SendMessage", user, message, lotId).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});