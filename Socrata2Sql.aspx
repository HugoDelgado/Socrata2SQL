<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Socrata2Sql.aspx.cs" Inherits="Socrata2SqlMigrationTool.Socrata2Sql" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Socrata 2 SQL Migration Tool</title>
  <meta name="viewport" content="width=device-width, initial-scale=1"/>
  <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css"/>
  <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.12.4/jquery.min.js"></script>
  <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js"></script>
</head>
<body>
    <form id="form1" runat="server">
        <div class="navbar navbar-inverse navbar-fixed-top">
            <div class="container">
                <div class="navbar-header">
                    <img src="logo2.jpg" runat="server" />
                    <button type="button" class="navbar-toggle" data-toggle="collapse" data-target=".navbar-collapse">
                        <span class="icon-bar"></span>
                        <span class="icon-bar"></span>
                        <span class="icon-bar"></span>
                    </button>
                    <a class="navbar-brand" runat="server" href="~/Socrata2Sql.aspx">Socrata2SqlServer Migration Tool</a>
                </div>
                <div class="navbar-collapse collapse">
                    <ul class="nav navbar-nav">
                        
                    </ul>
                </div>
            </div>
        </div>

        <div class="container" >
            
            <h3>Soctrata to SQL Server Migration Tool - JSON</h3>
            <img src="logo1.jpg" runat="server"/>
            <asp:Label ID="LabelTBGeturl" runat="server" Text="Please write the url of the resource:"></asp:Label>
            <asp:TextBox ID="TBurlSocrata" runat="server"></asp:TextBox>
            <asp:Button ID="ButtonGetUrl" runat="server" Text="Analyze Table" OnClick="GetUrl_Click" CssClass="bg-warning" />
            <asp:Button ID="ButtonInsert" runat="server" Text="Insert Data" CssClass="bg-warning" OnClick="InsertData_Click" Visible="false"/>
            <asp:LinkButton ID="ButtonCancel" runat="server" CssClass="bg-warning" Visible="false" Text="Cancel" PostBackUrl="~/Socrata2Sql.aspx"></asp:LinkButton>   

        </div>

        <div class="container">
            <asp:Label ID="LabelUId" runat="server" Text="Unique Identifier:"></asp:Label>
            <asp:Label ID="LabelGotUid" runat="server"></asp:Label>
        </div>
        <div class="container">
            <asp:Table runat="server" Width="100%" CssClass="table">
                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label ID="LabelFormat" runat="server" Text="Format:"></asp:Label></asp:TableCell>
                    <asp:TableCell>
                        <asp:Label ID="LabelGotFormat" runat="server"></asp:Label></asp:TableCell>
                    <asp:TableCell>
                        <asp:Label ID="LabelNumRows" runat="server" Text="Number of Rows:"></asp:Label></asp:TableCell>
                    <asp:TableCell>
                        <asp:Label ID="LabelGotNumRows" runat="server" Text=""></asp:Label></asp:TableCell>
                    <asp:TableCell>
                        <asp:Label ID="LabelNumFields" runat="server" Text="Number of fields:"></asp:Label></asp:TableCell>
                    <asp:TableCell>
                        <asp:Label ID="LabelGotNumRecords" runat="server" Text=""></asp:Label></asp:TableCell>
                </asp:TableRow>
            </asp:Table>
        </div>
        <div class="container">
            <asp:Label ID="LabelContent" runat="server" Text=""></asp:Label> 
        </div>

        <div class="container">
            <asp:Label ID="LabelData" runat="server" Text=""></asp:Label> 
        </div>
        <div class="container">
            <asp:GridView ID="GVDataTypes" runat="server" AutoGenerateColumns="False" DataSourceID="SQLgetStructure" BackColor="LightGoldenrodYellow" 
                BorderColor="Tan" BorderWidth="1px" CellPadding="2" ForeColor="Black" GridLines="None" Font-Size="Small">
                <AlternatingRowStyle BackColor="PaleGoldenrod" />
                <Columns>
                    <asp:BoundField DataField="COLUMN_NAME" HeaderText="COLUMN_NAME" SortExpression="COLUMN_NAME"></asp:BoundField>
                    <asp:TemplateField HeaderText="DATA_TYPE" SortExpression="DATA_TYPE">
                        <EditItemTemplate>
                            <asp:DropDownList ID="DropDownList1" runat="server" >
                                <asp:ListItem Text="Varchar" Value="VARCHAR"></asp:ListItem>
                                <asp:ListItem Text="Integer 32 " Value="INT"></asp:ListItem>
                                <asp:ListItem Text="Integer 64" Value="LONG"></asp:ListItem>
                                <asp:ListItem Text="Datetime" Value="DATETIME"></asp:ListItem>
                            </asp:DropDownList>
                            <asp:Label ID="Label1" runat="server" Text='<%# Eval("DATA_TYPE") %>'></asp:Label>
                        </EditItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="Label1" runat="server" Text='<%# Bind("DATA_TYPE") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="CHARACTER_MAXIMUM_LENGTH" HeaderText="CHARACTER_MAXIMUM_LENGTH" ReadOnly="True" SortExpression="CHARACTER_MAXIMUM_LENGTH"></asp:BoundField>
                    <asp:BoundField DataField="NUMERIC_PRECISION" HeaderText="NUMERIC_PRECISION" ReadOnly="True" SortExpression="NUMERIC_PRECISION"></asp:BoundField>
                    <asp:BoundField DataField="DATETIME_PRECISION" HeaderText="DATETIME_PRECISION" ReadOnly="True" SortExpression="DATETIME_PRECISION"></asp:BoundField>
                    <asp:BoundField DataField="IS_NULLABLE" HeaderText="IS_NULLABLE" ReadOnly="True" SortExpression="IS_NULLABLE"></asp:BoundField>
                    <asp:CommandField ShowEditButton="True" />
                </Columns>
                <FooterStyle BackColor="Tan" />
                <HeaderStyle BackColor="Tan" Font-Bold="True" />
                <PagerStyle BackColor="PaleGoldenrod" ForeColor="DarkSlateBlue" HorizontalAlign="Center" />
                <SelectedRowStyle BackColor="DarkSlateBlue" ForeColor="GhostWhite" />
                <SortedAscendingCellStyle BackColor="#FAFAE7" />
                <SortedAscendingHeaderStyle BackColor="#DAC09E" />
                <SortedDescendingCellStyle BackColor="#E1DB9C" />
                <SortedDescendingHeaderStyle BackColor="#C2A47B" />
            </asp:GridView>
            <asp:SqlDataSource runat="server" ID="SQLgetStructure" ConnectionString='<%$ ConnectionStrings:Socrata2SqlConnectionString %>' SelectCommand="select COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, 
       NUMERIC_PRECISION, DATETIME_PRECISION, 
       IS_NULLABLE 
