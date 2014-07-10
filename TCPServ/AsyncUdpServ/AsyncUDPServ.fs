namespace AsyncUdpServ
open System
open System.Net
open System.Net.Sockets
open System.Text
open System.Net.NetworkInformation
open System.Collections.Generic

type AsyncUdpServ(?port) = 

    let finishcommand = "complete"
    let requestIP = "requestIP"

    let NIC = NetworkInterface.GetAllNetworkInterfaces().[1] 
    let mac = NIC.GetPhysicalAddress().GetAddressBytes() 

    let port = defaultArg port 3000
    let mutable messageBuf = Array.create 1024 (byte 0)
    let mutable working = true

    let listener = new UdpClient(port)
    let listenerEndPoint = new IPEndPoint(IPAddress.Any, port)

    let sender = new UdpClient(port)
    let senderEndPoint = new IPEndPoint(IPAddress.Parse "192.168.0.255", port)
        

    let GetMessage() =
        messageBuf <- listener.Receive(ref listenerEndPoint)
        Encoding.ASCII.GetString(messageBuf, 0, messageBuf.Length)

    let sendMacAndIp() = 
        let NIC = NetworkInterface.GetAllNetworkInterfaces().[1] 
        let mac = NIC.GetPhysicalAddress().GetAddressBytes()
        let ip = Dns.GetHostAddresses(Dns.GetHostName()).[0].GetAddressBytes()
        let macAndIp = Array.append mac ip
        printfn "%s %A" "Sending macAndIp" macAndIp
        sender.Send(macAndIp, macAndIp.Length, senderEndPoint) |> ignore
        printfn "%s" "Sended"
        
    let rec loop() = async {
        if working then 
                        printfn "waiting message"
                        let msg = GetMessage()
                        printfn "%s recieved" msg
                        if (String.Compare(msg, requestIP, true) = 0) then sendMacAndIp()
                        else ()
        else ()
        return! loop()
    }

    do Async.Start (loop())

    interface IDisposable with
        member x.Dispose() = working <- false
        
