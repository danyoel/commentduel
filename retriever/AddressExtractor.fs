module AddressExtractor

open System.Text.RegularExpressions

let addrs text = 
    Regex.Match(text, "")