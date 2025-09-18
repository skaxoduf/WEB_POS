<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmFinger
    Inherits System.Windows.Forms.Form

    'Form은 Dispose를 재정의하여 구성 요소 목록을 정리합니다.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows Form 디자이너에 필요합니다.
    Private components As System.ComponentModel.IContainer

    '참고: 다음 프로시저는 Windows Form 디자이너에 필요합니다.
    '수정하려면 Windows Form 디자이너를 사용하십시오.  
    '코드 편집기에서는 수정하지 마세요.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Button1 = New Button()
        Button4 = New Button()
        picFingerImg = New PictureBox()
        CType(picFingerImg, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Button1
        ' 
        Button1.Location = New Point(237, 557)
        Button1.Name = "Button1"
        Button1.Size = New Size(115, 45)
        Button1.TabIndex = 9
        Button1.Text = "닫기"
        Button1.UseVisualStyleBackColor = True
        ' 
        ' Button4
        ' 
        Button4.Location = New Point(66, 557)
        Button4.Name = "Button4"
        Button4.Size = New Size(115, 45)
        Button4.TabIndex = 8
        Button4.Text = "지문등록"
        Button4.UseVisualStyleBackColor = True
        ' 
        ' picFingerImg
        ' 
        picFingerImg.Location = New Point(14, 20)
        picFingerImg.Name = "picFingerImg"
        picFingerImg.Size = New Size(400, 520)
        picFingerImg.SizeMode = PictureBoxSizeMode.StretchImage
        picFingerImg.TabIndex = 7
        picFingerImg.TabStop = False
        ' 
        ' frmFinger
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(428, 622)
        Controls.Add(Button1)
        Controls.Add(Button4)
        Controls.Add(picFingerImg)
        FormBorderStyle = FormBorderStyle.None
        MaximizeBox = False
        Name = "frmFinger"
        StartPosition = FormStartPosition.CenterScreen
        Text = "frmFinger"
        CType(picFingerImg, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents Button1 As Button
    Friend WithEvents Button4 As Button
    Friend WithEvents picFingerImg As PictureBox
End Class
