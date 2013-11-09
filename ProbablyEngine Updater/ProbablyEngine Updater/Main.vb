#Region "Imports"
Imports System.Net
Imports System.Text.RegularExpressions
Imports ComponentAce.Compression.ZipForge
Imports ComponentAce.Compression.Archiver
Imports System.IO

#End Region
Public Class Main
#Region "Values"
    Public UpdateVersionWeb As New WebClient : Public ReleaseVer As String = UpdateVersionWeb.DownloadString("http://probablyengine.com/updates/release.txt") : Public BetaVer As String = UpdateVersionWeb.DownloadString("http://probablyengine.com/updates/beta.txt")
    Dim DriveString As String = Mid(Environment.GetFolderPath(Environment.SpecialFolder.System), 1, 3)
    Dim ReleaseLink As String = "http://probablyengine.com/updates/release.zip" : Dim BetaLink As String = "http://probablyengine.com/updates/beta.zip"
    Dim archiver As ZipForge = New ZipForge
    Public FirstRun As Boolean = False
    Dim ZipRelease As String = "release.zip" : Dim ZipBeta As String = "beta.zip"
#End Region
#Region "Functions & Subs"
    Public Function Regex(Str As String) As String
        Dim rx As New Regex("[r|b]([0-9]{4})") : Dim m As Match = rx.Match(Str)
        If (m.Success) Then : Return m.Value : Else : Return False : End If
    End Function
    Delegate Sub DownloadCompleteSafe(ByVal cancelled As Boolean)
