<%@ Page Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="HostGame.aspx.cs" Inherits="WizardGame.HostGame" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        // page specific scripts
    </script>
</asp:Content>
<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="page-header" style="margin-top: 14px;">
            <h1>Host a new game</h1>
        </div>
        <div class="form-horizontal" role="form">
            <div id="MessageBox" class="alert alert-danger alert-dismissible fade in" role="alert" runat="server" visible="false">
                <button type="button" class="close" data-dismiss="alert"><span aria-hidden="true">×</span><span class="sr-only">Close</span></button>
                <p id="MessageBoxText" runat="server" />
            </div>
            <div class="form-group">
                <label for="MainContent_txtGameName" class="col-sm-3 control-label">Game name</label>
                <div class="col-sm-9">
                    <input type="text" runat="server" id="txtGameName" name="txtGameName" placeholder="Title of the game lobby" class="form-control" required />
                </div>
            </div>
            <div class="form-group">
                <label for="MainContent_selectMaxPlayers" class="col-sm-3 control-label">Max players</label>
                <div class="col-sm-9">
                    <select id="selectMaxPlayers" name="selectMaxPlayers" runat="server" class="form-control">
                        <option value="3">3</option>
                        <option value="4">4</option>
                        <option value="5">5</option>
                        <option value="6" selected>6</option>
                    </select>
                </div>
            </div>
            <div class="form-group">
                <label for="MainContent_txtPassword" class="col-sm-3 control-label">Optional password</label>
                <div class="col-sm-9">
                    <input type="password" runat="server" id="txtPassword" name="txtPassword" placeholder="Optional password to keep game lobby private" class="form-control" />
                </div>
            </div>
            <div class="form-group">
                <div class="col-sm-offset-3 col-sm-9">
                    <asp:Button ID="btnHostGame" runat="server" CssClass="btn btn-lg btn-primary" Text="Host game" OnClick="btnHostGame_Click" />
                    <a href="Home.aspx" class="btn btn-lg btn-warning">Cancel</a>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
