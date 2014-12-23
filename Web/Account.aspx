<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Account.aspx.cs" Inherits="WizardGame.Account" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        // execute on dom ready
        $(document).ready(function () {
            $(".navbar-nav li").removeClass("active");
            $("#link-account").addClass("active");
        });
    </script>
</asp:Content>
<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="page-header hidden-xs" style="margin-top: 14px;">
            <h1>Manage your account</h1>
        </div>
        <div class="row">
            <div class="tabpanel">
                <!-- Nav tabs -->
                <ul class="nav nav-tabs" role="tablist">
                    <li role="presentation" class="active">
                        <a href="#account" aria-controls="home" role="tab" data-toggle="tab">Account settings</a>
                    </li>
                    <li role="presentation">
                        <a href="#player" aria-controls="player" role="tab" data-toggle="tab">Player settings</a>
                    </li>
                </ul>
                <!-- Tab panes -->
                <div class="tab-content">
                    <div role="tabpanel" class="tab-pane active" id="account">
                        <h4>Account settings</h4>
                        <div class="form-horizontal well well-sm" role="form">
                            <div class="form-group">
                                <label class="col-sm-3 control-label">Username</label>
                                <div class="col-sm-9">
                                    <p class="form-control-static">username</p>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">Email Address</label>
                                <div class="col-sm-9">
                                    <input type="text" class="form-control" id="txtEmailAddress" runat="server" />
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">Password</label>
                                <div class="col-sm-9">
                                    <input type="password" class="form-control" id="txtPassword" runat="server" />
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">Confirm password</label>
                                <div class="col-sm-9">
                                    <input type="password" class="form-control" id="txtPassword2" runat="server" />
                                </div>
                            </div>
                        </div>
                    </div>
                    <div role="tabpanel" class="tab-pane" id="player">
                        <h4>Player settings</h4>
                        <div class="form-horizontal well well-sm" role="form">
                            <div class="form-group">
                                <label class="col-sm-3 control-label">Name</label>
                                <div class="col-sm-9">
                                    <input type="text" class="form-control" id="txtPlayerName" runat="server" />
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">Profile picture</label>
                                <div class="col-sm-9">
                                    <input type="file" class="form-control" id="ProfilePicture" runat="server" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <script>
                $(document).ready(function () {
                    $(".tabpanel").tab({
                    });
                });
                
            </script>
        </div>
</asp:Content>
