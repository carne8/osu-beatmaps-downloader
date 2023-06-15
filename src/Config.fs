module Config

open FSharp.Configuration

type private Config = YamlConfig<"config.yml">
let private config = Config()
config.Load(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "config.yml"))

let outputDir = config.outputDirectory
let userId = config.userId
let clientId = config.clientId
let clientSecret = config.clientSecret
let cookie = config.cookie
