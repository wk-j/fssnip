open System
open System.IO
open System.Net
open System.Drawing

let ImageSize = 300

let fetchImage (url : Uri) = 
    let req = WebRequest.Create (url) :?> HttpWebRequest
    use stream = req.GetResponse().GetResponseStream()
    Image.FromStream(stream)

let googleChartQRCodeImage mydata = 
    let addr = String.Format("http://chart.apis.google.com/chart?chs={0}x{0}&cht=qr&chl={1}&choe=ISO-8859-1", ImageSize, mydata)
    new Uri(addr, UriKind.Absolute)

let vcard = @"BEGIN:VCARD
VERSION:3.0
N:{1};{0};
FN:{0} {1}
NICKNAME:{2}
ORG:{3}
ROLE:{4}
TITLE:{4}
PHOTO;MEDIATYPE={6}:{5}
TEL;TYPE=CELL:{7}
X-TWITTER:{8}
URL:{9}
EMAIL;TYPE=PREF,INTERNET:{10}
ADR;TYPE=HOME:;;{11};{12};;{13};{14}
LABEL;TYPE=HOME:{11}\n{13} {12}\n{14}
REV:{15}
END:VCARD
"

let googleChartUri contact = 
    String.Format(vcard, contact)
    |> Uri.EscapeDataString
    |> googleChartQRCodeImage

let sample : obj [] = [|
    "Matt"; //first name 
    "Oneofus"; //surname
    "matias"; //nickname 
    "Company & co."; //organization
    "Guru"; //title
    "http://www.wpclipart.com/signs_symbol/icons_oversized/male_user_icon.png"; //image url
    "image/png"; //picture mime-type, eg. image/gif or image/jpeg
    "+358 50 123 4567"; //cell phone
    "Thoriumi"; // twitter account
    "http://www.iki.fi/thorium/"; // url
    "nospam@mailinator.com"; //email
    "Katuroad 1 A 5"; // street address
    "Espoo"; //city
    "02100"; //postal code
    "Finland"; //country
    System.DateTime.Now.ToString("yyyyMMddThhmmssZ"); // timestamp
    |]

open System.Windows.Forms
let showImage = 
    let form = new Form()
    let pb = new PictureBox()
    pb.Image <- sample |> googleChartUri |> fetchImage
    pb.SizeMode <- PictureBoxSizeMode.AutoSize
    form.Height <- ImageSize+50
    form.Width <- ImageSize+50
    form.Controls.Add(pb)
    form.Show()