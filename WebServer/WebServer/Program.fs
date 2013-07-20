let host = "http://localhost:8080/"
let msg = Array.create 128 0uy
let mutable reqs = 0

do
  let n = 8
  use server = new System.Net.HttpListener()
  server.Prefixes.Add host
  server.Start()
  let timer = System.Diagnostics.Stopwatch.StartNew()
  for i in 1..n do
    async { use client = new System.Net.WebClient()
            while timer.Elapsed.TotalSeconds < 10.0 do
              let! msg = client.AsyncDownloadString(System.Uri host)
              System.Threading.Interlocked.Increment &reqs
              |> ignore
            if i=1 then
              let rate = float reqs / timer.Elapsed.TotalSeconds
              printfn "%d requests in %f seconds => %f reqs/s"
                reqs timer.Elapsed.TotalSeconds rate }
    |> Async.Start
  while true do
    let context = server.GetContext()
    async { let response = context.Response
            response.ContentLength64 <- int64 msg.Length
            response.ContentEncoding <- System.Text.Encoding.ASCII
            response.ContentType <- "text/html"
            use stream = response.OutputStream
            do! stream.AsyncWrite(msg, 0, msg.Length) }
    |> Async.Start
