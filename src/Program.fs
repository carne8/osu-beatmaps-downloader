module App

// #r "nuget: Thoth.Json.Net, 11.0.0"
open Thoth.Json.Net
open System.Net.Http
open System.IO
open System.Threading.Tasks
open System.Collections.Generic

let client = new HttpClient()

let getAccessToken () =
    task {
        let url = "https://osu.ppy.sh/oauth/token"

        let req = new HttpRequestMessage(HttpMethod.Post, url)
        let collection = [
            KeyValuePair("client_id", Config.clientId |> string)
            KeyValuePair("client_secret", Config.clientSecret)
            KeyValuePair("grant_type", "client_credentials")
            KeyValuePair("scope", "public")
        ]

        req.Content <- new FormUrlEncodedContent(collection)

        let! res = client.SendAsync(req)
        let! rawContent = res.Content.ReadAsStringAsync()
        let content = Decode.Auto.fromString<{| access_token: string |}> rawContent

        match content with
        | Ok content -> return content.access_token
        | Error x ->
            printfn "%A: %s" res.StatusCode res.ReasonPhrase
            return failwith x

    }


let getBeatmapsPage (offset: int) =
    task {
        let url = sprintf "https://osu.ppy.sh/api/v2/users/%i/beatmapsets/most_played?limit=100&offset=%i" Config.userId offset
        let! token = getAccessToken()

        let req = new HttpRequestMessage(HttpMethod.Get, url)
        req.Headers.Add("Authorization", $"Bearer {token}")
        req.Headers.Add("cookie", Config.cookie);

        let! res = client.SendAsync(req)
        let! rawContent = res.Content.ReadAsStringAsync()
        let content = Decode.Auto.fromString<{| beatmap: {| beatmapset_id: int |} |}[]> rawContent

        match content with
        | Ok content -> return content |> Array.map (fun x -> x.beatmap.beatmapset_id)
        | Error x ->
            printfn "%A" rawContent
            return failwith x
    }

let getAllBeatmaps () =
    task {
        let beatmaps = new List<int>()

        let mutable finished = false

        while not finished do
            let! newBeatmaps = getBeatmapsPage beatmaps.Count
            beatmaps.AddRange(newBeatmaps)

            if newBeatmaps.Length = 0 then
                finished <- true

        printfn "Found %i beatmaps" beatmaps.Count

        return
            beatmaps
            |> Seq.toList
            |> List.distinct
    }

let getBeatmapBytes (id: int) =
    task {
        let client = new HttpClient();
        let request = new HttpRequestMessage(HttpMethod.Get, sprintf "https://osu.ppy.sh/beatmapsets/%i/download" id);
        request.Headers.Add("referer", sprintf "https://osu.ppy.sh/beatmapsets/%i" id);
        request.Headers.Add("cookie", Config.cookie);

        let! res = client.SendAsync(request)

        match res.IsSuccessStatusCode with
        | true -> return! res.Content.ReadAsByteArrayAsync()
        | false ->
            let! rawContent = res.Content.ReadAsStringAsync()
            let error =
                sprintf
                    "%s: %s\n%s"
                    (res.StatusCode.ToString())
                    res.ReasonPhrase
                    (rawContent
                     |> Seq.truncate 200
                     |> Seq.toArray
                     |> System.String)

            return failwith error
    }


let mutable total = 0.
let mutable saved = 0.

let saveBeatmap id =
    task {
        printfn "Downloading %i..." id

        let! bytes = getBeatmapBytes id
        do! File.WriteAllBytesAsync(sprintf "%s/%i.osz" Config.outputDir id, bytes)

        saved <- saved + 1.
        let percentage =
            saved / total * 100.
            |> float
            |> System.Math.Round
            |> int

        printfn "Saved %i ! %i/%i %i%%" id (saved |> int) (total |> int) percentage
        printfn ""

        do! Task.Delay 5000 // Wait between all call to avoid error 429
    }

[<EntryPoint>]
let main _ =
    let beatmaps = getAllBeatmaps().Result
    total <- beatmaps.Length

    printfn "Saving %i beatmaps" (total |> int)
    printfn ""

    Directory.CreateDirectory(Config.outputDir) |> ignore
    beatmaps |> List.iter (fun x -> saveBeatmap(x).Result)

    printfn "Done !"

    0