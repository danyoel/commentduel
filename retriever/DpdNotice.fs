module DPDNotice

open FSharp.Data
open FSharp.Data.TypeProviders
open System
open System.Text.RegularExpressions
open System.Net.Http
open System.Threading

type _RssFeed = XmlProvider<"http://web6.seattle.gov/dpd/luib/RSSAllAreas.aspx">
type Notice = { projectId: int; endDate: DateTime }


let getCommentEndDate (html: string) = 
    let start = html.IndexOf("<span id=\"cph_lblCommentsDate\"")
    if start = -1 then
        None
    else
        let start = html.IndexOf('>', start) + 1
        if start = 0 then
            None
        else
            let endIdx = html.IndexOf('<', start)
            if endIdx = 0 then
                None
            else
                match DateTime.TryParse(html.Substring(start, endIdx - start)) with
                | (true, d) -> Some d
                | _ -> None

 
let getProjectNo (item: _RssFeed.Item) = 
    let m = Regex.Match(item.Description, @"\(Project #(\d+)\)")
    if m.Success then
        Some (int (m.Groups.[1].Value))
    else
        None


let fetchContent (uri: string) = 
    let client = new HttpClient()
    client.GetStringAsync(uri) |> Async.AwaitTask |> Async.RunSynchronously


let getNotices () = 
    _RssFeed.Load("http://web6.seattle.gov/dpd/luib/rssallareas.aspx").Channel.Items
    |> Seq.filter (fun item -> item.Title.Contains("Application"))
    |> Seq.choose (fun item -> 
        let p = getProjectNo item
        let html = fetchContent item.Link
        let d = getCommentEndDate html
        Thread.
        match (p, d) with
        | (Some id, Some date) -> Some { projectId = id; endDate = date }
        | _ -> None)


(*
Sample RSS entry:

<item>
    <title> North/Northwest - Application (02/27/2017) 7359 24TH AVE NW </title>
    <link>http://web6.seattle.gov/dpd/luib/Notice.aspx?id=24559</link>
    <description>7359 24TH AVE NW (Project #3026871)</description>
</item>
*)
