namespace Trik
open System
open System.Net
open System.Net.Sockets
open System.Net.NetworkInformation
open System.Text


module server = 
    type ConsoleEvent =
    | Led of LedColor

    let Parse (x: string) : LedColor =
        match x with
        |"red" -> LedColor.Red
        |"green" -> LedColor.Green
        |"orange" -> LedColor.Orange
        |"off" -> LedColor.Off
        | _ -> LedColor.Off // временная заглушка
    type TCPServ(?port) =
        let port = defaultArg port 2000
        let obs_src = new Event<ConsoleEvent>()
        let obs = obs_src.Publish
        let obsNext = obs_src.Trigger

        let mutable working = false
        let messageBuf = Array.create 1024 (byte 0)
        let handleRequest (req: String) = 
            match req.Split([|' '|]) with    
            | [|"led"; x|] -> ConsoleEvent.Led (x |> Parse) |> obsNext
            | _ -> () 

        let rec clientLoop(client: TcpClient) = async {
            if client.Connected then
                try
                let! count = client.GetStream().AsyncRead(messageBuf, 0, messageBuf.Length)
                let msg = Encoding.ASCII.GetString(messageBuf, 0, count)
                msg |> handleRequest
                return! clientLoop(client)
                with
                | _ -> client.Close()
                       
        }

        let server = async {
            //let listener = new TcpListener(IPAddress.Any, port)
            let ip = Dns.GetHostAddresses(Dns.GetHostName()).[0]
            let listener = new TcpListener(ip, port)

            listener.Start()
            let rec loop() = async {
                let client = listener.AcceptTcpClient()
                printfn "%s" "new connection established"
                try
                 if working then Async.Start(clientLoop client) else ()
                with
                |_ -> listener.Stop()
                      new TCPServ(port) |> ignore
                return! loop()
            }
            Async.Start( loop() )
        }
        
        do working <- true
        do printfn "%s" "TCP server initial"
        do Async.Start server
        member val Observable = obs
        interface IDisposable with
            member x.Dispose() = working <- false

