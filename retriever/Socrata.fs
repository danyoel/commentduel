module Socrata

open System
open System.Net.Http
open System.IO
open FSharp.Data
open FSharp.Data.TypeProviders

//type _Permits = JsonProvider<"https://data.seattle.gov/resource/d27y-pibz.json">
type _Permits = JsonProvider<"https://data.seattle.gov/resource/uyyd-8gak.json"> //?permit_type=DESIGN+REVIEW+WITH+EDG">

let cachedStream uri cacheFile =
    if File.Exists cacheFile then
        async {
            eprintfn "using cached file %s" cacheFile
            return File.OpenRead cacheFile
        }
    else
        let client = new HttpClient()
        async {
            use! stream = client.GetStreamAsync(Uri(uri)) |> Async.AwaitTask
            use output = File.OpenWrite cacheFile
            stream.CopyTo output
            output.Close()
            return File.OpenRead cacheFile
        }


let getPermits () =
    async {
        //use! stream = cachedStream "https://data.seattle.gov/resource/d27y-pibz.json" cacheFile
        use! stream = cachedStream "https://data.seattle.gov/resource/uyyd-8gak.json" "uyyd-8gak.json"
        let permits = _Permits.Load stream
        return permits
    }