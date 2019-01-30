using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace todo.Controllers
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Models;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using System.Diagnostics;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class ItemController : Controller
    {

        [ActionName("Reply")]
        public async Task<ActionResult> Reply([Bind(Include = "Id,gender, FirstName, LastName, Subject, Enquiry, Reply")] Item item)
        {
            if (ModelState.IsValid)
            {
                string lang = new todo.Controllers.ItemController().DetectLanguage(item.Enquiry);
                string responselang = new todo.Controllers.ItemController().DetectLanguage(item.Reply);
                string translatedResponsetext = new todo.Controllers.ItemController().Translate(item.Reply, responselang, lang);
                item.Reply = translatedResponsetext;
                await DocumentDBRepository<Item>.UpdateItemAsync(item.Id, item);
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        [ActionName("Index")]
        public async Task<ActionResult> IndexAsync()
        {
            var items = await DocumentDBRepository<Item>.GetItemsAsync(d => !d.Completed);
            return View(items);
        }

#pragma warning disable 1998
        [ActionName("Create")]
        public async Task<ActionResult> CreateAsync()
        {
            Item item;
            return View();
        }
#pragma warning restore 1998

        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync([Bind(Include = "Id,gender, FirstName, LastName, Subject, Enquiry")] Item item)
        {
            if (ModelState.IsValid)
            {
                await DocumentDBRepository<Item>.CreateItemAsync(item);
                return RedirectToAction("Index");
            }

            return View(item);
        }

        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync([Bind(Include = "Id,gender, FirstName, LastName, Subject, Enquiry, Reply")] Item item)
        {
            if (ModelState.IsValid)
            {
                await DocumentDBRepository<Item>.UpdateItemAsync(item.Id, item);
                return View(item);
            }

            return View(item);
        }

        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Item item = await DocumentDBRepository<Item>.GetItemAsync(id);
            if (item == null)
            {
                return HttpNotFound();
            }

            return View(item);
        }

        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Item item = await DocumentDBRepository<Item>.GetItemAsync(id);
            if (item == null)
            {
                return HttpNotFound();
            }

            return View(item);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmedAsync([Bind(Include = "Id")] string id)
        {
            await DocumentDBRepository<Item>.DeleteItemAsync(id);
            return RedirectToAction("Index");
        }

        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(string id)
        {
            Item item = await DocumentDBRepository<Item>.GetItemAsync(id);
            return View(item);
        }

        public string getAuthToken()
        {
            string key = "*translate service key here*";
            var authTokenSource = new AzureAuthToken(key.Trim());
            string authToken;
            try
            {
                authToken = authTokenSource.GetAccessToken();
            }
            catch (HttpRequestException)
            {
                throw;
            }
            return authToken;

        }

        public string DetectLanguage(string text)
        {
            string authToken = getAuthToken();
            //string uri = "https://api.microsofttranslator.com/V2/Http.svc/Detect?text=" + text;

            string uri = "https://api.cognitive.microsofttranslator.com/detect?api-version=3.0";
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add("Authorization", authToken);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "[{\'Text\':\'" + text + "\'}]";
                System.Diagnostics.Debug.WriteLine("json to be sent: " + json);
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                string languageDetected = streamReader.ReadToEnd();
                JArray a = JArray.Parse(languageDetected);
                foreach (JObject o in a.Children<JObject>())
                {
                    foreach (JProperty p in o.Properties())
                    {
                        string name = p.Name;
                        if (name.Equals("language"))
                        {
                            string value = (string)p.Value;
                            languageDetected = value;                         
                        }
                    }
                }
                return languageDetected;
            }

        }

        public string Translate(string text, string detectedLanguage, string targetLanguage)
        {
            string authToken = getAuthToken();
            string from = detectedLanguage;
            //string to = "en";
            string to = targetLanguage;
        
            //string uri = "https://api.microsofttranslator.com/V2/Http.svc/Translate?text=" + HttpUtility.UrlEncode(text) + "&from=" + from + "&to=" + to;
            string uri = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0" + "&from=" + from + "&to=" + to;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add("Authorization", authToken);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            System.Diagnostics.Debug.WriteLine("text to be translated: "+text);

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "[{\'Text\':\'"+text+"\'}]";
                System.Diagnostics.Debug.WriteLine("json to be sent: " + json);
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                string translation = streamReader.ReadToEnd();
                JArray a = JArray.Parse(translation);
                foreach (JObject o in a.Children<JObject>())
                {
                    foreach (JProperty p in o.Properties())
                    {
                        foreach (JObject s in p.Value.Children<JObject>())
                        {
                            var RootObjects = JsonConvert.DeserializeObject<RootObject>(s.ToString());
                            translation = RootObjects.text;
                        }
                    }
                }
                return translation;
            }
        }
    }

   
}

