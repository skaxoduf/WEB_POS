Imports System.IO
Imports System.Reflection
Imports Microsoft.Data.SqlClient
Imports Microsoft.Identity.Extensions
Imports UCBioBSPCOMLib
Imports UCSAPICOMLib

Public Class frmFinger

    Public WithEvents objUCSAPICOM As New UCSAPI()
    Public objTerminalUserData As ITerminalUserData
    Public objServerUserData As IServerUserData
    Public objAccessLogData As IAccessLogData
    Public objAccessControlData As IAccessControlData
    Public objServerAuthentication As IServerAuthentication
    Public objTerminalOption As ITerminalOption

    'UCBioBSP Object
    Public WithEvents objUCBioBSP As New UCBioBSP()
    Public objDevice As IDevice
    Public objExtraction As IExtraction
    Public objMatching As IMatching
    Public objFPData As IFPData
    Public objFPImage As IFPImage

    Public objFastSearch As IFastSearch
    Public szTextEnrolledFIR As String

    Private binaryEnrolledFIR() As Byte

    'UCBioBSP Object-스마트카드
    Private objSmartCard As ISmartCard   ' RF카드용 선언 

    '지문 품질값 51~75 적절한, 76~100 우수한, 정상적인 매칭을 위해서는 76 이상에 지문사용필요함
    ' VB6의 Long (32비트)은 VB.NET의 Integer (32비트)에 해당함..
    Private gBioAPI_QUALITY As Integer


    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click

        ' 지문인식 시작

        Try
            ' Window style
            objUCBioBSP.WindowStyle = UCBioAPI_WINDOW_STYLE_INVISIBLE
            objUCBioBSP.FPForeColor = "000000"
            objUCBioBSP.FPBackColor = "FFFFFF"

            objUCBioBSP.FingerWnd = picFingerImg.Handle.ToInt32    ' 픽쳐박스에 지문이미지 표시함...
            'objUCBioBSP.FingerWnd = 0   ' 픽쳐박스에 지문이미지 표시안함

            ' Window 옵션 설정
            objUCBioBSP.WindowOption(UCBioAPI_WINDOW_STYLE_NO_FPIMG) = False
            objUCBioBSP.WindowOption(UCBioAPI_WINDOW_STYLE_NO_TOPMOST) = False
            objUCBioBSP.WindowOption(UCBioAPI_WINDOW_STYLE_NO_WELCOME) = False

            ' 모든 손가락 등록 비활성화
            For i = 0 To 9
                objUCBioBSP.DisableFingerForEnroll(i) = True
            Next

            ' 장치를 닫고 다시 오픈...
            objDevice.Close(UCBioAPI_DEVICE_ID_AUTO_DETECT)
            objDevice.Open(UCBioAPI_DEVICE_ID_AUTO_DETECT)

            'TxtMsg.Text &= vbCrLf
            'TxtMsg.Text &= "지문 장비 대기 시작 " & Date.Now.ToString("HH:mm:ss") & vbCrLf
            objExtraction.Capture()   ' 지문 캡처 시작    
            'TxtMsg.Text &= "지문 장비 대기 종료 " & Date.Now.ToString("HH:mm:ss") & vbCrLf

            If objUCBioBSP.ErrorCode <> UCBioAPIERROR_NONE Then
                MessageBox.Show($"{objUCBioBSP.ErrorDescription} [{objUCBioBSP.ErrorCode}]")
                Return
            End If

            ' byte 배열 형식으로 지문 데이터 가져오기
            Dim biFIR As Byte() = DirectCast(objExtraction.FIR, Byte())
            If biFIR Is Nothing OrElse biFIR.Length = 0 Then
                ' TxtMsg.Text &= vbCrLf & "지문 데이터를 얻어오지 못했습니다. (biFIR is Nothing)" & vbCrLf
                Return
            End If



            ' DB에 지문 저장 시도
            'If SaveFingerprintToDatabase("UPD", MemberNo, biFIR) Then
            '    ' DB 저장에 성공했다면, 메모리에도 실시간으로 추가(갱신)
            '    AddSingleFingerprintToEngine(MemberNo)  ' 이건 한 pc에서 사용할때는 쓸수있는데 다른 pc에서 사용할때는 안된다.
            '    TxtMsg.Text &= $"사용자 ID({MemberNo})의 지문 등록 완료!" & Date.Now.ToString("HH:mm:ss") & vbCrLf
            'End If


            ' 이건 그냥 텍스트박스에 뿌리는용도...주석처리해도됨....
            ' 결과: "1A-2B-3C-4D-..."
            ' byte로 받아온 데이타를 16진수로 변환
            Dim hexString = BitConverter.ToString(biFIR)
            'TxtMsg.Text &= vbCrLf & "--- Fingerprint Binary (HEX) ---" & vbCrLf
            'TxtMsg.Text &= hexString & vbCrLf
            'TxtMsg.Text &= "----------------------------------" & vbCrLf

            ' byte로 받아온 데이타를 base64로 변환
            Dim base64String = Convert.ToBase64String(biFIR)
            'TxtMsg.Text &= vbCrLf & "--- Fingerprint Binary (Base64) ---" & vbCrLf
            'TxtMsg.Text &= base64String & vbCrLf
            'TxtMsg.Text &= "------------------------------------" & vbCrLf

            'TxtMsg.Text &= "배열의 상한값: " & UBound(biFIR).ToString() & vbCrLf
            'TxtMsg.Text &= "objExtraction.FIRLength: " & objExtraction.FIRLength.ToString() & vbCrLf
            'TxtMsg.Text &= "지문 처리 성공!!" & Date.Now.ToString("HH:mm:ss") & vbCrLf

        Catch ex As Exception
            'TxtMsg.Text &= "종료 " & Date.Now.ToString("HH:mm:ss") & vbCrLf
            'TxtMsg.Text &= ex.Message & vbCrLf
            MessageBox.Show(ex.Message)
        Finally
            ' 장치 닫기
            objDevice.Close(UCBioAPI_DEVICE_ID_AUTO_DETECT)
        End Try


    End Sub

    Private Sub frmFinger_Load(sender As Object, e As EventArgs) Handles Me.Load

        Try
            Dim i As Integer
            Dim nNameID As Integer

            '// Create UCSCOM object
            objUCSAPICOM = New UCSAPI()
            objTerminalUserData = objUCSAPICOM.TerminalUserData
            objServerUserData = objUCSAPICOM.ServerUserData
            objAccessLogData = objUCSAPICOM.AccessLogData
            objAccessControlData = objUCSAPICOM.AccessControlData
            objServerAuthentication = objUCSAPICOM.ServerAuthentication
            objTerminalOption = objUCSAPICOM.TerminalOption

            '// Create UCBioBSP object
            objUCBioBSP = New UCBioBSP()
            objDevice = objUCBioBSP.Device
            objExtraction = objUCBioBSP.Extraction
            objMatching = objUCBioBSP.Matching
            objFPData = objUCBioBSP.FPData
            objFPImage = objUCBioBSP.FPImage
            objFastSearch = objUCBioBSP.FastSearch '//지문인증용
            objSmartCard = objUCBioBSP.SmartCard '//RF카드 인식용

            objDevice.Enumerate()  ' 현재 pc에 연결된 지문장치 목록을 가져옴
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub
End Class