open System
open System.IO
open System.Xml
open System.Text
open System.Net
open System.Globalization

let makeUrl symbol (dfrom:DateTime) (dto:DateTime) = 
    //Uses the not-so-known chart-data:
    new Uri("http://ichart.finance.yahoo.com/table.csv?s=" + symbol + 
       "&e=" + dto.Day.ToString() + "&d=" + dto.Month.ToString() + "&f=" + dto.Year.ToString() +
       "&g=d&b=" + dfrom.Day.ToString() + "&a=" + dfrom.Month.ToString() + "&c=" + dfrom.Year.ToString() + 
       "&ignore=.csv")

let fetch (url : Uri) = 
    let req = WebRequest.Create (url) :?> HttpWebRequest
    use stream = req.GetResponse().GetResponseStream()
    use reader = new StreamReader(stream)
    reader.ReadToEnd()

let reformat (response:string) = 
    let split (mark:char) (data:string) = 
        data.Split(mark) |> Array.toList
    response |> split '\n' 
    |> List.filter (fun f -> f<>"") 
    |> List.map (split ',') 
    
let getRequest uri = (fetch >> reformat) uri

//Example: Microsoft, from 2010-02-20 to 2010-03-21
let req = makeUrl "MSFT" (new DateTime(2010, 2, 20)) (new DateTime(2010, 3, 21))  |> getRequest

//val req : string list list =
//  [["Date"; "Open"; "High"; "Low"; "Close"; "Volume"; "Adj Close"];
//   ["2010-04-21"; "31.33"; "31.50"; "31.23"; "31.33"; "55343100"; "30.83"];
//   ["2010-04-20"; "31.22"; "31.44"; "31.13"; "31.36"; "52199500"; "30.86"];
//   ...