// Proxy class for interacting with orleans via frontend.

function OrleansClient(addr) {
    this.OrleansAddress = addr;
};

OrleansClient.prototype.CallMethodOnOtherGrainNew = function (grainid, methodName, args) {
    if (typeof $ != 'undefined') {
        return $.ajax({ url: this.OrleansAddress + "/api/CallMethod?grainid=" + grainid + "&methodName=" + methodName + "&args=" + args })
                .then(function (result) {
                    return result.response;
                });
    }
    return WinJS.xhr({ url: this.OrleansAddress + "/api/CallMethod?grainid=" + grainid + "&methodName=" + methodName + "&args=" + args })
                .then(function (result) {
                    return result.response;
                });
};

OrleansClient.prototype.CallMethodOnOtherGrain = function (grainid, methodName, args) {
    var url = this.OrleansAddress + "/api/CallMethod?grainid=" + grainid + "&methodName=" + methodName + "&args=" + args;
    var client = new XMLHttpRequest();
    client.open("GET", url, false);
    client.setRequestHeader("Content-Type", "text/plain");
    client.send();
    return client.response;
};

OrleansClient.prototype.InitGrain = function (grainid, graincode) {
    var params = "=" + encodeURIComponent(graincode);
    var url = this.OrleansAddress + "/api/Init/" + grainid;
    //var url = this.OrleansAddress + "/api/Init?grainid=" + grainid + "&grainCode=" + graincode;

    var client = new XMLHttpRequest();
    client.open("Put", url, false);
    client.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
    client.send(params);
    return client.response;
};
