namespace Trik
module UDPServ = 
    open System
    open System.Net
    open System.Net.Sockets
    open System.Text
    open System.Net.NetworkInformation
    let finishcommand = "complete"
    type UDPServ(?port) =
        let port = defaultArg port 3000
        let mutable messageBuf = Array.create 1024 (byte 0)

        let listener = new UdpClient(port)
        let listenerEndPoint = new IPEndPoint(IPAddress.Any, port)

        let NIC = NetworkInterface.GetAllNetworkInterfaces().[1] // NetworkInterface, надеемся что на трике он один
        let mac = NIC.GetPhysicalAddress().GetAddressBytes() // посылать в строке или сразу этот?
        let ip = Dns.GetHostAddresses(Dns.GetHostName()).[0].GetAddressBytes()
        let macAndIp = Array.append mac ip
        let sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        let senderEndPoint = new IPEndPoint(IPAddress.Parse "192.168.0.255", port)

        let GetMessage() =
            messageBuf <- listener.Receive(ref listenerEndPoint)
            let msg = Encoding.ASCII.GetString(messageBuf, 0, messageBuf.Length)
            printfn "%s"  msg
            msg
        let complete() =
            let msg = GetMessage()
            String.Compare(msg, finishcommand, true) = 0
        let rec until (condition: bool) (action: unit) =
            action
            if condition then until condition action
            else ()
        // потом переписать так же с использованием UDPClient
        let sendMacAndIp() = 
            sender.SendTo(macAndIp, senderEndPoint) |> ignore
        do printfn "%A" mac
        do printfn "%A" ip
        do until (complete()) (sendMacAndIp())
        interface IDisposable with
            member x.Dispose() = ()
       

         


