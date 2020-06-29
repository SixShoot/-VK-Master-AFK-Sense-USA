Imports System.ComponentModel
Imports System.Speech.Synthesis
Public Class Form2
    Dim VoiceName As String = Nothing
    Private WithEvents TextToSpeech As New SpeechSynthesizer
    Private Sub SayText(ByVal TextToSay As String)
        'Create new instance of spVoice and pass the TextToSay variable to the speak function 
        If TextToSpeech Is Nothing Then TextToSpeech = New SpeechSynthesizer
        TextToSpeech.SelectVoice(VoiceName)
        TextToSpeech.SpeakAsync(TextToSay)
    End Sub
    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles Me.Load
        Label1.Text = Form1.CharName
        CheckBox1.Checked = Form1.VoiceAlerts
        CheckBox2.Checked = Form1.ARnAFK
        ComboBox1.SelectedIndex = Form1.Rev - 1
        NumericUpDown1.Value = Form1.Interval
        CheckBox3.Checked = Form1.AC
        NumericUpDown2.Value = Form1.ACInterval
        VoiceName = Form1.VoiceName
        Dim voicecount As Integer = 0
        For Each TextToSpeechVoice In TextToSpeech.GetInstalledVoices
            voicecount += 1
            ComboBox2.Items.Add(TextToSpeechVoice.VoiceInfo.Name)
        Next
        If voicecount <> 0 Then
            ComboBox2.SelectedItem = VoiceName
        Else
            CheckBox1.Enabled = False
            ComboBox2.Enabled = False
        End If
    End Sub

    Private Sub Form2_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        Threading.Thread.VolatileWrite(Form1.VoiceAlerts, CheckBox1.Checked)
        Threading.Thread.VolatileWrite(Form1.ARnAFK, CheckBox2.Checked)
        Threading.Thread.VolatileWrite(Form1.Rev, ComboBox1.SelectedIndex + 1)
        Threading.Thread.VolatileWrite(Form1.Interval, NumericUpDown1.Value)
        Threading.Thread.VolatileWrite(Form1.AC, CheckBox3.Checked)
        Threading.Thread.VolatileWrite(Form1.ACInterval, NumericUpDown2.Value)
        Threading.Thread.VolatileWrite(Form1.VoiceName, ComboBox2.SelectedItem)
    End Sub

    Dim start As Boolean = True
    Private Sub ComboBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox2.SelectedIndexChanged
        If start = True Then
            start = False
            Exit Sub
        End If
        VoiceName = ComboBox2.SelectedItem
        Call SayText(VoiceName)
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        ComboBox2.Enabled = CheckBox1.Checked
    End Sub
End Class