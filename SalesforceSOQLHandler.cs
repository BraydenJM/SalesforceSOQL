using System.Text;
using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace SalesforceSOQL
{
    public class SalesforceSOQLHandler
    {
        private string loginEndpoint { get; set; }
        private string apiEndpoint { get; set; }
        private string serviceUrl { get; set; }
        private string authToken { get; set; }
        private string oAuthEndpoint { get; set; }
        private string username { get; set; }
        private string password { get; set; }
        private string consumerKey { get; set; }
        private string consumerSecret { get; set; }
        private string token { get; set; }

        #region constructors
        HttpClient client { get; set; }
        /// <summary>
        /// Constructor for the SOQLQuery class
        /// </summary>
        /// <param name="loginEndpoint">login end point, ex: "https://login.salesforce.com/services/oauth2/token"</param>
        /// <param name="apiEndpoint">api endpoint, ex: "/services/data/vXX.X/"</param>
        /// <param name="serviceUrl">service URL ex: "https://sample.my.salesforce.com/"</param>
        /// <param name="oAuthEndpoint">OAUTH endpoint URL, ex: "https://sample.my.salesforce.com/services/oauth2/token"</param>
        /// <param name="username">salesforce login username</param>
        /// <param name="password">salesforce login user password, must also contain password token</param>
        /// <param name="consumerKey">salesforce API consumer key</param>
        /// <param name="consumerSecret">API secret</param>
        /// <exception cref="Exception">thrown when one or more constructor values is null</exception>
        public SalesforceSOQLHandler(string loginEndpoint, String apiEndpoint, String serviceUrl, String oAuthEndpoint,
            String username, String password, String consumerKey, String consumerSecret)
        {
            if (loginEndpoint != null & apiEndpoint != null & serviceUrl != null & oAuthEndpoint != null & username != null
                & password != null & consumerKey != null & consumerSecret != null)
            {
                this.loginEndpoint = loginEndpoint;
                this.apiEndpoint = apiEndpoint;
                this.serviceUrl = serviceUrl;
                this.oAuthEndpoint = oAuthEndpoint;
                this.username = username;
                this.password = password;
                this.consumerKey = consumerKey;
                this.consumerSecret = consumerSecret;
                this.client = new HttpClient();
                getAuthToken();
            }
            else
            {
                throw new Exception("Error in constructing SOQL Query object. One or more constructor fields provided is Null");
            }

        }
        /// <summary>
        /// Constructor for the SOQLQuery class
        /// </summary>
        /// <param name="loginEndpoint">login end point, ex: "https://login.salesforce.com/services/oauth2/token"</param>
        /// <param name="apiEndpoint">api endpoint, ex: "/services/data/vXX.X/"</param>
        /// <param name="serviceUrl">service URL ex: "https://sample.my.salesforce.com/"</param>
        /// <param name="oAuthEndpoint">OAUTH endpoint URL, ex: "https://sample.my.salesforce.com/services/oauth2/token"</param>
        /// <param name="username">salesforce login username</param>
        /// <param name="password">salesforce login user password, must also contain password token</param>
        /// <param name="token">secret token used for logging into API services. Typically sent once in the users email</param>
        /// <param name="consumerKey">salesforce API consumer key</param>
        /// <param name="consumerSecret">API secret</param>
        /// <exception cref="Exception">thrown when one or more constructor values is null</exception>
        public SalesforceSOQLHandler(string loginEndpoint, string apiEndpoint, string serviceUrl, string oAuthEndpoint,
            string username, string password, string token, string consumerKey, string consumerSecret)
        {
            if (loginEndpoint != null & apiEndpoint != null & serviceUrl != null & oAuthEndpoint != null & username != null
                & password != null & consumerKey != null & consumerSecret != null)
            {
                this.loginEndpoint = loginEndpoint;
                this.apiEndpoint = apiEndpoint;
                this.serviceUrl = serviceUrl;
                this.oAuthEndpoint = oAuthEndpoint;
                this.username = username;
                this.password = password;
                this.token = token;
                this.consumerKey = consumerKey;
                this.consumerSecret = consumerSecret;
                this.client = new HttpClient();
                getAuthToken();
            }
            else
            {
                throw new Exception("Error in constructing SOQL Query object. One or more constructor fields provided is Null");
            }

        }
        /// <summary>
        /// Automatically called when the class is created. Method gets the authToken for the session and sets it as the
        /// authToken value.
        /// </summary>
        /// <exception cref="Exception">throws exeption when no response is recieved from the server using the
        /// parameters provided</exception>
        private void getAuthToken()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string>
                                {
                                {"grant_type", "password"},
                                {"client_id", this.consumerKey},
                                {"client_secret", this.consumerSecret},
                                {"username", this.username},
                                {"password", this.password + this.token}
                                });

            HttpResponseMessage message = client.PostAsync(loginEndpoint, content).Result;
            string response = message.Content.ReadAsStringAsync().Result;
            JObject obj = JObject.Parse(response);

            if ((string)obj["access_token"] != null)
            {
                this.authToken = (string)obj["access_token"];
                Console.WriteLine($"Auth Token recieved from HTTP request.\n" +
                    $" Sent request as: {this.username}\n" +
                    $"API Id: {this.consumerKey}\n" +
                    $"Server Response: {message.StatusCode.ToString()}");
            }
            else
            {
                throw new Exception("HttpResponseMessage returned NULL for access_token parameter");
            }
        }
        #endregion

        #region REST API calls
        /// <summary>
        /// sends SOQL request to the salesforce database using the URL and enpoint values provided by the object parameters.
        /// </summary>
        /// <param name="queryMessage">query message to send in http request</param>
        /// <returns>Returns the server response as a string. String is generated from a parsed JSON response sent by the server</returns>
        public string queryRecordString(string queryMessage)
        {

            string RESTQuery = $"{serviceUrl}{apiEndpoint}query?q={queryMessage}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, RESTQuery);
            request.Headers.Add("Authorization", "Bearer " + authToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.SendAsync(request).Result;
            string result = HTTPResponseToString(response);
            JObject obj = JObject.Parse(result);
            string check = (string)obj["nextRecordsUrl"];
            if(check != null)
            {
                while(check != null)
                {
                    result = result.Remove(result.Length - 8, 8);
                    result += ",\n";
                    request = new HttpRequestMessage(HttpMethod.Get, serviceUrl + check);
                    request.Headers.Add("Authorization", "Bearer " + authToken);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    response = client.SendAsync(request).Result;
                    string temp = HTTPResponseToString(response);
                    obj = JObject.Parse(temp);
                    check = (string)obj["nextRecordsUrl"];

                    foreach(JToken item in obj["records"])
                    {
                        if(item.ToString() == null)
                        {
                            
                            break;
                        }
                        result += item.ToString() + ",\n";
                    }
                    result = result.Remove(result.Length - 2, 2);
                    result += "\n   ]    \n}";  
                }
                return result;
            }
            else
            {
                return result;
            }
            


        }
        /// <summary>
        /// sends SOQL request to the salesforce database using the URL and enpoint values provided by the object parameters.
        /// </summary>
        /// <param name="queryMessage">query message to send in http request</param>
        /// <returns>Returns the server response as a JObject. JObject is parsed from the response sent by the server</returns>
        public JObject queryRecordJObject(string queryMessage)
        {
            string response = queryRecordString(queryMessage);
            JObject result = JObject.Parse(response);
            return result;

        }
        /// <summary>
        /// Returns a two dimensional list from the query sent. The first value of the outside list will contain the request sent to the query.
        /// For example, "SELECT Id, Name, Primary_Advisor__c FROM Account" will generate a 2d list, 3 values deep. The first value of the first
        /// nested list will contain "Id" followed by all account Id values returned by the salesforce database.
        /// </summary>
        /// <param name="queryMessage">SOQL HTTP response message to parse list from</param>
        /// <returns>two dimensional string list</returns>
        public List<List<String>> getStringListFromQuery(String queryMessage)
        {
            JObject temp = queryRecordJObject(queryMessage);//sends SOQL query to salesforce server

            List<List<String>> result = new List<List<String>>();
            List<String> cols = getSObjectColumn(queryMessage);//outside list generated based on selected values in query.

            foreach (string col in cols)
            {
                List<String> row = new List<String>(); //generate list to append to each outside list value
                row.Add(col);
                result.Add(row);
                foreach (var item in temp["records"]) //returned JSON always stores values nested inside of records
                {
                    row.Add(item[col].ToString());
                }
            }
            return result;
        }
        #endregion
        #region PATCH methods
        /// <summary>
        /// patches new values to specified record using salesforce REST API. full update message is constructed from public PATCH methods
        /// </summary>
        /// <param name="updateMessage">message to add to httpcontent</param>
        /// <param name="recordType">object type to patch to</param>
        /// <param name="recordId">record to patch values to</param>
        /// <returns>returns response message from the REST API</returns>
        private string PATCHRecord(string updateMessage, string recordType, string recordId)
        {
            HttpContent contentUpdate = new StringContent(updateMessage, Encoding.UTF8, "application/xml");
            string uri = $"{serviceUrl}{apiEndpoint}sobjects/{recordType}/{recordId}?_HttpMethod=PATCH";
            HttpRequestMessage requestUpdate = new HttpRequestMessage(HttpMethod.Patch, uri);
            requestUpdate.Headers.Add("Authorization", "Bearer " + authToken);
            requestUpdate.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            requestUpdate.Content = contentUpdate;

            HttpResponseMessage response = client.SendAsync(requestUpdate).Result;
            return response.Content.ReadAsStringAsync().Result;
        }
        /// <summary>
        /// constructs full updateMessage to patch new field values to salesforce's REST API.
        /// </summary>
        /// <param name="Id">Id field of object to patch</param>
        /// <param name="recordType">Object type to patch</param>
        /// <param name="field">field name to patch new value to</param>
        /// <param name="fieldValue">field value to patch to given field</param>
        /// <returns>returns response message from the REST API</returns>
        public string PATCHValue(string recordType, string Id, string field, string fieldValue)
        {
            string updateMessage = $"<root>" +
                $"<{field}>{fieldValue}</{field}" +
                $"</root>";

            string result = PATCHRecord(updateMessage, recordType, Id);
            return result;
        }
        #endregion
        #region POST methods
        /// <summary>
        /// Posts new values to specified record using salesforce REST API. full update message is constructed from public methods
        /// </summary>
        /// <param name="createMessage">message to append to http request header</param>
        /// <param name="recordType">record type to post</param>
        /// <returns>retursn response message from REST API</returns>
        private string POSTRecord(string createMessage, string recordType, string recordId)
        {
            HttpContent contentCreate = new StringContent(createMessage, Encoding.UTF8, "application/xml");
            string uri = $"{serviceUrl}{apiEndpoint}sobjects/{recordType}/{recordId}?_HttpMethod=POST";
            HttpRequestMessage requestCreate = new HttpRequestMessage(HttpMethod.Post, uri);
            requestCreate.Headers.Add("Authorization", "Bearer " + authToken);
            requestCreate.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            requestCreate.Content = contentCreate;

            HttpResponseMessage response = client.SendAsync(requestCreate).Result;
            return response.Content.ReadAsStringAsync().Result;

        }
        /// <summary>
        /// constructs full updateMessage to post new field values to salesforce's REST API.
        /// </summary>
        /// <param name="Id">Id field of object to post</param>
        /// <param name="recordType">Object type to post</param>
        /// <param name="field">field name to post new value to</param>
        /// <param name="fieldValue">field value to post to given field</param>
        /// <returns>returns response message from the REST API</returns>
        public string POSTValue(string recordType, string Id, string field, string fieldValue)
        {
            string updateMessage = $"<root>" +
                $"<{field}>{fieldValue}</{field}" +
                $"</root>";

            string result = POSTRecord(updateMessage, recordType, Id);
            return result;
        }
        #endregion
        #region helper methods
        /// <summary>
        /// helper method converts HTTP response messages to a parsed string
        /// </summary>
        /// <param name="response">http response to parse as string</param>
        /// <returns>HTTP response message as a string</returns>
        private string HTTPResponseToString(HttpResponseMessage response)
        {
            JObject temp = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            String result = temp.ToString();
            return result;
        }
        /// <summary>
        /// Returns a string list containing the field value selectors used in the SOQL query.
        /// </summary>
        /// <param name="queryMessage">message to parse query selectors from</param>
        /// <returns>Returns a string list containing the field value selectors used in the SOQL query.</returns>
        /// <exception cref="Exception">throws exception when message provided in the args is not a valid SOQL query</exception>
        private List<String> getSObjectColumn(String queryMessage)
        {
            var qm = queryMessage.Split(" ");
            int start = 0;
            int end = 0;
            List<String> result = new List<String>();
            for (int i = 0; i < qm.Length; i++)
            {
                if (qm[i] == "SELECT")
                {
                    start = i + 1; //set start to the first value after the "SELECT" string
                }
                if (qm[i] == "FROM")
                {
                    end = i - 1;//set end value to the value in frnt of the "FROM" string
                    break;
                }
            }
            if (start != 0 & end != 0)
            {
                while (start <= end) //add all values to list between SELECT and FROM
                {
                    String c = qm[start].Replace(",", "");
                    c.Replace(" ", "");
                    result.Add(c);
                    start++;
                }
                return result;
            }
            else
            {
                throw new Exception($"ERROR: no SObject columns found in query message: {queryMessage}");
            }
        }
        #endregion
    }
}
