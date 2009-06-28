

namespace Microsoft.FSharp.Plot
module Xceed
#nowarn "47"
#nowarn "54"

let AssertSameSize (arrs: (_ array) list) =
  match arrs with
  | [] -> ()
  | arr::arrs -> assert (arrs |> List.forall (fun arr2 -> arr2.Length = arr.Length))

open Microsoft.FSharp.Plot
open Microsoft.FSharp.Plot.Core // neutral plot datatypes
open Xceed.Chart.Standard
open Xceed.Chart.GraphicsCore
open Xceed.Chart
open Xceed.Chart.Core
open System.Windows.Forms
open System.Drawing

//! INDEX: dialog for users to get sitelic xceed developer license key

let addControl (c:Control) (f:#Control) = f.Controls.Add(c); f

let groupBox (a:Control) (box:GroupBox)  = 
    box.Controls.Add a  
    box

let keyDialog key handler = 
    let xceedURL = "http://itwebtools/sitelic/resources/subpages/install.asp?productid=168&ptype=77"
    let input = new TextBox(Text=key,Dock= DockStyle.Fill)
    let wb    = new WebBrowser(Dock= DockStyle.Fill)
    let table = new TableLayoutPanel(Dock= DockStyle.Fill) 
    let box1 = new GroupBox(Text="Enter Xceed Chart 4.1 for .NET license key, then press enter" ,
                            Dock= DockStyle.Fill,
                            Height=48) |> addControl input
    let box2 = new GroupBox(Text="Accept conditions of use to obtain license key (CorpNet only)" ,
                            Dock= DockStyle.Fill) |> addControl wb
    table.Controls.Add(box1,column=0,row=0)
    table.Controls.Add(box2,column=0,row=1)
    let form = new Form(Width=600,Height=600,Text="F# Plotting Xceed license initialisation (CorpNet only)") |> addControl table
    wb.Navigate xceedURL
    input.KeyPress.Add(fun args -> if args.KeyChar = '\013' then
                                    let key = input.Text.Trim()
                                    form.Dispose()                         
                                    form.DialogResult <- DialogResult.OK
                                    handler key)
    form.ShowDialog()

let keyName  =  let userRoot = @"HKEY_CURRENT_USER"
                let subkey   = @"Software\Microsoft Research\FSharp\Plotting"
                userRoot ^ "\\" ^ subkey   

let readKey() = match Microsoft.Win32.Registry.GetValue(keyName,"Key41","") with
                | :? string as key -> key
                | _ -> ""
let writeKey key = Microsoft.Win32.Registry.SetValue(keyName,"Key41",key)

let GetKey() = 
    keyDialog (readKey()) writeKey |> ignore

let MenuItemToggle description v0 handler =
    let mi = new MenuItem(Text=description,Checked=v0)  
    mi.Click.Add(fun _ -> mi.Checked <- not mi.Checked; handler mi.Checked)
    mi

let license() = 
    let key = readKey()
    let key = if key = "" then GetKey(); readKey() else key
    Xceed.Chart.Licenser.LicenseKey <- key

[<AbstractClass>]
type Plot2D() as self =
    let updateFire,updateEvent = Event.create()
    let mutable alignment = Alignment.Vertical
    abstract Series : SeriesBase
    abstract DimensionPreference : AxisScaleMode option array // internal - option preferences on dimension scales
    member this.Update = updateEvent : bool IEvent
    member this.UpdateFire() = updateFire true
    member this.Name 
        with get () = this.Series.Name 
        and set (c) = this.Series.Name <- c ; this.UpdateFire |> ignore
    member this.Alignment 
        with get() = alignment 
        and set(c) = alignment <- c ; this.UpdateFire()
    interface IPlot with
      member t.BasePlot = self :> IPlot

type Bars(xys : seq<float * string>) as self = 
    inherit Plot2D() 
    let ys,labels = Seq.to_array xys |> Array.split
    let series = new BarSeries(Name="Bar data")
    do  series.DataLabels.Mode <- DataLabelsMode.None
    do  Array.iter2 (fun x lab -> series.Add(x,lab)) ys labels
    let update() = self.UpdateFire()
    override this.Series = (series :> SeriesBase)

    override this.DimensionPreference = [| None; None; None |]
    member this.BSeries = series
    
    member this.Color  
        with set (c) = series.BarFillEffect.SetSolidColor(c) ; update()
        and get() = series.BarFillEffect.Color
        
    member this.Labels 
        with get () = 
            let labs : string list = series.Labels |> Seq.cast |> Seq.to_list
            List.to_array labs
        and set (c) = series.Labels.Clear();  Array.iter (fun x ->   series.Labels.Add(box x) |> ignore) c
                      update()
                      
    member this.BorderColor 
        with set(c) = series.BarBorder.Color <- c; update()
        and get () = series.BarBorder.Color 
        
    member this.ShowLabels
        with get () = series.DataLabels.Mode  = DataLabelsMode.Every
        and set(c) =  if c then series.DataLabels.Mode <- DataLabelsMode.Every
                      else series.DataLabels.Mode <- DataLabelsMode.None
                      update()
        
    
    member this.Values
        with set (c) = series.Values.Clear(); series.Values.FillFromEnumerable c; update()
        and get() = let vals : float list = series.Values |> Seq.cast |> Seq.to_list
                    List.to_array vals
                                

  
type HighLow(xys : seq<float*float*float>) as self = 
    inherit Plot2D() 
    let xys = Seq.to_array xys
    let xs = Array.map (fun (x,y,z) -> x) xys
    let ys0 = Array.map (fun (x,y,z) -> y) xys
    let ys1 = Array.map (fun (x,y,z) -> z) xys
    let series = new HighLowSeries(Name = "High Low data")
    let n1,n2,n3 = xs.Length,ys0.Length,ys1.Length
    do  Array.iteri (fun i x -> series.AddHighLow(ys0.[i],ys1.[i],x)) xs    
    do  series.DataLabels.Mode <- DataLabelsMode.None
    do  series.UseXValues <- true
    let update() = self.UpdateFire()
    override this.Series = (series :> SeriesBase)  
    //member this.Size  with set(s) = series.Size <- float32 s; update()
    member this.Color with set(c) = series.LowFillEffect.Color <- c; series.HighFillEffect.Color <- c; update()
    member this.LowColor with set(c) = series.LowFillEffect.Color <- c; update()
    member this.HighColor with set(c) = series.HighFillEffect.Color <- c; update()
    member this.Highs with set(x:float seq) = series.HighValues.FillFromEnumerable x; update()
    member this.Lows  with set(x:float seq) = series.HighValues.FillFromEnumerable x; update()
 //   [<OverloadID("2")>]
//    new (xs:float[],lowFun,highFun) = new HighLow(xs,Array.map lowFun xs,Array.map highFun xs)
    override this.DimensionPreference = [| None; None; Some AxisScaleMode.Numeric |]
  
type Points(xys: seq<float*float>) as self = 
    inherit Plot2D() 
    let xs,ys = Seq.to_array xys |> Array.split
    let n1,n2 = xs.Length,ys.Length
    do  assert(n1=n2)
    let series = new PointSeries(Name = "Points")
    do  series.UseXValues <- true
    do  Array.iter2 (fun x y -> series.AddXY(y,x)) xs ys
    do  series.Size <- 2.0f
    do  series.PointBorder.Width <- 0
    do  series.DataLabels.Mode <- DataLabelsMode.None
    let update() = self.UpdateFire()
    member this.DataLabels with set(x) = series.DataLabels.Mode <- x; update() // REVIEW: abstract
    member this.Color      with set(x) = series.PointFillEffect.Color <- x; update()
    member this.Style      with set(x) = series.PointStyle <- x; update() // REVIEW: abstract
    member this.Size       with set(x)  = series.Size <- float32 x; update()
    override this.Series = (series :> SeriesBase)
    override this.DimensionPreference = [| None; None; Some AxisScaleMode.Numeric |]
    
    member this.SSeries = series
    
    member this.MarkerColor  
        with get() = series.PointFillEffect.Color
        and set(c) = series.PointFillEffect.Color <- c
    member this.MarkerStyle 
        with get () = match series.Markers.Style with
                        | PointStyle.Star           -> MarkerStyle.Star
                        | PointStyle.Pyramid        -> MarkerStyle.Triangle
                        | PointStyle.Cross          -> MarkerStyle.Plus
                        | PointStyle.Ellipse        -> MarkerStyle.Circle
                        | PointStyle.DiagonalCross  -> MarkerStyle.X
                        | PointStyle.InvertedPyramid-> MarkerStyle.Diamond
                        | _ -> MarkerStyle.None
        and set (c) = series.Markers.Style <- 
                        match c with
                          |  MarkerStyle.Star       ->  PointStyle.Star           
                          |  MarkerStyle.Triangle   ->  PointStyle.Pyramid        
                          |  MarkerStyle.Plus       ->  PointStyle.Cross          
                          |  MarkerStyle.Circle     ->  PointStyle.Ellipse        
                          |  MarkerStyle.X          ->  PointStyle.DiagonalCross  
                          |  MarkerStyle.Diamond    ->  PointStyle.InvertedPyramid
                          | _                       ->  PointStyle.Bar
                      update()  
    
    member this.MarkerSize 
        with get() = float series.Size (*.Markers.Width*)
        and set(c) = series.Size <- float32 c; (*series.Markers.Width <- float32 c; *) update()
    
    member this.BorderColor 
        with set(c) = series.PointBorder.Color <- c; update()
        and get () = series.PointBorder.Color 
          
    member this.ShowLabels
        with get () = series.DataLabels.Mode  = DataLabelsMode.Every
        and set(c) =  if c then series.DataLabels.Mode <- DataLabelsMode.Every
                      else series.DataLabels.Mode <- DataLabelsMode.None
                      update()
                      
    member this.XYValues     
        with get() = let xvals : float list = series.XValues |> Seq.cast |> Seq.to_list
                     let yvals : float list = series.Values |> Seq.cast |> Seq.to_list
                     ( (List.to_array xvals) , (List.to_array yvals) )
        and set(c) = let xs,ys = c
                     series.Values.Clear() ; series.XValues.Clear()
                     Array.iter2 (fun x y -> series.AddXY(y,x)) xs ys
                     update()                    
            

  
type Lines(xys: seq<float*float>) as self = 
    inherit Plot2D() 
    let xs,ys = Seq.to_array xys |> Array.split
    let n1,n2 = xs.Length,ys.Length 
    do  assert(n1=n2)
    let series = new LineSeries(Name = "Lines")
    do  series.UseXValues <- true
    do  Array.iter2 (fun x y -> series.AddXY(y,x)) xs ys
    do  series.LineStyle <- LineSeriesStyle.Line
    do  series.LineWidth <- 0.1f
    do  series.LineBorder.Width <- 1
    do  series.DataLabels.Mode <- DataLabelsMode.None
    let update() = self.UpdateFire()
    
    override this.Series = (series :> SeriesBase)
    override this.DimensionPreference = [| None; None; Some AxisScaleMode.Numeric |]
    
    member this.LSeries = series
    
    member this.LineColor  
        with set(c) = series.LineFillEffect.Color <- c; update()
        and get () = series.LineFillEffect.Color
    member this.LineStyle
        with get() = match series.LineBorder.Pattern with
                        | LinePattern.Dash      -> LineStyle.Dashed
                        | LinePattern.Solid     -> LineStyle.Solid
                        | LinePattern.DashDot   -> LineStyle.Square 
                        | LinePattern.DashDotDot->  LineStyle.DotDot
                        | _ -> LineStyle.Solid
        and set(c) =   series.LineBorder.Pattern <-
                         match c with
                            | LineStyle.Dashed -> LinePattern.Dash      
                            | LineStyle.Solid  -> LinePattern.Solid     
                            | LineStyle.Square -> LinePattern.DashDot   
                            | LineStyle.DotDot -> LinePattern.DashDotDot
                            | _  -> LinePattern.Solid  
                       update()
    member this.LineSize 
        with get() = float series.LineWidth       
        and set(c) = series.LineWidth <- float32 c ; update()
    member this.MarkerColor  
        with get() = series.Markers.FillEffect.Color
        and set(c) = series.Markers.FillEffect.Color <- c ; update()   
    
    member this.MarkerStyle 
        with get () = match series.Markers.Style with
                        | PointStyle.Star           -> MarkerStyle.Star
                        | PointStyle.Pyramid        -> MarkerStyle.Triangle
                        | PointStyle.Cross          -> MarkerStyle.Plus
                        | PointStyle.Ellipse        -> MarkerStyle.Circle
                        | PointStyle.DiagonalCross  -> MarkerStyle.X
                        | PointStyle.InvertedPyramid-> MarkerStyle.Diamond
                        | _ -> MarkerStyle.None
        and set (c) = series.Markers.Style <- 
                        match c with
                          |  MarkerStyle.Star       ->  PointStyle.Star           
                          |  MarkerStyle.Triangle   ->  PointStyle.Pyramid        
                          |  MarkerStyle.Plus       ->  PointStyle.Cross          
                          |  MarkerStyle.Circle     ->  PointStyle.Ellipse        
                          |  MarkerStyle.X          ->  PointStyle.DiagonalCross  
                          |  MarkerStyle.Diamond    ->  PointStyle.InvertedPyramid
                          | _                       ->  PointStyle.Bar
                      update()  
    
    member this.MarkerSize 
        with get() = float series.Markers.Width
        and set(c) = series.Markers.Width <- (float32 c) ; update()
    member this.ShowLabels
        with get () = series.DataLabels.Mode  = DataLabelsMode.Every
        and set(c) =  if c then series.DataLabels.Mode <- DataLabelsMode.Every
                      else series.DataLabels.Mode <- DataLabelsMode.None
                      update()                     
        
    member this.BorderColor  
        with get() = series.LineBorder.Color
        and set(c) = series.LineBorder.Color <- c ; update()   
    
    member this.XYValues     
        with get() = let xvals : float list = series.XValues |> Seq.cast |> Seq.to_list
                     let yvals : float list = series.Values |> Seq.cast |> Seq.to_list
                     ( (List.to_array xvals) , (List.to_array yvals) )
        and set(c) = let xs,ys = c
                     series.Values.Clear() ; series.XValues.Clear()
                     Array.iter2 (fun x y -> series.AddXY(y,x)) xs ys
                     update()
                     
  
type Stock(xs:float[],openY:float[],highY:float[],lowY:float[],closeY:float[]) as self = 
    inherit Plot2D() 

    let series = new StockSeries()
    
    do  AssertSameSize [xs;openY;lowY;highY;closeY]
    do  Array.iteri (fun i x -> series.AddStock(openY.[i],highY.[i],lowY.[i],closeY.[i],x)) xs    
    do  series.DataLabels.Mode <- DataLabelsMode.None
    do  series.UseXValues <- true
    let update() = self.UpdateFire()
    override this.Series = (series :> SeriesBase)  
    //member this.Size  with set(s) = series.Size <- float32 s; update()
    member this.CandleWidth with set(c) = series.CandleWidth <- float32 c; update()
    
    new (xs:float[],highY:float[],lowY:float[],closeY:float[]) = 
        new Stock(xs,[||] ,highY ,lowY,closeY) 
    [<OverloadID("2")>]
    new (xs:float[], (openY : float->float), (highY : float->float),lowY,closeY) = new Stock(xs,Array.map openY xs,Array.map highY xs,Array.map lowY xs,Array.map closeY xs)
    override this.DimensionPreference = [| None; None; Some AxisScaleMode.Numeric |]
  
    member this.StockSeries = series
    member this.BarColor 
       with get () =  series.DownFillEffect.Color
       and set(c) = series.DownFillEffect.Color <- c ; update() 
    member this.LineColor 
       with get () =  series.UpFillEffect.Color
       and set(c) = series.UpFillEffect.Color <- c ;series.DownFillEffect.Color <- c; update()              
    member this.BorderColor 
        with get () = series.UpFillEffect.Color
        and set(c) = series.UpFillEffect.Color <- c ; update()  
    member this.ShowLabels
        with get () = series.DataLabels.Mode  = DataLabelsMode.Every
        and set(c) =  if c then series.DataLabels.Mode <- DataLabelsMode.Every
                      else series.DataLabels.Mode <- DataLabelsMode.None
                      update()                     

  
type Pie(ys:float[],xplodeList : int[], labels:string[]) as self = 
    inherit Plot2D() 
    let series = new PieSeries()
    let explode =  xplodeList
    do  Array.iter2 (fun x lab -> series.Add(x,lab); series.Detachments.Add(0 :> obj) |> ignore) ys labels
    do    Array.iteri (fun index x -> series.Detachments.set_Item(index,x)  |> ignore ) explode
    let update() = self.UpdateFire()
    override this.Series = (series :> SeriesBase)
  
    new (ys) = new Pie(ys,Array.map (fun y -> 0) ys, Array.map (fun y -> "") ys)
    
    new(ys, labels:string[]) = 
        new Pie(ys,Array.map (fun y -> 2) ys,labels)  //then
      
    member this.Detach(i : int, x : int) = 
        series.Detachments.set_Item(i,x)
    member this.ResetDetach() = 
        Array.iteri (fun index x -> series.Detachments.set_Item(index,0)  |> ignore ) explode
    override this.DimensionPreference = [| None; None; Some AxisScaleMode.Numeric |]    
    member this.PieSeries = series
    member this.Explose 
       with get () = series.Detachments.Count > 0
       and set(c) = 
        if c then
            let arrobj = Array.create (series.Values.Count) (new obj())
            Array.iter (fun e-> series.Detachments.Add(arrobj)|> ignore ) arrobj
        else series.Detachments.Clear()
        update()
        
    member this.Values
        with set (c : float[]) = series.Values.Clear(); series.Values.FillFromEnumerable c; update()
        and get() = let vals : float list = series.Values |> Seq.cast |> Seq.to_list
                    List.to_array vals
    member this.Labels
        with set (c : string[]) = series.Labels.Clear(); series.Labels.FillFromEnumerable c; update()
        and get() = let vals : string list = series.Labels |> Seq.cast |> Seq.to_list
                    List.to_array vals
    member this.ShowLabels
        with get () = series.DataLabels.Mode  = DataLabelsMode.Every
        and set(c) =  if c then series.DataLabels.Mode <- DataLabelsMode.Every
                      else series.DataLabels.Mode <- DataLabelsMode.None
                      update()                     
    member this.BorderColor 
        with get () = series.PieBorder.Color
        and set(c) = series.PieBorder.Color <- c ; update()  
    

  
  
type Area(xs:float[], ys:float[]) as self =
        inherit Plot2D() 
        let n1,n2 = xs.Length,ys.Length
        do  assert(n1=n2)
        let series = new AreaSeries()
        do  series.UseXValues <- true
        do  Array.iter2 (fun x y -> series.AddXY(x,y)) xs ys
        let update() = self.UpdateFire()
        override this.Series = (series :> SeriesBase)
        override this.DimensionPreference = [| None; None; Some AxisScaleMode.Numeric |]
         
        member this.AreaSeries = series
        member this.DataLabels 
            with set(x) = series.DataLabels.Mode <- x
            and get() = series.DataLabels.Mode
        
        member this.LineStyle 
            with set(value) = series.Appearance.LineMode <- value
            and get () = series.Appearance.LineMode 

        member this.EdgeDisplay
            with set(value) = series.DropLines <-value
            and get()= series.DropLines
        member this.BorderColor 
            with get()=  series.AreaBorder.Color
            and set (value) = 
                series.AreaBorder.Color <- value; update()
                
        member this.Color  
            with set(value) = series.AreaFillEffect.SetSolidColor(value) ; update()
            and get() = series.AreaFillEffect.Color
        member this.ShowLabels
            with get () = series.DataLabels.Mode  = DataLabelsMode.Every
            and set(c) =  if c then series.DataLabels.Mode <- DataLabelsMode.Every
                          else series.DataLabels.Mode <- DataLabelsMode.None
                          update()                     
    
                                 
    
type Chart2D() as self =
    do license()
    let panel = new Panel()
    do panel.Dock <- DockStyle.Fill
    let toolbar = new ChartToolbarControl(Dock=DockStyle.Top)
    do  panel.Controls.Add(toolbar)
    let myChart = new ChartControl(Height=panel.Height-toolbar.Height,
                                   Location=new Point(0,toolbar.Height),
                                   Width=panel.Width,
                                   Dock=DockStyle.Bottom,
                                   Anchor = (AnchorStyles.Right ||| AnchorStyles.Left ||| AnchorStyles.Top ||| AnchorStyles.Bottom))
    do  panel.Controls.Add(myChart)
    
    do  toolbar.ChartControl <- myChart
    let arrButtons = new ChartToolbarButtonsArray(toolbar)
    do  List.iter (arrButtons.Add >> ignore)
          [ChartToolbarButtons.ImageExport; ChartToolbarButtons.Print;     ChartToolbarButtons.Separator;
           ChartToolbarButtons.MouseOffset; ChartToolbarButtons.MouseZoom; ChartToolbarButtons.Separator;
           ChartToolbarButtons.ChartEditor; 
          ]
    do  toolbar.ButtonsConfig <- arrButtons    
    let showToolbar flag =
        toolbar.Visible <- flag
        let barHeight = if flag then toolbar.Height else 0
        myChart.Location <- Point(0,barHeight)
        myChart.Height   <- panel.Height-barHeight

    do myChart.InteractivityOperations.Add(new TrackballDragOperation()) |> ignore
    do myChart.Settings.EnableAntialiasing <- false

    let header = myChart.Labels.AddHeader("Chart2D")        

    let aChart = myChart.Charts.get_Item(0)    
    
    do aChart.Wall(ChartWallType.Floor).Width <- 0.01f
    do aChart.Wall(ChartWallType.Back ).Width <- 0.01f
    do aChart.Wall(ChartWallType.Left ).Width <- 0.01f
    do aChart.View.SetPredefinedProjection(PredefinedProjection.PerspectiveTilted)
    do aChart.LightModel.SetPredefinedScheme(LightScheme.ShinyTopLeft)
    let refresh _ = myChart.Refresh(); Application.DoEvents()
    let showHeader = function 
      | true  -> myChart.Labels.Add(header)    |> ignore; refresh()
      | false -> myChart.Labels.Remove(header) |> ignore; refresh()
    let showLegend = function 
      | true  -> myChart.Legends.Item(0).Mode <- LegendMode.Automatic; refresh()  // always have Item(0) ??
      | false -> myChart.Legends.Item(0).Mode <- LegendMode.Disabled;  refresh()

    let acceptTypes tys (x:IPlot) = List.exists (fun (ty:System.Type) -> ty.IsAssignableFrom(x.GetType())) tys
    
    interface IChart with
        member this.Control = panel :> Control
        member  this.Accepts(x) = acceptTypes [typeof<Bars>; typeof<HighLow>; typeof<Points>;typeof<Lines>;typeof<Stock>;typeof<Pie>;typeof<Area>] x // somebody needs to call this
        member this.Add(x:IPlot) =
            match x with
            | :? Plot2D as plot -> 
                if (this :> IChart).Accepts plot then
                    aChart.Series.Add(plot.Series) |> ignore;  // add series to chart
                    plot.Update.Add refresh;                   // listen to update events from plot      
                    Array.iteri (fun i pref ->
                              match pref with
                                 | None -> ()                                     // no pref for dimension i
                                 | Some mode -> aChart.Axis(2).ScaleMode <- mode) // set pref...
                       plot.DimensionPreference
                    refresh()
                else printf "You tried to add an unsupported IChart object..\n"
            | ty -> ()
        member this.Context = [| MenuItemToggle "Show Toolbar" true showToolbar;
                                 MenuItemToggle "Show Header"  true showHeader;
                                 MenuItemToggle "Show Legend"  true showLegend;
                              |]        
    member this.Added(plot:Plot2D) =       
        (this :> IChart).Add(plot) 
        plot
    member this.Title with set x = header.Text <- x; refresh()
    member this.Refresh() = myChart.Refresh()
    member this.Chart = aChart // expose
    member this.Width  with set x = aChart.Width  <- float32 x // demo required
    member this.Height with set x = aChart.Height <- float32 x // demo required
    member this.ChartControl = myChart // expose
    member this.ScaleNumeric() = 
        this.Chart.Axis(2).ScaleMode <- AxisScaleMode.Numeric
    member this.Header  with set(x) = showHeader x; refresh() 
    member this.Toolbar with set(x) = showToolbar x; refresh() 

type XCeedChartFactory() = 
    let acceptTypes tys (x:IPlot) = List.exists (fun (ty:System.Type) -> ty.IsAssignableFrom(x.GetType())) tys
    interface IChartFactory  with
        override this.Name     = "XCeed Charts"
        override this.Accepts(x) = acceptTypes [typeof<ILines>; typeof<Bars>; typeof<HighLow>; typeof<Points>;typeof<Lines>;typeof<Stock>;typeof<Pie>;typeof<Area>] x
        override this.Create() =  (new Chart2D() :> IChart) // ask xceed provider later


[<AbstractClass>]
type Plot3D() as self =
    let updateFire,updateEvent = Event.create()
    
    
    abstract Series : SeriesBase
    abstract DimensionPreference : AxisScaleMode option array // internal - option preferences on dimension scales
    member this.Update = updateEvent : bool IEvent
    member this.UpdateFire() = updateFire true

    interface IPlot with
      member t.BasePlot = self :> IPlot
  
/// X-Y scatter plot
type Points3D(xs:float[],ys:float[],zs:float[]) as self = 
    inherit Plot3D() 
    do  AssertSameSize [xs;ys;zs]
    let series = new PointSeries(Name="Points")
    do  series.UseXValues <- true
    do  series.UseZValues <- true
    do  Array.iteri (fun i x -> series.AddPoint(ys.[i],x,zs.[i])) xs
    do  series.Size <- 2.0f
    do  series.PointBorder.Width <- 0
    do  series.DataLabels.Mode <- DataLabelsMode.None
    let update() = self.UpdateFire()
    member this.Name       with set(s) = series.Name <- s; update()
    member this.DataLabels with set(x) = series.DataLabels.Mode <- x; update() // REVIEW: abstract
    member this.Color      with set(x) = series.PointFillEffect.Color <- x; update()
    member this.Style      with set(x) = series.PointStyle <- x; update() // REVIEW: abstract
    member this.Size       with set(x)  = series.Size <- float32 x; update()
    override this.Series = (series :> SeriesBase)
    override this.DimensionPreference = [| None; None; Some AxisScaleMode.Numeric |]
  
type Lines3D(xs:float[],ys:float[],zs:float[]) as self = 
    inherit Plot3D() 
    do  AssertSameSize [xs;ys;zs]
    let series = new LineSeries(Name="Lines")
    do  series.UseXValues <- true
    do  series.UseZValues <- true
    do  Array.iteri (fun i x -> series.AddXYZ(ys.[i],x,zs.[i])) xs     
    do  series.LineStyle <- LineSeriesStyle.Line
    do  series.LineWidth <- 0.1f
    do  series.LineBorder.Width <- 1
    do  series.DataLabels.Mode <- DataLabelsMode.None
    let update() = self.UpdateFire()
    member this.Name       with set(s) = series.Name <- s; update()
    member this.DataLabels with set(x) = series.DataLabels.Mode <- x; update() // REVIEW: abstract
    member this.Color      with set(x) = series.LineFillEffect.Color <- x; update()
    member this.Style      with set(x) = series.LineStyle <- x; update() // REVIEW: abstract
    member this.Size       with set(x) = series.LineWidth <- float32 x; update()
    override this.Series = (series :> SeriesBase)
    override this.DimensionPreference = [| None; None; Some AxisScaleMode.Numeric |]


type SurfaceGrid3D(xs:float[],ys:float[],f : int -> int -> float) = 
    inherit Plot3D()
    let n1,n2 = xs.Length,ys.Length
    let grid = seq {for i in 0 .. n1-1 do for j in 0 .. n2-1 do yield i,j}
    let gridSurfaceSeries = new GridSurfaceSeries(Name="Surface Grid")
    do gridSurfaceSeries.Data.SetGridSize(n1,n2)
    do grid |> Seq.iter (fun (i,j) -> gridSurfaceSeries.Data.SetValue(i,j,f i j))
    override this.Series = (gridSurfaceSeries :> SeriesBase)
    override this.DimensionPreference = [| None; None; Some AxisScaleMode.Numeric; Some AxisScaleMode.Numeric;  |]            
    member   this.Flat with set(x) = gridSurfaceSeries.DrawFlat <- x
    [<OverloadID("Grid2")>]
    new (xs:float[],ys:float[],f : float -> float -> float) = new SurfaceGrid3D(xs,ys,fun i j -> f xs.[i] ys.[j])


type SurfaceMesh3D(xs:float[],ys:float[],f) as self =
    inherit Plot3D()
    let n1,n2 = xs.Length,ys.Length
    let grid = seq {for i in 0 .. n1-1 do for j in 0 .. n2-1 do yield i,j}

    let meshSurfaceSeries = new MeshSurfaceSeries(Name = "Surface Mesh")
    do  meshSurfaceSeries.Data.SetGridSize(n1,n2)
    let setSurface f = 
      let eval i j = f xs.[i] ys.[j] : float
      grid |> Seq.iter (fun (i,j) -> meshSurfaceSeries.Data.SetValue(i,j,eval i j,xs.[i],ys.[j]))
    do setSurface f
    let update() = self.UpdateFire()
    override this.Series = (meshSurfaceSeries :> SeriesBase)
    override this.DimensionPreference = [| None; Some AxisScaleMode.Numeric; Some AxisScaleMode.Numeric;  |]
    member   this.Transparency with set(x) = meshSurfaceSeries.FillEffect.SetTransparencyPercent(x)
    member   this.Flat with set(x) = meshSurfaceSeries.DrawFlat <- x
    member   this.Surface with set(f) = setSurface f; update()


type Chart3D() as self =
    do license()
    let panel  = new Panel()
    let toolbar = new ChartToolbarControl(Dock=DockStyle.Top)
    do  panel.Controls.Add(toolbar)
    let myChart = new ChartControl(Height=panel.Height-toolbar.Height,
                                   Location=new Point(0,toolbar.Height),
                                   Width=panel.Width,
                                   Dock=DockStyle.Bottom,
                                   Anchor = (AnchorStyles.Right ||| AnchorStyles.Left ||| AnchorStyles.Top ||| AnchorStyles.Bottom))
    do  panel.Controls.Add(myChart)
    
    do  toolbar.ChartControl <- myChart
    let arrButtons = new ChartToolbarButtonsArray(toolbar)
    do  List.iter (arrButtons.Add >> ignore)
          [ChartToolbarButtons.ImageExport; ChartToolbarButtons.Print;     ChartToolbarButtons.Separator;
           ChartToolbarButtons.MouseTrackball; ChartToolbarButtons.MouseOffset; ChartToolbarButtons.MouseZoom; ChartToolbarButtons.Separator;
           ChartToolbarButtons.ChartEditor; 
           //ChartToolbarButtons.RenderDevice
          ]
    do  toolbar.ButtonsConfig <- arrButtons    
    let showToolbar flag = // same code as Chart2D
      toolbar.Visible <- flag
      let barHeight = if flag then toolbar.Height else 0
      myChart.Location <- Point(0,barHeight)
      myChart.Height   <- panel.Height-barHeight   

    do myChart.Settings.RenderDevice <- RenderDevice.OpenGL // for 3D
    do myChart.InteractivityOperations.Add(new TrackballDragOperation()) |> ignore
    do myChart.Settings.EnableAntialiasing <- false

    let header = myChart.Labels.AddHeader("Chart3D")              
    let aChart = myChart.Charts.get_Item(0)
    
    do aChart.Depth  <- 40.0f
    do aChart.Width  <- 40.0f
    do aChart.Height <- 40.0f
    
    do aChart.Wall(ChartWallType.Floor).Width <- 0.01f
    do aChart.Wall(ChartWallType.Back ).Width <- 0.01f
    do aChart.Wall(ChartWallType.Left ).Width <- 0.01f
    do aChart.View.SetPredefinedProjection(PredefinedProjection.PerspectiveTilted)
    do aChart.LightModel.SetPredefinedScheme(LightScheme.ShinyTopLeft)
    let refresh _ = myChart.Refresh()
    let showHeader = function 
      | true  -> myChart.Labels.Add(header)    |> ignore; refresh()
      | false -> myChart.Labels.Remove(header) |> ignore; refresh()
    let showLegend = function 
      | true  -> myChart.Legends.Item(0).Mode <- LegendMode.Automatic; refresh()  // always have Item(0) ??
      | false -> myChart.Legends.Item(0).Mode <- LegendMode.Disabled;  refresh()

    let acceptTypes tys (x:IPlot) = List.exists (fun (ty:System.Type) -> ty.IsAssignableFrom(x.GetType())) tys
                
    member this.Add(plot:Plot3D) =
      aChart.Series.Add(plot.Series) |> ignore;  // add series to chart
      plot.Update.Add refresh;                   // listen to update events from plot      
      Array.iteri (fun i pref -> match pref with
                                 | None -> ()                                     // no pref for dimension i
                                 | Some mode -> aChart.Axis(i).ScaleMode <- mode) // set pref...
                   plot.DimensionPreference
      let axD = aChart.Axis(StandardAxis.Depth) in axD.ScaleMode <- AxisScaleMode.Numeric // what number axis is this?
      refresh()
    member this.Added(plot:Plot3D) = this.Add(plot); plot
    member this.Title with set x = header.Text <- x; refresh()
    member this.Refresh() = myChart.Refresh()
    member this.Chart = aChart // expose
    member this.ChartControl = myChart // expose  
        
    interface IChart with
        member this.Control = panel :> Control
        member  this.Accepts(x) = acceptTypes [typeof<Points3D>;typeof<Lines3D>;typeof<SurfaceGrid3D>;typeof<SurfaceMesh3D>] x // somebody needs to call this
        member this.Add(x:IPlot) =
            match x with
                | :? Plot3D as plot -> 
                    if (this :> IChart).Accepts plot then
                        aChart.Series.Add(plot.Series) |> ignore;  // add series to chart
                        plot.Update.Add refresh;                   // listen to update events from plot      
                        Array.iteri (fun i pref ->
                                  match pref with
                                     | None -> ()                                     // no pref for dimension i
                                     | Some mode -> aChart.Axis(i).ScaleMode <- mode) // set pref...
                           plot.DimensionPreference
                        let axD = aChart.Axis(StandardAxis.Depth) in axD.ScaleMode <- AxisScaleMode.Numeric // what number axis is this?
                        refresh()
                    else printf "You tried to add an unsupported IChart object..\n"
                | ty -> ()  
        member this.Context = [| MenuItemToggle "Show Toolbar" true showToolbar;
                                 MenuItemToggle "Show Header"  true showHeader;
                                 MenuItemToggle "Show Legend"  true showLegend;                                 
                              |]        

type XCeedChart3DFactory() = 
    let acceptTypes tys (x:IPlot) = List.exists (fun ty -> ty = x.GetType()) tys
    interface IChartFactory  with
        override this.Name     = "XCeed Charts 2D"
        override this.Accepts(x) = acceptTypes [typeof<Points3D>;typeof<Lines3D>;typeof<SurfaceGrid3D>;typeof<SurfaceMesh3D>] x
        override this.Create() =  (new Chart3D() :> IChart) // ask xceed provider later

(*let x= Bars(xs)
let y = new Bars(args) :> XceedBarsx*)


#if extension 
type Microsoft.FSharp.Plot.Interactive.myplot with
    member this.XceedBars(args) = (new Bars(args) :> XceedBars)
    member this.XceedLines(args) = (new Lines(args) :> XceedLines)
    member this.XceedPoints(args) = (new Points(args) :> XceedPoints)
    member this.XceedStock (xs,openY:float[],highY,lowY,closeY) = (new Stock(xs,openY,highY,lowY,closeY) :> XceedStock)
    member this.XceedStock (xs,highY,lowY,closeY) = (new Stock(xs,highY,lowY,closeY) :> XceedStock)
    member this.XceedPie(ys,labels) = (new Pie(ys,labels) :> XceedPie)
    member this.XceedArea(xs,ys)   = (new Area(xs,ys) :> XceedArea)

#endif 


type XceedChartProvider () = 
    inherit ChartProvider()
    let bar (plot :Bars)=
        { new IBars with
          member t.Alignment    with get() = plot.Alignment and set x = plot.Alignment <- x
          member t.Color        with get() = plot.Color  and set x = plot.Color  <- x
          member t.Labels       with get() = plot.Labels and set x = plot.Labels <- x
          member t.Values       with get() = plot.Values and set x = plot.Values <- x
          member t.BorderColor  with get() = plot.BorderColor  and set x = plot.BorderColor  <- x
          member t.Name         with get() = plot.Name and set x = plot.Name <- x
          member t.ShowLabels   with get() = plot.ShowLabels and set x = plot.ShowLabels <- x        
          member t.BasePlot = plot :> IPlot
        }
    let lines (plot : Lines) = 
        { new ILines with
            member t.LineColor     with get() = plot.LineColor  and set x = plot.LineColor  <- x
            member t.LineStyle     with get() = plot.LineStyle and set x = plot.LineStyle <- x   
            member t.LineSize      with get() = plot.LineSize and set x = plot.LineSize <- x
            member t.MarkerColor   with get() = plot.MarkerColor  and set x = plot.MarkerColor  <- x
            member t.MarkerSize    with get() = plot.MarkerSize and set x = plot.MarkerSize <- x
            member t.MarkerStyle   with get() = plot.MarkerStyle and set x = plot.MarkerStyle <- x
            member t.XYValues      with get() = plot.XYValues and set x = plot.XYValues <- x
            member t.BorderColor   with get() = plot.BorderColor  and set x = plot.BorderColor  <- x
            member t.Name          with get() = plot.Name and set x = plot.Name <- x
            member t.ShowLabels    with get() = plot.ShowLabels and set x = plot.ShowLabels <- x
            member t.BasePlot = plot :> IPlot
        }
    let scatter (plot : Points) = 
        { new IScatter with
            member t.MarkerColor   with get() = plot.MarkerColor  and set x = plot.MarkerColor  <- x
            member t.MarkerSize    with get() = plot.MarkerSize and set x = plot.MarkerSize <- x   
            member t.MarkerStyle   with get() = plot.MarkerStyle and set x = plot.MarkerStyle <- x
            member t.XYValues      with get() = plot.XYValues and set x = plot.XYValues <- x
            member t.BorderColor   with get() = plot.BorderColor  and set x = plot.BorderColor  <- x
            member t.Name          with get() = plot.Name and set x = plot.Name <- x
            member t.ShowLabels    with get() = plot.ShowLabels and set x = plot.ShowLabels <- x
            member t.BasePlot = plot :> IPlot
        }   
    let stock (plot : Stock) = 
        { new IStock with
            member t.BarColor       with get() = plot.BarColor  and set x = plot.BarColor  <- x
            member t.LineColor      with get() = plot.LineColor  and set x = plot.LineColor  <- x   
            member t.BorderColor    with get() = plot.BorderColor  and set x = plot.BorderColor  <- x
            member t.Name           with get() = plot.Name and set x = plot.Name <- x
            member t.ShowLabels     with get() = plot.ShowLabels and set x = plot.ShowLabels <- x
            member t.BasePlot = plot :> IPlot
        }   
    let pie (plot : Pie) = 
        { new IPie with
            member t.Explose       with get() = plot.Explose and set x = plot.Explose <- x
            member t.Labels        with get() = plot.Labels and set x = plot.Labels  <- x   
            member t.Values        with get() = plot.Values and set x = plot.Values <- x
            member t.BorderColor   with get() = plot.BorderColor  and set x = plot.BorderColor <- x
            member t.Name          with get() = plot.Name and set x = plot.Name <- x
            member t.ShowLabels    with get() = plot.ShowLabels and set x = plot.ShowLabels <- x
            member t.BasePlot = plot :> IPlot
        } 
    let area (plot : Area) = 
        { new IArea with
            member t.Color         with get() = plot.Color  and set x = plot.Color   <- x
            member t.BorderColor   with get() = plot.BorderColor  and set x = plot.BorderColor  <- x   
            member t.Name          with get() = plot.Name and set x = plot.Name <- x
            member t.ShowLabels    with get() = plot.ShowLabels and set x = plot.ShowLabels<- x
            member t.BasePlot = plot :> IPlot
        }                
    override this.Bars(args)        = bar(new Bars(args))
    override this.Lines(args)      = lines(new Lines(args))
    override this.Scatter(args)    = scatter(new Points(args))
    override this.Stock (xs,openY,highY,lowY,closeY) 
                                    = stock(new Stock(xs,openY,highY,lowY,closeY))
    override this.Stock (xs,highY,lowY,closeY) 
                                    = stock(new Stock(xs,highY,lowY,closeY))
    override this.Pie(ys,labels)    = pie(new Pie(ys,labels))
    override this.Area(xs,ys)       = area(new Area(xs,ys))

let xceedProvider = new XceedChartProvider() :> ChartProvider
let RegisterProvider() = Microsoft.FSharp.Plot.Core.SetProvider xceedProvider
do  RegisterProvider()

do Microsoft.FSharp.Plot.Interactive.addFactory (new XCeedChartFactory()   :> IChartFactory);
do Microsoft.FSharp.Plot.Interactive.addFactory (new XCeedChart3DFactory() :> IChartFactory);
