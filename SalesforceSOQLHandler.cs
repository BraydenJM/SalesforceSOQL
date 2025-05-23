﻿using System.Text;
using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Azure;
using Azure.Core;
using System.Security.AccessControl;
using System.Net.Http.Json;
using Newtonsoft.Json;
using System;
using System.Security.Cryptography;

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
        /// Constructor for the SOQLQuery class
        /// </summary>
        /// <param name="azVault">Azure secret key vault containing parameters</param>
        /// <param name="loginEndpoint">login end point, ex: "https://login.salesforce.com/services/oauth2/token"</param>
        /// <param name="apiEndpoint">api endpoint, ex: "/services/data/vXX.X/"</param>
        /// <param name="serviceUrl">service URL ex: "https://sample.my.salesforce.com/"</param>
        /// <param name="oAuthEndpoint">OAUTH endpoint URL, ex: "https://sample.my.salesforce.com/services/oauth2/token"</param>
        /// <param name="username">salesforce login username</param>
        /// <param name="password">salesforce login user password, must also contain password token</param>
        /// <param name="token">secret token used for logging into API services. Typically sent once in the users email</param>
        /// <param name="consumerKey">salesforce API consumer key</param>
        /// <param name="consumerSecret">API secret</param>
        public SalesforceSOQLHandler(SecretClient azVault, string loginEndpoint, string apiEndpoint, string serviceUrl, string oAuthEndpoint,
                                    string username, string password, string token, string consumerKey, string consumerSecret)
        {
            KeyVaultSecret sec = azVault.GetSecret(loginEndpoint);
            this.loginEndpoint = sec.Value;
            sec = azVault.GetSecret(apiEndpoint);
            this.apiEndpoint = sec.Value;
            sec = azVault.GetSecret(serviceUrl);
            this.serviceUrl = sec.Value;
            sec = azVault.GetSecret(oAuthEndpoint);
            this.oAuthEndpoint = sec.Value;
            sec = azVault.GetSecret(username);
            this.username = sec.Value;
            sec = azVault.GetSecret(password);
            this.password = sec.Value;
            sec = azVault.GetSecret(token);
            this.token = sec.Value;
            sec = azVault.GetSecret(consumerKey);
            this.consumerKey = sec.Value;
            sec = azVault.GetSecret(consumerSecret);
            this.consumerSecret = sec.Value;
            this.client = new HttpClient();
            getAuthToken();
        }
        /// <summary>
        /// Constructor for the SOQLQuery class
        /// </summary>
        /// <param name="azVault">Azure secret key vault containing parameters</param>
        /// <param name="loginEndpoint">login end point, ex: "https://login.salesforce.com/services/oauth2/token"</param>
        /// <param name="apiEndpoint">api endpoint, ex: "/services/data/vXX.X/"</param>
        /// <param name="serviceUrl">service URL ex: "https://sample.my.salesforce.com/"</param>
        /// <param name="username">salesforce login username</param>
        /// <param name="password">salesforce login user password, must also contain password token</param>
        /// <param name="token">secret token used for logging into API services. Typically sent once in the users email</param>
        /// <param name="consumerKey">salesforce API consumer key</param>
        /// <param name="consumerSecret">API secret</param>
        public SalesforceSOQLHandler(SecretClient azVault, string loginEndpoint, string apiEndpoint, string serviceUrl,
                                    string username, string password, string token, string consumerKey, string consumerSecret)
        {
            KeyVaultSecret sec = azVault.GetSecret(loginEndpoint);
            this.loginEndpoint = sec.Value;
            sec = azVault.GetSecret(apiEndpoint);
            this.apiEndpoint = sec.Value;
            sec = azVault.GetSecret(serviceUrl);
            this.serviceUrl = sec.Value;
            sec = azVault.GetSecret(username);
            this.username = sec.Value;
            sec = azVault.GetSecret(password);
            this.password = sec.Value;
            sec = azVault.GetSecret(token);
            this.token = sec.Value;
            sec = azVault.GetSecret(consumerKey);
            this.consumerKey = sec.Value;
            sec = azVault.GetSecret(consumerSecret);
            this.consumerSecret = sec.Value;
            this.client = new HttpClient();
            getAuthToken();
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
            Console.WriteLine($"Attempting connection to endpoint: {this.loginEndpoint}\n" +
                $"Sending connection request as: {this.username}");
            HttpResponseMessage message = client.PostAsync(loginEndpoint, content).Result;
            string response = message.Content.ReadAsStringAsync().Result;
            JObject obj = JObject.Parse(response);

            if ((string)obj["access_token"] != null)
            {
                this.authToken = (string)obj["access_token"];
                Console.WriteLine($"Auth Token recieved from HTTP request.\n" +
                    $" Sent request as: {this.username}\n" +
                    $"API Id: {this.consumerKey}\n" +
                    $"Server status code: {message.StatusCode.ToString()}");
            }
            else
            {
                throw new Exception("HttpResponseMessage returned NULL for access_token parameter.\n" +
                    $"Server status code: {message.StatusCode.ToString()}\n" +
                    $"Request headers sent: {message.Headers}\n" +
                    $"Server response: {message.ReasonPhrase}");
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
        /// Method only sends the raw string in a HTTP message to server and returns the result. No other logic is used.
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
        public List<List<string>> getStringListFromQuery(string queryMessage)
        {
            JObject temp = queryRecordJObject(queryMessage);//sends SOQL query to salesforce server

            List<string> cols = getSObjectColumn(queryMessage);//outside list generated based on selected values in query.
            List<List<string>> result = new List<List<string>>();
            foreach (string col in cols)
            {
                result.Add(new List<string> { col });
            }
            if (temp.HasValues == true)
            {
                JToken values = temp.SelectToken("records");
                foreach (JToken token in values.Children())
                {
                    foreach (List<string> column in result)
                    {
                        JToken childValue = token.SelectToken(column[0]);
                        if (childValue != null)
                        {
                            string value = childValue.Value<string>();
                            if (value != null && !value.Equals("null", StringComparison.OrdinalIgnoreCase))
                            {
                                column.Add(value);
                            }
                            else
                            {
                                column.Add("");
                            }

                        }
                        else
                        {
                            column.Add("");
                        }
                    }
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
        public string PATCHRecord(string updateMessage, string recordType, string recordId)
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
        /// Posts new values to specified record using salesforce REST API. full update message is constructed from public methods
        /// </summary>
        /// <param name="jsonData">Object with parmeters to create in salesforce. Must be deserializable by newtsonsoft JSOn</param>
        /// <param name="recordType">record type to post</param>
        /// <returns>return response message from REST API</returns>
        public Object PATCHRecordJSon(Object jsonData, string recordType, string recordId)
        {

            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            string uri = $"{serviceUrl}{apiEndpoint}sobjects/{recordType}/{recordId}?_HttpMethod=PATCH";
            string JSON = JsonConvert.SerializeObject(jsonData);
            HttpContent contentCreate = new StringContent(JSON, Encoding.UTF8, "application/json");
            HttpRequestMessage requestCreate = new HttpRequestMessage(HttpMethod.Patch, uri);
            requestCreate.Headers.Add("Authorization", "Bearer " + authToken);
            requestCreate.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            requestCreate.Content = contentCreate;

            HttpResponseMessage response = client.Send(requestCreate);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return (Object)JsonConvert.DeserializeObject<Object>(response.Content.ReadAsStringAsync().Result);
            }
            else
            {
                throw new Exception(response.StatusCode.ToString());
            }
        }
        /// <summary>
        /// Posts new values to specified record using salesforce REST API. full update message is constructed from public methods
        /// </summary>
        /// <param name="jsonData">Dictionary with parameter key value pair</param>
        /// <param name="recordType">record type to post</param>
        /// <returns>return response message from REST API</returns>
        public string PATCHRecordJSon(Dictionary<string, string> jsonData, string recordType, string recordId)
        {

            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            string uri = $"{serviceUrl}{apiEndpoint}sobjects/{recordType}/{recordId}?_HttpMethod=PATCH";
            string JSON = JsonConvert.SerializeObject(jsonData);
            HttpContent contentCreate = new StringContent(JSON, Encoding.UTF8, "application/json");
            HttpRequestMessage requestCreate = new HttpRequestMessage(HttpMethod.Patch, uri);
            requestCreate.Headers.Add("Authorization", "Bearer " + authToken);
            requestCreate.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            requestCreate.Content = contentCreate;

            HttpResponseMessage response = client.Send(requestCreate);
            return response.StatusCode.ToString();
        }
        /// <summary>
        /// Posts new values to specified record using salesforce REST API. full update message is constructed from public methods
        /// </summary>
        /// <param name="jsonData">Formatted JSON string with fields and values</param>
        /// <param name="recordType">record type to post</param>
        /// <returns>return response message from REST API</returns>
        public string PATCHRecordJSon(string jsonData, string recordType, string recordId)
        {
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            string uri = $"{serviceUrl}{apiEndpoint}sobjects/{recordType}/{recordId}?_HttpMethod=PATCH";
            HttpContent contentCreate = new StringContent(jsonData, Encoding.UTF8, "application/json");
            HttpRequestMessage requestCreate = new HttpRequestMessage(HttpMethod.Patch, uri);
            requestCreate.Headers.Add("Authorization", "Bearer " + authToken);
            requestCreate.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            requestCreate.Content = contentCreate;

            HttpResponseMessage response = client.Send(requestCreate);
            return response.StatusCode.ToString();
        }
        /// <summary>
        /// constructs full updateMessage to patch new field value to salesforce's REST API.
        /// </summary>
        /// <param name="Id">Id field of object to patch</param>
        /// <param name="recordType">Object type to patch</param>
        /// <param name="field">field name to patch new value to</param>
        /// <param name="fieldValue">field value to patch to given field</param>
        /// <returns>returns response message from the REST API</returns>
        public string PATCHValue(string recordType, string Id, string field, string fieldValue)
        {
            string updateMessage = $"<root>" +
                $"<{field}>{fieldValue}</{field}>" +
                $"</root>";

            string result = PATCHRecord(updateMessage, recordType, Id);
            return result;
        }
        /// <summary>
        /// constructs full updateMessage to patch new list of field values to salesforce's REST API.
        /// </summary>
        /// <param name="Id">Id field of object to patch</param>
        /// <param name="recordType">Object type to patch</param>
        /// <param name="field">field name to patch new value to</param>
        /// <param name="fieldValue">field value to patch to given field</param>
        /// <exception cref="Exception">throws exeption when field and fieldValue lists are not of equal size
        /// <returns>returns response message from the REST API</returns>
        public string PATCHValues(string recordType, string Id, List<string> field, List<string> fieldValue)
        {
            if(field.Count == fieldValue.Count)
            {
                string updateMessage = "<root>";
                for(int i = 0; i < fieldValue.Count; i++)
                {
                    if (fieldValue[i] != null)
                    {
                        updateMessage += $"<{field[i]}>{fieldValue[i]}</{field[i]}>";
                    }
                }
                updateMessage += "</root>";

                return PATCHRecord(updateMessage, recordType, Id);
            }
            else
            {
                throw new Exception("ERROR: field and field value lists must be of equal size");
            }
        }
        /// <summary>
        /// constructs full updateMessage to patch new list of field values to salesforce's REST API.
        /// </summary>
        /// <param name="Id">Id field of object to patch</param>
        /// <param name="recordType">Object type to patch</param>
        /// <param name="field">field name to patch new value to</param>
        /// <param name="fieldValue">field value to patch to given field</param>
        /// <exception cref="Exception">throws exeption when field and fieldValue lists are not of equal size
        /// <returns>returns response message from the REST API</returns>
        public string PATCHValuesJson(string recordType, string Id, List<string> field, List<string> fieldValue)
        {
            Dictionary<string, string> recordValues = new Dictionary<string, string>();
            for (int i = 0; i < field.Count; i++)
            {
                recordValues.Add(field[i], fieldValue[i]);
            }
            return PATCHRecordJSon(recordValues, recordType, Id);
        }
        /// <summary>
        /// constructs full updateMessage to patch new list of field values to salesforce's REST API.
        /// </summary>
        /// <param name="Id">Id field of object to patch</param>
        /// <param name="recordType">Object type to patch</param>
        /// <param name="field">field name to patch new value to</param>
        /// <param name="fieldValue">field value to patch to given field</param>
        /// <exception cref="Exception">throws exeption when field and fieldValue lists are not of equal size
        /// <returns>returns response message from the REST API</returns>
        public string PATCHValuesJson(string recordType, string Id, List<List<string>> values)
        {
            if(values.Count > 1)
            {
                if (values[0].Count != values[1].Count)
                {
                    return "Invalid 2D list passed to method. All columns must be of equal length";
                }
            }
            List<string> fields = new List<string>();
            for(int i = 0; i < values.Count; i++)
            {
                fields.Add(values[i][0]);
            }
            for(int i = 1; i < values[0].Count; i++)
            {
                Dictionary<string, string> fieldValues = new Dictionary<string, string>();
                for(int k = 0; k < fields.Count; k++)
                {
                    fieldValues.Add(fields[k], values[k][i]);
                }
                PATCHRecordJSon(fieldValues, recordType, Id);
            }
            return "Operation complete. Still haven't added a proper response handling method";
        }
        #endregion
        #region POST methods
        /// <summary>
        /// Posts new values to specified record using salesforce REST API. full update message is constructed from public methods
        /// </summary>
        /// <param name="createMessage">message to append to http request header</param>
        /// <param name="recordType">record type to post</param>
        /// <returns>return response message from REST API</returns>
        public string POSTRecord(string createMessage, string recordType)
        {
            HttpContent contentCreate = new StringContent(createMessage, Encoding.UTF8, "application/xml");
            string uri = $"{serviceUrl}{apiEndpoint}sobjects/{recordType}";
            HttpRequestMessage requestCreate = new HttpRequestMessage(HttpMethod.Post, uri);
            requestCreate.Headers.Add("Authorization", "Bearer " + authToken);
            requestCreate.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            requestCreate.Content = contentCreate;

            HttpResponseMessage response = client.SendAsync(requestCreate).Result;
            return response.Content.ReadAsStringAsync().Result;

        }
        /// <summary>
        /// Posts new values to specified record using salesforce REST API. full update message is constructed from public methods
        /// </summary>
        /// <param name="jsonData">Object with parmeter to create in salesforce</param>
        /// <param name="recordType">record type to post</param>
        /// <returns>return response message from REST API</returns>
        public Object POSTRecordJSon(Object jsonData, string recordType)
        {
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            string uri = $"{serviceUrl}{apiEndpoint}sobjects/{recordType}";
            string JSON = JsonConvert.SerializeObject(jsonData);
            HttpContent contentCreate = new StringContent(JSON, Encoding.UTF8, "application/json");
            HttpRequestMessage requestCreate = new HttpRequestMessage(HttpMethod.Post, uri);
            requestCreate.Headers.Add("Authorization", "Bearer " + authToken);
            requestCreate.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            requestCreate.Content = contentCreate;

            HttpResponseMessage response = client.Send(requestCreate);
            if(response.StatusCode == HttpStatusCode.OK)
            {
                return (Object)JsonConvert.DeserializeObject<Object>(response.Content.ReadAsStringAsync().Result);
            }
            else
            {
                throw new Exception(response.StatusCode.ToString());
            }
        }
        /// <summary>
        /// Posts new values to specified record using salesforce REST API. full update message is constructed from public methods
        /// </summary>
        /// <param name="JsonData">message to append to http request header</param>
        /// <param name="recordType">record type to post</param>
        /// <returns>return response message from REST API</returns>
        public string POSTRecordJSon(string jsonData, string recordType)
        {
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            string uri = $"{serviceUrl}{apiEndpoint}sobjects/{recordType}";
            HttpContent contentCreate = new StringContent(jsonData, Encoding.UTF8, "application/json");
            HttpRequestMessage requestCreate = new HttpRequestMessage(HttpMethod.Post, uri);
            requestCreate.Headers.Add("Authorization", "Bearer " + authToken);
            requestCreate.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            requestCreate.Content = contentCreate;

            HttpResponseMessage response = client.Send(requestCreate);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                throw new Exception(response.StatusCode.ToString());
            }
        }
        /// <summary>
        /// constructs full updateMessage to post new field values to salesforce's REST API.
        /// </summary>
        /// <param name="Id">Id field of object to post</param>
        /// <param name="recordType">Object type to post</param>
        /// <param name="field">field name to post new value to</param>
        /// <param name="fieldValue">field value to post to given field</param>
        /// <returns>returns response message from the REST API</returns>
        public string POSTValue(string recordType, string field, string fieldValue)
        {
            string updateMessage = $"<root>" +
                $"<{field}>{fieldValue}</{field}>" +
                $"</root>";

            string result = POSTRecord(updateMessage, recordType);
            return result;
        }
        /// <summary>
        /// constructs full updateMessage to post new list of field values to salesforce's REST API.
        /// </summary>
        /// <param name="Id">Id field of object to patch</param>
        /// <param name="recordType">Object type to patch</param>
        /// <param name="field">field name to patch new value to</param>
        /// <param name="fieldValue">field value to patch to given field</param>
        /// <exception cref="Exception">throws exeption when field and fieldValue lists are not of equal size
        /// <returns>returns response message from the REST API</returns>
        public string POSTValues(string recordType, List<string> field, List<string> fieldValue)
        {
            if (field.Count == fieldValue.Count)
            {
                string updateMessage = "<root>";
                for (int i = 0; i < fieldValue.Count; i++)
                {
                    updateMessage += $"<{field}>{fieldValue}</{field}>";
                }
                updateMessage += "</root>";

                string result = POSTRecord(updateMessage, recordType);
                return result;
            }
            else
            {
                throw new Exception("ERROR: field and field value lists must be of equal size");
            }
        }
        /// <summary>
        /// Creates a new record of the given record type with field values provided in the 2d list. Each value under the header value will
        /// create a new Salesforce object the given string value. 
        /// </summary>
        /// <param name="recordType">Name of the object to create.</param>
        /// <param name="values">2n list with field names and values to use to create each new object</param>
        /// <returns>returns HTTP response message from the generated SOQL query</returns>
        public string POST2DListValues(string recordType, List<List<string>> values)
        {
            string result = "";
            //recieved list uses first row as header values
            if (values[0].Count > values.Count)
            {
                for(int i = 0; i <values.Count; i++)
                {
                    string parameters = "<root>";
                    for(int k = 1; k < values[i].Count; k++)
                    {
                        parameters += $"<{values[i][0]}><{values[i][k]}></{values[i][0]}>";
                    }
                    parameters += "</root>";
                    result += "\n" + POSTRecord(parameters, recordType);
                }
            }
            //recived list uses first col as header values
            else
            {
                for(int i = 0; i < values[0].Count; i++)
                {
                    string parameters = "<root>";
                    for(int k = 1; k < values.Count; i++)
                    {
                        parameters += $"<{values[0][i]}><{values[k][i]}></{values[0][i]}>";
                    }
                    parameters += "</root>";
                    result += "\n" + POSTRecord(parameters, recordType);
                }
            }
            return result;
        }
        /// <summary>
        /// Generates a POST query from a keyvalue pair. Method uses the key to create the objects field name (Use the fields API name in Salesforce)
        /// and the value to insert the fields value. The record type corrisponds with the SObject name as shown in the URL. For example; a Task object
        /// would be displayed as *yourOrgName*.lightnng.force.com/r/Task when viewing the task object.
        /// </summary>
        /// <param name="objectParameters">Keys are used for object field API names, Values insert the given value into the field</param>
        /// <param name="recordType">Name of the object to create.</param>
        /// <returns>returns HTTP response message from the generated SOQL query</returns>
        public string createObject(List<KeyValuePair<string, string>> objectParameters, string recordType)
        {
            string parameters = "<root>";
            foreach(KeyValuePair<string, string> parameter in objectParameters)
            {
                parameters += $"<{parameter.Key}>{parameter.Value}</{parameter.Key}>";
            }
            parameters += "</root>";
            string result = POSTRecord(parameters, recordType);
            return result;
        }
        #endregion
        #region DELETE methods
        #endregion
        #region helper methods
        /// <summary>
        /// helper method converts HTTP response messages to a parsed string
        /// </summary>
        /// <param name="response">http response to parse as string</param>
        /// <returns>HTTP response message as a string</returns>
        private string HTTPResponseToString(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode == true)
            {
                JObject temp = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                return temp.ToString();
            }
            else
            {
                return response.StatusCode.ToString();
            }
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
