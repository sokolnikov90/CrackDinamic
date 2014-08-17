<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebRole.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div style="float: left;">
        <div style="width: 169px; height: 44px; display:table-cell; vertical-align:middle; ">
            Carck length l0 =&nbsp; 
            <asp:TextBox ID="tb_l0" runat="server" Width="48px">0.03</asp:TextBox>
            </div>
        </div>
        <div style="height: 44px">
            <asp:Image ID="Image1" runat="server" ImageUrl="~/Images/fraction.gif" />
        </div>

        <p>
            delta =
            <asp:TextBox ID="tb_delta" runat="server">0.05</asp:TextBox>
        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            alpha =
            <asp:TextBox ID="tb_alpha" runat="server">2</asp:TextBox>
        &nbsp;&nbsp;&nbsp;
            </p>
        <p>
            a0 =<asp:TextBox ID="tb_a0" runat="server">0.00001</asp:TextBox>
        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            betta =
            <asp:TextBox ID="tb_betta" runat="server">2</asp:TextBox>
        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        </p>
        <p>
            sigma =
            <asp:TextBox ID="tb_sigma" runat="server">140</asp:TextBox>
        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            mm =
            <asp:TextBox ID="tb_mm" runat="server">10</asp:TextBox>
        </p>
        <p>
            KI_c_min =
            <asp:TextBox ID="tb_KI_c_min" runat="server" Height="21px">10</asp:TextBox>
        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            alpha_alpha =
            <asp:TextBox ID="tb_alpha_alpha" runat="server">2</asp:TextBox>
        </p>
        <p>
            KI_c_max =
            <asp:TextBox ID="tb_KI_c_max" runat="server">80</asp:TextBox>
        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            betta_betta =
            <asp:TextBox ID="tb_betta_betta" runat="server">2</asp:TextBox>
        </p>

        <p aria-checked="undefined">
            Задайте параметры случайного распределения.</p>
        <p aria-checked="undefined">
            Distribution type&nbsp;&nbsp;&nbsp;&nbsp;
            <asp:DropDownList ID="ddl_distributions" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddl_distributions_SelectedIndexChanged">
                <asp:ListItem>Interval</asp:ListItem>
                <asp:ListItem>Constant</asp:ListItem>
                <asp:ListItem>Uniform</asp:ListItem>
                <asp:ListItem>Normal</asp:ListItem>
                <asp:ListItem>Exponential</asp:ListItem>
                <asp:ListItem>Gamma</asp:ListItem>
                <asp:ListItem>Weibull</asp:ListItem>
            </asp:DropDownList>
&nbsp;&nbsp;&nbsp; Distribution value&nbsp;
            <asp:DropDownList ID="ddl_value" runat="server" OnSelectedIndexChanged="ddl_value_SelectedIndexChanged" AutoPostBack="True">
                <asp:ListItem>l0</asp:ListItem>
                <asp:ListItem>delta</asp:ListItem>
                <asp:ListItem>a0</asp:ListItem>
                <asp:ListItem>sigma</asp:ListItem>
                <asp:ListItem>KI_c_min</asp:ListItem>
                <asp:ListItem>KI_c_max</asp:ListItem>
                <asp:ListItem>alpha</asp:ListItem>
                <asp:ListItem>betta</asp:ListItem>
                <asp:ListItem>mm</asp:ListItem>
                <asp:ListItem>alpha_alpha</asp:ListItem>
                <asp:ListItem>betta_betta</asp:ListItem>
            </asp:DropDownList>
        </p>
        <p aria-checked="undefined">
<asp:Label ID="lb_param1" runat="server" Text="From"></asp:Label>
&nbsp;&nbsp;&nbsp;
            <asp:TextBox ID="tb_param1" runat="server" Width="70px">0.1</asp:TextBox>
&nbsp;&nbsp;&nbsp;
            <asp:Label ID="lb_param2" runat="server" Text="To"></asp:Label>
&nbsp;&nbsp;&nbsp;
            <asp:TextBox ID="tb_param2" runat="server" Width="70px">0.94</asp:TextBox>
&nbsp;&nbsp;&nbsp;
            <asp:Label ID="lb_param3" runat="server" Text="Step"></asp:Label>
&nbsp;&nbsp;&nbsp;
            <asp:TextBox ID="tb_param3" runat="server" Width="70px">0.025</asp:TextBox>
&nbsp;&nbsp;&nbsp;
            <asp:Label ID="lb_param4" runat="server" Text="Seed" Visible="False"></asp:Label>
