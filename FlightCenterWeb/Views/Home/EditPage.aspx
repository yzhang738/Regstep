<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<FlightCenterModels.SalesViewModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <% using (Html.BeginForm("EditPageSubmit", "Home", FormMethod.Post))
       {
           if (Model != null)
           {%>
        <%: Html.ValidationSummary(true)%>

            <div id="Sales_Id" style="visibility: hidden">"<%: Html.TextBoxFor(model => model.SalesId) %>"</div>

            <table style="margin-top:10px; margin-left:20px;">
                <tr>                        
                    <td class="td_text"> Passenger Name: </td>
                    <td class="td_textbox"><%: Html.TextBoxFor(model => model.PassengerName)%>
                                            <%: Html.ValidationMessageFor(model => model.PassengerName)%>
                    </td>

                    <td class="td_text"> User ID: </td>
                    <td class="td_textbox"><%: Html.TextBoxFor(model => model.UserId)%>
                                            <%: Html.ValidationMessageFor(model => model.UserId)%>
                    </td>

                    <td class="td_text"> Deposit: </td>
                    <td class="td_textbox"><%: Html.TextBoxFor(model => model.Deposit)%>
                                            <%: Html.ValidationMessageFor(model => model.Deposit)%>
                    </td>  
                     
                    <td class="td_text"> Number of Passengers: </td>
                    <td class="td_textbox"><%: Html.TextBoxFor(model => model.NumberPassengers, new { style = "width: 117px;" })%>
                                            <%: Html.ValidationMessageFor(model => model.NumberPassengers)%>
                    </td>                
                </tr>   

                <tr>                                                              
                    <td class="td_text"> Booking Date: </td>
                    <td class="td_textbox"> <%= Html.Kendo().DatePicker()
                                            .Name("BookingDate")
                                            .Min(DateTime.Now.AddYears(-10))
                                            .Max(DateTime.Now.AddSeconds(1))
                                            .Value(Model.BookingDate)
                                            .HtmlAttributes(new { style = "height:17px;vertical-align:bottom;" })
                                    %>                            
                                    <%: Html.ValidationMessageFor(model => model.BookingDate)%>
                    </td>      

                    <td class="td_text"> Destination: </td>
                    <td class="td_textbox"><%: Html.DropDownListFor(model => model.Destination, Model.Destinations)%>
                                            <%: Html.ValidationMessageFor(model => model.Destinations)%>
                    </td>                        

                    <td class="td_text"> Shop: </td>
                    <td class="td_textbox"><%: Html.DropDownListFor(model => model.Shop, Model.Shops)%>
                                            <%: Html.ValidationMessageFor(model => model.Shops)%>
                    </td>
                                            
                    <td class="td_text"> Sale Location: </td>
                    <td class="td_textbox"><%: Html.DropDownListFor(model => model.SaleLocation, Model.SaleLocations, new { style = "width: 120px;" })%>
                                            <%: Html.ValidationMessageFor(model => model.SaleLocations)%>
                    </td>                        
                </tr>   

                <tr>                      
                    <td class="td_text"> Source: </td>
                    <td class="td_textbox"><%: Html.DropDownListFor(model => model.SourceType0, Model.SourceTypes0, Model.SourceType0_Init, new { style = "width: 130px;" })%>
                                            <%: Html.ValidationMessageFor(model => model.SourceTypes0)%>
                    </td> 

                    <td class="td_text"> Suppliers: </td>
                    <td class="td_textbox"><%: Html.DropDownListFor(model => model.Supplier0, Model.Suppliers0, Model.Supplier0_Init, new { style = "width: 210px;" })%>
                                            <%: Html.ValidationMessageFor(model => model.Suppliers0)%>
                    </td>   
                                                                
                    <td class="td_text"> Sale Value: </td>
                    <td class="td_textbox"><%: Html.TextBoxFor(model => model.SaleValue0)%>
                                            <%: Html.ValidationMessageFor(model => model.SaleValue0)%>
                    </td>   
                                                                   
                    <td class="td_text"> Note: </td>
                    <td rowspan="3" class="td_textarea"><%: Html.TextAreaFor(model => model.Comments, new {  @cols = 20, @rows = 3 })%>
                                            <%: Html.ValidationMessageFor(model => model.Comments)%>
                    </td>                
                </tr>

                <tr>                      
                    <td class="td_text"> Source: </td>
                    <td class="td_textbox"><%: Html.DropDownListFor(model => model.SourceType1, Model.SourceTypes1, Model.SourceType1_Init, new { style = "width: 130px;" })%>
                                            <%: Html.ValidationMessageFor(model => model.SourceTypes1)%>
                    </td> 

                    <td class="td_text"> Suppliers: </td>
                    <td class="td_textbox"><%: Html.DropDownListFor(model => model.Supplier1, Model.Suppliers1, Model.Supplier1_Init, new { style = "width: 210px;" })%>
                                            <%: Html.ValidationMessageFor(model => model.Suppliers1)%>
                    </td>   
                                                                
                    <td class="td_text"> Sale Value: </td>
                    <td class="td_textbox"><%: Html.TextBoxFor(model => model.SaleValue1)%>
                                            <%: Html.ValidationMessageFor(model => model.SaleValue1)%>
                    </td>                                                                   
                </tr>

                <tr>                      
                    <td class="td_text"> Source: </td>
                    <td class="td_textbox"><%: Html.DropDownListFor(model => model.SourceType2, Model.SourceTypes2, Model.SourceType2_Init, new { style = "width: 130px;" })%>
                                            <%: Html.ValidationMessageFor(model => model.SourceTypes2)%>
                    </td> 

                    <td class="td_text"> Suppliers: </td>
                    <td class="td_textbox"><%: Html.DropDownListFor(model => model.Supplier2, Model.Suppliers2, Model.Supplier2_Init, new { style = "width: 210px;" })%>
                                            <%: Html.ValidationMessageFor(model => model.Suppliers2)%>
                    </td>   
                                                                
                    <td class="td_text"> Sale Value: </td>
                    <td class="td_textbox"><%: Html.TextBoxFor(model => model.SaleValue2)%>
                                            <%: Html.ValidationMessageFor(model => model.SaleValue2)%>
                    </td>                                                                   
                </tr>

                <tr>                      
                    <td class="td_text"> Source: </td>
                    <td class="td_textbox"><%: Html.DropDownListFor(model => model.SourceType3, Model.SourceTypes3, Model.SourceType3_Init, new { style = "width: 130px;" })%>
                                            <%: Html.ValidationMessageFor(model => model.SourceTypes3)%>
                    </td> 

                    <td class="td_text"> Suppliers: </td>
                    <td class="td_textbox"><%: Html.DropDownListFor(model => model.Supplier3, Model.Suppliers3, Model.Supplier3_Init, new { style = "width: 210px;" })%>
                                            <%: Html.ValidationMessageFor(model => model.Suppliers3)%>
                    </td>   
                                                                
                    <td class="td_text"> Sale Value: </td>
                    <td class="td_textbox"><%: Html.TextBoxFor(model => model.SaleValue3)%>
                                            <%: Html.ValidationMessageFor(model => model.SaleValue3)%>
                    </td>   
                </tr>

                <tr>                      
                    <td class="td_text"> Source: </td>
                    <td class="td_textbox"><%: Html.DropDownListFor(model => model.SourceType4, Model.SourceTypes4, Model.SourceType4_Init, new { style = "width: 130px;" })%>
                                            <%: Html.ValidationMessageFor(model => model.SourceTypes4)%>
                    </td> 

                    <td class="td_text"> Suppliers: </td>
                    <td class="td_textbox"><%: Html.DropDownListFor(model => model.Supplier4, Model.Suppliers4, Model.Supplier4_Init, new { style = "width: 210px;" })%>
                                            <%: Html.ValidationMessageFor(model => model.Suppliers4)%>
                    </td>   
                                                                
                    <td class="td_text"> Sale Value: </td>
                    <td class="td_textbox"><%: Html.TextBoxFor(model => model.SaleValue4)%>
                                            <%: Html.ValidationMessageFor(model => model.SaleValue4)%>
                    </td>   
                </tr>

                <tr>                      
                    <td class="td_text"> Source: </td>
                    <td class="td_textbox"><%: Html.DropDownListFor(model => model.SourceType5, Model.SourceTypes5, Model.SourceType5_Init, new { style = "width: 130px;" })%>
                                            <%: Html.ValidationMessageFor(model => model.SourceTypes5)%>
                    </td> 

                    <td class="td_text"> Suppliers: </td>
                    <td class="td_textbox"><%: Html.DropDownListFor(model => model.Supplier5, Model.Suppliers5, Model.Supplier5_Init, new { style = "width: 210px;" })%>
                                            <%: Html.ValidationMessageFor(model => model.Suppliers5)%>
                    </td>   
                                                                
                    <td class="td_text"> Sale Value: </td>
                    <td class="td_textbox"><%: Html.TextBoxFor(model => model.SaleValue5)%>
                                            <%: Html.ValidationMessageFor(model => model.SaleValue5)%>
                    </td>   
                </tr>

                <tr>                      
                    <td class="td_text"> Source: </td>
                    <td class="td_textbox"><%: Html.DropDownListFor(model => model.SourceType6, Model.SourceTypes6, Model.SourceType6_Init, new { style = "width: 130px;" })%>
                                            <%: Html.ValidationMessageFor(model => model.SourceTypes6)%>
                    </td> 

                    <td class="td_text"> Suppliers: </td>
                    <td class="td_textbox"><%: Html.DropDownListFor(model => model.Supplier6, Model.Suppliers6, Model.Supplier6_Init, new { style = "width: 210px;" })%>
                                            <%: Html.ValidationMessageFor(model => model.Suppliers6)%>
                    </td>   
                                                                
                    <td class="td_text"> Sale Value: </td>
                    <td class="td_textbox"><%: Html.TextBoxFor(model => model.SaleValue6)%>
                                            <%: Html.ValidationMessageFor(model => model.SaleValue6)%>
                    </td>   
                </tr>

                <tr>                      
                    <td class="td_text"> Source: </td>
                    <td class="td_textbox"><%: Html.DropDownListFor(model => model.SourceType7, Model.SourceTypes7, Model.SourceType7_Init, new { style = "width: 130px;" })%>
                                            <%: Html.ValidationMessageFor(model => model.SourceTypes7)%>
                    </td> 

                    <td class="td_text"> Suppliers: </td>
                    <td class="td_textbox"><%: Html.DropDownListFor(model => model.Supplier7, Model.Suppliers7, Model.Supplier7_Init, new { style = "width: 210px;" })%>
                                            <%: Html.ValidationMessageFor(model => model.Suppliers7)%>
                    </td>   
                                                                
                    <td class="td_text"> Sale Value: </td>
                    <td class="td_textbox"><%: Html.TextBoxFor(model => model.SaleValue7)%>
                                            <%: Html.ValidationMessageFor(model => model.SaleValue7)%>
                    </td>   
                </tr>

                <tr>                      
                    <td class="td_text"> Source: </td>
                    <td class="td_textbox"><%: Html.DropDownListFor(model => model.SourceType8, Model.SourceTypes8, Model.SourceType8_Init, new { style = "width: 130px;" })%>
                                            <%: Html.ValidationMessageFor(model => model.SourceTypes8)%>
                    </td> 

                    <td class="td_text"> Suppliers: </td>
                    <td class="td_textbox"><%: Html.DropDownListFor(model => model.Supplier8, Model.Suppliers8, Model.Supplier8_Init, new { style = "width: 210px;" })%>
                                            <%: Html.ValidationMessageFor(model => model.Suppliers8)%>
                    </td>   
                                                                
                    <td class="td_text"> Sale Value: </td>
                    <td class="td_textbox"><%: Html.TextBoxFor(model => model.SaleValue8)%>
                                            <%: Html.ValidationMessageFor(model => model.SaleValue8)%>
                    </td>   
                </tr>

                <tr>                      
                    <td class="td_text"> Source: </td>
                    <td class="td_textbox"><%: Html.DropDownListFor(model => model.SourceType9, Model.SourceTypes9, Model.SourceType9_Init, new { style = "width: 130px;" })%>
                                            <%: Html.ValidationMessageFor(model => model.SourceTypes9)%>
                    </td> 

                    <td class="td_text"> Suppliers: </td>
                    <td class="td_textbox"><%: Html.DropDownListFor(model => model.Supplier9, Model.Suppliers9, Model.Supplier9_Init, new { style = "width: 210px;" })%>
                                            <%: Html.ValidationMessageFor(model => model.Suppliers9)%>
                    </td>   
                                                                
                    <td class="td_text"> Sale Value: </td>
                    <td class="td_textbox"><%: Html.TextBoxFor(model => model.SaleValue9)%>
                                            <%: Html.ValidationMessageFor(model => model.SaleValue9)%>
                    </td>   
                </tr>

            </table>           

            <table style="margin: 20px auto 10px auto;">
                <tr>
                    <td style="width:102px; height:29px; padding-right: 100px" >
                        <input type="image" src="../../Images/SUBMIT_bttn1.gif" alt="Submit" />
                    </td>
                    <td style=" width:102px; height:29px; padding-left: 100px" >
                        <a href='<%= Url.Action("ListSales", "Home")%>'><img src="/Images/Cancel_bttn1.gif" /></a>
                    </td>
                </tr>
<%--                <div style="margin: 20px auto 10px auto; width:136px; height:32px" >
                    <input type="image" src="../../Images/SUBMIT_bttn.gif" alt="Submit" />
                </div>
--%>            
            </table>

            <div class="clear"></div>

        <% }
           else
           { %>
                <div class="groupbox_title" >
                    <h2> No data is available! </h2>
                </div>
           <% }
       } %>


    <script type="text/javascript">
        $('[id^=SourceType]').change(function () {
            var id = this.id;
            var lid = id.replace(/[^\d]/g, '');
            var sourceType = $(this).val();

            $.ajax(
            {
                type: "POST",
                dataType: "json",
                url: '/Home/GetSuppliers',
                data: { type: sourceType },
                success: function (data) {
                    var supplierSelect = $('#Supplier' + lid);

                    // Empty the dropdownlist
                    supplierSelect.empty();

                    // Reload the dropdownlist
                    $.each(data, function (index, item) {
                        var itemtext = '<option value="' + item.Value + '">' + item.Text + '</option>';
                        supplierSelect.append(itemtext);
                    });
                }
            });
        });

    </script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
