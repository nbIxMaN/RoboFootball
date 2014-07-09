namespace Trik
open server
open Trik
open UDPServ
open System.Net.NetworkInformation// почистить
module interpret = 
    [<EntryPoint>]

    let main _ = 
        Helpers.I2C.init "/dev/i2c-2" 0x48 1
        use UDPs = new UDPServ()
        use model = new Model()
        use TCPs = new TCPServ()
        let led = model.Led
        use cmds = TCPs.Observable.Subscribe(fun x -> 
            match x with
            | Led x -> led.SetColor x
            )
        System.Console.ReadKey() |> ignore
        0
