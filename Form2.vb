Imports System.IO
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Windows.Forms
Imports AForge.Video
Imports AForge.Video.DirectShow
Imports System.ComponentModel

Public Class Form2
    Private videoDevices As FilterInfoCollection
    Private videoSource As VideoCaptureDevice
    Private capturedImage As Bitmap ' 현재 프레임 이미지를 저장
    Private parentForm As Form1

    Public Sub New(ByVal callingForm As Form1)
        Me.InitializeComponent()
        parentForm = callingForm
    End Sub

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Me.PictureBox1.SizeMode = PictureBoxSizeMode.Zoom ' 확대돼서 이상하게보임 
        Me.PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage   ' 꽉차게 보임 
        'Me.PictureBox1.SizeMode = PictureBoxSizeMode.Normal

    End Sub

    ' 웹캠 연결 
    Public Sub StartWebcam()
        videoDevices = New FilterInfoCollection(FilterCategory.VideoInputDevice)

        If videoDevices.Count > 0 Then
            If videoSource IsNot Nothing Then
                If videoSource.IsRunning Then
                    videoSource.SignalToStop()
                    videoSource.WaitForStop()
                End If
                RemoveHandler videoSource.NewFrame, AddressOf Video_NewFrame
                videoSource = Nothing
            End If

            videoSource = New VideoCaptureDevice(videoDevices(0).MonikerString)

            ' 해상도 설정
            For Each cap In videoSource.VideoCapabilities
                'If cap.FrameSize.Width = 1280 AndAlso cap.FrameSize.Height = 720 Then ' 1280 * 720    베이스64 글자수 7만자나옴 
                'If cap.FrameSize.Width = 1920 AndAlso cap.FrameSize.Height = 1080 Then ' 1920 * 1080   베이스64 글자수 17만자나옴 
                If cap.FrameSize.Width = 640 AndAlso cap.FrameSize.Height = 480 Then
                    videoSource.VideoResolution = cap
                    Exit For
                End If
                'Debug.WriteLine($"이 웹캠이 지원하는 해상도 : {cap.FrameSize.Width}x{cap.FrameSize.Height}")
            Next
            AddHandler videoSource.NewFrame, AddressOf Video_NewFrame
            videoSource.Start()
            'MessageBox.Show("웹캠 연결!!!!!", "웹캠", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            MessageBox.Show("웹캠이 연결되지 않았습니다!", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)
            'Return
        End If

    End Sub
    Private Sub Video_NewFrame(sender As Object, eventArgs As NewFrameEventArgs)
        capturedImage = CType(eventArgs.Frame.Clone(), Bitmap)
        PictureBox1.Image = capturedImage
    End Sub

    Private Async Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        '사진찍기
        If capturedImage IsNot Nothing Then
            Try

                Dim targetWidth As Integer = 400
                Dim targetHeight As Integer = 520

                ' 비율을 유지하며 리사이징 (targetWidth, targetHeight 해상도로 안될수도 있음..비율을 유지하기 때문)
                Dim resizedBitmap As Bitmap = ImageHelper.ResizeImagePreservingAspectRatio(capturedImage, targetWidth, targetHeight)

                ' 비율을 무시하고 지정된 크기로 리사이징(targetWidth, targetHeight)
                'Dim resizedBitmap As Bitmap = ImageHelper.ResizeImage(capturedImage, targetWidth, targetHeight)


                If resizedBitmap IsNot Nothing Then
                    ' 로컬에 사진파일로 저장 
                    Dim fileName = "TestImg_" & Now.ToString("yyyyMMddHHmmss") & ".jpg"
                    Dim folderPath = Application.StartupPath
                    Dim filePath = Path.Combine(folderPath, fileName)
                    resizedBitmap.Save(filePath, ImageFormat.Jpeg)
                    'MessageBox.Show("사진 저장 완료: " & filePath)


                    ' 이미지를 Base64 문자열로 변환
                    Dim base64String = ImageToBase64(resizedBitmap, ImageFormat.Jpeg)
                    'Debug.Print("base64String ::::::::  " & base64String)


                    ' Base64 문자열을 텍스트 파일로 저장
                    'Dim base64FileName = "TestImg_" & Now.ToString("yyyyMMddHHmmss") & ".txt"
                    'Dim base64FilePath = Path.Combine(folderPath, base64FileName)
                    'File.WriteAllText(base64FilePath, base64String)
                    'MessageBox.Show("Base64 텍스트 파일 저장 완료: " & base64FilePath)


                    If parentForm IsNot Nothing Then
                        'Dim targetWebViewUrl As String = "http://julist.webpos.co.kr/login/default_base64.asp"   ' 사용안함
                        Me.Close()  '사진을 찍었으면 창을 닫아준다.
                        Await parentForm.SendBase64ToWebView(base64String, "")
                    Else
                        MessageBox.Show("Base64 전송실패!!!", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End If
                    resizedBitmap.Dispose()
                Else
                    MessageBox.Show("이미지 리사이징 실패!", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If

            Catch ex As Exception
                MessageBox.Show("파일 저장 및 Base64 변환/저장 중 오류 발생: " & ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        Else
            MessageBox.Show("아직 웹캠이 준비되지 않음!")
        End If

    End Sub
    Private Sub StopWebcam()
        ' 웹캠 리소스 해제
        If videoSource IsNot Nothing Then
            If videoSource.IsRunning Then
                videoSource.SignalToStop()
                videoSource.WaitForStop()
            End If
            RemoveHandler videoSource.NewFrame, AddressOf Video_NewFrame
            videoSource = Nothing
        End If

        If PictureBox1.Image IsNot Nothing Then
            PictureBox1.Image.Dispose()
            PictureBox1.Image = Nothing
        End If
        capturedImage = Nothing
    End Sub
    Private Function ImageToBase64(img As Image, format As ImageFormat) As String
        Using ms As New MemoryStream()
            img.Save(ms, format)
            Dim imageBytes As Byte() = ms.ToArray()
            Return Convert.ToBase64String(imageBytes)
        End Using
    End Function
    Private Sub Form2_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        StopWebcam()
    End Sub
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.Close()  '닫기
    End Sub

    Private Sub Form2_Closed(sender As Object, e As EventArgs) Handles Me.Closed

    End Sub
End Class