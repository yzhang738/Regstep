<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<FlightCenterModels.SalesViewModel>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div style="margin-top: -10px; margin-bottom: 10px; text-align:center">
        <h2>List of Sales Table</h2>
    </div>

        <% Html.Kendo().Grid<FlightCenterModels.SalesViewModel>()
        .Name("Sales")
        .TableHtmlAttributes(new { style = "font-size:9pt;" })
        .HtmlAttributes(new { style = "height:475px;" })
        .DataSource(dataSource => dataSource
              .Ajax()
              .Model(model => model.Id(a => a.SalesId))
              .ServerOperation(true)
              .Create(create => create.Action("InsertSaleRecord", "Home"))
              .Read(read => read.Action("SaleRecordsGridAjax", "Home"))
              .Update(update => update.Action("UpdateSaleRecord", "Home"))
              .Destroy(destroy => destroy.Action("DeleteSaleRecord", "Home").Data("onDeleteSaleRecord"))
              //.Events(events => events.Error("error_handler").Sync("sync_handler"))
              .Events(events => events.RequestEnd("onRequestEnd"))
           )
        .Events(events => events
                        .Edit("onSalesGridEdit")
                        //.Cancel("onAthleteGridCancel")
                )
        .Pageable(pager => pager.Input(true)
                                .Refresh(true)
                                .PageSizes(new[] { 10, 20, 30 })
                )
        .Scrollable(scrolling => scrolling.Height(285))
        .Sortable()
        .Selectable()
        .Filterable()
        .Resizable(resize => resize.Columns(true))
        .ToolBar(t =>
        {
            t.Custom().Name("Add a New Sale Record").Action("EditPage", "Home");
        })
        .Editable(e => e.Mode(Kendo.Mvc.UI.GridEditMode.InLine).Enabled(true))
        .Columns(columns =>
        {
            columns.Bound(a => a.SalesId)
                .Title("Sale ID")
                .Filterable(false)
                .Width(60);
            columns.Bound(a => a.PassengerName)
                .Title("Name")
                .Width(100);
            columns.Bound(a => a.UserId)
                .Title("User ID")
                .Width(100);
            columns.Bound(a => a.Shop)
                .Width(120);
            columns.Bound(a => a.CreatedDateTime)
                .Title("Created Date")
                .Format("{0:yyyy/MM/dd HH:mm:ss}")
                .Width(150);
            columns.Bound(a => a.BookingDate)
                .Title("Booking Date")
                .Format("{0:yyyy/MM/dd}")
                .HeaderHtmlAttributes(new { title = "Booking Date" })
                .Width(100);
            columns.Bound(a => a.Destination)
                .Width(110);
            columns.Bound(a => a.Deposit)
                .Filterable(false)
                .Format("{0:C}")
                .Width(70);
            columns.Bound(a => a.NumberPassengers)
                .Filterable(false)
                .Title("Number of Passengers")
                .HeaderHtmlAttributes(new { title = "Number of Passengers" })
                .Width(50);
            columns.Bound(a => a.Invoice)
                .Filterable(false)
                .Title("Invoice")
                .Format("{0:C}")
                .HeaderHtmlAttributes(new { title = "Invoice" })
                .Width(80);
            columns.Bound(a => a.SaleLocation)
                .Title("Sale Location")
                .HeaderHtmlAttributes(new { title = "SaleLocation" })
                .Width(100);
            columns.Bound(a => a.Comments)
                .Title("Note")
                .HtmlAttributes(new { style = "white-space:nowrap;" })
                .HtmlAttributes(new { title = "#= Comments #" })
                .Width(90);
            columns.Command(a =>
                 {
                     a.Custom("Edit").Click("onSalesGridEdit");
                     a.Destroy();
                 });
        }).Render();
                
        %>

    <script type="text/javascript">
        function onDeleteSaleRecord(e) {
            return { id: e.SalesId }
        }

        function onSalesGridEdit(e) {
            e.preventDefault();
            var dataItem = this.dataItem($(e.currentTarget).closest("tr"));

            var url = "/Home/EditPage";
            window.location.href = url + "?salesId=" + dataItem.SalesId;
        }

        function onRequestEnd(e) {
            if (e.type == "update" || e.type == "destroy") {
                this.read();
            }
        }

        function error_handler(e) {
            
        }

        function sync_handler(e) {
            this.read();
        }

    </script>

</asp:Content>

