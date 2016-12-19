module CoreNLP

open FSharp.Data
open FSharp.Data.TypeProviders
open System.Net.Http

type SentimentResult = JsonProvider< @"C:\data\dpd\sentiment\sentiment-140612.json">

let nlpUri = "http://corenlp.westus2.cloudapp.azure.com:9000"

let annotate doc =
    let client = new HttpClient()
    let uri = nlpUri + "/?properties={\"annotators\":\"sentiment\",\"outputFormat\":\"json\"}"
    let content = new StringContent(doc)
    async {
        let! result = client.PostAsync(uri, content) |> Async.AwaitTask
        let! json = result.Content.ReadAsStringAsync() |> Async.AwaitTask
        return json
    }


let parseSentiment (file: string) =
    let docId = file.Substring(32, 6) // hack
    let sent = SentimentResult.Load file
    sent.Sentences
    |> Array.map (fun s -> (docId, s.Index, s.Sentiment, s.SentimentValue))