module xpdf

open System.Diagnostics
open System.IO

let extractText xpdfPath filename =
    let outfile = Path.ChangeExtension(filename, ".txt")
    let args = sprintf "-eol dos -table %s %s" filename outfile
    let pinfo = ProcessStartInfo(xpdfPath, args)
    pinfo.UseShellExecute <- false
    use proc = Process.Start(pinfo)
    proc.WaitForExit()
    outfile