from INFORMATION_SCHEMA.COLUMNS
where TABLE_NAME=@tablename">
                <SelectParameters>
                    <asp:ControlParameter ControlID="LabelGotUid" PropertyName="Text" DefaultValue="" Name="tablename"></asp:ControlParameter>
                </SelectParameters>
            </asp:SqlDataSource>
          
        </div>

        <div class="container">
            <asp:GridView ID="GVTableUploaded" runat="server" DataSourceID="SqlTableUploaded" Visible="False" BackColor="LightGoldenrodYellow" BorderColor="Tan" BorderWidth="1px" CellPadding="2" ForeColor="Black" GridLines="None" Font-Size="Smaller">
                <AlternatingRowStyle BackColor="PaleGoldenrod" />
                <FooterStyle BackColor="Tan" />
                <HeaderStyle BackColor="Tan" Font-Bold="True" />
                <PagerStyle BackColor="PaleGoldenrod" ForeColor="DarkSlateBlue" HorizontalAlign="Center" />
                <SelectedRowStyle BackColor="DarkSlateBlue" ForeColor="GhostWhite" />
                <SortedAscendingCellStyle BackColor="#FAFAE7" />
                <SortedAscendingHeaderStyle BackColor="#DAC09E" />
                <SortedDescendingCellStyle BackColor="#E1DB9C" />
                <SortedDescendingHeaderStyle BackColor="#C2A47B" />
            </asp:GridView>
            <asp:SqlDataSource runat="server" ID="SqlTableUploaded" ConnectionString='<%$ ConnectionStrings:Socrata2SqlConnectionString %>' SelectCommand="">
            </asp:SqlDataSource>
        </div>
    </form>
</body>
</html>
