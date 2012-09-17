var github = {};

void function() {

    github.login= function(clientID,redirectURI,scope,state) {
        window.location = "https://github.com/login/oauth/authorize?client_id="+encodeURIComponent(clientID)
            +(redirectURI?"&redirect_uri="+encodeURIComponent(redirectURI):"")
            +(scope?"&scope="+encodeURIComponent(scope):"")
            +(state?"&state="+encodeURIComponent(state):"");
    }

    github.access_token = function(){
        var name = "access_token";
        var cookies=document.cookie.split("; ");
        for(var i=0;i<cookies.length;i++)
        {
            var s=cookies[i].split("=");
            if(s[0]==name)return unescape(s[1]);
        }
    }();
    
    github.host = "https://api.github.com";
    
    github.accept = "application/vnd.github.v3.full+json";


    function GithubAPI(method,path,params){
        return Wind.Async.Task.create(function (t) {
            
            var xhr = new XMLHttpRequest();
            
            xhr.open(method, github.host+path, true);
            
            
            xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            
            xhr.setRequestHeader("Accept", github.accept);
            
            xhr.setRequestHeader("Authorization", "token "+github.access_token);
            
            xhr.onreadystatechange = function () {
                if (this.readyState == 4)
                    t.complete("success",JSON.parse(xhr.responseText));
            }
            
            xhr.send(JSON.stringify(params));
        });

    }
    github.getUser = function(user){
        return GithubAPI("GET","/users/"+user,null);
    }
    github.getAuthenticatedUser = function(){
        return GithubAPI("GET","/user",null);
    }
    /*
    github.updateAuthenticatedUser = function(params){
        return GithubAPI("PATCH","/user",params);
    }*/
    github.listIssues = function(params){        
        return GithubAPI("GET","/issues",params);
    }
    github.listIssuesForRepository = function(user,repo,params){        
        return GithubAPI("GET","/repos/"+user+"/"+repo+"/issues",params);
    }
    github.createIssue = function(user,repo,params){        
        return GithubAPI("POST","/repos/"+user+"/"+repo+"/issues",params);
    }
}();