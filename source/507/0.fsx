(*
http://4clojure.com/problem/92

Roman numerals are easy to recognize, but not everyone knows all the rules necessary to work with them. 
Write a function to parse a Roman-numeral string and return the number it represents. 

You can assume that the input will be well-formed, in upper-case, and follow the subtractive principle. 
You don't need to handle any numbers greater than MMMCMXCIX (3999), 
the largest number representable with ordinary letters.

(= 14 (__ "XIV"))

(= 827 (__ "DCCCXXVII"))

(= 3999 (__ "MMMCMXCIX"))

(= 48 (__ "XLVIII"))

*)


let numbers = Map.ofList [('I',1);('V',5);('X',10);('L',50);('C',100);('D',500);('M',1000)] 
let readromannumerals (s:string) =
  Array.foldBack(fun ele (prevalue,total) -> 
        let currentvalue = numbers.Item ele 
        if currentvalue >= prevalue then (currentvalue, total+currentvalue) 
        else (currentvalue ,total - currentvalue))
        (s.ToCharArray()) (0,0) 
        |> snd
 

readromannumerals "XIV";;
readromannumerals "DCCCXXVII";;
readromannumerals "MMMCMXCIX";;
readromannumerals "XLVIII";;