Imports Microsoft.Win32
Imports System.IO

Public Class Options
#Region "Locals"
    Dim DriveString As String = Mid(Environment.GetFolderPath(Environment.SpecialFolder.System), 1, 3)
    Dim Reg As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run", True) : Dim Key As Object = Registry.CurrentUser.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run", True).GetValue("ProbablyEngineUpdater Startup")
#End Region

    Private Sub Options_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        'Save Settings
        If CheckBox1.Checked Then : My.Settings.Startup = True : Else : My.Settings.Startup = False : End If 'Windows Startup
        If CheckBox2.Checked Then : My.Settings.AutoSync = True : Else : My.Settings.AutoSync = False : End If 'Auto Sync Current Version
        If CheckBox3.Checked Then : My.Settings.AutoUpdates = True : Else : My.Settings.AutoUpdates = False : End If 'Automatic Updates
        If CheckBox4.Checked Then : My.Settings.BetaChecks = True : Else : My.Settings.BetaChecks = False : End If 'Check for Beta Releases
        If CheckBox5.Checked Then : My.Settings.DisplayNotif = True : Else : My.Settings.DisplayNotif = False : End If 'Display Notifications
        If CheckBox6.Checked Then : My.Settings.Minimize = True : Else : My.Settings.Minimize = False : End If 'Minimize to System Tray.
        If Not TextBox1.Text Is Nothing Then : My.Settings.WoWDirectory = TextBox1.Text : End If ' WoW Directory String
        If CheckBox2.Checked Then : My.Settings.AutoUp = TrackBar2.Value : Else : My.Settings.AutoUp = "5" : End If
        My.Settings.Save()

        If Not IO.Directory.Exists(My.Settings.WoWDirectory & "\Interface\AddOns") Then : e.Cancel = True : MsgBox("Please select a valid WoW Directory!" & Environment.NewLine & "Proper Path Example: " & DriveString & "Program Files\World of Warcraft") : End If

        If Main.FirstRun = True Then
            If IO.File.Exists(My.Settings.WoWDirectory & "\Interface\AddOns\Probably\probably.lua") Then
                Dim Code As String = File.ReadAllText(My.Settings.WoWDirectory & "\Interface\AddOns\Probably\probably.lua")
                'Fetch Versions, from online and locally through regex. 
                Main.Label2.Text = "Local Version: " & Main.Regex(Code) : Main.Label1.Text = "Current Version: " & "r" & Main.ReleaseVer : If Not Main.BetaVer = "1" Then : Main.Label1.Text = "Current Version: " & "r" & Main.BetaVer : End If

                'Check for Release Updates.
                If Main.Regex(Code) < "r" & Main.ReleaseVer Then
                    Main.Button1.Enabled = True : Main.ToolStripStatusLabel1.Text = "Status: Update Available, woohoo!" & " | " & "Release Version: " & "r" & Main.ReleaseVer : Else : Main.Button1.Enabled = False : Main.ToolStripStatusLabel1.Text = "Status: No Update Available :("
                End If
            Else : Main.Label2.Text = "Local Version: " & "Not Installed..." : Main.Button1.Enabled = True : Main.ToolStripStatusLabel1.Text = "Status: Addon needs to be installed!" : Main.Label1.Text = "Current Version: " & "r" & Main.ReleaseVer : If Not Main.BetaVer = "1" Then : Main.Label1.Text = "Current Version: " & "b" & Main.BetaVer : End If
            End If
        End If
        'Set or disable startup.
        If My.Settings.Startup = True Then : If Key Is Nothing Then : Reg.SetValue("ProbablyEngineUpdater Startup", """" & Application.ExecutablePath.ToString() & """") : End If : Else : If Key Is Nothing Then : Else : Reg.DeleteValue("ProbablyEngineUpdater Startup", False) : End If : End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        'Folder dialog handles for WoW Folder.
        Dim FBD As New FolderBrowserDialog : FBD.RootFolder = Environment.SpecialFolder.MyComputer : FBD.ShowDialog() : If Not IO.Directory.Exists(FBD.SelectedPath & "\Interface\AddOns") Then : MsgBox("Not a valid World of Warcraft directory!" & Environment.NewLine & "Proper Path Example: " & DriveString & "Program Files\World of Warcraft", MsgBoxStyle.Critical, "Error") : Else : TextBox1.Text = FBD.SelectedPath : End If
    End Sub

    Private Sub Options_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Load Settings
        If Not My.Settings.AutoUp = "0" Then : TrackBar2.Value = My.Settings.AutoUp : Label2.Text = My.Settings.AutoUp & " minute(s)" : Else : TrackBar2.Value = "5" : Label2.Text = TrackBar2.Value & " minute(s)" : End If : If Not My.Settings.WoWDirectory Is Nothing Then : TextBox1.Text = My.Settings.WoWDirectory : Else : TextBox1.Text = DriveString : End If : If My.Settings.Startup Then : CheckBox1.Checked = True : End If : If My.Settings.AutoSync Then : CheckBox2.Checked = True : End If : If My.Settings.AutoUpdates = True Then : CheckBox3.Checked = True : End If : If My.Settings.BetaChecks = True Then : CheckBox4.Checked = True : End If : If My.Settings.DisplayNotif = True Then : CheckBox5.Checked = True : End If : If My.Settings.Minimize = True Then : CheckBox6.Checked = True : End If
    End Sub

    Private Sub TrackBar2_Scroll(sender As Object, e As EventArgs) Handles TrackBar2.Scroll
        Label2.Text = TrackBar2.Value & " minute(s)"
    End Sub
End Class