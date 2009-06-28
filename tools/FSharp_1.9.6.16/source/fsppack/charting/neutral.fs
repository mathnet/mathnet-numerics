namespace Microsoft.FSharp.Plot
module Core

open Microsoft.FSharp.Plot
open System.Drawing

type Alignment =
    | Vertical = 0
    | Horizontal =1 
    
type LineStyle =
    | Solid  = 0
    | Dashed = 1
    | Square = 2
    | DotDot = 3
    
type MarkerStyle =
    | None      = 0
    | Circle    = 1
    | Diamond   = 2
    | Star      = 3
    | Triangle  = 4
    | X         = 5
    | Plus      = 6
    
type IBars =
    abstract Alignment    : Alignment with get,set
    abstract Color        : Color with get,set
    abstract Labels       : string [] with get,set
    abstract Values       : float []  with get,set
    abstract BorderColor  : Color with get,set
    abstract Name         : string with set, get
    abstract ShowLabels   : bool with set, get
    interface IPlot

type ILines =
    abstract LineColor       : Color with get,set
    abstract LineStyle       : LineStyle with get,set
    abstract LineSize        : float with get,set
    abstract MarkerColor     : Color with get,set
    abstract MarkerSize      : float with get,set
    abstract MarkerStyle     : MarkerStyle with get,set
    abstract XYValues        : float[]*float[] with get,set
    abstract BorderColor     : Color with get,set
    abstract Name            : string with set, get
    abstract ShowLabels      : bool with set, get
    interface IPlot

type IScatter =
    abstract MarkerColor     : Color with get,set
    abstract MarkerSize      : float with get,set
    abstract MarkerStyle     : MarkerStyle with get,set
    abstract XYValues        : float [] * float [] with get,set
    abstract BorderColor     : Color with get,set
    abstract Name            : string with set, get
    abstract ShowLabels      : bool with set, get
    interface IPlot

type IStock =  
    abstract BarColor        : Color with get,set
    abstract LineColor       : Color with get,set
    abstract BorderColor     : Color with get,set
    abstract Name            : string with set, get
    abstract ShowLabels      : bool with set, get
    interface IPlot

type IPie =
    abstract Explose     : bool with get,set
    abstract Labels      : string [] with get,set
    abstract Values      : float [] with get,set
    abstract BorderColor : Color with get,set
    abstract Name        : string with set, get
    abstract ShowLabels  : bool with set, get
    interface IPlot
  
type IArea =
    abstract Color       : Color with get,set
    abstract BorderColor : Color with get,set
    abstract Name        : string with set, get
    abstract ShowLabels  : bool with set, get
    interface IPlot

type IIChart =
    abstract  HasLegend : bool with get,set
    abstract  Title : string with get,set
    interface IChart 

[<AbstractClass>]
type ChartProvider ()= 
    abstract Bars    : seq<float*string> -> IBars
    abstract Lines   : seq<float*float> -> ILines
    abstract Scatter : seq<float*float> -> IScatter
    abstract Stock   : float[]*float[]*float[]*float[]*float[]-> IStock
    abstract Stock   : float[]*float[]*float[]*float[]-> IStock
    abstract Pie     : float[]* string[] -> IPie
    abstract Area    : float[]*float[] -> IArea


type myplot() =
  class
    // members added via extension methods
  end

let mutable CurrentProvider = None : ChartProvider option
  
let GetProvider() =
  match CurrentProvider with
  | Some p -> p
  | None   -> let msg = "No plot provider has been registered.\nTry registering one? e.g.\n\nMicrosoft.FSharp.Plot.Xceed.RegisterProvider();;\nMicrosoft.FSharp.Plot.Excel.RegisterProvider();;\n\n"
              printf "%s\n" msg
              failwith msg
let SetProvider p = CurrentProvider <- Some p

let Bars(inp) = GetProvider().Bars(inp)
let Lines(inp) = GetProvider().Lines(inp)
let Scatter(inp) = GetProvider().Scatter(inp)
let Stock (xs,openY,highY,lowY,closeY) = GetProvider().Stock(Seq.to_array xs,openY,highY,lowY,closeY)
let Stock2 (xs,highY,lowY,closeY) = GetProvider().Stock(Seq.to_array xs,highY,lowY,closeY)
let Pie(ys,labels) = GetProvider().Pie(Seq.to_array ys,labels)
let Area(xs,ys)   = GetProvider().Area(Seq.to_array xs,Seq.to_array ys)    