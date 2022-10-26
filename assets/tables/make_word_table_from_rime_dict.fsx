open System.IO

let separator = '\t'

let make (tablePath, outputName, (folder: string list -> _ -> _)) =
    printfn "%s -> %s" tablePath outputName
    use f = File.OpenText(tablePath)
    f.ReadToEnd().Split("\n")
    |> Array.where (fun i -> i.Length > 3 && i[1] = separator)
    |> Array.Parallel.map (fun i -> 
        let r = i.Split(separator)
        r[0], r[1])
    |> Array.groupBy (fun (i, _) -> i)
    |> Array.Parallel.collect (fun (i, j) -> j |> Seq.fold folder [] |> Seq.map (fun j -> $"{i}\t{j}") |> Array.ofSeq)
    |> String.concat "\n"
    |> fun s ->
        let ff = File.CreateText(outputName)
        ff.Write s
        ff.Flush()
        ff.Close()

List.iter make [
    "pinyin_simp.dict.yaml", "pinyin.tsv", fun state (_, item) -> item :: state
    "wubi86_jidian.dict.yaml", "wubi86.tsv", fun state (_, item) -> 
        match state |> List.tryFindIndex (fun i -> i.[0] = item.[0]) with
        | Some i ->
            let x = state[i]
            if item.Length > x.Length then item :: (state |> List.removeAt i)
            else state
        | None -> (item :: state)
]