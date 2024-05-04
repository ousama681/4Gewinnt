"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();