public class RootObject
{
    public string text { get; set; }
    public string language { get; set; }
}

public class AzureAuthToken
{
    /// URL of the token service                      
    private static readonly Uri ServiceUrl = new Uri("https://api.cognitive.microsoft.com/sts/v1.0/issuetoken");

    /// Name of header used to pass the subscription key to the token service
    private const string OcpApimSubscriptionKeyHeader = "Ocp-Apim-Subscription-Key";

    /// After obtaining a valid token, this class will cache it for this duration.
    /// Use a duration of 5 minutes, which is less than the actual token lifetime of 10 minutes.
    private static readonly TimeSpan TokenCacheDuration = new TimeSpan(0, 5, 0);

    /// Cache the value of the last valid token obtained from the token service.
    private string _storedTokenValue = string.Empty;

    /// When the last valid token was obtained.
    private DateTime _storedTokenTime = DateTime.MinValue;

    /// Gets the subscription key.
    public string SubscriptionKey { get; }

    /// Gets the HTTP status code for the most recent request to the token service.
    public HttpStatusCode RequestStatusCode { get; private set; }

    /// <summary>
    /// Creates a client to obtain an access token.
    /// </summary>
    /// <param name="key">Subscription key to use to get an authentication token.</param>
    public AzureAuthToken(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException(nameof(key), "A subscription key is required");
        }

        this.SubscriptionKey = key;
        this.RequestStatusCode = HttpStatusCode.InternalServerError;
    }

    /// <summary>
    /// Gets a token for the specified subscription.
    /// </summary>
    /// <returns>The encoded JWT token prefixed with the string "Bearer ".</returns>
    /// <remarks>
    /// This method uses a cache to limit the number of request to the token service.
    /// A fresh token can be re-used during its lifetime of 10 minutes. After a successful
    /// request to the token service, this method caches the access token. Subsequent 
    /// invocations of the method return the cached token for the next 5 minutes. After
    /// 5 minutes, a new token is fetched from the token service and the cache is updated.
    /// </remarks>
    public async Task<string> GetAccessTokenAsync()
    {
        if (string.IsNullOrWhiteSpace(this.SubscriptionKey))
        {
            return string.Empty;
        }

        // Re-use the cached token if there is one.
        if ((DateTime.Now - _storedTokenTime) < TokenCacheDuration)
        {
            return _storedTokenValue;
        }

        using (var client = new HttpClient())
        using (var request = new HttpRequestMessage())
        {
            request.Method = HttpMethod.Post;
            request.RequestUri = ServiceUrl;
            request.Content = new StringContent(string.Empty);
            request.Headers.TryAddWithoutValidation(OcpApimSubscriptionKeyHeader, this.SubscriptionKey);
            client.Timeout = TimeSpan.FromSeconds(2);
            var response = await client.SendAsync(request);
            this.RequestStatusCode = response.StatusCode;
            response.EnsureSuccessStatusCode();
            var token = await response.Content.ReadAsStringAsync();
            _storedTokenTime = DateTime.Now;
            _storedTokenValue = "Bearer " + token;
            return _storedTokenValue;
        }
    }

    /// <summary>
    /// Gets a token for the specified subscription. Synchronous version.
    /// Use of async version preferred
    /// </summary>
    /// <returns>The encoded JWT token prefixed with the string "Bearer ".</returns>
    /// <remarks>
    /// This method uses a cache to limit the number of request to the token service.
    /// A fresh token can be re-used during its lifetime of 10 minutes. After a successful
    /// request to the token service, this method caches the access token. Subsequent 
    /// invocations of the method return the cached token for the next 5 minutes. After
    /// 5 minutes, a new token is fetched from the token service and the cache is updated.
    /// </remarks>
    public string GetAccessToken()
    {
        // Re-use the cached token if there is one.
        if ((DateTime.Now - _storedTokenTime) < TokenCacheDuration)
        {
            return _storedTokenValue;
        }

        string accessToken = null;
        var task = Task.Run(async () =>
        {
            accessToken = await this.GetAccessTokenAsync();
        });

        while (!task.IsCompleted)
        {
            System.Threading.Thread.Yield();
        }
        if (task.IsFaulted)
        {
            throw task.Exception;
        }
        if (task.IsCanceled)
        {
            throw new Exception("Timeout obtaining access token.");
        }
        return accessToken;
    }

}
