function add(a, b) { return a + b; }

function fact(a) { if (a < 0) { return -1; } if (a === 0) { return 1; } else { return a * fact(a - 1); } }

function callbacktest(result)
{
    return result;
}

function calcFactOnOther(grainid, n)
{
    return CallMethodOnOtherGrain(grainid, "fact", n);
}

function callmethodonothergraintwice(grainid)
{
    var cb = function (result) { return result; }
    CallDotNetMethod.CallMethodOnOtherGrain(grainid, "fact", 5, cb);
    CallDotNetMethod.CallMethodOnOtherGrain(grainid, "fact", 7, cb);
    return true;
}



//factAsync(a).then(function (response) { console.log(response); });

//"var a = false; \n var res; \n {0}({1}).then(function (response) {{ res = response; a = true; }}) \n while (!a) \n {{ }} \n res"

function factAsync(n) {
    var a = false;
    var res;
    factpromise(n).then(function (response) { res = response;
        a = true;
    });
    while (!a) {
        
    }
    return res;
}

function factpromise(a)
{
    return new Promise(function (resolve, reject) {
        resolve(fact(a));
    });
}