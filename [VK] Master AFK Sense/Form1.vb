Imports System.ComponentModel
Imports System.Media
Imports System.Speech.Synthesis
Public Class Form1
    Protected PID As Integer
    Protected hProcPID As Integer
    Protected VoiceAlerts As Boolean = False
    Protected ARnAFK As Boolean = False
    Protected Rev As Integer = 1
    Protected Interval As Integer = 1
    Protected AC As Boolean = False
    Protected ACInterval As Integer = 1
    Protected VoiceName As String = Nothing
    Protected HPThreshold1 As Integer = 80
    Protected HPThreshold2 As Integer = 50
    Protected EliteNotify As Boolean = False
    Protected EliteHPThreshold As Integer = 30
    Protected AutoClosed As Boolean = False
    Protected prevtarg As String = ""
    Private WithEvents TextToSpeech As New SpeechSynthesizer
    Private Sub SayText(ByVal TextToSay As String)
        Debug.WriteLine("SayText: " & TextToSay)
        Try
            If VoiceAlerts = False Then
                Exit Sub
            End If
            If TextToSpeech Is Nothing Then TextToSpeech = New SpeechSynthesizer
            TextToSpeech.SelectVoice(VoiceName)
            TextToSpeech.SpeakAsync(TextToSay)
        Catch ex As Exception
            Debug.WriteLine("SayText Error : " & ex.Message)
        End Try
    End Sub

    Private Function ConvertToWord(ByVal status As Boolean)
        Dim str As String = "Disabled"
        If status = True Then
            str = "Enabled"
        End If
        Return str
    End Function

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        On Error GoTo ShowError
        '                                  0      1    2    3   4    5    6           7             8     9    10      11
        '                     Command()  PID   VA  ARnAFK RevM RevI  AC ACInt       VAName         HPTH1 HPTH2 ENotif EHPTH
        Dim cmd As String = Command() '"[VK]8544 -True -True -1 -1 -True -1 -Microsoft|Zira|Desktop -80 -50 -True -30"
        If cmd.Contains("[VK]") Then
            Dim Arguments() As String = Split(cmd)
            'PID
            PID = Arguments(0).Replace("[VK]", "")
            hProcPID = OpenProcess(2035711, 0, PID)
            'VA
            If Arguments(1) = "-True" Then
                'VAName
                If Arguments(7) <> "" Then
                    VoiceName = Arguments(7).Replace("-", "").Replace("|", " ")
                    VoiceAlerts = True
                Else
                    GoTo ShowError
                End If
            End If
            'ARnAFK
            If Arguments(2) = "-True" Then
                'Rev Mode
                Select Case Arguments(3)
                    Case "-1"
                        Rev = 1
                    Case "-2"
                        Rev = 2
                    Case Else
                        GoTo ShowError
                End Select
                If Arguments(4) <> "" Then
                    ARnAFK = True
                    'Int
                    Interval = Arguments(4).Replace("-", "")
                Else
                    GoTo ShowError
                End If
            End If
            'AC
            If Arguments(5) = "-True" Then
                'ACInt
                If Arguments(6) <> "" Then
                    ACInterval = Arguments(6).Replace("-", "")
                    AC = True
                Else
                    GoTo ShowError
                End If
            End If
            'HPTH1
            If Arguments(8) <> "" Then
                HPThreshold1 = Arguments(8).Replace("-", "")
            Else
                GoTo ShowError
            End If
            'HPTH2
            If Arguments(9) <> "" Then
                HPThreshold2 = Arguments(9).Replace("-", "")
            Else
                GoTo ShowError
            End If
            'EliteNotify
            If Arguments(10) = "-True" Then
                If Arguments(11) <> "" Then
                    EliteNotify = True
                    'EliteHPTH
                    EliteHPThreshold = Arguments(11).Replace("-", "")
                Else
                    GoTo ShowError
                End If
            End If

            'Set Voice Alerts and Narrator
            If VoiceAlerts = True Then
                Label14.Text = "ENABLED"
            Else
                Label14.Text = "DISABLED"
            End If
            If VoiceName = "" Then
                Label15.Text = "UNDEFINED"
            Else
                Label15.Text = VoiceName
            End If
            'Report Recieved Arguments
            Debug.WriteLine(PID & " " & VoiceAlerts & " " & ARnAFK & " " & Rev & " " & Interval & " " & AC & " " & ACInterval & " " & HPThreshold1 & " " & HPThreshold2 & " " & EliteNotify & " " & EliteHPThreshold)
            'Start
            BackgroundWorker1.RunWorkerAsync()
            BackgroundWorker2.RunWorkerAsync()
            BackgroundWorker3.RunWorkerAsync()
        Else
