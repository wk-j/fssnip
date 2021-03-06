// This program is an optimizer that can generate optimal code
// for any bitwise function with up to 4 parameters.
// Only 65536 such functions exists so they can be precomputed
// and cached, given enough time and an efficient searcher.
// 
// This F# function was not optimised by the F# 3.0 compiler:
//   let booltest a b  = (a ||| b) &&& (~~~ (a &&& b))
// It generated this CIL code:
//   [Ldarg 0; Ldarg 1; Or; Ldarg 0; Ldarg 1; And; Not; And]
// Instead of the optimal code:
//   [Ldarg 0; Ldarg 1; Xor]

open System

type ops = Ldarg of int | And | Or | Xor | Not | Dup | Pop | Ones | Zeros

[<EntryPoint>]
let main argv =   
    
    /// Bit patterns for computing the truth table in parallel
    let args = [|0b1010101010101010; 0b1100110011001100;
                 0b1111000011110000; 0b1111111100000000|]

    /// Instructions understood and used by the optimiser
    let validinstructions = [|Ldarg 0; Ldarg 1; Ldarg 2; Ldarg 3;
                              And; Or; Xor; Dup; Pop; Not; Ones; Zeros|]


    /// Execute a single stack based instruction
    let exec = function
               | stk,Ldarg n   -> Some(args.[n]::stk)
               | x::y::stk,And -> Some((x &&& y)::stk)
               | x::y::stk,Or  -> Some((x ||| y)::stk)
               | x::y::stk,Xor -> Some((x ^^^ y)::stk)
               | x::stk,Not    -> Some((x ^^^ 0xffff)::stk)
               | x::stk,Dup    -> Some(x::x::stk)
               | x::stk,Pop    -> Some(stk)
               | stk,Ones      -> Some(0xffff::stk)
               | stk,Zeros     -> Some(0::stk)
               | _             -> None


    /// Calculate one truth table for each return value of a function
    let truthtables = 
        List.fold (fun acc op -> match acc with
                                 | None -> None
                                 | Some(xs) -> exec (xs,op)) (Some([]) )


    /// Increment a number in the form of a list of digits
    let rec increment max =
        function
        | x::xs when x = max -> 0::(increment max xs)
        | x::xs              -> (x + 1)::xs
        | _                  -> failwith "Empty list!"


    /// Create a list of all numbers of length and radix
    let makenumbers length radix = 
        let rec loop number =
            seq { yield number
                  yield! loop (increment (radix - 1) number)
            }

        loop (List.init length (fun _ -> 0))
        |> Seq.take (int (float radix ** float length))


    /// Populating the dictionary with the optimal functions,
    //  by testing the functions by increasing length from 1 to size
    //  the first function to generate a truth table is the shortest one.
    let createdictionary size =
        seq { 0 .. size }
        |> Seq.map (fun i -> (makenumbers i validinstructions.Length))
        |> Seq.concat
        |> Seq.map (List.map (fun n -> validinstructions.[n]))
        |> Seq.map (fun p -> (truthtables p) |> (Option.map (fun x -> (x,p))))
        |> Seq.choose id
        |> Seq.distinctBy fst
        |> Seq.fold (fun acc (a,b) -> Map.add a b acc) Map.empty 

    let optimal = createdictionary 5
    printfn "Number of optimal functions in dictionary: %A\n" optimal.Count


    /// Display results
    let show = 
        function
        | prog,Some(result) -> if optimal.ContainsKey result = true
                               then printfn " Source:  %A" prog
                                    printfn "Optimal:  %A\n" optimal.[result]
                               else printfn "Not found:  %A\n" prog
        | prog,None         -> printfn "Invalid:  %A" prog 


    // Functions to be optimized
    let progs = 
        [ [Ldarg 0; Ldarg 1; Or; Ldarg 0; Ldarg 1; And; Not; And]
          [Ldarg 0; Ldarg 0; Xor]
          [Ldarg 0; Pop]
          [Ldarg 0; Ldarg 1; And; Dup; Or]
          [Zeros; Not]
        ]

    List.map truthtables progs
    |> List.zip progs
    |> List.iter show  

    //Console.ReadKey() |> ignore
    0