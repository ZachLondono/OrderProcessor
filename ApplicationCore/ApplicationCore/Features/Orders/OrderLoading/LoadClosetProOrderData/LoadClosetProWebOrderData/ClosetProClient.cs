using RestSharp;
using System.Net;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.LoadClosetProWebOrderData;

internal class ClosetProClient {

    private readonly string _username;
    private readonly string _password;
    private readonly string _instanceName;
    
    public ClosetProClient(string username, string password, string instanceName) {
        _username = username;
        _password = password;
        _instanceName = instanceName;
    }

    public async Task<string?> GetCutListDataAsync(int designId) {
    
        var client = CreateClient();
        
        var loginRequest = CreateLoginRequest(_username, _password);
        var loginResponse = await client.ExecuteAsync(loginRequest);
        
        if (loginResponse.StatusCode == HttpStatusCode.Redirect) {
        
        
            var cutlistRequest = GenerateCutListRequest(designId);
            var stdCLResponse = await client.ExecuteAsync(cutlistRequest);
            
            if (stdCLResponse.StatusCode == HttpStatusCode.Redirect) {
                
                var downloadRequest = CreateDownloadRequest(designId);
                var downloadResponse = await client.ExecuteAsync(downloadRequest);
                
                return downloadResponse.Content;
                
            } else {
            
                Console.WriteLine("Cut List was not generated");
                return null;
            
            }
        
        } else {
        
            Console.WriteLine("Login Failed");
            return null;
        
        }
    
    }

    private RestClient CreateClient() {
    
        var cookieJar = new CookieContainer();
        var options = new RestClientOptions("https://royalcabinet.closetprosoftware.com") {
          MaxTimeout = -1,
          UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36",
          FollowRedirects = false,
          CookieContainer = cookieJar
        };
        
        return new(options);
    
    }
    
    private RestRequest CreateDownloadRequest(int designId) {
    
        var request = new RestRequest("StandardCutList.aspx?PID=0&CID=5&T=1&P=C", Method.Get);
        AddCPHeaders(request);
        
        request.AddUrlSegment("T", "1");    // Type?
        request.AddUrlSegment("P", "C");
        request.AddUrlSegment("CID", designId);
        request.AddUrlSegment("PID", "0");
        
        return request;
    
    }
    
    private RestRequest GenerateCutListRequest(int designId) {
    
        var request = new RestRequest("GenerateCutList.aspx", Method.Get);
        AddCPHeaders(request);
        
        request.AddUrlSegment("T", "1");    // Type?
        request.AddUrlSegment("P", "C");    
        request.AddUrlSegment("CID", designId);
        request.AddUrlSegment("PT", "N");
        
        return request;
    
    }
    
    private RestRequest CreateLoginRequest(string username, string password) {
    
        var request = new RestRequest("/login.aspx", Method.Post);
        AddCPHeaders(request);
            
        request.AddParameter("UserEmail", username);
        request.AddParameter("UserPW", password);
        request.AddParameter("__EVENTVALIDATION", "/wEdAAuoS7FKyV6NReK05NZsBYuvmbPYTfnUmq9VB1vL+V5Bf9VidFe+StlUq9Z5UBVcgio85pbWlDO2hADfoPXD/5tdeas8b2dSfWoH06jyW707oc+N+aipBT3RLGXvV35pNmBYL6cxLwS2xjEYOm1IHlC7SKcSLRmm5MrwL3b0QpXZM0tyCzci6nG+dE4KwPO/C2cM43Ckv+gfXlmlRzrs0RwhCqmjXEP4iHv8SHq1w27kbRrCWtJnM30OCxcqSjqWgMbQecWNOA6AClnm/wJyB9HH");
        request.AddParameter("__VIEWSTATE", "/wEPDwUJMjM4NDkzMTg3D2QWAmYPZBYEAgEPDxYCHgRUZXh0BRhSb3lhbCBDYWJpbmV0IFVTRVIgTE9HSU5kZAIDD2QWAgIOD2QWAgIGDxYCHgdWaXNpYmxlaGRk7nXLMLHuVhHhoCWM/88u/EPcc/j8XQkKo9DGcmVOBvk=");
        request.AddParameter("__VIEWSTATEGENERATOR", "C2EE9ABB");
        request.AddParameter("btnSubmit", "Login");
        
        request.AddParameter("RegEmail", "");
        request.AddParameter("RegFName", "");
        request.AddParameter("RegLName", "");
        request.AddParameter("RegPhone", "");
        request.AddParameter("RegUserPW", "");
        request.AddParameter("RegUserPW2", "");
        request.AddParameter("__EVENTARGUMENT", "");
        request.AddParameter("__EVENTTARGET", "");
        request.AddParameter("__LASTFOCUS", "");
        request.AddParameter("g-recaptcha-response", "");
    
        return request;
    
    }

    public void AddCPHeaders(RestRequest request) {
        request.AddHeader("host", $"{_instanceName}.closetprosoftware.com");
        request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
        request.AddHeader("sec-ch-ua", "\"Not.A/Brand\";v=\"8\", \"Chromium\";v=\"114\", \"Google Chrome\";v=\"114\"");
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddHeader("Sec-Fetch-Dest", "document");
        request.AddHeader("Sec-Fetch-Mode", "navigate");
        request.AddHeader("Sec-Fetch-Site", "same-origin");
        request.AddHeader("Sec-Fetch-User", "?1");
        request.AddHeader("sec-ch-ua-mobile", "?0");
        request.AddHeader("sec-ch-ua-platform", "\"Windows\"");
        request.AddHeader("Upgrade-Insecure-Requests", "1");
    }

}
