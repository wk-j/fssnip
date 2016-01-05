open System

#r "FSharp.PowerPack.Parallel.Seq";;
open Microsoft.FSharp.Collections

let md5 (plainText : string) =
    plainText,
     BitConverter.ToString(
      Security.Cryptography.MD5.Create().ComputeHash (
       Text.Encoding.ASCII.GetBytes plainText)).Replace("-","").ToLower()

(* perm code from
   http://stackoverflow.com/questions/4495597/combinations-and-permutations-in-f 

   Generates the cartesian outer product of a list of sequences LL *)
let rec outerProduct = function
  | []    -> Seq.singleton []
  | L::Ls -> L |> Seq.collect (fun x -> 
                  outerProduct Ls |> Seq.map (fun L -> x::L))

(* Generates all n-element combination from a list L *)
let getPermsWithRep n L = 
    List.replicate n L |> outerProduct  
 
let listToStr xs = 
   List.toArray xs |> fun c -> new string (c)

let crmd md5' (charset : string) n = 

  getPermsWithRep n (charset |> Seq.toList)
  |> PSeq.map (PSeq.toList >> listToStr >> md5) 

  |> Seq.filter (fun e -> snd e = md5')
  
(* ("c1a2bb1", "34a79dcbe2670a58abfa4d502ae0fe77") *)
let md = md5 "c1a2bb1"

crmd (snd md) "abc123" 7 