ShowError:
            MessageBox.Show("The program failed To initialize. No rights To open this program.", ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Close()
        End If
    End Sub

    Protected AFKSenseMode As Boolean = True
    Protected AlrmTimeout As Integer
    Protected CharName As String = ""
    Protected SafeCharName As String
    Protected ARnAFKInterval As Integer
    Protected AFKResult As Integer = 0
    Protected CurrentHP As Integer
    Protected TotalHP As Integer
    Protected TName As String = ""
    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Debug.WriteLine("Start " & hProcPID)
        Try
            Dim Xcur, X, YCur, Y As Single
            Dim timeout As Integer = 20
            Dim stuck As Boolean = False
            Dim num10 As Integer = Nothing
            Dim num3 As Integer = Nothing
            Dim C As Integer = Nothing
            Dim Ccur As Integer = Nothing
            ARnAFKInterval = Interval * 60
            Dim PTHP As Integer
            Dim HP, MaxHP, HPtick As Integer
            Dim dcase As Integer = 1
            Dim died As Integer = 0
            Dim AFKX As Single
            Dim AFKY As Single
            Dim AFKStatus As Boolean = False
            HPtick = 0
            AlrmTimeout = 0
            CharName = targetname(hProcPID)
            Label1.Text = CharName
            SafeCharName = GetSafeString(CharName)
            Debug.WriteLine(CharName & " " & SafeCharName)
            'Update UI
            BackgroundWorker1.ReportProgress(0)
            BackgroundWorker1.ReportProgress(3, CharName)
            Do
                If AFKSenseMode = False Then
                    If AutoClosed = True Then
                        'Reset Everything
                        BackgroundWorker1.ReportProgress(23)
                        BackgroundWorker1.ReportProgress(33)
                        BackgroundWorker1.ReportProgress(55)
                    End If
                    Exit Do
                End If
                Try
                    If CheckPID(PID) = True Then
                        'Report Available
                        BackgroundWorker1.ReportProgress(11)

                        'Report Auto Rev n AFK Enabled or Disabled
                        If ARnAFK = True Then
                            BackgroundWorker1.ReportProgress(31)
                        Else
                            BackgroundWorker1.ReportProgress(33)
                        End If

                        'Report Rev Mode
                        Select Case Rev
                            Case 1
                                BackgroundWorker1.ReportProgress(41)
                            Case 2
                                BackgroundWorker1.ReportProgress(42)
                        End Select

                        'Report Elite Notify
                        If EliteNotify = True AndAlso Not (TName.Contains("[Elite]") Or TName.Contains("[E]")) Then
                            'Report EliteNotify Enabled
                            BackgroundWorker1.ReportProgress(51)
                            BackgroundWorker1.ReportProgress(53, False)
                        ElseIf EliteNotify = False Then
                            'Report EliteNotify Disabled
                            BackgroundWorker1.ReportProgress(55)
                        End If

                        'Report Auto Close
                        If AC = True Then
                            'Report Auto Close Death Remaining
                            BackgroundWorker1.ReportProgress(61, died & " / " & ACInterval & " deaths")
                            If died = ACInterval Then
                                BackgroundWorker1.ReportProgress(4)
                            End If
                        Else
                            'Report Auto Close DISABLED
                            BackgroundWorker1.ReportProgress(62)
                        End If

                        'Get AFK Status
                        If ReadProcessMemory(hProcPID, &H15A4819, AFKResult, 1, Nothing) = 1 Then
                            Threading.Thread.VolatileWrite(AFKResult, AFKResult)
                            'Get AFK Coordinates----------------------------------------------------------------------------
                            If AFKResult = 1 AndAlso AFKStatus = False Then
                                AFKStatus = True
                                ReadProcessMemoryS(hProcPID, &H15A4820, AFKX, 4, Nothing)
                                ReadProcessMemoryS(hProcPID, &H15A4828, AFKY, 4, Nothing)
                                Debug.WriteLine(AFKX & ", " & AFKY)
                            ElseIf AFKResult = 0 AndAlso AFKStatus = True Then
                                AFKStatus = False
                            End If

                            Select Case AFKResult
                                Case 0
                                    Debug.WriteLine("AFK: OFF")
                                    stuck = False
                                    BackgroundWorker1.ReportProgress(0)
                                    'Report AFK Inactive
                                    BackgroundWorker1.ReportProgress(23)

                                Case 1
                                    'Report AFK Active
                                    BackgroundWorker1.ReportProgress(21)
                                    'Get HP------------------------------------------------------------------------------
                                    ReadProcessMemory(hProcPID, &H15763B4, PTHP, 4, Nothing)
                                    ReadProcessMemory(hProcPID, PTHP + 4, HP, 4, Nothing)
                                    ReadProcessMemory(hProcPID, PTHP + 8, MaxHP, 4, Nothing)
                                    Debug.WriteLine("HP: " & HP & " / " & MaxHP & " = " & Math.Round((HP / MaxHP) * 100))
                                    If HP = 0 Then
                                        If AFKResult = 1 Then
                                            If ARnAFK = True AndAlso AutoClosed = False Then
                                                ARnAFKInterval -= 1
                                                'Report Auto Rev n AFK Reviving Countdown
                                                BackgroundWorker1.ReportProgress(32, "Reviving in " & ARnAFKInterval)
                                            End If
                                        End If
                                        stuck = True
                                    Else
                                        'Check HP Thresholds
                                        Select Case HPtick
                                            Case 0
                                                If HP < MaxHP * (HPThreshold1 / 100) Then
                                                    HPtick = 1
                                                    BackgroundWorker1.ReportProgress(5, CharName & "'s HP is below " & HPThreshold1 & "%")
                                                End If
                                            Case 1
                                                If HP < MaxHP * (HPThreshold2 / 100) Then
                                                    If Not HPtick = 2 Then
                                                        HPtick = 2
                                                        BackgroundWorker1.ReportProgress(5, CharName & "'s HP is below " & HPThreshold2 & "%")
                                                    End If
                                                Else
                                                    If HP = MaxHP Or HP >= MaxHP * ((HPThreshold1 + 5) / 100) Then
                                                        HPtick = 0
                                                    End If
                                                End If
                                            Case 2
                                                If HP > MaxHP * ((HPThreshold2 + 5) / 100) Then
                                                    HPtick = 1
                                                End If
                                        End Select
                                        ARnAFKInterval = Interval * 60
                                    End If

                                    'Check if Stuck------------------------------------------------------------------
                                    If timeout = 0 Then
                                        Debug.WriteLine("Trigger")
                                        timeout = 10
                                        stuck = True
                                    Else
                                        Dim PX1, PX2, PX3 As Integer
                                        ReadProcessMemory(hProcPID, &H15762F4, PX1, 4, Nothing)
                                        PX1 += 4
                                        ReadProcessMemory(hProcPID, PX1, PX2, 4, Nothing)
                                        PX2 += &H1F4
                                        ReadProcessMemory(hProcPID, PX2, PX3, 4, Nothing)
                                        ReadProcessMemoryS(hProcPID, PX3 + &H14, Xcur, 4, Nothing)
                                        ReadProcessMemoryS(hProcPID, PX3 + &H1C, YCur, 4, Nothing)
                                        'Round X and Y
                                        Xcur = Math.Round(Xcur, 0)
                                        YCur = Math.Round(YCur, 0)
                                        'Get Character Action (Moving/Channeling/etc)
                                        ReadProcessMemory(hProcPID, &H1575EAC, num10, 4, Nothing)
                                        num3 = num10 + &H14
                                        ReadProcessMemory(hProcPID, num3, Ccur, 1, Nothing)
                                        'Check if Position is the same   Check if Action is thesame
                                        If X = Xcur AndAlso Y = YCur AndAlso C = Ccur Then
                                            timeout -= 1
                                        Else
                                            timeout = 20
                                            stuck = False
                                            BackgroundWorker1.ReportProgress(2)
                                        End If
                                        'Replace X,Y with Current Position and C with Current Action
                                        X = Xcur
                                        Y = YCur
                                        C = Ccur
                                    End If

                                    'Auto Close----------------------------------------------------------------
                                    If dcase = 1 AndAlso HP = 0 Then
                                        dcase = 0
                                        died += 1
                                        If AC = True AndAlso ACInterval = died Then
                                            Debug.WriteLine("Process Kill Command")
                                            Dim client As Process = Process.GetProcessById(PID)
                                            client.CloseMainWindow()
                                        End If
                                    ElseIf Not HP = 0 AndAlso dcase = 0 Then
                                        dcase = 1
                                    End If
                                    Debug.WriteLine("Died : " & died)

                                    'Auto Revive and ReAFK-----------------------------------------------------
                                    Debug.WriteLine("ARnAFKInterval : " & ARnAFKInterval)
                                    If ARnAFK = True AndAlso ARnAFKInterval <= 0 AndAlso HP = 0 AndAlso AutoClosed = True Then
                                        Debug.WriteLine("Respawn Process Start")
                                        'Report Auto Rev n AFK Reviving
                                        BackgroundWorker1.ReportProgress(32, "Reviving")
                                        'AFK Off
                                        WriteProcessMemory(hProcPID, &H15A4819, 0, 1, Nothing)
                                        'Respawn On
                                        Dim Pointer As Integer
                                        ReadProcessMemory(hProcPID, 22504328, Pointer, Translate("34"), Nothing)
                                        Select Case Rev
                                            Case 1
                                                Debug.WriteLine("Silver Revive")
                                                WriteProcessMemory(hProcPID, Pointer, 1, Translate("34"), Nothing)
                                                WriteProcessMemory(hProcPID, 4656044, 17070208, Translate("34"), Nothing)
                                            Case 2
                                                Debug.WriteLine("Gold Revive")
                                                WriteProcessMemory(hProcPID, Pointer, 0, Translate("34"), Nothing)
                                                WriteProcessMemory(hProcPID, 4656044, 17070208, Translate("34"), Nothing)
                                        End Select
                                        'If hp still 0 for 10 sec Then perform free revive
                                        Dim reviveinterval As Integer = 10
                                        Do While HP = 0
                                            ReadProcessMemory(hProcPID, PTHP + 4, HP, 4, Nothing)
                                            If reviveinterval <= 0 Then
                                                BackgroundWorker1.ReportProgress(43)
                                                Debug.WriteLine("Free Revive")
                                                WriteProcessMemory(hProcPID, Pointer, 2, Translate("34"), Nothing)
                                                WriteProcessMemory(hProcPID, 4656044, 17070208, Translate("34"), Nothing)
                                                Threading.Thread.Sleep(1000)
                                                Debug.WriteLine("Fast Revive Off")
                                                WriteProcessMemory(hProcPID, 4656044, 292992, Translate("34"), Nothing)
                                                BackgroundWorker1.ReportProgress(5, CharName & " failed to revive using the selected method. Free revive is used instead. Auto-AFK will now be disabled.")
                                                'Reset
                                                ARnAFKInterval = Interval * 60
                                                Debug.WriteLine("Respawn Process Detour Complete")
                                                GoTo UIDetour
                                            Else
                                                reviveinterval -= 1
                                            End If
                                            Debug.WriteLine("Revive Interval : " & reviveinterval)
                                            Threading.Thread.Sleep(1000)
                                        Loop
                                        Threading.Thread.Sleep(1000)
                                        Debug.WriteLine("Fast Revive Off")
                                        WriteProcessMemory(hProcPID, 4656044, 292992, Translate("34"), Nothing)
                                        Debug.WriteLine("Apply AFK Coordinates")
                                        WriteProcessMemoryS(hProcPID, &H15A4820, AFKX, 4, Nothing)
                                        WriteProcessMemoryS(hProcPID, &H15A4828, AFKY, 4, Nothing)
                                        Debug.WriteLine("Reactivate AFK")
                                        WriteProcessMemory(hProcPID, &H15A4819, 1, 1, Nothing)
                                        'Reset
                                        ARnAFKInterval = Interval * 60
                                        Debug.WriteLine("Respawn Process Complete")
                                    End If

                                    'EliteNotify
                                    If EliteNotify = True Then
                                        'Begin
                                        Dim Pointer, PT1, PT2, PT3 As Integer

                                        Dim Chr As Single
                                        Dim Max As Integer
                                        Dim Cur As Integer
                                        ReadProcessMemory(hProcPID, &H15763C8, Pointer, 4, Nothing)

                                        Chr = 0
                                        TName = ""
                                        ReadProcessMemory(hProcPID, &H15763C8, PT1, 4, Nothing)
                                        PT1 += &H20
                                        ReadProcessMemory(hProcPID, PT1, PT2, 4, Nothing)
                                        PT2 += &H58
                                        ReadProcessMemory(hProcPID, PT2, PT3, 4, Nothing)
                                        Do
                                            ReadProcessMemory(hProcPID, PT3, Chr, 1, Nothing)
                                            If Not Chr = 0 Then
                                                If Not Chr = 10 Then ' Chr 10 is Environment.NewLine
                                                    TName &= Convert.ToString(Convert.ToChar(CInt(Chr)))
                                                End If
                                                PT3 += 2
                                            Else
                                                Exit Do
                                            End If
                                        Loop
                                        If TName = "" Then
                                            TName = "No Target"
                                            Cur = 0
                                            Max = 1
                                        Else
                                            ReadProcessMemory(hProcPID, Pointer + &H8, Cur, 4, Nothing)
                                            ReadProcessMemory(hProcPID, Pointer + &HC, Max, 4, Nothing)
                                        End If
                                        'Process Result
                                        If TName.Contains("[Elite]") Or TName.Contains("[E]") Then
                                            'Report EliteNotify Active
                                            BackgroundWorker1.ReportProgress(52)
                                            BackgroundWorker1.ReportProgress(53, True)
                                            BackgroundWorker1.ReportProgress(54, (Cur / Max) * 100)
                                        End If
                                        If Not prevtarg = TName Then
                                            prevtarg = TName
                                            Debug.WriteLine("New Target: " & TName)
                                        End If

                                    End If
                            End Select
UIDetour:
                            'UI
                            If stuck = True Then
                                'Report AFK Stucked
                                BackgroundWorker1.ReportProgress(22)
                                If HP = 0 Then
                                    If AFKResult = 1 AndAlso ARnAFK = True Then
                                        Dim revmsg As String = ""
                                        Select Case Rev
                                            Case 1
                                                revmsg &= "Silver revive in " & ARnAFKInterval & " seconds"
                                            Case 2
                                                revmsg &= "Gold revive in " & ARnAFKInterval & " seconds"
                                        End Select
                                        BackgroundWorker1.ReportProgress(1, CharName & " was killed. " & revmsg)
                                    Else
                                        BackgroundWorker1.ReportProgress(1, CharName & " was killed")
                                    End If
                                Else
                                    BackgroundWorker1.ReportProgress(1, "Check " & CharName & "'s client")
                                End If
                            ElseIf AFKResult = 1 Then
                                Debug.WriteLine("Trigger in: " & timeout)
                            End If
                        Else
                            If ACInterval = died Then
                                'Report Auto Close Death Remaining
                                BackgroundWorker1.ReportProgress(61, died & " / " & ACInterval & " deaths")
                                'VK Initiated
                                BackgroundWorker1.ReportProgress(4)
                            Else
                                'Process id no longer valid
                                BackgroundWorker1.ReportProgress(1, CharName & "'s client is closed")
                                BackgroundWorker1.ReportProgress(12)
                            End If
                        End If
                    Else
                        'Process id no longer valid
                        BackgroundWorker1.ReportProgress(1, CharName & "'s client is closed")
                        BackgroundWorker1.ReportProgress(12)
                    End If
                    Debug.WriteLine("- - - - - - - - - - - - - - - - - - - - - - - - - - -")
                Catch ex As Exception
                    BackgroundWorker1.ReportProgress(404, "Stage2 " & ex.Message)
                End Try
                Threading.Thread.Sleep(1000)
            Loop
            BackgroundWorker1.ReportProgress(0)
        Catch ex As Exception
            BackgroundWorker1.ReportProgress(404, "Stage1" & ex.Message)
        End Try
    End Sub

    Protected AlrmIcon As Integer = 2
    Protected WhiteIcon As Icon = My.Resources.VKIconW
    Protected GreenIcon As Icon = My.Resources.VKIconG
    Protected RedIcon As Icon = My.Resources.VKIconR
    Protected NIName As String
    Protected EnotifTrigrd As Boolean = False
    Protected SP As SoundPlayer
    Private Sub BackgroundWorker1_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged
        Try
            Select Case e.ProgressPercentage
                Case 0
                    'AFK Status Disabled
                    If Not NotifyIcon1.Text = NIName & " (Passive)" Then
                        NotifyIcon1.Icon = WhiteIcon
                        NotifyIcon1.Text = NIName & " (Passive)"
                        AlrmTimeout = 0
                    End If
                Case 1
                    'Animate Tray Icon
                    Select Case AlrmIcon
                        Case 1
                            AlrmIcon = 2
                            NotifyIcon1.Icon = WhiteIcon
                        Case 2
                            AlrmIcon = 1
                            NotifyIcon1.Icon = RedIcon
                    End Select
                    'Alarm
                    If AlrmTimeout = 0 Then
                        Debug.WriteLine("Notify")
                        AlrmTimeout = 15
                        NotifyIcon1.BalloonTipText = e.UserState.ToString
                        NotifyIcon1.BalloonTipIcon = ToolTipIcon.Warning
                        NotifyIcon1.ShowBalloonTip(1)
                    Else
                        Debug.WriteLine("Notify in: " & AlrmTimeout)
                        AlrmTimeout -= 1
                        If AlrmTimeout = 2 Then
                            SP = New SoundPlayer(My.Resources.Beep)
                            SP.Play()
                        ElseIf AlrmTimeout = 1 Then
                            Call SayText(e.UserState.ToString.Replace(CharName, SafeCharName).Replace(ARnAFKInterval, ARnAFKInterval - 4))
                        End If
                    End If
                    If Not NotifyIcon1.Text = NIName & " (Warning)" Then
                        NotifyIcon1.Text = NIName & " (Warning)"
                        BringFocus(PID)
                    End If
                Case 2
                    'AFK Status Enabled
                    If Not NotifyIcon1.Text = NIName & " (Active)" Then
                        NotifyIcon1.Icon = GreenIcon
                        NotifyIcon1.Text = NIName & " (Active)"
                        AlrmTimeout = 0
                    End If
                Case 3
                    'Initialize NotifyIcon
                    NotifyIcon1.Text = e.UserState.ToString
                    NIName = e.UserState.ToString
                    NotifyIcon1.Visible = True
                Case 4
                    'Auto Close Client Finished
                    Label16.ForeColor = Color.Lime
                    AutoClosed = True
                    AFKSenseMode = False
                    Button28.Text = "D I S A B L E D"
                    Button28.Enabled = False
                Case 5
                    'Warning and Narrate
                    Dim msg As String = e.UserState.ToString
                    Dim say As String = msg.Replace(CharName, SafeCharName).Replace(CurMsg, SafeMsg)
                    BringFocus(PID)
                    SP = New SoundPlayer(My.Resources.Warning)
                    SP.Play()
                    NotifyIcon1.BalloonTipText = msg
                    NotifyIcon1.BalloonTipIcon = ToolTipIcon.Info
                    NotifyIcon1.ShowBalloonTip(1)
                    Call SayText(say)

               'Client
                Case 11
                    Label10.Text = "AVAILABLE"
                    Label10.ForeColor = Color.White
                Case 12
                    Label10.Text = "UNAVAILABLE"
                    Label10.ForeColor = Color.Yellow
                'AFK Mode
                Case 21
                    Label11.Text = "ACTIVE"
                    Label11.ForeColor = Color.White
                Case 22
                    Label11.Text = "STUCKED"
                    Label11.ForeColor = Color.Red
                Case 23
                    Label11.Text = "INACTIVE"
                    Label11.ForeColor = Color.Gray
                'ARnAFK
                Case 31
                    Label12.Text = "ENABLED"
                    Label12.ForeColor = Color.White
                Case 32
                    Label12.Text = e.UserState
                    Label12.ForeColor = Color.Lime
                Case 33
                    Label12.Text = "DISABLED"
                    Label12.ForeColor = Color.Gray
                'Rev Mode
                Case 41
                    Label13.Text = "SILVER"
                    Label13.ForeColor = Color.Silver
                Case 42
                    Label13.Text = "GOLD"
                    Label13.ForeColor = Color.Gold
                Case 43
                    Label13.Text = "FREE"
                    Label13.ForeColor = Color.LightGray
                'Elite Notify
                Case 51
                    Label17.Text = "ENABLED"
                    Label17.BackColor = Color.Transparent
                    EnotifTrigrd = False
                Case 52
                    Label17.Text = "ACTIVE"
                    If EnotifTrigrd = False Then
                        Label17.BackColor = Color.DodgerBlue
                        BringFocus(PID)
                        SP = New SoundPlayer(My.Resources.Beep)
                        SP.Play()
                    End If
                Case 53
                    If e.UserState = False Then
                        ProgressBar1.Value = 0
                    End If
                    ProgressBar1.Enabled = e.UserState
                Case 54
                    ProgressBar1.Value = e.UserState
                    If e.UserState <= EliteHPThreshold AndAlso EnotifTrigrd = False Then
                        EnotifTrigrd = True
                        Label17.BackColor = Color.LimeGreen
                        'Notify and Narrate
                        BringFocus(PID)
                        SP = New SoundPlayer(My.Resources.Beep)
                        SP.Play()
                        NotifyIcon1.BalloonTipText = TName & " is below " & EliteHPThreshold & " percent"
                        NotifyIcon1.BalloonTipIcon = ToolTipIcon.Info
                        NotifyIcon1.ShowBalloonTip(1)
                        Call SayText(NotifyIcon1.BalloonTipText.Replace("[Elite]", "Elite ").Replace("[E]", "Elite "))
                    End If
                Case 55
                    Label17.Text = "DISABLED"
                    Label17.ForeColor = Color.White
                    Label17.BackColor = Color.Transparent
                'Auto Close
                Case 61
                    Label16.Text = e.UserState
                Case 62
                    Label16.Text = "DISABLED"

                Case 404
                    MessageBox.Show("[VK] Master AFK Sense has encountered an error at BGW1:" & Environment.NewLine &
                                    e.UserState.ToString, ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Close()
            End Select
        Catch ex As Exception
            MessageBox.Show("[VK] Master AFK Sense has encountered an error at BGWPC1:" & ex.Message, ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Close()
        End Try
    End Sub

    Protected CurMsg, SafeMsg As String
    Private Sub BackgroundWorker2_DoWork(sender As Object, e As DoWorkEventArgs) Handles BackgroundWorker2.DoWork
        Dim PTST1, PTST2, PTMap1, PTMap2, PTMap3 As Integer
        Dim sbytechar As SByte
        Dim Msg, MapCoordinates, sbytestr As String
        Msg = " "
        Do
            If AFKSenseMode = False Then
                Exit Do
            End If
            Try
                If CheckPID(PID) = True Then
                    'System Message
                    CurMsg = ""
                    SafeMsg = ""
                    ReadProcessMemory(hProcPID, &H15763A4, PTST1, 4, Nothing)
                    PTST1 += &H20
                    ReadProcessMemory(hProcPID, PTST1, PTST2, 4, Nothing)
                    Do
                        ReadProcessMemory(hProcPID, PTST2, sbytechar, 1, Nothing)
                        sbytestr = Convert.ToString(Convert.ToChar(CInt(sbytechar)))
                        If Not sbytechar = 0 Then
                            CurMsg &= sbytestr
                            PTST2 += 2
                        Else
                            Exit Do
                        End If
                    Loop
                    If Not Msg = CurMsg Then
                        Msg = CurMsg
                        If CurMsg.ToLower.Contains("quest complete") Then
                            BackgroundWorker2.ReportProgress(1, CharName & "'s quest completed")
                        ElseIf CurMsg.ToLower.Contains("killed") Then
                            Debug.WriteLine("Send Chat Message Started")
                            For i = 0 To CurMsg.Length - 1
                                If Char.IsLetter(CurMsg.Chars(i)) Then
                                    SafeMsg &= CurMsg.Chars(i)
                                Else
                                    SafeMsg &= " "
                                End If
                            Next
                            BackgroundWorker2.ReportProgress(1, CharName & ", " & CurMsg)
                            'Get MAP
                            Debug.WriteLine("Send Chat Message Getting Map")
                            MapCoordinates = ""
                            sbytechar = 0
                            ReadProcessMemory(hProcPID, &H1575C38, PTMap1, 4, Nothing)
                            PTMap1 += &H5C
                            ReadProcessMemory(hProcPID, PTMap1, PTMap2, 4, Nothing)
                            PTMap2 += &H58
                            ReadProcessMemory(hProcPID, PTMap2, PTMap3, 4, Nothing)
                            Do
                                ReadProcessMemory(hProcPID, PTMap3, sbytechar, 1, Nothing)
                                If Not sbytechar = 0 Then
                                    MapCoordinates &= Convert.ToString(Convert.ToChar(CInt(sbytechar)))
                                    PTMap3 += 2
                                Else
                                    Exit Do
                                End If
                            Loop
                            'Get Coordinates
                            Debug.WriteLine("Send Chat Message Getting Coordinates")
                            Dim PTX1, PTX2, PTX3 As Integer
                            Dim X, Y As Single
                            ReadProcessMemory(hProcPID, &H15762F4, PTX1, 4, Nothing)
                            PTX1 += 4
                            ReadProcessMemory(hProcPID, PTX1, PTX2, 4, Nothing)
                            PTX2 += &H1F4
                            ReadProcessMemory(hProcPID, PTX2, PTX3, 4, Nothing)
                            ReadProcessMemoryS(hProcPID, PTX3 + &H14, X, 4, Nothing)
                            ReadProcessMemoryS(hProcPID, PTX3 + &H1C, Y, 4, Nothing)
                            'Get Current Time
                            Debug.WriteLine("Send Chat Message Getting Time")
                            Dim PTT1, PTT2, PTT3, PTT4 As Integer
                            Dim chrtime As SByte = 0
                            Dim Time As String = ""
                            ReadProcessMemory(hProcPID, &H1575C38, PTT1, 4, Nothing)
                            PTT1 += &H58
                            ReadProcessMemory(hProcPID, PTT1, PTT2, 4, Nothing)
                            PTT2 += &H164
                            ReadProcessMemory(hProcPID, PTT2, PTT3, 4, Nothing)
                            PTT3 += &H58
                            ReadProcessMemory(hProcPID, PTT3, PTT4, 4, Nothing)
                            PTT4 += &H58
                            Do
                                ReadProcessMemory(hProcPID, PTT4, chrtime, 1, Nothing)
                                If Not chrtime = 0 Then
                                    Time &= Convert.ToString(Convert.ToChar(CInt(chrtime)))
                                    PTT4 += 2
                                Else
                                    Exit Do
                                End If
                            Loop
                            MapCoordinates &= "(" & Math.Round(X, 0) & "," & Math.Round(Y, 0) & ")"
                            Debug.WriteLine("Chat Request: " & "[" & Time & "] " & CurMsg.Replace("you", "me. ") & MapCoordinates)
                            BackgroundWorker2.ReportProgress(2, "[" & Time & "] " & CurMsg.Replace("you", "me. ") & MapCoordinates)
                        End If
                    End If
                End If
            Catch ex As Exception
                Debug.WriteLine("Internal Catch Exception 2 : " & ex.Message)
            End Try
            Threading.Thread.Sleep(50)
        Loop
    End Sub

    'Speech Systhesis
    Private Sub BackgroundWorker2_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles BackgroundWorker2.ProgressChanged
        Try
            If AFKResult = 1 Then
                Select Case e.ProgressPercentage
                    Case 1
                        NotifyIcon1.BalloonTipText = e.UserState.ToString
                        NotifyIcon1.BalloonTipIcon = ToolTipIcon.Info
                        NotifyIcon1.ShowBalloonTip(1)
                        Call SayText(NotifyIcon1.BalloonTipText.Replace(CharName, SafeCharName).Replace(CurMsg, SafeMsg))
                    Case 2
                        SendIGMessage(PID, e.UserState.ToString)
                End Select
            End If
        Catch ex As Exception
            Debug.WriteLine("BGW2 Progress Changed Error : " & ex.Message)
        End Try
    End Sub

    'Ping
    Private Sub BackgroundWorker3_DoWork(sender As Object, e As DoWorkEventArgs) Handles BackgroundWorker3.DoWork
        Dim PTC1, PTC2 As Integer
        Dim sbytechar As SByte = 0
        Dim ChatRes, CurChatRes As String
        ChatRes = ""
        Do
            If AFKSenseMode = False Then
                Exit Do
            End If
            Try
                CurChatRes = ""
                ReadProcessMemory(hProcPID, &H15762D4, PTC1, 4, Nothing)
                PTC1 += &H1B4
                ReadProcessMemory(hProcPID, PTC1, PTC2, 4, Nothing)
                PTC2 += &H2
                Do
                    ReadProcessMemory(hProcPID, PTC2, sbytechar, 1, Nothing)
                    If Not sbytechar = 0 Then
                        CurChatRes &= Convert.ToString(Convert.ToChar(CInt(sbytechar)))
                        PTC2 += 2
                    Else
                        Exit Do
                    End If
                Loop
                If CurChatRes.Contains(":") Then
                    CurChatRes = CurChatRes.Replace(":", "")
                End If
                If Not ChatRes = CurChatRes Then Debug.WriteLine("Chat: " & CurChatRes)
                If CurChatRes.ToLower.Contains("!" & CharName.ToLower) AndAlso Not ChatRes = CurChatRes Then
                    NotifyIcon1.BalloonTipText = CurChatRes.Replace("!" & CharName, CharName)
                    NotifyIcon1.BalloonTipIcon = ToolTipIcon.Info
                    NotifyIcon1.ShowBalloonTip(1)
                    SP = New SoundPlayer(My.Resources.Alarm01)
                    SP.Play()
                    Call SayText(CurChatRes.Replace("!" & CharName, CharName))
                    SendIGMessage(PID, "Recieved")
                End If
                ChatRes = CurChatRes
                Threading.Thread.Sleep(100)
            Catch ex As Exception
                Debug.WriteLine("Internal Catch Exception 3 : " & ex.Message)
            End Try
        Loop
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        Close()
    End Sub

    Private Sub BackgroundWorker1_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        If BackgroundWorker2.IsBusy = False AndAlso BackgroundWorker3.IsBusy = False Then
            If Not Button28.Text = "D I S A B L E D" Then
                Button28.Text = "S T A R T"
                Button28.Enabled = True
            End If
            Debug.WriteLine("All BGW Stopped - BGW 1")
            Else
                Debug.WriteLine("BGW 1 Stopped")
        End If
    End Sub

    Private Sub BackgroundWorker2_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles BackgroundWorker2.RunWorkerCompleted
        If BackgroundWorker1.IsBusy = False AndAlso BackgroundWorker3.IsBusy = False Then
            If Not Button28.Text = "D I S A B L E D" Then
                Button28.Text = "S T A R T"
                Button28.Enabled = True
            End If
            Debug.WriteLine("All BGW Stopped - BGW 2")
        Else
            Debug.WriteLine("BGW 2 Stopped")
        End If
    End Sub

    Private Sub BackgroundWorker3_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles BackgroundWorker3.RunWorkerCompleted
        If BackgroundWorker1.IsBusy = False AndAlso BackgroundWorker2.IsBusy = False Then
            If Not Button28.Text = "D I S A B L E D" Then
                Button28.Text = "S T A R T"
                Button28.Enabled = True
            End If
            Debug.WriteLine("All BGW Stopped - BGW 3")
        Else
            Debug.WriteLine("BGW 3 Stopped")
        End If
    End Sub

    Private Sub Button28_Click(sender As Object, e As EventArgs) Handles Button28.Click
        Select Case Button28.Text
            Case "S T A R T"
                If Not BackgroundWorker1.IsBusy AndAlso Not BackgroundWorker2.IsBusy AndAlso Not BackgroundWorker3.IsBusy AndAlso AutoClosed = False Then
                    Button28.Enabled = False
                    AFKSenseMode = True
                    BackgroundWorker1.RunWorkerAsync()
                    BackgroundWorker2.RunWorkerAsync()
                    BackgroundWorker3.RunWorkerAsync()
                    Do While Not BackgroundWorker1.IsBusy AndAlso Not BackgroundWorker2.IsBusy AndAlso Not BackgroundWorker3.IsBusy
                    Loop
                    Button28.Text = "S T O P"
                    Button28.Enabled = False
                End If
            Case "S T O P"
                Button28.Enabled = False
                AFKSenseMode = False
        End Select
    End Sub
End Class