&nbsp;&nbsp;&nbsp; <asp:TextBox ID="tb_param4" runat="server" Visible="False" Width="70px">7</asp:TextBox>
        </p>
        <p aria-checked="undefined">
            Expirement name&nbsp;&nbsp;&nbsp;&nbsp;
            <asp:TextBox ID="tb_name" runat="server"></asp:TextBox>
        &nbsp;&nbsp;&nbsp;
            <asp:Button ID="Button3" runat="server" OnClick="Button3_Click" Text="Start Simulations" />
        &nbsp;&nbsp;&nbsp;
            <asp:Button ID="bt_view" runat="server" OnClick="bt_view_Click" Text="View Result" />
&nbsp;&nbsp;&nbsp;
            <asp:Button ID="bt_droup_data" runat="server" Text="Clear Tables &amp; Queue" OnClick="bt_droup_data_Click" />
        &nbsp;&nbsp;&nbsp;
            <asp:Label ID="lb_result" runat="server"></asp:Label>
        </p>
        <p aria-checked="undefined">
            <asp:Button ID="bt_view_distribution" runat="server" OnClick="bt_view_distribution_Click" Text="View distribution" />
        &nbsp;&nbsp;&nbsp;
            <asp:Button ID="Button4" runat="server" OnClick="Button4_Click" Text="Show From" />
        &nbsp;&nbsp;&nbsp;
            <asp:Button ID="Button5" runat="server" OnClick="Button5_Click" Text="Button" />
        </p>
        <p aria-checked="undefined">
            Start calculation time:&nbsp;&nbsp;&nbsp;
            <asp:Label ID="lb_start" runat="server"></asp:Label>
        </p>
        <p aria-checked="undefined">
            Finish calculation time:&nbsp;&nbsp;&nbsp;
            <asp:Label ID="lb_finish" runat="server"></asp:Label>
        </p>
        <p aria-checked="undefined">
            Calculation time:&nbsp;&nbsp;&nbsp;
            <asp:Label ID="lb_time" runat="server"></asp:Label>
        </p>

        
            <div style="float: left;">
            <asp:Chart ID="crt_distribution" runat="server" Height="500px" Width="1000px">
                <Series>
                    <asp:Series Name="Series1" YValuesPerPoint="2" ChartType="Point">
                    </asp:Series>
                </Series>
                <ChartAreas>
                    <asp:ChartArea Name="ChartArea1">
                    </asp:ChartArea>
                </ChartAreas>
                <Titles>
                    <asp:Title Name="Title1" Text="Случайные числа">
                    </asp:Title>
                </Titles>
            </asp:Chart>
                </div>
            <div style="float: left;">
            </div>
        <div>
            </div>
            <p>
            <asp:Chart ID="Chart1" runat="server" Height="500px" Width="1000px">
                <Series>
                    <asp:Series ChartArea="ChartArea1" ChartType="Spline" Name="Series1">
                    </asp:Series>
                </Series>
                <ChartAreas>
                    <asp:ChartArea Name="ChartArea1">
                        <AxisX IntervalAutoMode="VariableCount" IntervalType="Milliseconds">
                        </AxisX>
                    </asp:ChartArea>
                </ChartAreas>
            </asp:Chart>
            <asp:Chart ID="Chart2" runat="server" Height="500px" Width="1000px">
                <Series>
                    <asp:Series ChartType="Point" Name="Series1">
                    </asp:Series>
                </Series>
                <ChartAreas>
                    <asp:ChartArea Name="ChartArea1">
                    </asp:ChartArea>
                </ChartAreas>
            </asp:Chart>
        </p>
        <p>
            <asp:Chart ID="crt_histogram" runat="server" Height="500px" Width="1000px">
                <Series>
                    <asp:Series Name="Series1" ChartArea="ChartArea1">
                    </asp:Series>
                </Series>
                <ChartAreas>
                    <asp:ChartArea Name="ChartArea1">
                    </asp:ChartArea>
                </ChartAreas>
                <Titles>
                    <asp:Title Name="Title1" Text="Плотность вероятностей">
                    </asp:Title>
                </Titles>
            </asp:Chart>
        </p>
                <asp:Chart ID="crt_distrib_func" runat="server" Height="500px" Width="1000px">
                    <Series>
                        <asp:Series ChartType="Spline" Name="Series1">
                        </asp:Series>
                    </Series>
                    <ChartAreas>
                        <asp:ChartArea Name="ChartArea1">
                        </asp:ChartArea>
                    </ChartAreas>
                    <Titles>
                        <asp:Title Name="Title1" Text="Функция распределения">
                        </asp:Title>
                    </Titles>
        </asp:Chart>
            <asp:GridView ID="gv_simulation" runat="server">
            </asp:GridView>
        Total time simulation:
        <asp:Label ID="lb_total_time" runat="server"></asp:Label>
        <br />
            <asp:GridView ID="gv_data" runat="server">
            </asp:GridView>
    </form>
</body>
</html>
