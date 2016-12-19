module CognitiveServices

open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open System.Net.Http
open System.Net.Http.Headers

type Document = { id : string; text: string }
type DocumentRequest = { documents : Document array }

let getClient (apiKey: string) =
    let client = new HttpClient()
    client.BaseAddress <- new Uri("https://westus.api.cognitive.microsoft.com/")
    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client

let shortenTo limit (str: string) =
    if str.Length > limit then
        str.Substring(0, limit)
    else
        str
    

let textApiCall (uri: string) (client: HttpClient) (docs: Document array) =
    let shortDocs = docs 
    let content = 
        let json = JsonConvert.SerializeObject { documents = docs }
        new StringContent(json)
    content.Headers.ContentType <- new MediaTypeHeaderValue("application/json");
    async {
        let! response = client.PostAsync(uri, content) |> Async.AwaitTask
        let! json = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        return json
    }

let keyPhrases : HttpClient -> Document array -> Async<string> = 
    textApiCall "text/analytics/v2.0/keyPhrases"
let sentiment : HttpClient -> Document array -> Async<string> = 
    textApiCall "text/analytics/v2.0/sentiment"