#End Region

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'StreamReader to fetch the file information.
        If IO.Directory.Exists(My.Settings.WoWDirectory & "\Interface\AddOns") Then
            If Not IO.File.Exists(My.Settings.WoWDirectory & "\Interface\AddOns\Probably\probably.lua") Then
                Label2.Text = "Local Version: " & "Not Installed..." : Button1.Enabled = True : Button1.Text = "Install!" : ToolStripStatusLabel1.Text = "Status: Addon needs to be installed!" : Label1.Text = "Current Version: " & "r" & ReleaseVer : If Not BetaVer = "1" Then : Label1.Text = "Current Version: " & "b" & BetaVer : End If
            Else
                Dim Code As String = File.ReadAllText(My.Settings.WoWDirectory & "\Interface\AddOns\Probably\probably.lua")
                'Fetch Versions, from online and locally through regex. 
                Label2.Text = "Local Version: " & Regex(Code) : Label1.Text = "Current Version: " & "r" & ReleaseVer : If Not BetaVer = "1" Then : Label1.Text = "Current Version: " & "b" & BetaVer : End If

                'Check for Release Updates.
                If Regex(Code) < "r" & ReleaseVer Then
                    Button1.Enabled = True : ToolStripStatusLabel1.Text = "Status: Update Available, woohoo!" & " | " & "Release Version: " & "r" & ReleaseVer : Else : Button1.Enabled = False : ToolStripStatusLabel1.Text = "Status: No Update Available!"
                End If

                'Check for Beta Updates.
                If My.Settings.BetaChecks Then
                    If Not BetaVer = "1" Then : If Regex(Code) < "b" & BetaVer Then : Button1.Enabled = True : ToolStripStatusLabel1.Text = "Status: Beta Update Available, woohoo!" & " | " & "Beta Version: " & "r" & BetaVer : End If : End If
                End If
            End If
        Else : If My.Settings.WoWDirectory = "" Then : MsgBox("Please select a World of Warcraft directory!", MsgBoxStyle.Information, "Important!") : Options.Show() : FirstRun = True : End If
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Sync.Tick
        If My.Settings.AutoSync = True Then
            If IO.File.Exists(My.Settings.WoWDirectory & "\Interface\AddOns\Probably\probably.lua") Then
                Sync.Interval = My.Settings.AutoUp * 60000
                Label1.Text = "Current Version: " & "r" & ReleaseVer
                Dim Contents As String = File.ReadAllText(My.Settings.WoWDirectory & "\Interface\AddOns\Probably\probably.lua") : Dim LocalVur As String = Contents
                If Regex(LocalVur) < "r" & ReleaseVer Then
                    Button1.Enabled = True : ToolStripStatusLabel1.Text = "Status: Update Available, woohoo!" & " | " & "Release Version: " & "r" & ReleaseVer : Else : Button1.Enabled = False : ToolStripStatusLabel1.Text = "Status: No Update Available!"
                End If
            End If
        End If
    End Sub

    Private Sub OptionsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OptionsToolStripMenuItem.Click
        Options.Show()
    End Sub

    Private Sub AboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AboutToolStripMenuItem.Click
        About.Show()
    End Sub

    Private Sub CloseToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CloseToolStripMenuItem.Click
        Application.Exit()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If My.Settings.WoWDirectory = "" Or Not IO.Directory.Exists(My.Settings.WoWDirectory & "\Interface\AddOns") Then
            MsgBox("Not a valid World of Warcraft directory!" & Environment.NewLine & "Proper Path Example: " & DriveString & "Program Files\World of Warcraft", MsgBoxStyle.Critical, "Error") : Options.Show()
        Else
            'Download release or update zip.
            Using wClient As New System.Net.WebClient
                AddHandler wClient.DownloadFileCompleted, AddressOf Completed_Download
                If BetaVer = "1" Then
                    wClient.DownloadFileAsync(New Uri(ReleaseLink), My.Settings.WoWDirectory & "\Interface\AddOns\" & ZipRelease) : Else : wClient.DownloadFileAsync(New Uri(BetaLink), My.Settings.WoWDirectory & "\Interface\AddOns\" & ZipBeta)
                End If
            End Using
        End If
    End Sub
    Sub Completed_Download(ByVal sender As Object, ByVal e As System.ComponentModel.AsyncCompletedEventArgs)
        If e.Error Is Nothing Then
            If My.Computer.FileSystem.DirectoryExists(My.Settings.WoWDirectory & "\Interface\AddOns\Probably") Then : System.IO.Directory.Delete(My.Settings.WoWDirectory & "\Interface\AddOns\Probably", True) : End If
            Try
                If BetaVer = "1" Then
                    archiver.FileName = My.Settings.WoWDirectory & "\Interface\AddOns\" & ZipRelease : Else : archiver.FileName = My.Settings.WoWDirectory & "\Interface\AddOns\" & ZipBeta
                End If
                archiver.OpenArchive(System.IO.FileMode.Open) : archiver.BaseDir = My.Settings.WoWDirectory & "\Interface\AddOns\" : archiver.ExtractFiles("*.*") : archiver.CloseArchive()
            Catch ae As ArchiverException
                MsgBox(ae.Message, ae.ErrorCode)
            End Try

            If My.Computer.FileSystem.FileExists(My.Settings.WoWDirectory & "\Interface\AddOns\" & ZipRelease) Then : System.IO.File.Delete(My.Settings.WoWDirectory & "\Interface\AddOns\" & ZipRelease) : End If : If My.Computer.FileSystem.FileExists(My.Settings.WoWDirectory & "\Interface\AddOns\" & ZipBeta) Then : System.IO.File.Delete(My.Settings.WoWDirectory & "\Interface\AddOns\" & ZipBeta) : End If
            If IO.File.Exists(My.Settings.WoWDirectory & "\Interface\AddOns\Probably\probably.lua") Then
                Dim Code As String = File.ReadAllText(My.Settings.WoWDirectory & "\Interface\AddOns\Probably\probably.lua")
                Label2.Text = "Local Version: " & Regex(Code) : Button1.Text = "Update!" : Button1.Enabled = False : ToolStripStatusLabel1.Text = "Status: No Update Available!"
            End If
        End If
    End Sub

    Private Sub Timer1_Tick_1(sender As Object, e As EventArgs) Handles Refresh.Tick
        If Not IO.File.Exists(My.Settings.WoWDirectory & "\Interface\AddOns\Probably\probably.lua") Then
            Label2.Text = "Local Version: " & "Not Installed..." : Button1.Enabled = True : Button1.Text = "Install!" : ToolStripStatusLabel1.Text = "Status: Addon needs to be installed!"
        Else
            Dim Code As String = File.ReadAllText(My.Settings.WoWDirectory & "\Interface\AddOns\Probably\probably.lua")
            If My.Settings.AutoUpdates And Regex(Code) < "r" & ReleaseVer Or Regex(Code) < "b" & BetaVer Then ' Auto Updates
                Refresh.Interval = My.Settings.AutoUp * 60000
                If My.Settings.WoWDirectory = "" Or Not IO.Directory.Exists(My.Settings.WoWDirectory & "\Interface\AddOns") Then
                    MsgBox("Not a valid World of Warcraft directory!" & Environment.NewLine & "Proper Path Example: " & DriveString & "Program Files\World of Warcraft", MsgBoxStyle.Critical, "Error") : Options.Show()
                Else
                    'Download release or beta zip.
                    Using wClient As New System.Net.WebClient
                        AddHandler wClient.DownloadFileCompleted, AddressOf Completed_Download
                        If BetaVer = "1" Then
                            wClient.DownloadFileAsync(New Uri(ReleaseLink), My.Settings.WoWDirectory & "\Interface\AddOns\" & ZipRelease) : Else : wClient.DownloadFileAsync(New Uri(BetaLink), My.Settings.WoWDirectory & "\Interface\AddOns\" & ZipBeta)
                        End If
                        If My.Settings.DisplayNotif Then : NotifyIcon1.ShowBalloonTip("10", "Automatic Update", "ProbablyEngine has been updated to version: " & "r" & ReleaseVer, ToolTipIcon.Info) : End If
                    End Using
                End If
            End If
        End If
    End Sub

    Private Sub Main_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If My.Settings.Minimize Then
            If Me.WindowState = FormWindowState.Minimized Then
                Me.WindowState = FormWindowState.Minimized
                NotifyIcon1.Visible = True
                Me.Hide()
                NotifyIcon1.ShowBalloonTip("5", "ProbablyEngine Updater", "Minimized to system tray.", ToolTipIcon.Info)
            End If
        End If
    End Sub

    Private Sub ShowToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ShowToolStripMenuItem.Click
        Me.Show()
        Me.WindowState = FormWindowState.Normal
        NotifyIcon1.Visible = False
    End Sub

    Private Sub CloseToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles CloseToolStripMenuItem1.Click
        Application.Exit()
    End Sub

    Private Sub NotifyIcon1_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon1.MouseDoubleClick
        Me.Show()
        Me.WindowState = FormWindowState.Normal
        NotifyIcon1.Visible = False
    End Sub
End Class