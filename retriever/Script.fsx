// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.

#r "System.Net.Http.dll"
#r @"c:\src\commentduel\master\retriever\packages\FSharp.Data.2.3.2\lib\net40\FSharp.Data.dll"
#r @"c:\src\commentduel\master\retriever\packages\FSharp.Data.TypeProviders.5.0.0.2\lib\net40\FSharp.Data.TypeProviders.dll"
#r @"c:\src\commentduel\master\retriever\packages\Newtonsoft.Json.9.0.1\lib\net45\Newtonsoft.Json.dll"

#load "CognitiveServices.fs"
#load "CoreNLP.fs"
#load "DPD.fs"
#load "Socrata.fs"
#load "xpdf.fs"
#load "Library1.fs"

open System
open System.IO
open System.Net
open CognitiveServices
open CoreNLP

let scrape id =
    eprintfn "%d" id
    let filename = sprintf "project-%d.json" id
    let delay = TimeSpan.FromSeconds 5.0
    async {
        if File.Exists filename then
            eprintfn "project %d already exists; skipping" id
        else
            try
                do! retriever.scrapeProject delay id
                //retriever.saveProject proj filename
            with
            | e -> eprintfn "ERROR: %s" (e.ToString())
            do! Async.Sleep (int delay.TotalMilliseconds)
    }


Socrata.getPermits () 
|> Async.RunSynchronously
|> Seq.filter (fun p -> p.Status <> "CANCELLED" && p.PermitType <> null && p.PermitType.Contains("DESIGN REVIEW WITH EDG"))
//|> Seq.iter (fun p -> printfn "%d" p.ApplicationPermitNumber)
|> Seq.map (fun p -> p.ApplicationPermitNumber)
|> Seq.sort
//|> Seq.skip 40
//|> Seq.take 60
|> Seq.iter (fun id -> scrape id |> Async.RunSynchronously)


let download (doc: DPD.Document) =
    let client = new WebClient()
    match DPD.extractDocId doc.DownloadLink with
    | Some id -> 
        let filename = sprintf @"c:\data\dpd\doc-%d.%s" id doc.FileExtension
        if File.Exists filename then
            eprintfn "document %s already exists; skipping" filename
            false
        else
            client.DownloadFile(doc.DownloadLink, filename)
            true
    | None -> 
        eprintfn "WARNING: couldn't find id in link %s" doc.DownloadLink
        false
    

let downloadDocs () = 
    Directory.EnumerateFiles(@"c:\data\dpd", "documents-*.json")
    |> Seq.map (fun file -> DPD._Documents.Load file)
    |> Seq.collect (fun doclist -> doclist.Documents)
    |> Seq.filter (fun doc -> doc.DisplayTitle.StartsWith("Public Comment"))
    |> Seq.iter (fun doc ->
        eprintfn "downloading \"%s\" from %s" doc.DisplayTitle doc.DownloadLink
        if download doc then
            System.Threading.Thread.Sleep 5000)

let extractText () =
    let extract = xpdf.extractText @"C:\utils\xpdfbin-win-3.04\bin64\pdftotext.exe"
    Directory.EnumerateFiles(@"c:\data\dpd", "doc-*.pdf")
    |> Seq.iter (fun f -> 
        eprintfn "extracting %s" f
        extract f |> ignore)

let fixup (text: string array) =
    let lines = 
        text
        |> Seq.filter (not << String.IsNullOrWhiteSpace)
        |> Seq.map (fun line -> line.Trim())
    String.Join("\n", lines)    
    

let fixupText () =
    Directory.EnumerateFiles(@"c:\data\dpd\documents\txt", "doc-*.txt")
    |> Seq.map (fun filename -> (filename, File.ReadAllLines(filename)))
    |> Seq.map (fun (filename, text) -> (filename, fixup text))
    |> Seq.iter (fun (filename, text) -> File.WriteAllText(filename, text))

(*
let runSentiment batchSize =
    let client = CognitiveServices.getClient "455d87e6819c43fcb7284efba4e56df9"
    let getId file = Path.GetFileNameWithoutExtension(file).Substring(4)
    let read file = File.ReadAllText file
    Directory.EnumerateFiles(@"c:\data\dpd\documents\txt", "doc-*.txt")
    //|> Seq.take 10
    |> Seq.map (fun file -> 
        let text = shortenTo 8192 (read file)
        { id = getId file; text = text })
    |> Seq.chunkBySize batchSize
    |> Seq.map (fun docs ->
        eprintfn "batch starts with %s" docs.[0].id
        async {
            let! result = sentiment client docs
            do! Async.Sleep 1000
            return result
        } |> Async.RunSynchronously
    )
    |> Seq.iteri (fun i result ->
        let outfile = sprintf "sentiment-batch-%03d.json" i
        File.WriteAllText(outfile, result)
    )
*)

let runSentiment () =
    let getId file = Path.GetFileNameWithoutExtension(file).Substring(4)
    Directory.EnumerateFiles(@"c:\data\dpd\documents\txt", "doc-*.txt")
    |> Seq.skip 100
    //|> Seq.take 100
    |> Seq.map (fun file -> 
        let text = File.ReadAllText file
        { id = getId file; text = text })
    |> Seq.iteri (fun i result ->
        async {
            if i % 20 = 0 then eprintfn "processed %d files" i
            let outfile = sprintf @"c:\data\dpd\sentiment-%s.json" result.id
            let! json = annotate result.text
            File.WriteAllText(outfile, json)
        } |> Async.RunSynchronously
    )

let sfile = new StreamWriter(@"c:\data\dpd\sentiment\all.csv")
Directory.EnumerateFiles(@"c:\data\dpd\sentiment", "sentiment-??????.json")
|> Seq.filter (fun fn -> not (fn.Contains("batch")))
|> Seq.collect parseSentiment
|> Seq.iter (fun (docid, idx, sent, sv) ->
    fprintfn sfile "%s,%d,%s,%d" docid idx sent sv)
sfile.Close()