namespace Microsoft.FSharp.Plot
open System
open System.Drawing
open System.Windows.Forms

// Abstract objects that represent "plottable" items
type IPlot = 
    abstract BasePlot : IPlot // extend to array of IPlot (for compound plots)
  
// Control which can accept selected IPlot objects.
type IChart = 
    abstract Control : Control
    abstract Accepts : IPlot -> bool
    abstract Add     : IPlot -> unit
    abstract Context : MenuItem[]

// Each 
type IChartFactory = 
    abstract Name    : string
    abstract Accepts : IPlot -> bool
    abstract Create  : unit -> IChart


// The window management functions for charts/plots.
module Interactive =
    
    let menuItem str handler = 
        let item = new MenuItem(str:string)
        item.Click.Add(fun _ -> handler())
        item

    // should be a property for dynamic load
    let factories = ref [] : IChartFactory list ref
    let addFactory f = factories := !factories @ [f]   
               
    // Frames are container Controls in which to add ChartControls.
    // Focus is the current/last IChart - where to add next plots where possible.

    // naive next frame
    // let nextFrame() = let form = new Form(Visible=true,Title="F# Plot Window") in form :> Control

    // tabbing next frame
    // 1. tabControl
    let mutable tabControl = null : TabControl
    let ensureTabControl() = 
      if tabControl <> null && not tabControl.IsDisposed then
        tabControl
      else
        let form = new Form(TopMost=true,Width=400,Height=400,Text="F# Plot Window")
        let tab = new TabControl()
        tabControl <- tab
        form.Controls.Add(tab)
        form.Show()
        tab.Dock <- DockStyle.Fill
        let deleteSelected _    = tab.SelectedTab.Dispose()
        let deleteNonSelected _ = let keep = tab.SelectedTab
                                  let pages = tab.Controls |> Seq.cast : TabPage seq 
                                  pages |> Seq.to_array |> Array.iter (fun page -> if page <> keep then page.Dispose())
        tab.ContextMenu <- new ContextMenu [| menuItem "Delete selected tab"         deleteSelected;
                                              menuItem "Delete un-selected tab" deleteNonSelected;
                                           |]
        tab
        
    type myTabPage(chart : IChart) = 
        inherit TabPage()
        let mutable m_chart = chart
        member this.Chart 
            with get() = m_chart
            and set ch = m_chart <- ch
    
    let mutable pageNumber = 0    
    let nextFrame (ch) = 
      let tab2 = ensureTabControl()  
      let page = new myTabPage(ch)
      pageNumber <- pageNumber + 1
      page.Text <- sprintf "P%d" pageNumber
      let () = tab2.Controls.Add(page)
      tab2.SelectTab(page)
      (page :> Control)

    //let mutable focus = ((new TextChartControl() :>IChart) : IChart) // dummy value
    //do  focus.Control.Dispose()
    let setFocus x = () // focus <- x
    
    let plot(pl:'a :> IPlot) =
        let p = pl.BasePlot
        match !factories |> List.tryFind (fun fac -> fac.Accepts p) with
        | None     -> printf "Sorry, not accepted by a registered charting engine\n"; failwith "unknown plot"
        | Some fac -> let chart = fac.Create()
                      let frame = nextFrame(chart)
                      frame.Controls.Add(chart.Control)
                      chart.Control.Dock <- DockStyle.Fill
                      chart.Add p
                      let items =                       
                        [| new MenuItem("-");
                           menuItem "Delete" (fun _ -> frame.Dispose())
                        |]
                      frame.ContextMenu <- new ContextMenu(Array.append chart.Context items)
                      setFocus chart
                      (pl : 'a)
    
    open System.Windows.Forms
    let chart(ch : 'a :> IChart) =      
      let frame = nextFrame(ch)
      frame.Controls.Add(ch.Control)
      ch.Control.Dock <- DockStyle.Fill
      let items =
        [| new MenuItem("-");
           menuItem "Delete" (fun _ -> frame.Dispose())
        |]
      frame.ContextMenu <- new ContextMenu(Array.append ch.Context items)
      setFocus ch
      (ch : 'a)

    let multiplot(parray : IPlot array) = 
        let myTab = ensureTabControl() 
        for p in parray do
            let tabPageList : myTabPage list = myTab.Controls  |> Seq.cast |> Seq.to_list 
            let existingTab = tabPageList |> List.tryFind(fun c -> c.Chart.Accepts p)   
            match existingTab with
                | Some(c) -> c.Chart.Add p
                | _       -> plot p |> ignore; ()

    let replot(p:'a :> IPlot) = 
        let myTab = ensureTabControl()
        if myTab.TabCount > 0 then
            let page = myTab.Controls.Item(myTab.SelectedIndex) :?> myTabPage
            if page.Chart.Accepts p then
                page.Chart.Add p
                (p:'a)
            else
                plot p
        else plot p
     
    type MultiChartControl(tc : TableLayoutPanel) = 
        let table = tc
        interface IChart with
            override this.Control = table :> Control
            override this.Accepts(x) = false // denies all
            override this.Add(x:IPlot) = ()
            override this.Context = [| |]        
        
    let subplot(parray : IPlot array) = 
        let calcRowsColumns len = 
            let rows = int (sqrt (float len) )
            let cols = if (len % rows) > 0 then (len / rows) + 1
                       else len / rows
            rows,cols
        
        let r, c = calcRowsColumns parray.Length
        let myTab = ensureTabControl()
        
        let table = new TableLayoutPanel()
        table.Dock <- DockStyle.Fill
        table.RowCount <- r
        table.ColumnCount <- c
        
        
        let arrangeRowsCols (tbl : TableLayoutPanel)= 
            tbl.RowStyles.Clear()
            tbl.ColumnStyles.Clear()
            let rows = tbl.RowCount
            let cols = tbl.ColumnCount
              
            for i=0 to tbl.RowCount - 1 do 
                tbl.RowStyles.Add(new RowStyle(SizeType.Percent, float32 (100/rows))) |>ignore
            for i=0 to tbl.ColumnCount - 1 do
                tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent , float32 (100 / cols))) |>ignore
        
        arrangeRowsCols table
        let multiChart = new MultiChartControl(table)
        let frame = nextFrame(multiChart)
        frame.Controls.Add table
        
        Array.iter (fun p ->
                    match !factories |> List.tryFind (fun fac -> fac.Accepts p) with
                        | None     -> printf "Sorry, %A not accepted by a registered charting engine\n" p
                        | Some fac ->   let chart = fac.Create()                                
                                        chart.Control.Dock <- DockStyle.Fill
                                        chart.Add p
                                        table.Controls.Add chart.Control
                                        setFocus chart) parray
        


   
    
