
namespace Microsoft.FSharp.Plot
module Excel

open System;
open System.Drawing;
open Microsoft.Office.Interop.Owc11
open Microsoft.Office.Interop.Excel
open System.Windows.Forms
open Microsoft.FSharp.Plot
open Microsoft.FSharp.Plot.Core // neutral plot datatypes

let ConvertToObjectArray  (xs:'a array)= 
      let oArray = Array.create (xs.Length) (new obj())
      Array.iteri (fun i x -> oArray.[i] <- (x:>obj) ) xs
      oArray

let RevertBackFromObjArray (xs:obj array)= 
        let oArray = (Array.zeroCreate xs.Length)
        Array.iteri (fun i x -> oArray.[i] <- (unbox x)  ) xs
        oArray                          

type Plot2D() as self=
  class    
    let updateFire,updateEvent = Event.create()
    
    let mutable m_name = ""
    let mutable series : ChSeries =null 
    let mutable m_bordercolor = Color.Empty
    let mutable m_showlabels = false
    abstract Create : ChSeries -> unit 
    
    member this.Update = updateEvent : bool IEvent

    /// Does Caption and Set Legend to True
    override this.Create (e: ChSeries ) = 
         series <- e
         e.Caption <- m_name
         if m_name <> "" then
            e.Parent.HasLegend <- true
         
         let count = e.DataLabelsCollection.Count 
         for i=0 to count - 1 do e.DataLabelsCollection.Delete(0)                        
         if m_showlabels then e.DataLabelsCollection.Add() |> ignore
            
         if m_bordercolor <> Color.Empty then
            e.Border.Color <- m_bordercolor.Name
    
    
    member  this.update() = 
                if series <> null then this.Create series
                updateFire true   
  
    member this.Series 
        with get () = series     
                
    
    member this.Name 
        with set(s) =   
            m_name <- s
            this.update()
        and get() = m_name
   
    member this.BorderColor   
        with get() = m_bordercolor
        and set (c) = 
            m_bordercolor <- c
            this.update() 
 
    member this.ShowLabels 
        with get () = m_showlabels
        and set (c) = m_showlabels <- c; this.update() 
    
    interface IPlot with
        member t.BasePlot = self :> IPlot
    end
  end;;

//////////////////////////////////////////////////////
//////////////////////////////////////////////////////
//////////////////////////////////////////////////////
//////////////////////////////////////////////////////
type Bars (xys : seq<float * string>) as self  = 
    class
        inherit Plot2D() 
        let ys,labels = Seq.to_array xys |> Array.split
        let mutable m_ys = ConvertToObjectArray ys
        let mutable m_labels  = ConvertToObjectArray labels
        let mutable m_color  = Color.Empty
        let mutable m_alignment = Alignment.Vertical
 
        override this.Create  (e : ChSeries) = 
            base.Create(e)        
            e.Type <- if m_alignment = Alignment.Vertical
                      then ChartChartTypeEnum.chChartTypeColumnStacked
                      else ChartChartTypeEnum.chChartTypeBarStacked
            e.SetData(ChartDimensionsEnum.chDimValues,-1,m_ys)
            e.SetData(ChartDimensionsEnum.chDimCategories, -1 , m_labels)            
            
            if m_color <>  Color.Empty then
                e.Interior.Color <- m_color.Name
            
        member this.Alignment
            with get()= m_alignment
            and set (c)= m_alignment <- c ; this.update()
            
        member this.Color 
            with set(c) = 
                m_color <- c
                this.update()
            and get() = m_color
                        
        member this.Values 
            with set(x:float[]) =
                m_ys <- ConvertToObjectArray x
                this.update()
            and get() = RevertBackFromObjArray m_ys : float[]
                
        member this.Labels
            with set (y:string[]) =
                m_labels <- ConvertToObjectArray y
                this.update()
            and get() = RevertBackFromObjArray m_labels : string[]

       
end


let RelevantMarkerStyle  (m:MarkerStyle) =
    match m with
        | MarkerStyle.Circle -> ChartMarkerStyleEnum.chMarkerStyleCircle
        | MarkerStyle.Diamond -> ChartMarkerStyleEnum.chMarkerStyleDiamond
        | MarkerStyle.None -> ChartMarkerStyleEnum.chMarkerStyleNone
        | MarkerStyle.Plus -> ChartMarkerStyleEnum.chMarkerStylePlus
        | MarkerStyle.Star -> ChartMarkerStyleEnum.chMarkerStyleStar
        | MarkerStyle.Triangle -> ChartMarkerStyleEnum.chMarkerStyleTriangle
        | MarkerStyle.X -> ChartMarkerStyleEnum.chMarkerStyleX
        | _ -> ChartMarkerStyleEnum.chMarkerStyleDiamond
                
let RelevantLineStyle (l:LineStyle) =
    match l with
        | LineStyle.Solid  -> ChartLineDashStyleEnum.chLineSolid
        | LineStyle.Dashed -> ChartLineDashStyleEnum.chLineDash
        | LineStyle.Square -> ChartLineDashStyleEnum.chLineSquareDot
        | LineStyle.DotDot -> ChartLineDashStyleEnum.chLineDashDotDot
        | _ -> ChartLineDashStyleEnum.chLineRoundDot
                    

//////////////////////////////////////////////////////
//////////////////////////////////////////////////////
//////////////////////////////////////////////////////
//////////////////////////////////////////////////////   

       
type Lines (xys : seq<float*float>) as self = 
    class
        inherit Plot2D() 
        let xs,ys = Seq.to_array xys |> Array.split
        let n1, n2 = xs.Length , ys.Length
        do assert (n1 = n2)
      
        let mutable m_linecolor = Color.Empty
        let mutable m_linestyle = LineStyle.Solid
        let mutable m_smooth = false
        let mutable m_linesize = 1
        
        let arrange (xs:float array) (ys:float array) = 
            let xsSorted = xs.Clone() :?> float array
            System.Array.Sort<float>(xsSorted)
            
            let ysnew = Array.create ys.Length 0.
            for i = 0 to xs.Length - 1 do         
                let index= Array.find_index (fun e -> e =xs.[i]   ) xsSorted
                ysnew.[i] <- ys.[index]
            done
            xsSorted, ysnew
        
        let mutable m_markerColor = Color.Empty
        
        let mutable m_markersize =3
        let mutable m_markerstyle = MarkerStyle.Diamond
        
        let xs, ys = arrange xs ys
        let mutable m_xs = ConvertToObjectArray xs
        let mutable m_ys = ConvertToObjectArray ys

        override this.Create  (e : ChSeries) = 
            base.Create(e)  
            if m_smooth then e.Type <- ChartChartTypeEnum.chChartTypeSmoothLineStackedMarkers
            else e.Type <- ChartChartTypeEnum.chChartTypeLineStackedMarkers
            
            e.Marker.Style <- RelevantMarkerStyle m_markerstyle
            e.Marker.Size <- m_markersize
            
            if m_markerColor <> Color.Empty then
                e.Interior.Color <- m_markerColor.Name
            
            e.Line.DashStyle <- RelevantLineStyle m_linestyle
            
            if m_linesize > 3 then 
                e.Line.Weight <- LineWeightEnum.owcLineWeightThick
            elif m_linesize >1 then e.Line.Weight <- LineWeightEnum.owcLineWeightMedium
            else e.Line.Weight <- LineWeightEnum.owcLineWeightThin
            
            if m_linecolor <> Color.Empty then
                e.Line.Color <- m_linecolor.Name
                        
            e.SetData(ChartDimensionsEnum.chDimValues,-1,m_ys)
            e.SetData(ChartDimensionsEnum.chDimCategories,-1,m_xs)

        member this.SmoothLine 
            with get () = m_smooth
            and set(c) = m_smooth<- c ; this.update()      
            
      
        member this.LineColor  
            with get() = m_linecolor
            and set (c) = 
                m_linecolor <- c
                this.update()
       
        member this.MarkerColor  
            with get() = m_markerColor
            and set (c) = 
                m_markerColor <- c
                this.update()
            
        member this.LineStyle 
            with get() = m_linestyle
            and set(c) = 
                m_linestyle <- c
                this.update()
        member this.LineSize 
            with get () = float m_linesize
            and set(c) = m_linesize <- int c
                         this.update()   
        member this.MarkerSize 
            with get () = float m_markersize
            and set(c)= 
                m_markersize <- int c
                this.update()
                
        member this.MarkerStyle 
            with get() = m_markerstyle
            and set(c) = 
                m_markerstyle <- c
                this.update()
             
        member this.XYValues 
            with set (v:float[]*float[]) =
                let x,y = v
                let xs,ys = arrange x y
                m_xs <- ConvertToObjectArray xs
                m_ys <- ConvertToObjectArray ys
                this.update()
            and get() = 
                let x = RevertBackFromObjArray m_xs : float[]
                let y = RevertBackFromObjArray m_ys : float[]
                x,y 
    end

//////////////////////////////////////////////////////
//////////////////////////////////////////////////////
//////////////////////////////////////////////////////
//////////////////////////////////////////////////////


type Scatter(xys: seq<float*float>) as self = 
    class
        inherit Plot2D() 
        let xs,ys = Seq.to_array xys |> Array.split
        let n1, n2 = xs.Length , ys.Length
        do assert (n1 = n2)
        let mutable series = null : ChSeries
        
        let mutable m_name = System.String.Empty
        
        let mutable m_xs = ConvertToObjectArray xs
        let mutable m_ys = ConvertToObjectArray ys
        
        let mutable m_markerColor = Color.Empty
        let mutable m_markersize =3
        let mutable m_markerstyle = MarkerStyle.Diamond
  
        override this.Create  (e : ChSeries) = 
            base.Create(e)
            e.Type <- ChartChartTypeEnum.chChartTypeScatterMarkers
            
            e.Marker.Style <- RelevantMarkerStyle m_markerstyle
            e.Marker.Size <- m_markersize
            
            if m_markerColor <> Color.Empty then
                e.Interior.Color <- m_markerColor.Name
                      
            e.SetData(ChartDimensionsEnum.chDimXValues,-1,m_xs)
            e.SetData(ChartDimensionsEnum.chDimYValues,-1,m_ys)

        member this.MarkerSize 
            with get () = float  m_markersize
            and set(c)= 
                m_markersize <-int  c
                this.update()
                
        member this.MarkerStyle 
            with get() = m_markerstyle
            and set(c) = 
                m_markerstyle <- c
                this.update()
        
        member this.MarkerColor  
            with get() = m_markerColor
            and set (c) = 
                m_markerColor <- c
                this.update()
                     
        member this.XYValues 
            with set(v:float[]*float[]) =
                let xs,ys = v
                m_xs <- ConvertToObjectArray xs
                m_ys <- ConvertToObjectArray ys
                this.update()
            and get() = 
                let x = RevertBackFromObjArray m_xs : float[]
                let y  = RevertBackFromObjArray m_ys : float[]
                x,y 
    end

//////////////////////////////////////////////////////
//////////////////////////////////////////////////////
//////////////////////////////////////////////////////
//////////////////////////////////////////////////////

type Stock(xs:float[],openY:float[],highY:float[],lowY:float[],closeY:float[]) as self = 
  class
    inherit Plot2D() 
    let m_xs = ConvertToObjectArray xs
    let m_openY = ConvertToObjectArray openY
    let m_highY = ConvertToObjectArray highY
    let m_lowY = ConvertToObjectArray lowY
    let m_closeY = ConvertToObjectArray closeY
    
    let mutable m_linecolor = Color.Empty
    let mutable m_barcolor  = Color.Empty
    
    new (xs:float[],highY:float[],lowY:float[],closeY:float[]) = 
        new Stock(xs,[||] ,highY ,lowY,closeY) 
    
    override this.Create  (e : ChSeries) = 
        base.Create e

        if m_openY = [||] then      
            e.Type <- ChartChartTypeEnum.chChartTypeStockHLC
            
        else 
            e.Type <- ChartChartTypeEnum.chChartTypeStockOHLC
            e.SetData(ChartDimensionsEnum.chDimOpenValues,-1,m_openY)
                    
        e.SetData(ChartDimensionsEnum.chDimCategories,-1,m_xs)
        e.SetData(ChartDimensionsEnum.chDimHighValues,-1,m_highY)
        e.SetData(ChartDimensionsEnum.chDimLowValues,-1,m_lowY)
        e.SetData(ChartDimensionsEnum.chDimCloseValues,-1,m_closeY)
        
        if m_linecolor <> Color.Empty then e.Line.Color <- m_linecolor.Name
        if m_barcolor <> Color.Empty then e.Interior.Color  <- m_barcolor.Name
    member this.LineColor  
        with get () = m_linecolor
        and set (c) = 
            m_linecolor <- c
            this.update()
        
    member this.BarColor  
        with get () = m_barcolor
        and set (c) = 
            m_barcolor <-c
            this.update()
  end



type Pie(ys:float[],labels:string[]) = 
      class
    inherit Plot2D() 
    
    let mutable m_ys = ConvertToObjectArray ys
    let mutable m_labels = ConvertToObjectArray labels
    
    let mutable m_explose = false
    
    override this.Create  (e : ChSeries) = 
        base.Create e
        if m_explose then         
            e.Type <- ChartChartTypeEnum.chChartTypePieExploded
            e.Explosion <- 3
        else   e.Type <- ChartChartTypeEnum.chChartTypePie
                            
        e.SetData(ChartDimensionsEnum.chDimValues,-1,m_ys)
        e.SetData(ChartDimensionsEnum.chDimCategories,-1,m_labels)
    member this.Values 
        with get() = RevertBackFromObjArray m_ys : float[]
        and set (c :float[]) =  m_ys <- ConvertToObjectArray c;       this.update()
    member this.Explose
        with get () = m_explose
        and set(c) = 
            m_explose <- c
            this.update()        
    member this.Labels 
        with get() = RevertBackFromObjArray m_labels : string[]
        and set (c:string[]) = m_labels <- ConvertToObjectArray c; this.update()
  end
//////////////////////////////////////////////////////
//////////////////////////////////////////////////////
//////////////////////////////////////////////////////
//////////////////////////////////////////////////////


  
type Area(xs:float[], ys:float[]) =
 class
    inherit Plot2D() 
    
    let m_xs = ConvertToObjectArray xs
    let m_ys = ConvertToObjectArray ys
    
    let mutable m_Color  = Color.Empty
    
    override this.Create  (e : ChSeries) = 
        base.Create e
        e.Type <- ChartChartTypeEnum.chChartTypeAreaStacked
       
        e.SetData(ChartDimensionsEnum.chDimValues,-1,m_ys)
        e.SetData(ChartDimensionsEnum.chDimCategories,-1,m_xs)
        
        if m_Color  <> Color.Empty then e.Interior.Color <- m_Color .Name
   
    member this.Color  
        with get () = m_Color 
        and set (c) = m_Color  <- c

  end
//////////////////////////////////////////////////////
//////////////////////////////////////////////////////
//////////////////////////////////////////////////////
//////////////////////////////////////////////////////  


type Chart2D() as self =
        let panel = new Panel()
        let acceptTypes tys (x:IPlot) = List.exists (fun (ty:System.Type) -> ty.IsAssignableFrom(x.GetType())) tys
        let chartToolbar = new System.Windows.Forms.ToolStrip(Dock=DockStyle.Bottom)        
        do panel.Controls.Add(chartToolbar)
        let chartContainer = new ChartSpaceClass()        
        let imageControl = new System.Windows.Forms.PictureBox(Dock =DockStyle.Fill)
        let setPicturetoImage   (chSpace:ChartSpaceClass) w h =
            use memImage = new System.IO.MemoryStream( chartContainer.GetPicture("gif",w,h):?> System.Byte[])
            let image = new System.Drawing.Bitmap(memImage)
            memImage.Close()
            image
        do panel.Controls.Add(imageControl)        
        let refreshImage () = setPicturetoImage chartContainer imageControl.Width  (imageControl.Height- chartToolbar.Height)
        
        do imageControl.SizeChanged.Add(fun ev->  imageControl.Image <- refreshImage() )
        
        let btnSave = new System.Windows.Forms.ToolStripButton(Text ="&Save")
        do btnSave.Click.Add( fun _  ->
                        let saveDialog = new SaveFileDialog()
                        saveDialog.Filter <- "gif (*.gif)|*.gif|jpg (*.jpg)|*.jpg|png (*.png)|*.png"
                        if saveDialog.ShowDialog() = DialogResult.OK then
                            let fileName = saveDialog.FileName
                            let a = fileName.Split(".".ToCharArray())
                            let filterName = a.[a.Length-1]
                            chartContainer.ExportPicture(fileName,filterName,imageControl.Width,imageControl.Height)
                            )
        do chartToolbar.Items.Add(btnSave) |>ignore
        
        let refresh _ = imageControl.Image <- refreshImage()
        do refresh()
        let aChart =  chartContainer.Charts.Add(chartContainer.Charts.Count)
        member this.Refresh () =   refresh()
        interface IIChart with 
            member this.HasLegend 
                with get() = aChart.HasLegend
                and set(c) = aChart.HasLegend <- c ;  refresh()
                
            member this.Title 
                with set x = if System.String.IsNullOrEmpty(x) then aChart.HasTitle <- false
                             else aChart.HasTitle <- true;  aChart.Title.Caption <- x  
                             this.Refresh()
                and get() = if aChart.HasTitle then aChart.Title.Caption
                            else System.String.Empty
        end 
        
        interface IChart with 
            member this.Control = panel :> Control
            member  this.Accepts(x) = acceptTypes  [typeof<Bars>;  typeof<Scatter>;typeof<Lines>;typeof<Stock>;typeof<Pie>;typeof<Area> ] x
            member this.Add(x:IPlot) =
                match x with
                | :? Plot2D as plot -> 
                    if (this :> IChart).Accepts plot then
                        let chartSeries= aChart.SeriesCollection.Add(aChart.SeriesCollection.Count)
                        plot.Create chartSeries
                        plot.Update.Add refresh
                        refresh()
                    else printf "You tried to add an unsupported IChart object..\n"
                | ty -> ()
            member this.Context = [| |]        

        member this.Chart = aChart   
        member this.ChartContainer = chartContainer         
     
    
//////////////////////////////////////////////////////
//////////////////////////////////////////////////////
//////////////////////////////////////////////////////
//////////////////////////////////////////////////////
type ExcelChartFactory() = class 
    let acceptTypes tys (x:IPlot) = List.exists (fun (ty:System.Type) -> ty.IsAssignableFrom(x.GetType())) tys
    interface IChartFactory  with
     override this.Name     = "Excel Charts"
     override this.Accepts(x) = acceptTypes  [typeof<Bars>;  typeof<Scatter>;typeof<Lines>;typeof<Stock>;typeof<Pie>;typeof<Area>] x
     override this.Create() =  (new Chart2D() :> IChart) 
    end
end   

type ExcelChartProvider () = class
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
        { 
            new ILines with
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
    let scatter (plot : Scatter) = 
        { 
            new IScatter with
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
        { 
            new IStock with
            member t.BarColor       with get() = plot.BarColor  and set x = plot.BarColor  <- x
            member t.LineColor      with get() = plot.LineColor  and set x = plot.LineColor  <- x   
            member t.BorderColor    with get() = plot.BorderColor  and set x = plot.BorderColor  <- x
            member t.Name           with get() = plot.Name and set x = plot.Name <- x
            member t.ShowLabels     with get() = plot.ShowLabels and set x = plot.ShowLabels <- x
            member t.BasePlot = plot :> IPlot
        }   
    let pie (plot : Pie) = 
        { 
            new IPie with
            member t.Explose       with get() = plot.Explose and set x = plot.Explose <- x
            member t.Labels        with get() = plot.Labels and set x = plot.Labels  <- x   
            member t.Values        with get() = plot.Values and set x = plot.Values <- x
            member t.BorderColor   with get() = plot.BorderColor  and set x = plot.BorderColor <- x
            member t.Name          with get() = plot.Name and set x = plot.Name <- x
            member t.ShowLabels    with get() = plot.ShowLabels and set x = plot.ShowLabels <- x
            member t.BasePlot = plot :> IPlot
        } 
    let area (plot : Area) = 
        { 
            new IArea with
            member t.Color         with get() = plot.Color  and set x = plot.Color   <- x
            member t.BorderColor   with get() = plot.BorderColor  and set x = plot.BorderColor  <- x   
            member t.Name          with get() = plot.Name and set x = plot.Name <- x
            member t.ShowLabels    with get() = plot.ShowLabels and set x = plot.ShowLabels<- x
            member t.BasePlot = plot :> IPlot
        }                
    override this.Bars(args)        = bar(new Bars(args))
    override this.Lines(args)      = lines(new Lines(args))
    override this.Scatter(args)    = scatter(new Scatter(args))
    override this.Stock (xs,openY,highY,lowY,closeY) 
                                    = stock(new Stock(xs,openY,highY,lowY,closeY))
    override this.Stock (xs,highY,lowY,closeY) 
                                    = stock(new Stock(xs,highY,lowY,closeY))
    override this.Pie(ys,labels)    = pie(new Pie(ys,labels))
    override this.Area(xs,ys)       = area(new Area(xs,ys))
end

#if extension
type Microsoft.FSharp.Plot.Plotter.myplot with
    member this.ExcelBars(args) = (new Bars(args) :> ExcelBars)
    member this.ExcelLines(xs,ys) = (new Lines(xs,ys) :> ExcelLines)
    member this.ExcelScatter(xs,ys) = (new Scatter(xs,ys) :> ExcelScatter)
    member this.ExcelStock (xs,openY,highY,lowY,closeY) = (new Stock(xs,openY,highY,lowY,closeY) :> ExcelStock)
    member this.ExcelStock (xs,highY,lowY,closeY) = (new Stock(xs,highY,lowY,closeY) :> ExcelStock)
    member this.ExcelPie(ys,labels) = (new Pie(ys,labels) :> ExcelPie)
    member this.ExcelArea(xs,ys)   = (new Area(xs,ys) :> ExcelArea)

#endif

let excelProvider = new ExcelChartProvider() :> ChartProvider
//do Microsoft.FSharp.Plot.Interactive.Plot.addProvider (excelProvider)
let RegisterProvider() = Microsoft.FSharp.Plot.Core.SetProvider excelProvider
do  RegisterProvider()

do Microsoft.FSharp.Plot.Interactive.addFactory (new ExcelChartFactory() :> IChartFactory)
