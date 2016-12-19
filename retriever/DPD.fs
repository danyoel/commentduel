module DPD

open FSharp.Data
open FSharp.Data.TypeProviders
open System
open System.Text.RegularExpressions

type _Permits = JsonProvider<"https://data.seattle.gov/resource/uyyd-8gak.json"> //?permit_type=DESIGN+REVIEW+WITH+EDG">
type _Project = JsonProvider<"http://www.seattle.gov/dpddrive/jsonproxy.asp?url=http%3A%2F%2Fapp6.seattle.gov%2Fdpd%2Fws%2FMapAPI%2Fapi%2FapStatus%2F%3Fapno%3D3021177">
type _AddressDetail = JsonProvider<"http://www.seattle.gov/dpddrive/jsonproxy.asp?url=http%3A%2F%2Fapp6.seattle.gov%2Fdpd%2Fws%2FMapAPI%2Fapi%2FaddressDetail%2F%3Faddress%3D1501%2BNW%2B59TH%2BST%2B%26latitude%3D47.6714686%26longitude%3D-122.37649924">
type _Documents = JsonProvider<"http://www.seattle.gov/dpddrive/jsonproxy.asp?url=http%3A%2F%2Fapp6.seattle.gov%2Fdpd%2Fws%2FMapAPI%2Fapi%2FaddressDocument%2F%3FaddressKey%3D10086">

type Permit = _Permits.Root
type Project = _Project.Result
type Address = _AddressDetail.Result
type Document = _Documents.Document

type MasterProject = { retrieved: DateTimeOffset; project: Project; address: Address; documents: Document array }

let cache<'a> () = ()
    

let inline failOnFault response =
    let success = ( ^a : (member Success : bool) response)
    if not success then
        failwith "DPD call returned an unsuccessful payload"

let getProject (id: int) = //: Async<Project> =
    eprintfn "getting project"
    let uriBase = "http://www.seattle.gov/dpddrive/jsonproxy.asp?url=http%3A%2F%2Fapp6.seattle.gov%2Fdpd%2Fws%2FMapAPI%2Fapi%2FapStatus%2F%3Fapno%3D" 
    let id' = id |> string |> Uri.EscapeDataString
    let uri = uriBase + id'
    uri
    //eprintfn "%s" uri
    (*async {
        let! resp = _Project.AsyncLoad uri
        failOnFault resp
        return resp.Result
    }*)
    //_Project.AsyncLoad uri

let getAddressDetail address latitude longitude = 
    eprintfn "getting address detail"
    let query = sprintf "address=%s&latitude=%f&longitude=%f" address latitude longitude
    let uriBase = "http://www.seattle.gov/dpddrive/jsonproxy.asp?url=http%3A%2F%2Fapp6.seattle.gov%2Fdpd%2Fws%2FMapAPI%2Fapi%2FaddressDetail%2F%3F"
    let uri = uriBase + (Uri.EscapeDataString query)
    //eprintfn "%s" uri
    uri
    (*async {
        let! resp = _AddressDetail.AsyncLoad uri
        failOnFault resp
        return resp.Result
    }*)
    //_AddressDetail.AsyncLoad uri

let getDocuments (address: Address) =
    eprintfn "getting documents list"
    let uriBase = "http://www.seattle.gov/dpddrive/jsonproxy.asp?url=http%3A%2F%2Fapp6.seattle.gov%2Fdpd%2Fws%2FMapAPI%2Fapi%2FaddressDocument%2F%3FaddressKey%3D"
    let akey = address.AddressKey |> string |> Uri.EscapeDataString
    let uri = uriBase + akey
    uri
    //eprintfn "%s" uri
    (*async {
        let! resp = _Documents.AsyncLoad uri
        failOnFault resp
        return resp.Documents
    }*)
    //_Documents.AsyncLoad uri
    
/// <summary>Gets the document ID from a DPD document store URI</summary>
let extractDocId uri = 
    let m = Regex.Match(uri, "[&\?]id=(\d+)[&\?$]")
    if m.Success then
        Some (int (m.Groups.[1].Captures.[0].Value))
    else
        None