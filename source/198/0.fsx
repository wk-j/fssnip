#r "System.Windows.Forms.DataVisualization.dll"
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting

let data = [ "Conservative", 306; "Labour", 258; "Liberal Democrat", 57 ]

// Create a chart containing a default area and show it on a form
let chart = new Chart(Dock = DockStyle.Fill)
let form = new Form(Visible = true, Width = 700, Height = 500)
chart.ChartAreas.Add(new ChartArea("MainArea"))
form.Controls.Add(chart)

// Create series and add it to the chart
let series = new Series(ChartType = SeriesChartType.Doughnut)
chart.Series.Add(series)
// Specify data for the series using data-binding
series.Points.DataBindXY(data, "Item1", data, "Item2")
