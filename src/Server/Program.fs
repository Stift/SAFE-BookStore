﻿/// Server program entry point module.
module ServerCode.Program

open System
open System.IO

let GetEnvVar var = 
    match Environment.GetEnvironmentVariable(var) with
    | null -> None
    | value -> Some value

let getPortsOrDefault defaultVal = 
    match Environment.GetEnvironmentVariable("SUAVE_FABLE_PORT") with
    | null -> defaultVal
    | value -> value |> uint16

[<EntryPoint>]
let main args =
    try
        let args = Array.toList args
        let clientPath =
            match args with
            | clientPath:: _  when Directory.Exists clientPath -> clientPath
            | _ -> 
                // did we start from server folder?
                let devPath = Path.Combine("..","Client")
                if Directory.Exists devPath then devPath 
                else
                    // maybe we are in root of project?
                    let devPath = Path.Combine("src","Client")
                    if Directory.Exists devPath then devPath 
                    else @"./client"
            |> Path.GetFullPath

        let database =
            args
            |> List.tryFind(fun arg -> arg.StartsWith "AzureConnection=")
            |> Option.map(fun arg ->
                arg.Substring "AzureConnection=".Length
                |> ServerCode.Storage.AzureTable.AzureConnection
                |> Database.DatabaseType.AzureStorage)
            |> Option.defaultValue Database.DatabaseType.FileSystem

        let port = getPortsOrDefault 8085us
        WebServer.start database clientPath port
        0
    with
    | exn ->
        let color = Console.ForegroundColor
        Console.ForegroundColor <- System.ConsoleColor.Red
        Console.WriteLine(exn.Message)
        Console.ForegroundColor <- color
        1
