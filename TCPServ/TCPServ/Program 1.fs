namespace Trik
open System
open System.Net
open System.Net.Sockets
open System.Reactive.Linq
open System.Text
open System.Collections.Generic

module server = 
    type ConsoleEvent =
    | Led of LedColor

    let Parse (x: string) : LedColor =
        match x with
        |"red" -> LedColor.Red
        |"green" -> LedColor.Green
        |"orange" -> LedColor.Orange
        |"off" -> LedColor.Off

    type TCPServ(?port) =
        let port = defaultArg port 2000
        let obs_src = new Event<ConsoleEvent>()
        let obs = obs_src.Publish
        let obsNext = obs_src.Trigger

        let mutable working = false
        let messageBuf = Array.create 1024 (byte 0)
        let handleRequest (req: String) = 
            match req.Split([|' '|]) with    // разбивает строку на подстроки, которые разделены пробелом
            | [|"led"; x|] -> ConsoleEvent.Led (x |> Parse) |> obsNext
            | _ -> printfn "%A" "unknown command"

        let rec clientLoop(client: TcpClient) = async {
            if client.Connected then
                let! count = client.GetStream().AsyncRead(messageBuf, 0, messageBuf.Length)
                let msg = Encoding.ASCII.GetString(messageBuf, 0, count)
                msg |> handleRequest
                return! clientLoop(client)
        }

        let server = async {
            let listener = new TcpListener(IPAddress.Any, port)
            listener.Start()
            printfn "%A" "Listen begin"
            let rec loop() = async {
                let client = listener.AcceptTcpClient()
                if working then Async.Start(clientLoop client) else ()
                return! loop()
            }
            Async.Start( loop() )
        }

        do working <- true
        do Async.Start server
        member val Observable = obs
        interface IDisposable with
            member x.Dispose() = working <- false


    //[<EntryPoint>]
    //let main argv = 
    //    
    //    0 
