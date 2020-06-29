Imports System.Runtime.InteropServices
Module Module1
    'Declarations
    Friend Declare Function OpenProcess Lib "kernel32" (ByVal dwDesiredAccess As Integer, ByVal bInheritHandle As Integer, ByVal dwProcessId As Integer) As Integer
    Friend Declare Function ReadProcessMemory Lib "kernel32" Alias "ReadProcessMemory" (ByVal hProcess As Integer, ByVal lpBaseAddress As Integer, ByRef lpBuffer As Integer, ByVal nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Integer
    Friend Declare Function WriteProcessMemory Lib "kernel32" Alias "WriteProcessMemory" (ByVal hProcess As Integer, ByVal lpBaseAddress As Integer, ByRef lpBuffer As Integer, ByVal nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Integer
    Friend Declare Function ReadProcessMemoryD Lib "kernel32" Alias "ReadProcessMemory" (ByVal hProcess As Integer, ByVal lpBaseAddress As Integer, ByRef lpBuffer As Long, ByVal nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Integer
    Friend Declare Function WriteProcessMemoryD Lib "kernel32" Alias "WriteProcessMemory" (ByVal hProcess As Integer, ByVal lpBaseAddress As Integer, ByRef lpBuffer As Long, ByVal nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Integer
    Friend Declare Function ReadProcessMemoryS Lib "kernel32" Alias "ReadProcessMemory" (ByVal hProcess As Integer, ByVal lpBaseAddress As Integer, ByRef lpBuffer As Single, ByVal nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Integer
    Friend Declare Function WriteProcessMemoryS Lib "kernel32" Alias "WriteProcessMemory" (ByVal hProcess As Integer, ByVal lpBaseAddress As Integer, ByRef lpBuffer As Single, ByVal nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Integer
    Friend Declare Function FindWindow Lib "user32" (ByVal sClass As String, ByVal sWindow As String) As Integer
    Friend Declare Function SetForegroundWindow Lib "user32" (ByVal hwnd As Integer) As Integer
    Friend Declare Function SendMessage Lib "user32" (ByVal hWnd As IntPtr, ByVal Msg As UInteger, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
    Friend Declare Function GetAsyncKeyState Lib "user32" (ByVal vkey As Integer) As Short
    Friend Declare Function ShowWindow Lib "user32" (ByVal hWnd As IntPtr, ByVal flags As ShowWindowEnum) As Boolean
    Friend Enum ShowWindowEnum
        Hide = 0
        ShowNormal = 1
        ShowMinimized = 2
        ShowMaximized = 3
        Maximize = 3
        ShowNormalNoActivate = 4
        Show = 5
        Minimize = 6
        ShowMinNoActivate = 7
        ShowNoActivate = 8
        Restore = 9
        ShowDefault = 10
        ForceMinimized = 11
    End Enum
    Friend Function CheckPID(ByVal PID As Integer) As Boolean
        Dim Exists As Boolean = False
        For Each Process As Process In Process.GetProcesses
            If Process.Id = PID Then
                Dim hProcPID As Integer = OpenProcess(2035711, 0, PID)
                Dim GodsWar As Integer = 0
                ReadProcessMemory(hProcPID, &H400000, GodsWar, 4, Nothing)
                If GodsWar = 9460301 Then
                    Exists = True
                End If
            End If
        Next
        Return Exists
    End Function
    Friend Function Translate(ByVal hex As String)
        Dim text As New Text.StringBuilder(hex.Length \ 2)
        For i As Integer = 0 To hex.Length - 2 Step 2
            text.Append(Chr(Convert.ToByte(hex.Substring(i, 2), 16)))
        Next
        Return text.ToString
    End Function
    Friend Function targetname(ByVal hProcPID As Integer)
        Dim PT As Integer
        Dim chr As SByte = 0
        Dim CharName As String = ""
        PT += &H15B6158
        Do
            ReadProcessMemory(hProcPID, PT, chr, 1, Nothing)
            If Not chr = 0 Then
                CharName &= Convert.ToString(Convert.ToChar(CInt(chr)))
                PT += 1
            Else
                Exit Do
            End If
        Loop
        If CharName <> "" Then
            Return CharName
        Else
            Return Nothing
        End If
    End Function
    Friend Sub AntiDC(ByVal hProcPID As Integer, ByVal Mode As Boolean)
        Select Case Mode
            Case True
                WriteProcessMemoryD(hProcPID, &H4589B2, 2425393296, Translate("34"), Nothing)
                WriteProcessMemoryD(hProcPID, &H4589B7, 1200459920, Translate("34"), Nothing)
                WriteProcessMemoryD(hProcPID, &H4589B4, 2425393296, Translate("34"), Nothing)
                'WriteProcessMemoryD(hProcPID, &H4589ED, 3257661300, Translate("34"), Nothing)
            Case False
                WriteProcessMemoryD(hProcPID, &H4589B2, 1200293234, Translate("34"), Nothing)
                WriteProcessMemoryD(hProcPID, &H4589B7, 1200423915, Translate("34"), Nothing)
                WriteProcessMemoryD(hProcPID, &H4589B4, 3942926219, Translate("34"), Nothing)
                'WriteProcessMemoryD(hProcPID, &H4589ED, 3257661301, Translate("34"), Nothing)
        End Select
    End Sub
    Friend Function GetSafeString(ByVal input As String) As String
        'Get Letters Only
        Dim output As String = ""
        For i = 0 To input.Length - 1
            If Char.IsLetter(input.Chars(i)) Then
                output &= input.Chars(i)
            Else
                output &= "%"
            End If
        Next
        'Replace Double Spaces To Single
        Do
            If output.Contains("%%") Then
                output = output.Replace("%%", "%")
            Else
                Exit Do
            End If
        Loop
        'Remove Spaces In Front
        Do
            If output.First = Convert.ToChar("%") Then
                output = output.Remove(0, 1)
            Else
                Exit Do
            End If
        Loop
        'Remove Spaces At The Back
        Do
            If output.Last = Convert.ToChar("%") Then
                output = output.Remove(output.Length - 1, 1)
            Else
                Exit Do
            End If
        Loop
        'Finalize
        output = output.Replace("%", " ")
        Return output
    End Function
    Private Const WM_CHAR = &H102
    Friend Sub SendIGMessage(ByVal PID As Integer, ByVal sText As String)
        Debug.WriteLine("SendIGMessage: " & sText)
        Dim Proc As Process = Process.GetProcessById(PID)
        Dim hProcPID As Integer = OpenProcess(2035711, 0, PID)
        Dim ADCValue As Long
        Dim msgtochat As String() = sText.Split(New Char() {"|"c})
        For Each msgchat As String In msgtochat
            'Press Enter
            SendMessage(Proc.MainWindowHandle, 256, Keys.Enter, 0)
            SendMessage(Proc.MainWindowHandle, 257, Keys.Enter, 0)
            msgchat = "/g " & msgchat
            For Each c As Char In msgchat
                SendMessage(Proc.MainWindowHandle, WM_CHAR, Asc(c), 0)
            Next
            'Get AntiDC status
            ReadProcessMemoryD(hProcPID, &H4589B2, ADCValue, Translate("34"), Nothing)
            If ADCValue = 2425393296 Then
                AntiDC(hProcPID, False)
                'Press Enter
                SendMessage(Proc.MainWindowHandle, 256, Keys.Enter, 0)
                SendMessage(Proc.MainWindowHandle, 257, Keys.Enter, 0)
                'Restore AntiDC status
                AntiDC(hProcPID, True)
            Else
                'Press Enter
                SendMessage(Proc.MainWindowHandle, 256, Keys.Enter, 0)
                SendMessage(Proc.MainWindowHandle, 257, Keys.Enter, 0)
            End If
            Threading.Thread.Sleep(500)
        Next
        Debug.WriteLine("SendIGMessage Sent")
    End Sub
    Friend Sub BringFocus(ByVal PID As Integer)
        If CheckPID(PID) = True Then
            Dim proc As Process = Process.GetProcessById(PID)
            If proc IsNot Nothing Then
                If proc.MainWindowHandle = IntPtr.Zero Then
                    ShowWindow(proc.Handle, ShowWindowEnum.Restore)
                End If
                SetForegroundWindow(proc.MainWindowHandle)
            End If
        End If
    End Sub
End Module
