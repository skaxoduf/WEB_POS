<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        pnlCSMain = New Panel()
        Label2 = New Label()
        Label1 = New Label()
        txtPosNo = New TextBox()
        txtCompanyCode = New TextBox()
        btnSave = New Button()
        WebView21 = New Microsoft.Web.WebView2.WinForms.WebView2()
        Button1 = New Button()
        Button2 = New Button()
        Button3 = New Button()
        CheckBox1 = New CheckBox()
        TextBox1 = New TextBox()
        Button4 = New Button()
        txtFingerDataLog = New TextBox()
        pnlCSMain.SuspendLayout()
        CType(WebView21, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' pnlCSMain
        ' 
        pnlCSMain.BackColor = Color.White
        pnlCSMain.BackgroundImage = CType(resources.GetObject("pnlCSMain.BackgroundImage"), Image)
        pnlCSMain.BackgroundImageLayout = ImageLayout.Zoom
        pnlCSMain.Controls.Add(Label2)
        pnlCSMain.Controls.Add(Label1)
        pnlCSMain.Controls.Add(txtPosNo)
        pnlCSMain.Controls.Add(txtCompanyCode)
        pnlCSMain.Controls.Add(btnSave)
        pnlCSMain.Location = New Point(12, 12)
        pnlCSMain.Name = "pnlCSMain"
        pnlCSMain.Size = New Size(866, 595)
        pnlCSMain.TabIndex = 1
        ' 
        ' Label2
        ' 
        Label2.BackColor = Color.Black
        Label2.Font = New Font("맑은 고딕", 24F, FontStyle.Bold, GraphicsUnit.Point, CByte(129))
        Label2.ForeColor = Color.SeaShell
        Label2.ImageAlign = ContentAlignment.BottomLeft
        Label2.Location = New Point(18, 95)
        Label2.Name = "Label2"
        Label2.Size = New Size(153, 50)
        Label2.TabIndex = 2
        Label2.Text = "포스번호"
        ' 
        ' Label1
        ' 
        Label1.BackColor = Color.Black
        Label1.Font = New Font("맑은 고딕", 24F, FontStyle.Bold, GraphicsUnit.Point, CByte(129))
        Label1.ForeColor = Color.SeaShell
        Label1.ImageAlign = ContentAlignment.BottomLeft
        Label1.Location = New Point(18, 24)
        Label1.Name = "Label1"
        Label1.Size = New Size(149, 50)
        Label1.TabIndex = 2
        Label1.Text = "업체코드"
        ' 
        ' txtPosNo
        ' 
        txtPosNo.BorderStyle = BorderStyle.FixedSingle
        txtPosNo.Font = New Font("맑은 고딕", 24F, FontStyle.Bold)
        txtPosNo.Location = New Point(177, 95)
        txtPosNo.Name = "txtPosNo"
        txtPosNo.Size = New Size(82, 50)
        txtPosNo.TabIndex = 1
        txtPosNo.Text = "10"
        ' 
        ' txtCompanyCode
        ' 
        txtCompanyCode.BorderStyle = BorderStyle.FixedSingle
        txtCompanyCode.Font = New Font("맑은 고딕", 24F, FontStyle.Bold)
        txtCompanyCode.Location = New Point(173, 24)
        txtCompanyCode.Name = "txtCompanyCode"
        txtCompanyCode.Size = New Size(400, 50)
        txtCompanyCode.TabIndex = 1
        txtCompanyCode.Text = "A2121212121212312"
        ' 
        ' btnSave
        ' 
        btnSave.BackgroundImageLayout = ImageLayout.None
        btnSave.Font = New Font("맑은 고딕", 24F, FontStyle.Bold, GraphicsUnit.Point, CByte(129))
        btnSave.ForeColor = Color.AliceBlue
        btnSave.Image = CType(resources.GetObject("btnSave.Image"), Image)
        btnSave.ImageAlign = ContentAlignment.BottomLeft
        btnSave.Location = New Point(332, 475)
        btnSave.Name = "btnSave"
        btnSave.Size = New Size(187, 52)
        btnSave.TabIndex = 0
        btnSave.Text = "저장하기"
        btnSave.UseVisualStyleBackColor = True
        ' 
        ' WebView21
        ' 
        WebView21.AllowExternalDrop = True
        WebView21.CreationProperties = Nothing
        WebView21.DefaultBackgroundColor = Color.White
        WebView21.Location = New Point(681, 487)
        WebView21.Name = "WebView21"
        WebView21.Size = New Size(800, 422)
        WebView21.TabIndex = 2
        WebView21.ZoomFactor = 1R
        ' 
        ' Button1
        ' 
        Button1.Location = New Point(158, 681)
        Button1.Name = "Button1"
        Button1.Size = New Size(160, 45)
        Button1.TabIndex = 3
        Button1.Text = "자바스크립트 함수 호출"
        Button1.UseVisualStyleBackColor = True
        Button1.Visible = False
        ' 
        ' Button2
        ' 
        Button2.Location = New Point(12, 681)
        Button2.Name = "Button2"
        Button2.Size = New Size(140, 45)
        Button2.TabIndex = 3
        Button2.Text = "Json 문자열 넘기기"
        Button2.UseVisualStyleBackColor = True
        ' 
        ' Button3
        ' 
        Button3.Location = New Point(324, 681)
        Button3.Name = "Button3"
        Button3.Size = New Size(104, 45)
        Button3.TabIndex = 3
        Button3.Text = "웹캠 연결"
        Button3.UseVisualStyleBackColor = True
        ' 
        ' CheckBox1
        ' 
        CheckBox1.AutoSize = True
        CheckBox1.Checked = True
        CheckBox1.CheckState = CheckState.Checked
        CheckBox1.Location = New Point(1211, 12)
        CheckBox1.Name = "CheckBox1"
        CheckBox1.Size = New Size(86, 19)
        CheckBox1.TabIndex = 5
        CheckBox1.Text = "CheckBox1"
        CheckBox1.UseVisualStyleBackColor = True
        ' 
        ' TextBox1
        ' 
        TextBox1.Location = New Point(170, 639)
        TextBox1.Name = "TextBox1"
        TextBox1.Size = New Size(258, 23)
        TextBox1.TabIndex = 6
        ' 
        ' Button4
        ' 
        Button4.Location = New Point(434, 681)
        Button4.Name = "Button4"
        Button4.Size = New Size(104, 45)
        Button4.TabIndex = 3
        Button4.Text = "지문 등록"
        Button4.UseVisualStyleBackColor = True
        ' 
        ' txtFingerDataLog
        ' 
        txtFingerDataLog.Location = New Point(827, 229)
        txtFingerDataLog.Multiline = True
        txtFingerDataLog.Name = "txtFingerDataLog"
        txtFingerDataLog.ScrollBars = ScrollBars.Vertical
        txtFingerDataLog.Size = New Size(494, 289)
        txtFingerDataLog.TabIndex = 7
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1333, 803)
        Controls.Add(txtFingerDataLog)
        Controls.Add(TextBox1)
        Controls.Add(CheckBox1)
        Controls.Add(pnlCSMain)
        Controls.Add(Button4)
        Controls.Add(Button3)
        Controls.Add(Button2)
        Controls.Add(Button1)
        Controls.Add(WebView21)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "Form1"
        Text = "Argos APT"
        pnlCSMain.ResumeLayout(False)
        pnlCSMain.PerformLayout()
        CType(WebView21, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub
    Friend WithEvents pnlCSMain As Panel
    Friend WithEvents btnSave As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents txtCompanyCode As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents txtPosNo As TextBox
    Friend WithEvents WebView21 As Microsoft.Web.WebView2.WinForms.WebView2
    Friend WithEvents Button1 As Button
    Friend WithEvents Button2 As Button
    Friend WithEvents Button3 As Button
    Friend WithEvents CheckBox1 As CheckBox
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents Button4 As Button
    Friend WithEvents txtFingerDataLog As TextBox

End Class
