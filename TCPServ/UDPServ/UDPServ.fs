namespace Trik
module UDPServ = 
    open System
    open System.Net
    open System.Net.Sockets
    open System.Text
    open System.Net.NetworkInformation
    let finishcommand = "complete"
    let requestIP = "requestIP"

    type UDPServ(?port) =
        let port = defaultArg port 3000
        let mutable messageBuf = Array.create 1024 (byte 0)

        let listener = new UdpClient(port)
        let listenerEndPoint = new IPEndPoint(IPAddress.Any, port)

        let NIC = NetworkInterface.GetAllNetworkInterfaces().[1] // NetworkInterface, надеемся что это то что надо на всех триках
        let mac = NIC.GetPhysicalAddress().GetAddressBytes() // посылать в строке или сразу этот?
        let ip = Dns.GetHostAddresses(Dns.GetHostName()).[0].GetAddressBytes()
        let macAndIp = Array.append mac ip
        let senderEndPoint = new IPEndPoint(IPAddress.Parse "192.168.0.255", port)
        let sender = new UdpClient(port)

        let GetMessage() =
            printfn "%s" "Waiting message"
            messageBuf <- listener.Receive(ref listenerEndPoint)
            let msg = Encoding.ASCII.GetString(messageBuf, 0, messageBuf.Length)
            printfn "%s"  msg
            msg

        let sendMacAndIp() = 
            printfn "%s %A" "Sending macAndIp" macAndIp
            sender.Send(macAndIp, macAndIp.Length, senderEndPoint) |> ignore
            printfn "%s" "Sended"
        let rec loop() =
            let msg = GetMessage()
            if (String.Compare(msg, finishcommand, true) = 0) then sender.Close()
                                                                   listener.Close()
                                                                   printfn "%s" "complete command recieve"
            elif (String.Compare(msg, requestIP, true) = 0) then sendMacAndIp()
                                                                 loop()
            else loop()
           
//        do printfn "%A" mac
//        do printfn "%A" ip
        do loop()
        

        interface IDisposable with
            member x.Dispose() = ()
       

         


