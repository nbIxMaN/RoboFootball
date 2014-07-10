namespace Trik
open server
open Trik
open UDPServ
open System.Net.NetworkInformation// почистить
open AsyncUdpServ
module interpret = 
    [<EntryPoint>]

    let main _ = 
        Helpers.I2C.init "/dev/i2c-2" 0x48 1
        let AUDPs = new AsyncUdpServ()
        use TCPs = new TCPServ()
        use model = new Model()
        let led = model.Led
        use cmds = TCPs.Observable.Subscribe(fun x -> 
            match x with
            | Led x -> led.SetColor x
            )
        System.Console.ReadKey() |> ignore
        0
