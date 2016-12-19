module retriever

open System
open System.IO
open Newtonsoft.Json
open DPD

(*let scrapeProject (delay: TimeSpan) pid =
    eprintfn "scraping %d" pid
    let master = 
        async {
            let! proj = getProject pid
            do! Async.Sleep (int (delay.TotalMilliseconds))
            let! addr = getAddressDetail (proj.Address.Display.ToUpper()) (float proj.Coordinate.Latitude) (float proj.Coordinate.Longitude)
            do! Async.Sleep (int (delay.TotalMilliseconds))
            let! docs = getDocuments addr
            return { retrieved = DateTimeOffset.UtcNow; project = proj; address = addr; documents = docs }
        } |> Async.RunSynchronously
    master*)

let cacheUrl uri filename =
    if File.Exists filename then
        filename
    else
        uri

let cacheResult filename (content: FSharp.Data.JsonValue) =
    if not (File.Exists filename) then
        use writer = new StreamWriter(filename)
        content.WriteTo(writer, FSharp.Data.JsonSaveOptions.None)

let scrapeProject (delay: TimeSpan) pid =
    async { 
        let fn = sprintf "project-%d.json" pid
        let uri = getProject pid
        let! proj = _Project.AsyncLoad uri
        cacheResult fn proj.JsonValue
        do! Async.Sleep (int (delay.TotalMilliseconds))
        
        let fn = sprintf "address-%d.json" pid
        let uri = getAddressDetail (proj.Result.Address.Display.ToUpper()) (float proj.Result.Coordinate.Latitude) (float proj.Result.Coordinate.Longitude)
        let! addr = _AddressDetail.AsyncLoad uri
        cacheResult fn addr.JsonValue
        do! Async.Sleep (int (delay.TotalMilliseconds))

        let fn = sprintf "documents-%d.json" pid
        let uri = getDocuments addr.Result
        let! docs = _Documents.AsyncLoad uri
        cacheResult fn docs.JsonValue
        do! Async.Sleep (int (delay.TotalMilliseconds))
    }

(*let saveProject (project: MasterProject) filename = 
    let json = JsonConvert.SerializeObject(project, Formatting.Indented)
    File.WriteAllText(filename, json)*)