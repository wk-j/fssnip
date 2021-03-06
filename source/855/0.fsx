open System

[<AutoOpen>]
module AsyncEx = 
    type internal SuccessException<'T>(value : 'T) =
        inherit Exception()
        member self.Value = value

    type Microsoft.FSharp.Control.Async with
  
      static member Choice<'T>(workflows : seq<Async<'T option>>) : Async<'T option> =
        async {
            try
                let! _ = 
                    workflows
                    |> Seq.map (fun workflow -> async { 
                                                    let! optionValue = workflow 
                                                    match optionValue with 
                                                    | None -> return None
                                                    | Some v -> return raise <| new SuccessException<'T>(v)
                                               })
                    |> Async.Parallel

                return None
            with 
            | :? SuccessException<'T> as ex -> return Some ex.Value
        }

// Examples
let delay n f = 
    async {
        for i in [1..n] do
            do! Async.Sleep(1000) 
            printfn "%d %d" i n
        return f n
    }

Async.Choice [ delay 10 (fun n -> Some n); delay 20 (fun n -> Some n)]
|> Async.RunSynchronously // Some 10

Async.Choice [ delay 10 (fun n -> None); delay 20 (fun n -> Some n)]
|> Async.RunSynchronously // Some 20

Async.Choice [ delay 10 (fun n -> raise <| new Exception("oups") ); delay 20 (fun n -> Some n)]
|> Async.RunSynchronously // Exception("oups")

Async.Choice [ delay 10 (fun n -> Some n ); delay 20 (fun n -> raise <| new Exception("oups"))]
|> Async.RunSynchronously // Some 10

Async.Choice<int>[ delay 10 (fun n -> None); delay 20 (fun n -> raise <| new Exception("oups"))]
|> Async.RunSynchronously // Exception("oups")


Async.Choice<int>[ delay 10 (fun n -> raise <| new Exception("oups1")); delay 20 (fun n -> raise <| new Exception("oups2"))]
|> Async.RunSynchronously // Exception("oups1")

Async.Choice<int>[ delay 10 (fun n -> None); delay 20 (fun n -> None)]
|> Async.RunSynchronously // None